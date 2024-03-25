using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using HKW.HKWUtils;
using HKW.HKWUtils.Observable;
using LinePutScript;
using LinePutScript.Converter;
using LinePutScript.Localization.WPF;
using VPet.ModMaker.Models.ModModel;
using VPet_Simulator.Core;

namespace VPet.ModMaker.Models;

/// <summary>
/// 宠物模型
/// </summary>
public class PetModel : I18nModel<I18nPetInfoModel>
{
    public static PetModel Default { get; } = new();

    #region FromMain
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private bool _fromMain;

    /// <summary>
    /// 来自本体
    /// </summary>
    public bool FromMain
    {
        get => _fromMain;
        set => SetProperty(ref _fromMain, value);
    }
    #endregion

    #region Id
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _id;

    /// <summary>
    /// Id
    /// </summary>
    public string ID
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }
    #endregion
    #region PetNameId
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _petNameId;

    /// <summary>
    /// 名称Id
    /// </summary>
    public string PetNameId
    {
        get => _petNameId;
        set => SetProperty(ref _petNameId, value);
    }
    #endregion

    #region DescriptionId
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _descriptionId;

    /// <summary>
    /// 描述Id
    /// </summary>
    public string DescriptionId
    {
        get => _descriptionId;
        set => SetProperty(ref _descriptionId, value);
    }
    #endregion

    #region TouchHeadRect
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ObservableRectangleLocation<double> _touchHeadRect = new(159, 16, 189, 178);

    /// <summary>
    /// 头部点击区域
    /// </summary>
    public ObservableRectangleLocation<double> TouchHeadRect
    {
        get => _touchHeadRect;
        set => SetProperty(ref _touchHeadRect, value);
    }
    #endregion

    #region TouchBodyRect
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ObservableRectangleLocation<double> _touchBodyRect = new(166, 206, 163, 136);

    /// <summary>
    /// 身体区域
    /// </summary>
    public ObservableRectangleLocation<double> TouchBodyRect
    {
        get => _touchBodyRect;
        set => SetProperty(ref _touchBodyRect, value);
    }
    #endregion

    /// <summary>
    /// 提起区域
    /// </summary>
    #region TouchRaisedRect
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ObservableMultiStateRect _touchRaisedRect =
        new(
            new(0, 50, 500, 200),
            new(0, 50, 500, 200),
            new(0, 50, 500, 200),
            new(0, 200, 500, 300)
        );

    public ObservableMultiStateRect TouchRaisedRect
    {
        get => _touchRaisedRect;
        set => SetProperty(ref _touchRaisedRect, value);
    }
    #endregion

    #region RaisePoint
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ObservableMultiStatePoint _raisePoint =
        new(new(290, 128), new(290, 128), new(290, 128), new(225, 115));

    /// <summary>
    /// 提起定位
    /// </summary>
    public ObservableMultiStatePoint RaisePoint
    {
        get => _raisePoint;
        set => SetProperty(ref _raisePoint, value);
    }
    #endregion

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

    #region AnimeCount
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private int _AnimeCount;

    public int AnimeCount
    {
        get => _AnimeCount;
        set => SetProperty(ref _AnimeCount, value);
    }
    #endregion

    public PetModel()
    {
        PetNameId = $"{ID}_{nameof(PetNameId)}";
        DescriptionId = $"{ID}_{nameof(DescriptionId)}";
        //TODO
        //ID.ValueChanged += (s, e) =>
        //{
        //    PetNameId = $"{e.NewValue}_{nameof(PetNameId)}";
        //    DescriptionId = $"{e.NewValue}_{nameof(DescriptionId)}";
        //};
        //AnimeCount.AddNotifySender(Animes);
        //AnimeCount.AddNotifySender(FoodAnimes);
        //AnimeCount.SenderPropertyChanged += (s, _) =>
        //{
        //    s.Value = Animes.Count + FoodAnimes.Count;
        //};
    }

    public PetModel(PetModel model)
        : this()
    {
        ID = model.ID;
        PetNameId = model.PetNameId;
        TouchHeadRect = model.TouchHeadRect.Clone();
        TouchBodyRect = model.TouchBodyRect.Clone();
        TouchRaisedRect = model.TouchRaisedRect.Copy();
        RaisePoint = model.RaisePoint.Copy();
        foreach (var work in model.Works)
            Works.Add(work);

        foreach (var item in model.I18nDatas)
            I18nDatas[item.Key] = item.Value.Copy();
        CurrentI18nData = I18nDatas[I18nHelper.Current.CultureName];
    }

    public PetModel(PetLoader loader, bool fromMain = false)
        : this()
    {
        ID = loader.Name;
        PetNameId = loader.PetName;
        DescriptionId = loader.Intor;

        TouchHeadRect = new(
            loader.Config.TouchHeadLocate.X,
            loader.Config.TouchHeadLocate.Y,
            loader.Config.TouchHeadSize.Width,
            loader.Config.TouchHeadSize.Height
        );

        TouchBodyRect = new(
            loader.Config.TouchBodyLocate.X,
            loader.Config.TouchBodyLocate.Y,
            loader.Config.TouchBodySize.Width,
            loader.Config.TouchBodySize.Height
        );

        TouchRaisedRect.Happy = new(
            loader.Config.TouchRaisedLocate[0].X,
            loader.Config.TouchRaisedLocate[0].Y,
            loader.Config.TouchRaisedSize[0].Width,
            loader.Config.TouchRaisedSize[0].Height
        );
        TouchRaisedRect.Nomal = new(
            loader.Config.TouchRaisedLocate[1].X,
            loader.Config.TouchRaisedLocate[1].Y,
            loader.Config.TouchRaisedSize[1].Width,
            loader.Config.TouchRaisedSize[1].Height
        );
        TouchRaisedRect.PoorCondition = new(
            loader.Config.TouchRaisedLocate[2].X,
            loader.Config.TouchRaisedLocate[2].Y,
            loader.Config.TouchRaisedSize[2].Width,
            loader.Config.TouchRaisedSize[2].Height
        );
        TouchRaisedRect.Ill = new(
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
            Works.Add(new(work));
        foreach (var move in loader.Config.Moves)
            Moves.Add(new(move));
    }

    public void RefreshId()
    {
        PetNameId = $"{ID}_{nameof(PetNameId)}";
        DescriptionId = $"{ID}_{nameof(DescriptionId)}";
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
            FromMain
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
            ModInfoModel.SaveI18nDatas[cultureName].TryAdd(ID, I18nDatas[cultureName].Name);
            ModInfoModel
                .SaveI18nDatas[cultureName]
                .TryAdd(PetNameId, I18nDatas[cultureName].PetName);
            ModInfoModel
                .SaveI18nDatas[cultureName]
                .TryAdd(DescriptionId, I18nDatas[cultureName].Description);
        }
        var petFile = Path.Combine(path, $"{ID}.lps");
        if (File.Exists(petFile) is false)
            File.Create(petFile).Close();
        var lps = new LPS();
        // 如果本体中存在相同的宠物, 则只保存差异信息
        if (ModMakerInfo.MainPets.TryGetValue(ID, out var mainPet))
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
                ModInfoModel
                    .SaveI18nDatas[cultureName]
                    .TryAdd(work.Id, work.I18nDatas[cultureName].Name);
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
        // 如果值不为默认并且不与本体值相同, 则保存
        if (TouchHeadRect != Default.TouchHeadRect && TouchHeadRect != mainPet.TouchHeadRect)
            SavePetTouchHeadInfo(lps);
        if (TouchBodyRect != Default.TouchBodyRect && TouchBodyRect != mainPet.TouchBodyRect)
            SavePetTouchBodyInfo(lps);
        if (
            TouchRaisedRect != Default.TouchRaisedRect
            && TouchRaisedRect != mainPet.TouchRaisedRect
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
                new Sub("intor", DescriptionId),
                new Sub("path", ID),
                new Sub("petname", PetNameId)
            }
        );
    }

    private void SavePetTouchHeadInfo(LPS lps)
    {
        lps.Add(
            new Line("touchhead")
            {
                new Sub("px", TouchHeadRect.X),
                new Sub("py", TouchHeadRect.Y),
                new Sub("sw", TouchHeadRect.Width),
                new Sub("sh", TouchHeadRect.Height),
            }
        );
    }

    private void SavePetTouchBodyInfo(LPS lps)
    {
        lps.Add(
            new Line("touchbody")
            {
                new Sub("px", TouchBodyRect.X),
                new Sub("py", TouchBodyRect.Y),
                new Sub("sw", TouchBodyRect.Width),
                new Sub("sh", TouchBodyRect.Height),
            }
        );
    }

    private void SavePetTouchRaisedInfo(LPS lps)
    {
        lps.Add(
            new Line("touchraised")
            {
                new Sub("happy_px", TouchRaisedRect.Happy.X),
                new Sub("happy_py", TouchRaisedRect.Happy.Y),
                new Sub("happy_sw", TouchRaisedRect.Happy.Width),
                new Sub("happy_sh", TouchRaisedRect.Happy.Height),
                //
                new Sub("nomal_px", TouchRaisedRect.Nomal.X),
                new Sub("nomal_py", TouchRaisedRect.Nomal.Y),
                new Sub("nomal_sw", TouchRaisedRect.Nomal.Width),
                new Sub("nomal_sh", TouchRaisedRect.Nomal.Height),
                //
                new Sub("poorcondition_px", TouchRaisedRect.PoorCondition.X),
                new Sub("poorcondition_py", TouchRaisedRect.PoorCondition.Y),
                new Sub("poorcondition_sw", TouchRaisedRect.PoorCondition.Width),
                new Sub("poorcondition_sh", TouchRaisedRect.PoorCondition.Height),
                //
                new Sub("ill_px", TouchRaisedRect.Ill.X),
                new Sub("ill_py", TouchRaisedRect.Ill.Y),
                new Sub("ill_sw", TouchRaisedRect.Ill.Width),
                new Sub("ill_sh", TouchRaisedRect.Ill.Height),
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

public class I18nPetInfoModel : ObservableObjectX<I18nPetInfoModel>
{
    #region Name
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _name = string.Empty;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
    #endregion
    #region PetName
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _petName = string.Empty;

    public string PetName
    {
        get => _petName;
        set => SetProperty(ref _petName, value);
    }
    #endregion
    #region Description
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _description = string.Empty;

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }
    #endregion

    public I18nPetInfoModel Copy()
    {
        var result = new I18nPetInfoModel();
        result.Name = Name;
        result.PetName = PetName;
        result.Description = Description;
        return result;
    }
}

public class ObservableMultiStateRect
    : ObservableObjectX<ObservableMultiStateRect>,
        IEquatable<ObservableMultiStateRect>
{
    #region Happy
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ObservableRectangleLocation<double> _happy;
    public ObservableRectangleLocation<double> Happy
    {
        get => _happy;
        set => SetProperty(ref _happy, value);
    }
    #endregion

    #region Nomal
    private ObservableRectangleLocation<double> _nomal;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    public ObservableRectangleLocation<double> Nomal
    {
        get => _nomal;
        set => SetProperty(ref _nomal, value);
    }
    #endregion

    #region PoorCondition
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ObservableRectangleLocation<double> _poorCondition;
    public ObservableRectangleLocation<double> PoorCondition
    {
        get => _poorCondition;
        set => SetProperty(ref _poorCondition, value);
    }
    #endregion

    #region Ill
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ObservableRectangleLocation<double> _ill;
    public ObservableRectangleLocation<double> Ill
    {
        get => _ill;
        set => SetProperty(ref _ill, value);
    }
    #endregion

    public ObservableMultiStateRect()
    {
        Happy = new();
        Nomal = new();
        PoorCondition = new();
        Ill = new();
    }

    public ObservableMultiStateRect(
        ObservableRectangleLocation<double> happy,
        ObservableRectangleLocation<double> nomal,
        ObservableRectangleLocation<double> poorCondition,
        ObservableRectangleLocation<double> ill
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
            Happy = Happy.Clone(),
            Nomal = Nomal.Clone(),
            PoorCondition = PoorCondition.Clone(),
            Ill = Ill.Clone(),
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
            && EqualityComparer<ObservableRectangleLocation<double>>.Default.Equals(
                Happy,
                temp.Happy
            )
            && EqualityComparer<ObservableRectangleLocation<double>>.Default.Equals(
                Nomal,
                temp.Nomal
            )
            && EqualityComparer<ObservableRectangleLocation<double>>.Default.Equals(
                PoorCondition,
                temp.PoorCondition
            )
            && EqualityComparer<ObservableRectangleLocation<double>>.Default.Equals(Ill, temp.Ill);
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
    : ObservableObjectX<ObservableMultiStatePoint>,
        IEquatable<ObservableMultiStatePoint>
{
    #region Happy
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ObservablePoint<double> _happy;
    public ObservablePoint<double> Happy
    {
        get => _happy;
        set => SetProperty(ref _happy, value);
    }
    #endregion

    #region Nomal
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ObservablePoint<double> _nomal;
    public ObservablePoint<double> Nomal
    {
        get => _nomal;
        set => SetProperty(ref _nomal, value);
    }
    #endregion

    #region PoorCondition
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ObservablePoint<double> _poorCondition;
    public ObservablePoint<double> PoorCondition
    {
        get => _poorCondition;
        set => SetProperty(ref _poorCondition, value);
    }
    #endregion

    #region Ill
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ObservablePoint<double> _ill;
    public ObservablePoint<double> Ill
    {
        get => _ill;
        set => SetProperty(ref _ill, value);
    }
    #endregion
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
            Happy = Happy.Clone(),
            Nomal = Nomal.Clone(),
            PoorCondition = PoorCondition.Clone(),
            Ill = Ill.Clone(),
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
