using System.Diagnostics.CodeAnalysis;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using HKW.HKWUtils;
using HKW.HKWUtils.Drawing;
using HKW.HKWUtils.Observable;
using HKW.Mapper;
using HKW.MVVM;
using HKW.MVVM.SourceGenerator;
using LinePutScript;
using LinePutScript.Converter;
using VPet_Simulator.Core;
using VPet.ModMaker.ViewModels;

namespace VPet.ModMaker.Models;

/// <summary>
/// 宠物模型
/// </summary>
[MapTarget(typeof(PetModel))]
public sealed partial class PetModel : ObservableModelBase, IDisposable
{
    /// <inheritdoc/>
    public PetModel(ObservableI18nResource<string, string> i18nResource)
    {
        I18nResource = i18nResource;
    }

    /// <inheritdoc/>
    /// <param name="model">宠物模型</param>
    public PetModel(PetModel model, ObservableI18nResource<string, string> i18nResource)
    {
        this.MapFrom(model);
        I18nResource = i18nResource;
    }

    /// <inheritdoc/>
    /// <param name="loader">宠物载入器</param>
    /// <param name="i18nResource">I18n资源</param>
    /// <param name="fromMain">来自本体</param>
    [SetsRequiredMembers]
    public PetModel(
        PetLoader loader,
        ObservableI18nResource<string, string> i18nResource,
        bool fromMain = false
    )
    {
        ID = loader.Name;
        I18nResource = i18nResource;

        if (loader.PetName != PetNameID)
        {
            if (string.IsNullOrWhiteSpace(loader.PetName))
            {
                i18nResource.SetData(PetNameID, loader.Name);
            }
            else if (i18nResource.RenameKey(loader.PetName, PetNameID) is false)
            {
                i18nResource.SetData(PetNameID, loader.PetName);
            }
        }
        if (
            string.Equals(loader.Intor, DescriptionID) is false
            && i18nResource.RenameKey(loader.Intor, DescriptionID) is false
        )
        {
            i18nResource.SetData(DescriptionID, loader.Intor);
        }

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
        FromMain = fromMain;
        if (FromMain)
            return;

        foreach (var work in loader.Config.Works)
            Works.Add(new(work, i18nResource));
        foreach (var move in loader.Config.Moves)
            Moves.Add(new(move));
    }

    /// <summary>
    /// 来自本体
    /// </summary>
    [ObservableProperty]
    public bool FromMain { get; set; }

    /// <summary>
    /// Id
    /// </summary>
    [ObservableProperty]
    public string ID { get; set; } = string.Empty;

    /// <summary>
    /// 名称ID
    /// </summary>
    [MapIgnoreProperty]
    [NotifyPropertyChangeFrom(nameof(ID))]
    public string PetNameID => $"{ID}_PetName";

    /// <summary>
    /// 描述ID
    /// </summary>
    [MapIgnoreProperty]
    [NotifyPropertyChangeFrom(nameof(ID))]
    public string DescriptionID => $"{ID}_Description";

    /// <summary>
    /// I18n资源
    /// </summary>
    [MapIgnoreProperty]
    [ObservableProperty]
    public ObservableI18nResource<string, string> I18nResource { get; set; }

    partial void OnI18nResourceChanged(
        ObservableI18nResource<string, string> oldValue,
        ObservableI18nResource<string, string> newValue
    )
    {
        I18nObserver.Resource = newValue;
    }

    [MapIgnoreProperty]
    public I18nObserver<PetModel, string, string> I18nObserver =>
        field ??= new(I18nResource, this, (s, p) => s.OnPropertyChanged(p))
        {
            new(s => s.ID, nameof(Name)),
            new(s => s.PetNameID, nameof(PetName)),
            new(s => s.DescriptionID, nameof(Description)),
        };

    /// <summary>
    /// 名称
    /// </summary>
    [MapIgnoreProperty]
    public string Name
    {
        get => I18nObserver.GetDataOrDefault(ID);
        set => I18nObserver.SetData(ID, value);
    }

    /// <summary>
    /// 宠物名称
    /// </summary>
    [MapIgnoreProperty]
    public string PetName
    {
        get => I18nObserver.GetDataOrDefault(PetNameID);
        set => I18nObserver.SetData(PetNameID, value);
    }

    /// <summary>
    /// 详情
    /// </summary>
    [MapIgnoreProperty]
    public string Description
    {
        get => I18nObserver.GetDataOrDefault(DescriptionID);
        set => I18nObserver.SetData(DescriptionID, value);
    }

    /// <summary>
    /// 标签
    /// </summary>
    [ObservableProperty]
    public string Tags { get; set; } = string.Empty;

    public static ObservableRectangle<double> DefaultTouchHeadRectangleLocation { get; } =
        new(159, 16, 189, 178);

    /// <summary>
    /// 头部点击区域
    /// </summary>
    [ObservableProperty]
    public ObservableRectangle<double> TouchHeadRectangleLocation { get; set; } =
        new(DefaultTouchHeadRectangleLocation);

    public static ObservableRectangle<double> DefaultTouchBodyRectangleLocation { get; } =
        new(166, 206, 163, 136);

    /// <summary>
    /// 身体区域
    /// </summary>
    [ObservableProperty]
    public ObservableRectangle<double> TouchBodyRectangleLocation { get; set; } =
        new(DefaultTouchBodyRectangleLocation);

    public static ObservableMultiStateRectangleLocation DefaultTouchRaisedRectangleLocation { get; } =
        new(
            new(0, 50, 500, 200),
            new(0, 50, 500, 200),
            new(0, 50, 500, 200),
            new(0, 200, 500, 300)
        );

    /// <summary>
    /// 提起区域
    /// </summary>
    [ObservableProperty]
    public ObservableMultiStateRectangleLocation TouchRaisedRectangleLocation { get; set; } =
        DefaultTouchRaisedRectangleLocation.Clone();

    public static ObservableMultiStatePoint DefaultRaisePoint { get; } =
        new(new(290, 128), new(290, 128), new(290, 128), new(225, 115));

    /// <summary>
    /// 提起定位
    /// </summary>
    [ObservableProperty]
    public ObservableMultiStatePoint RaisePoint { get; set; } = DefaultRaisePoint.Clone();

    /// <summary>
    /// 工作
    /// </summary>
    [MapProperty(typeof(PetModel), MapType = MapPropertyType.Reference)]
    public ObservableList<WorkModel> Works { get; set; } = [];

    /// <summary>
    /// 移动
    /// </summary>
    [MapProperty(typeof(PetModel), MapType = MapPropertyType.Reference)]
    public ObservableList<MoveModel> Moves { get; set; } = [];

    /// <summary>
    /// 动画
    /// </summary>
    [MapProperty(typeof(PetModel), MapType = MapPropertyType.Reference)]
    public ObservableList<IAnimeModel> Animes { get; set; } = [];

    ///// <summary>
    /////食物动画
    ///// </summary>
    //public ObservableList<FoodAnimeTypeModel> FoodAnimes { get; } = [];

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
        if (ModMaker.MainPets.TryGetValue(ID, out var mainPet))
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
        if (Directory.Exists(petAnimePath))
            Directory.Delete(petAnimePath, true);
        foreach (var anime in Animes)
            anime.Save(petAnimePath);
    }

    private static readonly GraphHelper.Move _move = new();

    /// <summary>
    /// 保存移动信息
    /// </summary>
    /// <param name="lps"></param>
    void SaveMoveInfo(LPS lps)
    {
        foreach (var move in Moves)
        {
            lps.Add(LPSConvert.SerializeObjectToLine<Line>(move.MapTo(_move), "move"));
        }
    }

    private static readonly GraphHelper.Work _work = new();

    /// <summary>
    /// 保存工作信息
    /// </summary>
    /// <param name="lps"></param>
    void SaveWorksInfo(LPS lps)
    {
        foreach (var work in Works)
        {
            lps.Add(LPSConvert.SerializeObjectToLine<Line>(work.MapTo(_work), "work"));
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
            TouchHeadRectangleLocation != DefaultTouchHeadRectangleLocation
            && TouchHeadRectangleLocation != mainPet.TouchHeadRectangleLocation
        )
            SavePetTouchHeadInfo(lps);
        if (
            TouchBodyRectangleLocation != DefaultTouchBodyRectangleLocation
            && TouchBodyRectangleLocation != mainPet.TouchBodyRectangleLocation
        )
            SavePetTouchBodyInfo(lps);
        if (
            TouchRaisedRectangleLocation != DefaultTouchRaisedRectangleLocation
            && TouchRaisedRectangleLocation != mainPet.TouchRaisedRectangleLocation
        )
            SavePetTouchRaisedInfo(lps);
        if (RaisePoint != DefaultRaisePoint && RaisePoint != mainPet.RaisePoint)
            SavePetRaisePointInfo(lps);
    }

    private void SavePetBasicInfo(LPS lps)
    {
        lps.Add(
            new Line("pet", ID)
            {
                new Sub("path", ID),
                new Sub("petname", PetNameID),
                new Sub("intor", DescriptionID),
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
                new Sub("nomal_px", TouchRaisedRectangleLocation.Nomal.X),
                new Sub("nomal_py", TouchRaisedRectangleLocation.Nomal.Y),
                new Sub("nomal_sw", TouchRaisedRectangleLocation.Nomal.Width),
                new Sub("nomal_sh", TouchRaisedRectangleLocation.Nomal.Height),
                new Sub("poorcondition_px", TouchRaisedRectangleLocation.PoorCondition.X),
                new Sub("poorcondition_py", TouchRaisedRectangleLocation.PoorCondition.Y),
                new Sub("poorcondition_sw", TouchRaisedRectangleLocation.PoorCondition.Width),
                new Sub("poorcondition_sh", TouchRaisedRectangleLocation.PoorCondition.Height),
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
                new Sub("nomal_x", RaisePoint.Nomal.X),
                new Sub("nomal_y", RaisePoint.Nomal.Y),
                new Sub("poorcondition_x", RaisePoint.PoorCondition.X),
                new Sub("poorcondition_y", RaisePoint.PoorCondition.Y),
                new Sub("ill_x", RaisePoint.Ill.X),
                new Sub("ill_y", RaisePoint.Ill.Y),
            }
        );
    }
    #endregion
    #endregion

    public void Dispose()
    {
        I18nObserver?.Dispose();
    }

    public void DisposeAll()
    {
        Dispose();
        foreach (var work in Works)
            work.Dispose();
        foreach (var anime in Animes)
            anime.Dispose();
    }
}

/// <summary>
/// 可观察的多状态矩形位置
/// </summary>
public sealed partial class ObservableMultiStateRectangleLocation
    : ObservableObjectEx,
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
        Happy = happy.Clone();
        Nomal = nomal.Clone();
        PoorCondition = poorCondition.Clone();
        Ill = ill.Clone();
    }

    /// <summary>
    /// 开心时的范围
    /// </summary>
    [ObservableProperty]
    public ObservableRectangle<double> Happy { get; set; }

    /// <summary>
    /// 普通时的范围
    /// </summary>
    [ObservableProperty]
    public ObservableRectangle<double> Nomal { get; set; }

    /// <summary>
    /// 低状态时的范围
    /// </summary>
    [ObservableProperty]
    public ObservableRectangle<double> PoorCondition { get; set; }

    /// <summary>
    /// 生病时的范围
    /// </summary>
    [ObservableProperty]
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
public sealed partial class ObservableMultiStatePoint
    : ObservableObjectEx,
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
        Happy = happy.Clone();
        Nomal = nomal.Clone();
        PoorCondition = poorCondition.Clone();
        Ill = ill.Clone();
    }

    /// <summary>
    /// 开心时的点位
    /// </summary>
    [ObservableProperty]
    public ObservablePoint<double> Happy { get; set; }

    /// <summary>
    /// 普通时的点位
    /// </summary>
    [ObservableProperty]
    public ObservablePoint<double> Nomal { get; set; }

    /// <summary>
    /// 低状态时的点位
    /// </summary>
    [ObservableProperty]
    public ObservablePoint<double> PoorCondition { get; set; }

    /// <summary>
    /// 生病时的点位
    /// </summary>
    [ObservableProperty]
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
