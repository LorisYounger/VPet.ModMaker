using HKW.HKWUtils;
using HKW.HKWUtils.Observable;
using LinePutScript;
using LinePutScript.Converter;
using LinePutScript.Localization.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using VPet.ModMaker.Models.ModModel;
using VPet_Simulator.Core;

namespace VPet.ModMaker.Models;

/// <summary>
/// 宠物模型
/// </summary>
public class PetModel : I18nModel<I18nPetInfoModel>
{
    /// <summary>
    /// 来自本体
    /// </summary>
    public ObservableValue<bool> FromMain { get; } = new(false);

    /// <summary>
    /// Id
    /// </summary>
    public ObservableValue<string> Id { get; } = new();

    /// <summary>
    /// 名称Id
    /// </summary>
    public ObservableValue<string> PetNameId { get; } = new();

    /// <summary>
    /// 描述Id
    /// </summary>
    public ObservableValue<string> DescriptionId { get; } = new();

    /// <summary>
    /// 头部点击区域
    /// </summary>
    public ObservableValue<ObservableRect<double>> TouchHeadRect { get; } =
        new(new(159, 16, 189, 178));

    /// <summary>
    /// 身体区域
    /// </summary>
    public ObservableValue<ObservableRect<double>> TouchBodyRect { get; } =
        new(new(166, 206, 163, 136));

    /// <summary>
    /// 提起区域
    /// </summary>
    public ObservableValue<ObservableMultiStateRect> TouchRaisedRect { get; } =
        new(
            new(
                new(0, 50, 500, 200),
                new(0, 50, 500, 200),
                new(0, 50, 500, 200),
                new(0, 200, 500, 300)
            )
        );

    /// <summary>
    /// 提起定位
    /// </summary>
    public ObservableValue<ObservableMultiStatePoint> RaisePoint { get; } =
        new(new(new(290, 128), new(290, 128), new(290, 128), new(225, 115)));

    /// <summary>
    /// 工作
    /// </summary>
    public ObservableCollection<WorkModel> Works { get; } = new();

    /// <summary>
    /// 移动
    /// </summary>
    public ObservableCollection<MoveModel> Moves { get; } = new();

    /// <summary>
    /// 动画
    /// </summary>
    public ObservableCollection<AnimeTypeModel> Animes { get; } = new();

    /// <summary>
    ///食物动画
    /// </summary>
    public ObservableCollection<FoodAnimeTypeModel> FoodAnimes { get; } = new();

    public ObservableValue<int> AnimeCount { get; } = new();

    public PetModel()
    {
        PetNameId.Value = $"{Id.Value}_{nameof(PetNameId)}";
        DescriptionId.Value = $"{Id.Value}_{nameof(DescriptionId)}";
        Id.ValueChanged += (s, e) =>
        {
            PetNameId.Value = $"{e.NewValue}_{nameof(PetNameId)}";
            DescriptionId.Value = $"{e.NewValue}_{nameof(DescriptionId)}";
        };
        AnimeCount.AddNotifySender(Animes);
        AnimeCount.AddNotifySender(FoodAnimes);
        AnimeCount.SenderPropertyChanged += (s, _) =>
        {
            s.Value = Animes.Count + FoodAnimes.Count;
        };
    }

    public PetModel(PetModel model)
        : this()
    {
        Id.Value = model.Id.Value;
        PetNameId.Value = model.PetNameId.Value;
        TouchHeadRect.Value = model.TouchHeadRect.Value.Copy();
        TouchBodyRect.Value = model.TouchBodyRect.Value.Copy();
        TouchRaisedRect.Value = model.TouchRaisedRect.Value.Copy();
        RaisePoint.Value = model.RaisePoint.Value.Copy();
        foreach (var work in model.Works)
            Works.Add(work);

        foreach (var item in model.I18nDatas)
            I18nDatas[item.Key] = item.Value.Copy();
        CurrentI18nData.Value = I18nDatas[I18nHelper.Current.CultureName.Value];
    }

    public PetModel(PetLoader loader, bool fromMain = false)
        : this()
    {
        Id.Value = loader.Name;
        PetNameId.Value = loader.PetName;
        DescriptionId.Value = loader.Intor;

        TouchHeadRect.Value = new(
            loader.Config.TouchHeadLocate.X,
            loader.Config.TouchHeadLocate.Y,
            loader.Config.TouchHeadSize.Width,
            loader.Config.TouchHeadSize.Height
        );

        TouchBodyRect.Value = new(
            loader.Config.TouchBodyLocate.X,
            loader.Config.TouchBodyLocate.Y,
            loader.Config.TouchBodySize.Width,
            loader.Config.TouchBodySize.Height
        );

        TouchRaisedRect.Value.Happy = new(
            loader.Config.TouchRaisedLocate[0].X,
            loader.Config.TouchRaisedLocate[0].Y,
            loader.Config.TouchRaisedSize[0].Width,
            loader.Config.TouchRaisedSize[0].Height
        );
        TouchRaisedRect.Value.Nomal = new(
            loader.Config.TouchRaisedLocate[1].X,
            loader.Config.TouchRaisedLocate[1].Y,
            loader.Config.TouchRaisedSize[1].Width,
            loader.Config.TouchRaisedSize[1].Height
        );
        TouchRaisedRect.Value.PoorCondition = new(
            loader.Config.TouchRaisedLocate[2].X,
            loader.Config.TouchRaisedLocate[2].Y,
            loader.Config.TouchRaisedSize[2].Width,
            loader.Config.TouchRaisedSize[2].Height
        );
        TouchRaisedRect.Value.Ill = new(
            loader.Config.TouchRaisedLocate[3].X,
            loader.Config.TouchRaisedLocate[3].Y,
            loader.Config.TouchRaisedSize[3].Width,
            loader.Config.TouchRaisedSize[3].Height
        );

        RaisePoint.Value.Happy = new(loader.Config.RaisePoint[0].X, loader.Config.RaisePoint[0].Y);
        RaisePoint.Value.Nomal = new(loader.Config.RaisePoint[1].X, loader.Config.RaisePoint[1].Y);
        RaisePoint.Value.PoorCondition = new(
            loader.Config.RaisePoint[2].X,
            loader.Config.RaisePoint[2].Y
        );
        RaisePoint.Value.Ill = new(loader.Config.RaisePoint[3].X, loader.Config.RaisePoint[3].Y);
        // 如果这个宠物数据来自本体, 则不载入 Work 和 Move
        if (FromMain.Value = fromMain)
            return;

        foreach (var work in loader.Config.Works)
            Works.Add(new(work));
        foreach (var move in loader.Config.Moves)
            Moves.Add(new(move));
    }

    public void RefreshId()
    {
        PetNameId.Value = $"{Id.Value}_{nameof(PetNameId)}";
        DescriptionId.Value = $"{Id.Value}_{nameof(DescriptionId)}";
    }

    public void Close()
    {
        foreach (var anime in Animes)
            anime.Close();
        foreach (var anime in FoodAnimes)
            anime.Close();
    }

    #region Save

    /// <summary>
    /// 能被保存
    /// </summary>
    /// <returns></returns>
    public bool CanSave()
    {
        if (
            FromMain.Value
            && Works.Count == 0
            && Moves.Count == 0
            && Animes.Count == 0
            && FoodAnimes.Count == 0
        )
            return false;
        return true;
    }

    /// <summary>
    /// 保存宠物
    /// </summary>
    /// <param name="path">路径</param>
    public void Save(string path)
    {
        foreach (var cultureName in I18nHelper.Current.CultureNames)
        {
            ModInfoModel.SaveI18nDatas[cultureName].TryAdd(
                Id.Value,
                I18nDatas[cultureName].Name.Value
            );
            ModInfoModel.SaveI18nDatas[cultureName].TryAdd(
                PetNameId.Value,
                I18nDatas[cultureName].PetName.Value
            );
            ModInfoModel.SaveI18nDatas[cultureName].TryAdd(
                DescriptionId.Value,
                I18nDatas[cultureName].Description.Value
            );
        }
        var petFile = Path.Combine(path, $"{Id.Value}.lps");
        if (File.Exists(petFile) is false)
            File.Create(petFile).Close();
        var lps = new LPS();
        // 如果本体中存在相同的宠物, 则只保存差异信息
        if (ModMakerInfo.MainPets.TryGetValue(Id.Value, out var mainPet))
            SaveDifferentPetInfo(lps, mainPet);
        else
            SavePetInfo(lps);
        SaveWorksInfo(lps);
        SaveMoveInfo(lps);
        File.WriteAllText(petFile, lps.ToString());

        // 保存图片
        SaveAnime(path);
    }

    private void SaveAnime(string path)
    {
        var petAnimePath = Path.Combine(path, Id.Value);
        foreach (var anime in Animes)
            anime.Save(petAnimePath);
        foreach (var anime in FoodAnimes)
            anime.Save(petAnimePath);
    }

    /// <summary>
    /// 保存移动信息
    /// </summary>
    /// <param name="lps"></param>
    /// <param name="pet"></param>
    void SaveMoveInfo(LPS lps)
    {
        foreach (var move in Moves)
        {
            lps.Add(LPSConvert.SerializeObjectToLine<Line>(move.ToMove(), "move"));
        }
    }

    /// <summary>
    /// 保存工作信息
    /// </summary>
    /// <param name="lps"></param>
    /// <param name="pet"></param>
    void SaveWorksInfo(LPS lps)
    {
        foreach (var work in Works)
        {
            lps.Add(LPSConvert.SerializeObjectToLine<Line>(work.ToWork(), "work"));
            foreach (var cultureName in I18nHelper.Current.CultureNames)
            {
                ModInfoModel.SaveI18nDatas[cultureName].TryAdd(
                    work.Id.Value,
                    work.I18nDatas[cultureName].Name.Value
                );
            }
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
        if (TouchHeadRect != mainPet.TouchHeadRect)
            SavePetTouchHeadInfo(lps);
        if (TouchBodyRect != mainPet.TouchBodyRect)
            SavePetTouchBodyInfo(lps);
        if (TouchRaisedRect != mainPet.TouchRaisedRect)
            SavePetTouchRaisedInfo(lps);
        if (RaisePoint != mainPet.RaisePoint)
            SavePetRaisePointInfo(lps);
    }

    private void SavePetBasicInfo(LPS lps)
    {
        lps.Add(
            new Line("pet", Id.Value)
            {
                new Sub("intor", DescriptionId.Value),
                new Sub("path", Id.Value),
                new Sub("petname", PetNameId.Value)
            }
        );
    }

    private void SavePetTouchHeadInfo(LPS lps)
    {
        lps.Add(
            new Line("touchhead")
            {
                new Sub("px", TouchHeadRect.Value.X),
                new Sub("py", TouchHeadRect.Value.Y),
                new Sub("sw", TouchHeadRect.Value.Width),
                new Sub("sh", TouchHeadRect.Value.Height),
            }
        );
    }

    private void SavePetTouchBodyInfo(LPS lps)
    {
        lps.Add(
            new Line("touchbody")
            {
                new Sub("px", TouchBodyRect.Value.X),
                new Sub("py", TouchBodyRect.Value.Y),
                new Sub("sw", TouchBodyRect.Value.Width),
                new Sub("sh", TouchBodyRect.Value.Height),
            }
        );
    }

    private void SavePetTouchRaisedInfo(LPS lps)
    {
        lps.Add(
            new Line("touchraised")
            {
                new Sub("happy_px", TouchRaisedRect.Value.Happy.X),
                new Sub("happy_py", TouchRaisedRect.Value.Happy.Y),
                new Sub("happy_sw", TouchRaisedRect.Value.Happy.Width),
                new Sub("happy_sh", TouchRaisedRect.Value.Happy.Height),
                //
                new Sub("nomal_px", TouchRaisedRect.Value.Nomal.X),
                new Sub("nomal_py", TouchRaisedRect.Value.Nomal.Y),
                new Sub("nomal_sw", TouchRaisedRect.Value.Nomal.Width),
                new Sub("nomal_sh", TouchRaisedRect.Value.Nomal.Height),
                //
                new Sub("poorcondition_px", TouchRaisedRect.Value.PoorCondition.X),
                new Sub("poorcondition_py", TouchRaisedRect.Value.PoorCondition.Y),
                new Sub("poorcondition_sw", TouchRaisedRect.Value.PoorCondition.Width),
                new Sub("poorcondition_sh", TouchRaisedRect.Value.PoorCondition.Height),
                //
                new Sub("ill_px", TouchRaisedRect.Value.Ill.X),
                new Sub("ill_py", TouchRaisedRect.Value.Ill.Y),
                new Sub("ill_sw", TouchRaisedRect.Value.Ill.Width),
                new Sub("ill_sh", TouchRaisedRect.Value.Ill.Height),
            }
        );
    }

    private void SavePetRaisePointInfo(LPS lps)
    {
        lps.Add(
            new Line("raisepoint")
            {
                new Sub("happy_x", RaisePoint.Value.Happy.X),
                new Sub("happy_y", RaisePoint.Value.Happy.Y),
                //
                new Sub("nomal_x", RaisePoint.Value.Nomal.X),
                new Sub("nomal_y", RaisePoint.Value.Nomal.Y),
                //
                new Sub("poorcondition_x", RaisePoint.Value.PoorCondition.X),
                new Sub("poorcondition_y", RaisePoint.Value.PoorCondition.Y),
                //
                new Sub("ill_x", RaisePoint.Value.Ill.X),
                new Sub("ill_y", RaisePoint.Value.Ill.Y),
            }
        );
    }
    #endregion
    #endregion
}

public class I18nPetInfoModel
{
    public ObservableValue<string> Name { get; } = new();
    public ObservableValue<string> PetName { get; } = new();
    public ObservableValue<string> Description { get; } = new();

    public I18nPetInfoModel Copy()
    {
        var result = new I18nPetInfoModel();
        result.Name.Value = Name.Value;
        result.PetName.Value = PetName.Value;
        result.Description.Value = Description.Value;
        return result;
    }
}

public class ObservableMultiStateRect
    : ObservableClass<ObservableMultiStateRect>,
        IEquatable<ObservableMultiStateRect>
{
    private ObservableRect<double> _happy;
    public ObservableRect<double> Happy
    {
        get => _happy;
        set => SetProperty(ref _happy, value);
    }
    private ObservableRect<double> _nomal;
    public ObservableRect<double> Nomal
    {
        get => _nomal;
        set => SetProperty(ref _nomal, value);
    }
    private ObservableRect<double> _poorCondition;
    public ObservableRect<double> PoorCondition
    {
        get => _poorCondition;
        set => SetProperty(ref _poorCondition, value);
    }
    private ObservableRect<double> _ill;
    public ObservableRect<double> Ill
    {
        get => _ill;
        set => SetProperty(ref _ill, value);
    }

    public ObservableMultiStateRect()
    {
        Happy = new();
        Nomal = new();
        PoorCondition = new();
        Ill = new();
    }

    public ObservableMultiStateRect(
        ObservableRect<double> happy,
        ObservableRect<double> nomal,
        ObservableRect<double> poorCondition,
        ObservableRect<double> ill
    )
    {
        Happy = happy;
        Nomal = nomal;
        PoorCondition = poorCondition;
        Ill = ill;
    }

    public ObservableMultiStateRect Copy()
    {
        return new()
        {
            Happy = Happy.Copy(),
            Nomal = Nomal.Copy(),
            PoorCondition = PoorCondition.Copy(),
            Ill = Ill.Copy(),
        };
    }

    #region Other

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(Happy, Nomal, PoorCondition, Ill);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is ObservableMultiStateRect temp
            && EqualityComparer<ObservableRect<double>>.Default.Equals(Happy, temp.Happy)
            && EqualityComparer<ObservableRect<double>>.Default.Equals(Nomal, temp.Nomal)
            && EqualityComparer<ObservableRect<double>>.Default.Equals(
                PoorCondition,
                temp.PoorCondition
            )
            && EqualityComparer<ObservableRect<double>>.Default.Equals(Ill, temp.Ill);
    }

    /// <inheritdoc/>
    public bool Equals(ObservableMultiStateRect? other)
    {
        return Equals(obj: other);
    }

    /// <inheritdoc/>
    public static bool operator ==(ObservableMultiStateRect a, ObservableMultiStateRect b)
    {
        return Equals(a, b);
    }

    /// <inheritdoc/>
    public static bool operator !=(ObservableMultiStateRect a, ObservableMultiStateRect b)
    {
        return Equals(a, b) is not true;
    }

    #endregion
}

public class ObservableMultiStatePoint
    : ObservableClass<ObservableMultiStatePoint>,
        IEquatable<ObservableMultiStatePoint>
{
    private ObservablePoint<double> _happy;
    public ObservablePoint<double> Happy
    {
        get => _happy;
        set => SetProperty(ref _happy, value);
    }
    private ObservablePoint<double> _nomal;
    public ObservablePoint<double> Nomal
    {
        get => _nomal;
        set => SetProperty(ref _nomal, value);
    }
    private ObservablePoint<double> _poorCondition;
    public ObservablePoint<double> PoorCondition
    {
        get => _poorCondition;
        set => SetProperty(ref _poorCondition, value);
    }
    private ObservablePoint<double> _ill;
    public ObservablePoint<double> Ill
    {
        get => _ill;
        set => SetProperty(ref _ill, value);
    }

    public ObservableMultiStatePoint()
    {
        Happy = new();
        Nomal = new();
        PoorCondition = new();
        Ill = new();
    }

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

    public ObservableMultiStatePoint Copy()
    {
        return new()
        {
            Happy = Happy.Copy(),
            Nomal = Nomal.Copy(),
            PoorCondition = PoorCondition.Copy(),
            Ill = Ill.Copy(),
        };
    }

    #region Other

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return HashCode.Combine(Happy, Nomal, PoorCondition, Ill);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is ObservableMultiStatePoint temp
            && EqualityComparer<ObservablePoint<double>>.Default.Equals(Happy, temp.Happy)
            && EqualityComparer<ObservablePoint<double>>.Default.Equals(Nomal, temp.Nomal)
            && EqualityComparer<ObservablePoint<double>>.Default.Equals(
                PoorCondition,
                temp.PoorCondition
            )
            && EqualityComparer<ObservablePoint<double>>.Default.Equals(Ill, temp.Ill);
    }

    /// <inheritdoc/>
    public bool Equals(ObservableMultiStatePoint? other)
    {
        return Equals(obj: other);
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
