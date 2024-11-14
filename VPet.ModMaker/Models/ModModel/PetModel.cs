using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using HKW.HKWMapper;
using HKW.HKWReactiveUI;
using HKW.HKWUtils;
using HKW.HKWUtils.Observable;
using LinePutScript;
using LinePutScript.Converter;
using LinePutScript.Localization.WPF;
using VPet.ModMaker.Models.ModModel;
using VPet.ModMaker.ViewModels;
using VPet_Simulator.Core;

namespace VPet.ModMaker.Models;

/// <summary>
/// 宠物模型
/// </summary>
public partial class PetModel : ViewModelBase
{
    /// <inheritdoc/>
    public PetModel()
    {
        Animes.PropertyChanged += Animes_PropertyChanged;
        FoodAnimes.PropertyChanged += FoodAnimes_PropertyChanged;
    }

    /// <inheritdoc/>
    /// <param name="model">宠物模型</param>
    public PetModel(PetModel model)
        : this()
    {
        ID = model.ID;
        Tags = model.Tags;
        TouchHeadRectangleLocation = model.TouchHeadRectangleLocation.Clone();
        TouchBodyRectangleLocation = model.TouchBodyRectangleLocation.Clone();
        TouchRaisedRectangleLocation = model.TouchRaisedRectangleLocation.Clone();
        RaisePoint = model.RaisePoint.Clone();
        foreach (var work in model.Works)
            Works.Add(work);
    }

    /// <inheritdoc/>
    /// <param name="loader">宠物载入器</param>
    /// <param name="i18nResource">I18n资源</param>
    /// <param name="fromMain">来自本体</param>
    [SetsRequiredMembers]
    public PetModel(
        PetLoader loader,
        I18nResource<string, string> i18nResource,
        bool fromMain = false
    )
        : this()
    {
        ID = loader.Name;
        if (loader.PetName != PetNameID)
            i18nResource.ReplaceCultureDataKey(loader.PetName, PetNameID);
        if (loader.Intor != DescriptionID)
            i18nResource.ReplaceCultureDataKey(loader.Intor, DescriptionID);
        I18nResource = i18nResource;

        Tags = loader.Config.Data["tag"].Info;

        TouchHeadRectangleLocation = new(
            loader.Config.TouchHeadLocate.X,
            loader.Config.TouchHeadLocate.Y,
            loader.Config.TouchHeadSize.Width,
            loader.Config.TouchHeadSize.Height
        );

        TouchBodyRectangleLocation = new(
            loader.Config.TouchBodyLocate.X,
            loader.Config.TouchBodyLocate.Y,
            loader.Config.TouchBodySize.Width,
            loader.Config.TouchBodySize.Height
        );

        TouchRaisedRectangleLocation.Happy = new(
            loader.Config.TouchRaisedLocate[0].X,
            loader.Config.TouchRaisedLocate[0].Y,
            loader.Config.TouchRaisedSize[0].Width,
            loader.Config.TouchRaisedSize[0].Height
        );
        TouchRaisedRectangleLocation.Nomal = new(
            loader.Config.TouchRaisedLocate[1].X,
            loader.Config.TouchRaisedLocate[1].Y,
            loader.Config.TouchRaisedSize[1].Width,
            loader.Config.TouchRaisedSize[1].Height
        );
        TouchRaisedRectangleLocation.PoorCondition = new(
            loader.Config.TouchRaisedLocate[2].X,
            loader.Config.TouchRaisedLocate[2].Y,
            loader.Config.TouchRaisedSize[2].Width,
            loader.Config.TouchRaisedSize[2].Height
        );
        TouchRaisedRectangleLocation.Ill = new(
            loader.Config.TouchRaisedLocate[3].X,
            loader.Config.TouchRaisedLocate[3].Y,
            loader.Config.TouchRaisedSize[3].Width,
            loader.Config.TouchRaisedSize[3].Height
        );

        RaisePoint.Happy = new(loader.Config.RaisePoint[0].X, loader.Config.RaisePoint[0].Y);
        RaisePoint.Nomal = new(loader.Config.RaisePoint[1].X, loader.Config.RaisePoint[1].Y);
        RaisePoint.PoorCondition = new(
            loader.Config.RaisePoint[2].X,
            loader.Config.RaisePoint[2].Y
        );
        RaisePoint.Ill = new(loader.Config.RaisePoint[3].X, loader.Config.RaisePoint[3].Y);
        // 如果这个宠物数据来自本体, 则不载入 Work 和 Move
        if (FromMain = fromMain)
            return;

        foreach (var work in loader.Config.Works)
            Works.Add(new(work) { I18nResource = I18nResource! });
        foreach (var move in loader.Config.Moves)
            Moves.Add(new(move));
    }

    /// <summary>
    /// 默认宠物
    /// </summary>
    public static PetModel Default { get; } = new() { I18nResource = new() };

    /// <summary>
    /// 来自本体
    /// </summary>
    [ReactiveProperty]
    public bool FromMain { get; set; }

    /// <summary>
    /// Id
    /// </summary>
    [ReactiveProperty]
    public string ID { get; set; } = string.Empty;

    /// <summary>
    /// 名称ID
    /// </summary>
    [NotifyPropertyChangeFrom(nameof(ID))]
    public string PetNameID => $"{ID}_PetName";

    /// <summary>
    /// 描述ID
    /// </summary>
    [NotifyPropertyChangeFrom(nameof(ID))]
    public string DescriptionID => $"{ID}_Description";

    /// <summary>
    /// I18n资源
    /// </summary>
    [MapIgnoreProperty]
    [ReactiveProperty]
    public required I18nResource<string, string> I18nResource { get; set; }

    partial void OnI18nResourceChanged(
        I18nResource<string, string> oldValue,
        I18nResource<string, string> newValue
    )
    {
        oldValue?.I18nObjects.Remove(I18nObject);
        newValue?.I18nObjects?.Add(I18nObject);
    }

    /// <summary>
    /// 初始化I18n资源
    /// </summary>
    public void InitializeI18nResource()
    {
        foreach (var work in Works)
            work.I18nResource = I18nResource;
        if (FromMain)
        {
            foreach (var cultureName in LocalizeCore.AvailableCultures)
            {
                if (CultureUtils.TryGetCultureInfo(cultureName, out var culture) is false)
                    continue;
                if (LocalizeCore.Localizations.TryGetValue(cultureName, out var data) is false)
                    continue;
                foreach (var line in data)
                {
                    if (line?.Name == ID)
                    {
                        I18nResource.AddCultureData(culture, ID, line.Info);
                    }
                    else if (line?.Name == PetNameID)
                    {
                        I18nResource.AddCultureData(culture, PetNameID, line.Info);
                    }
                    else if (line?.Name == DescriptionID)
                    {
                        I18nResource.AddCultureData(culture, DescriptionID, line.Info);
                    }
                }
                I18nResource.AddCultureData(culture, ID, ID);
                I18nResource.AddCultureData(culture, PetNameID, PetNameID);
                I18nResource.AddCultureData(culture, DescriptionID, DescriptionID);
            }
        }
    }

    /// <summary>
    /// I18n对象
    /// </summary>
    [NotifyPropertyChangeFrom("")]
    public I18nObject<string, string> I18nObject => new(this);

    /// <summary>
    /// 名称
    /// </summary>
    [MapIgnoreProperty]
    [ReactiveI18nProperty("I18nResource", nameof(I18nObject), nameof(ID))]
    public string Name
    {
        get => I18nResource.GetCurrentCultureDataOrDefault(ID);
        set => I18nResource.SetCurrentCultureData(ID, value);
    }

    /// <summary>
    /// 宠物名称
    /// </summary>
    [MapIgnoreProperty]
    [ReactiveI18nProperty("I18nResource", nameof(I18nObject), nameof(PetNameID))]
    public string PetName
    {
        get => I18nResource.GetCurrentCultureDataOrDefault(PetNameID);
        set => I18nResource.SetCurrentCultureData(PetNameID, value);
    }

    /// <summary>
    /// 详情
    /// </summary>
    [MapIgnoreProperty]
    [ReactiveI18nProperty("I18nResource", nameof(I18nObject), nameof(DescriptionID))]
    public string Description
    {
        get => I18nResource.GetCurrentCultureDataOrDefault(DescriptionID);
        set => I18nResource.SetCurrentCultureData(DescriptionID, value);
    }

    /// <summary>
    /// 标签
    /// </summary>
    [ReactiveProperty]
    public string Tags { get; set; } = string.Empty;

    /// <summary>
    /// 头部点击区域
    /// </summary>
    [ReactiveProperty]
    public ObservableRectangle<double> TouchHeadRectangleLocation { get; set; } =
        new(159, 16, 189, 178);

    /// <summary>
    /// 身体区域
    /// </summary>
    [ReactiveProperty]
    public ObservableRectangle<double> TouchBodyRectangleLocation { get; set; } =
        new(166, 206, 163, 136);

    /// <summary>
    /// 提起区域
    /// </summary>
    [ReactiveProperty]
    public ObservableMultiStateRectangleLocation TouchRaisedRectangleLocation { get; set; } =
        new(
            new(0, 50, 500, 200),
            new(0, 50, 500, 200),
            new(0, 50, 500, 200),
            new(0, 200, 500, 300)
        );

    /// <summary>
    /// 提起定位
    /// </summary>
    [ReactiveProperty]
    public ObservableMultiStatePoint RaisePoint { get; set; } =
        new(new(290, 128), new(290, 128), new(290, 128), new(225, 115));

    /// <summary>
    /// 工作
    /// </summary>
    public ObservableList<WorkModel> Works { get; } = [];

    /// <summary>
    /// 移动
    /// </summary>
    public ObservableList<MoveModel> Moves { get; } = [];

    /// <summary>
    /// 动画
    /// </summary>
    public ObservableList<AnimeTypeModel> Animes { get; } = [];

    /// <summary>
    ///食物动画
    /// </summary>
    public ObservableList<FoodAnimeTypeModel> FoodAnimes { get; } = [];

    /// <summary>
    /// 动画数量
    /// </summary>
    [ReactiveProperty]
    public int AnimeCount { get; set; }

    private void FoodAnimes_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        AnimeCount = Animes.Count + FoodAnimes.Count;
    }

    private void Animes_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        AnimeCount = Animes.Count + FoodAnimes.Count;
    }

    /// <summary>
    /// 关闭
    /// </summary>
    public void Close()
    {
        foreach (var anime in Animes)
            anime.Close();
        foreach (var anime in FoodAnimes)
            anime.Close();
        I18nResource.I18nObjects.Remove(I18nObject);
    }

    #region Save

    /// <summary>
    /// 保存宠物
    /// </summary>
    /// <param name="path">路径</param>
    public void Save(string path)
    {
        var petFile = Path.Combine(path, $"{ID}.lps");
        if (File.Exists(petFile) is false)
            File.Create(petFile).Close();
        var lps = new LPS();
        // 如果本体中存在相同的宠物, 则只保存差异信息
        if (NativeData.MainPets.TryGetValue(ID, out var mainPet))
            SaveDifferentPetInfo(lps, mainPet);
        else
            SavePetInfo(lps);
        SaveWorksInfo(lps);
        SaveMoveInfo(lps);
        File.WriteAllText(petFile, lps.ToString());

        SaveAnime(path);
    }

    private void SaveAnime(string path)
    {
        var petAnimePath = Path.Combine(path, ID);
        foreach (var anime in Animes)
            anime.Save(petAnimePath);
        foreach (var anime in FoodAnimes)
            anime.Save(petAnimePath);
    }

    /// <summary>
    /// 保存移动信息
    /// </summary>
    /// <param name="lps"></param>
    void SaveMoveInfo(LPS lps)
    {
        foreach (var move in Moves)
        {
            lps.Add(LPSConvert.SerializeObjectToLine<Line>(move.MapToMove(new()), "move"));
        }
    }

    /// <summary>
    /// 保存工作信息
    /// </summary>
    /// <param name="lps"></param>
    void SaveWorksInfo(LPS lps)
    {
        foreach (var work in Works)
        {
            lps.Add(LPSConvert.SerializeObjectToLine<Line>(work.ToWork(), "work"));
        }
    }

    #region SavePetInfo
    /// <summary>
    /// 保存宠物信息
    /// </summary>
    /// <param name="lps"></param>
    private void SavePetInfo(LPS lps)
    {
        SavePetBasicInfo(lps);
        SavePetTouchHeadInfo(lps);
        SavePetTouchBodyInfo(lps);
        SavePetTouchRaisedInfo(lps);
        SavePetRaisePointInfo(lps);
    }

    /// <summary>
    /// 保存差异宠物信息
    /// <para>
    /// 用于本体存在同名宠物的情况下
    /// </para>
    /// </summary>
    /// <param name="lps"></param>
    /// <param name="mainPet">本体宠物</param>
    private void SaveDifferentPetInfo(LPS lps, PetModel mainPet)
    {
        SavePetBasicInfo(lps);
        // 如果值不为默认并且不与本体值相同, 则保存
        if (
            TouchHeadRectangleLocation != Default.TouchHeadRectangleLocation
            && TouchHeadRectangleLocation != mainPet.TouchHeadRectangleLocation
        )
            SavePetTouchHeadInfo(lps);
        if (
            TouchBodyRectangleLocation != Default.TouchBodyRectangleLocation
            && TouchBodyRectangleLocation != mainPet.TouchBodyRectangleLocation
        )
            SavePetTouchBodyInfo(lps);
        if (
            TouchRaisedRectangleLocation != Default.TouchRaisedRectangleLocation
            && TouchRaisedRectangleLocation != mainPet.TouchRaisedRectangleLocation
        )
            SavePetTouchRaisedInfo(lps);
        if (RaisePoint != Default.RaisePoint && RaisePoint != mainPet.RaisePoint)
            SavePetRaisePointInfo(lps);
    }

    private void SavePetBasicInfo(LPS lps)
    {
        lps.Add(
            new Line("pet", ID)
            {
                new Sub("intor", DescriptionID),
                new Sub("path", ID),
                new Sub("petname", PetNameID)
            }
        );
        if (string.IsNullOrWhiteSpace(Tags) is false)
            lps.Add(new Line("tag", Tags));
    }

    private void SavePetTouchHeadInfo(LPS lps)
    {
        lps.Add(
            new Line("touchhead")
            {
                new Sub("px", TouchHeadRectangleLocation.X),
                new Sub("py", TouchHeadRectangleLocation.Y),
                new Sub("sw", TouchHeadRectangleLocation.Width),
                new Sub("sh", TouchHeadRectangleLocation.Height),
            }
        );
    }

    private void SavePetTouchBodyInfo(LPS lps)
    {
        lps.Add(
            new Line("touchbody")
            {
                new Sub("px", TouchBodyRectangleLocation.X),
                new Sub("py", TouchBodyRectangleLocation.Y),
                new Sub("sw", TouchBodyRectangleLocation.Width),
                new Sub("sh", TouchBodyRectangleLocation.Height),
            }
        );
    }

    private void SavePetTouchRaisedInfo(LPS lps)
    {
        lps.Add(
            new Line("touchraised")
            {
                new Sub("happy_px", TouchRaisedRectangleLocation.Happy.X),
                new Sub("happy_py", TouchRaisedRectangleLocation.Happy.Y),
                new Sub("happy_sw", TouchRaisedRectangleLocation.Happy.Width),
                new Sub("happy_sh", TouchRaisedRectangleLocation.Happy.Height),
                //
                new Sub("nomal_px", TouchRaisedRectangleLocation.Nomal.X),
                new Sub("nomal_py", TouchRaisedRectangleLocation.Nomal.Y),
                new Sub("nomal_sw", TouchRaisedRectangleLocation.Nomal.Width),
                new Sub("nomal_sh", TouchRaisedRectangleLocation.Nomal.Height),
                //
                new Sub("poorcondition_px", TouchRaisedRectangleLocation.PoorCondition.X),
                new Sub("poorcondition_py", TouchRaisedRectangleLocation.PoorCondition.Y),
                new Sub("poorcondition_sw", TouchRaisedRectangleLocation.PoorCondition.Width),
                new Sub("poorcondition_sh", TouchRaisedRectangleLocation.PoorCondition.Height),
                //
                new Sub("ill_px", TouchRaisedRectangleLocation.Ill.X),
                new Sub("ill_py", TouchRaisedRectangleLocation.Ill.Y),
                new Sub("ill_sw", TouchRaisedRectangleLocation.Ill.Width),
                new Sub("ill_sh", TouchRaisedRectangleLocation.Ill.Height),
            }
        );
    }

    private void SavePetRaisePointInfo(LPS lps)
    {
        lps.Add(
            new Line("raisepoint")
            {
                new Sub("happy_x", RaisePoint.Happy.X),
                new Sub("happy_y", RaisePoint.Happy.Y),
                //
                new Sub("nomal_x", RaisePoint.Nomal.X),
                new Sub("nomal_y", RaisePoint.Nomal.Y),
                //
                new Sub("poorcondition_x", RaisePoint.PoorCondition.X),
                new Sub("poorcondition_y", RaisePoint.PoorCondition.Y),
                //
                new Sub("ill_x", RaisePoint.Ill.X),
                new Sub("ill_y", RaisePoint.Ill.Y),
            }
        );
    }
    #endregion
    #endregion
}

/// <summary>
/// 可观察的多状态矩形位置
/// </summary>
public partial class ObservableMultiStateRectangleLocation
    : ViewModelBase,
        IEquatable<ObservableMultiStateRectangleLocation>,
        ICloneable<ObservableMultiStateRectangleLocation>
{
    /// <inheritdoc/>
    public ObservableMultiStateRectangleLocation()
    {
        Happy = new();
        Nomal = new();
        PoorCondition = new();
        Ill = new();
    }

    /// <inheritdoc/>
    public ObservableMultiStateRectangleLocation(
        ObservableRectangle<double> happy,
        ObservableRectangle<double> nomal,
        ObservableRectangle<double> poorCondition,
        ObservableRectangle<double> ill
    )
    {
        Happy = happy;
        Nomal = nomal;
        PoorCondition = poorCondition;
        Ill = ill;
    }

    /// <summary>
    /// 开心时的范围
    /// </summary>
    [ReactiveProperty]
    public ObservableRectangle<double> Happy { get; set; }

    /// <summary>
    /// 普通时的范围
    /// </summary>
    [ReactiveProperty]
    public ObservableRectangle<double> Nomal { get; set; }

    /// <summary>
    /// 低状态时的范围
    /// </summary>
    [ReactiveProperty]
    public ObservableRectangle<double> PoorCondition { get; set; }

    /// <summary>
    /// 生病时的范围
    /// </summary>
    [ReactiveProperty]
    public ObservableRectangle<double> Ill { get; set; }

    /// <inheritdoc/>
    public ObservableMultiStateRectangleLocation Clone()
    {
        return new()
        {
            Happy = Happy.Clone(),
            Nomal = Nomal.Clone(),
            PoorCondition = PoorCondition.Clone(),
            Ill = Ill.Clone(),
        };
    }

    object ICloneable.Clone() => Clone();

    #region Other

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(Happy, Nomal, PoorCondition, Ill);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return Equals(obj as ObservableMultiStateRectangleLocation);
    }

    /// <inheritdoc/>
    public bool Equals(ObservableMultiStateRectangleLocation? other)
    {
        return Happy.Equals(other?.Happy)
            && Nomal.Equals(other?.Nomal)
            && PoorCondition.Equals(other?.PoorCondition)
            && Ill.Equals(other?.Ill);
    }

    /// <inheritdoc/>
    public static bool operator ==(
        ObservableMultiStateRectangleLocation a,
        ObservableMultiStateRectangleLocation b
    )
    {
        return Equals(a, b);
    }

    /// <inheritdoc/>
    public static bool operator !=(
        ObservableMultiStateRectangleLocation a,
        ObservableMultiStateRectangleLocation b
    )
    {
        return Equals(a, b) is not true;
    }

    #endregion
}

/// <summary>
/// 可观察的多状态点位
/// </summary>
public partial class ObservableMultiStatePoint
    : ViewModelBase,
        IEquatable<ObservableMultiStatePoint>,
        ICloneable<ObservableMultiStatePoint>
{
    /// <inheritdoc/>
    public ObservableMultiStatePoint()
    {
        Happy = new();
        Nomal = new();
        PoorCondition = new();
        Ill = new();
    }

    /// <inheritdoc/>
    public ObservableMultiStatePoint(
        ObservablePoint<double> happy,
        ObservablePoint<double> nomal,
        ObservablePoint<double> poorCondition,
        ObservablePoint<double> ill
    )
    {
        Happy = happy;
        Nomal = nomal;
        PoorCondition = poorCondition;
        Ill = ill;
    }

    /// <summary>
    /// 开心时的点位
    /// </summary>
    [ReactiveProperty]
    public ObservablePoint<double> Happy { get; set; }

    /// <summary>
    /// 普通时的点位
    /// </summary>
    [ReactiveProperty]
    public ObservablePoint<double> Nomal { get; set; }

    /// <summary>
    /// 低状态时的点位
    /// </summary>
    [ReactiveProperty]
    public ObservablePoint<double> PoorCondition { get; set; }

    /// <summary>
    /// 生病时的点位
    /// </summary>
    [ReactiveProperty]
    public ObservablePoint<double> Ill { get; set; }

    /// <inheritdoc/>
    public ObservableMultiStatePoint Clone()
    {
        return new()
        {
            Happy = Happy.Clone(),
            Nomal = Nomal.Clone(),
            PoorCondition = PoorCondition.Clone(),
            Ill = Ill.Clone(),
        };
    }

    object ICloneable.Clone() => Clone();

    #region Other

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(Happy, Nomal, PoorCondition, Ill);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return Equals(obj as ObservableMultiStatePoint);
    }

    /// <inheritdoc/>
    public bool Equals(ObservableMultiStatePoint? other)
    {
        return Happy.Equals(other?.Happy)
            && Nomal.Equals(other?.Nomal)
            && PoorCondition.Equals(other?.PoorCondition)
            && Ill.Equals(other?.Ill);
    }

    /// <inheritdoc/>
    public static bool operator ==(ObservableMultiStatePoint a, ObservableMultiStatePoint b)
    {
        return Equals(a, b);
    }

    /// <inheritdoc/>
    public static bool operator !=(ObservableMultiStatePoint a, ObservableMultiStatePoint b)
    {
        return Equals(a, b) is not true;
    }

    #endregion
}
