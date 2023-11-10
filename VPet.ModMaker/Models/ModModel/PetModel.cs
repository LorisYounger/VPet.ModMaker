using HKW.HKWViewModels.SimpleObservable;
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
    /// 显示的Id 若不为空则判断为来自本体的宠物
    /// </summary>
    public string? SourceId { get; set; } = null;

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

    public bool IsSimplePetModel { get; } = false;

    public PetModel()
    {
        PetNameId.Value = $"{Id.Value}_{nameof(PetNameId)}";
        DescriptionId.Value = $"{Id.Value}_{nameof(DescriptionId)}";
        Id.ValueChanged += (o, n) =>
        {
            PetNameId.Value = $"{n}_{nameof(PetNameId)}";
            DescriptionId.Value = $"{n}_{nameof(DescriptionId)}";
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

    public PetModel(PetLoader loader)
        : this()
    {
        Id.Value = loader.Name;
        PetNameId.Value = loader.PetName;
        DescriptionId.Value = loader.Intor;

        TouchHeadRect.Value.SetValue(
            loader.Config.TouchHeadLocate.X,
            loader.Config.TouchHeadLocate.Y,
            loader.Config.TouchHeadSize.Width,
            loader.Config.TouchHeadSize.Height
        );

        TouchBodyRect.Value.SetValue(
            loader.Config.TouchBodyLocate.X,
            loader.Config.TouchBodyLocate.Y,
            loader.Config.TouchBodySize.Width,
            loader.Config.TouchBodySize.Height
        );

        TouchRaisedRect.Value.Happy.Value.SetValue(
            loader.Config.TouchRaisedLocate[0].X,
            loader.Config.TouchRaisedLocate[0].Y,
            loader.Config.TouchRaisedSize[0].Width,
            loader.Config.TouchRaisedSize[0].Height
        );
        TouchRaisedRect.Value.Nomal.Value.SetValue(
            loader.Config.TouchRaisedLocate[1].X,
            loader.Config.TouchRaisedLocate[1].Y,
            loader.Config.TouchRaisedSize[1].Width,
            loader.Config.TouchRaisedSize[1].Height
        );
        TouchRaisedRect.Value.PoorCondition.Value.SetValue(
            loader.Config.TouchRaisedLocate[2].X,
            loader.Config.TouchRaisedLocate[2].Y,
            loader.Config.TouchRaisedSize[2].Width,
            loader.Config.TouchRaisedSize[2].Height
        );
        TouchRaisedRect.Value.Ill.Value.SetValue(
            loader.Config.TouchRaisedLocate[3].X,
            loader.Config.TouchRaisedLocate[3].Y,
            loader.Config.TouchRaisedSize[3].Width,
            loader.Config.TouchRaisedSize[3].Height
        );

        RaisePoint.Value.Happy.Value.SetValue(
            loader.Config.RaisePoint[0].X,
            loader.Config.RaisePoint[0].Y
        );
        RaisePoint.Value.Nomal.Value.SetValue(
            loader.Config.RaisePoint[1].X,
            loader.Config.RaisePoint[1].Y
        );
        RaisePoint.Value.PoorCondition.Value.SetValue(
            loader.Config.RaisePoint[2].X,
            loader.Config.RaisePoint[2].Y
        );
        RaisePoint.Value.Ill.Value.SetValue(
            loader.Config.RaisePoint[3].X,
            loader.Config.RaisePoint[3].Y
        );

        foreach (var work in loader.Config.Works)
            Works.Add(new(work));
        foreach (var move in loader.Config.Moves)
            Moves.Add(new(move));
    }

    public PetModel(PetLoader loader, bool isSimplePet)
        : this()
    {
        Id.Value = loader.Name;
        IsSimplePetModel = isSimplePet;
    }

    public void Close() { }

    #region Save
    /// <summary>
    /// 保存宠物
    /// </summary>
    /// <param name="path">路径</param>
    public void Save(string path)
    {
        if (SourceId is not null)
            Id.Value = SourceId;
        if (IsSimplePetModel)
        {
            SaveSimplePetInfo(path);
            return;
        }
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
        SavePetInfo(lps);
        SaveWorksInfo(lps);
        SaveMoveInfo(lps);
        File.WriteAllText(petFile, lps.ToString());

        var petAnimePath = Path.Combine(path, Id.Value);
        foreach (var anime in Animes)
            anime.Save(petAnimePath);
        foreach (var anime in FoodAnimes)
            anime.Save(petAnimePath);
        if (SourceId is not null)
            Id.Value = SourceId + " (来自本体)".Translate();
    }

    private void SaveSimplePetInfo(string path)
    {
        if (Works.Count == 0 && Moves.Count == 0 && Animes.Count == 0)
            return;
        var petFile = Path.Combine(path, $"{Id.Value}.lps");
        var lps = new LPS { new Line("pet", Id.Value) { new Sub("path", Id.Value), } };
        SaveWorksInfo(lps);
        SaveMoveInfo(lps);
        File.WriteAllText(petFile, lps.ToString());
        var petAnimePath = Path.Combine(path, Id.Value);
        foreach (var animeType in Animes)
            animeType.Save(petAnimePath);
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

    /// <summary>
    /// 保存宠物信息
    /// </summary>
    /// <param name="lps"></param>
    /// <param name="pet"></param>
    private void SavePetInfo(LPS lps)
    {
        lps.Add(
            new Line("pet", Id.Value)
            {
                new Sub("intor", DescriptionId.Value),
                new Sub("path", Id.Value),
                new Sub("petname", PetNameId.Value)
            }
        );
        lps.Add(
            new Line("touchhead")
            {
                new Sub("px", TouchHeadRect.Value.X.Value),
                new Sub("py", TouchHeadRect.Value.Y.Value),
                new Sub("sw", TouchHeadRect.Value.Width.Value),
                new Sub("sh", TouchHeadRect.Value.Height.Value),
            }
        );
        lps.Add(
            new Line("touchbody")
            {
                new Sub("px", TouchBodyRect.Value.X.Value),
                new Sub("py", TouchBodyRect.Value.Y.Value),
                new Sub("sw", TouchBodyRect.Value.Width.Value),
                new Sub("sh", TouchBodyRect.Value.Height.Value),
            }
        );
        lps.Add(
            new Line("touchraised")
            {
                new Sub("happy_px", TouchRaisedRect.Value.Happy.Value.X.Value),
                new Sub("happy_py", TouchRaisedRect.Value.Happy.Value.Y.Value),
                new Sub("happy_sw", TouchRaisedRect.Value.Happy.Value.Width.Value),
                new Sub("happy_sh", TouchRaisedRect.Value.Happy.Value.Height.Value),
                //
                new Sub("nomal_px", TouchRaisedRect.Value.Nomal.Value.X.Value),
                new Sub("nomal_py", TouchRaisedRect.Value.Nomal.Value.Y.Value),
                new Sub("nomal_sw", TouchRaisedRect.Value.Nomal.Value.Width.Value),
                new Sub("nomal_sh", TouchRaisedRect.Value.Nomal.Value.Height.Value),
                //
                new Sub("poorcondition_px", TouchRaisedRect.Value.PoorCondition.Value.X.Value),
                new Sub("poorcondition_py", TouchRaisedRect.Value.PoorCondition.Value.Y.Value),
                new Sub("poorcondition_sw", TouchRaisedRect.Value.PoorCondition.Value.Width.Value),
                new Sub("poorcondition_sh", TouchRaisedRect.Value.PoorCondition.Value.Height.Value),
                //
                new Sub("ill_px", TouchRaisedRect.Value.Ill.Value.X.Value),
                new Sub("ill_py", TouchRaisedRect.Value.Ill.Value.Y.Value),
                new Sub("ill_sw", TouchRaisedRect.Value.Ill.Value.Width.Value),
                new Sub("ill_sh", TouchRaisedRect.Value.Ill.Value.Height.Value),
            }
        );
        lps.Add(
            new Line("raisepoint")
            {
                new Sub("happy_x", RaisePoint.Value.Happy.Value.X.Value),
                new Sub("happy_y", RaisePoint.Value.Happy.Value.Y.Value),
                //
                new Sub("nomal_x", RaisePoint.Value.Nomal.Value.X.Value),
                new Sub("nomal_y", RaisePoint.Value.Nomal.Value.Y.Value),
                //
                new Sub("poorcondition_x", RaisePoint.Value.PoorCondition.Value.X.Value),
                new Sub("poorcondition_y", RaisePoint.Value.PoorCondition.Value.Y.Value),
                //
                new Sub("ill_x", RaisePoint.Value.Ill.Value.X.Value),
                new Sub("ill_y", RaisePoint.Value.Ill.Value.Y.Value),
            }
        );
    }
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
{
    public ObservableValue<ObservableRect<double>> Happy { get; } = new(new());
    public ObservableValue<ObservableRect<double>> Nomal { get; } = new(new());
    public ObservableValue<ObservableRect<double>> PoorCondition { get; } = new(new());
    public ObservableValue<ObservableRect<double>> Ill { get; } = new(new());

    public ObservableMultiStateRect() { }

    public ObservableMultiStateRect(
        ObservableRect<double> happy,
        ObservableRect<double> nomal,
        ObservableRect<double> poorCondition,
        ObservableRect<double> ill
    )
    {
        Happy.Value = happy;
        Nomal.Value = nomal;
        PoorCondition.Value = poorCondition;
        Ill.Value = ill;
    }

    public ObservableMultiStateRect Copy()
    {
        var result = new ObservableMultiStateRect();
        result.Happy.Value = Happy.Value.Copy();
        result.Nomal.Value = Nomal.Value.Copy();
        result.PoorCondition.Value = PoorCondition.Value.Copy();
        result.Ill.Value = Ill.Value.Copy();
        return result;
    }
}

public class ObservableMultiStatePoint
{
    public ObservableValue<ObservablePoint<double>> Happy { get; } = new(new());
    public ObservableValue<ObservablePoint<double>> Nomal { get; } = new(new());
    public ObservableValue<ObservablePoint<double>> PoorCondition { get; } = new(new());
    public ObservableValue<ObservablePoint<double>> Ill { get; } = new(new());

    public ObservableMultiStatePoint() { }

    public ObservableMultiStatePoint(
        ObservablePoint<double> happy,
        ObservablePoint<double> nomal,
        ObservablePoint<double> poorCondition,
        ObservablePoint<double> ill
    )
    {
        Happy.Value = happy;
        Nomal.Value = nomal;
        PoorCondition.Value = poorCondition;
        Ill.Value = ill;
    }

    public ObservableMultiStatePoint Copy()
    {
        var result = new ObservableMultiStatePoint();
        result.Happy.Value = Happy.Value.Copy();
        result.Nomal.Value = Nomal.Value.Copy();
        result.PoorCondition.Value = PoorCondition.Value.Copy();
        result.Ill.Value = Ill.Value.Copy();
        return result;
    }
}

public class ObservableRect<T>
{
    public ObservableValue<T> X { get; } = new();
    public ObservableValue<T> Y { get; } = new();
    public ObservableValue<T> Width { get; } = new();
    public ObservableValue<T> Height { get; } = new();

    public ObservableRect() { }

    public ObservableRect(T x, T y, T width, T hetght)
    {
        X.Value = x;
        Y.Value = y;
        Width.Value = width;
        Height.Value = hetght;
    }

    public void SetValue(T x, T y, T width, T hetght)
    {
        X.Value = x;
        Y.Value = y;
        Width.Value = width;
        Height.Value = hetght;
    }

    public ObservableRect<T> Copy()
    {
        var result = new ObservableRect<T>();
        result.X.Value = X.Value;
        result.Y.Value = Y.Value;
        result.Width.Value = Width.Value;
        result.Height.Value = Height.Value;
        return result;
    }
}

public class ObservablePoint<T>
{
    public ObservableValue<T> X { get; } = new();
    public ObservableValue<T> Y { get; } = new();

    public ObservablePoint() { }

    public ObservablePoint(T x, T y)
    {
        X.Value = x;
        Y.Value = y;
    }

    public void SetValue(T x, T y)
    {
        X.Value = x;
        Y.Value = y;
    }

    public ObservablePoint<T> Copy()
    {
        var result = new ObservablePoint<T>();
        result.X.Value = X.Value;
        result.Y.Value = Y.Value;
        return result;
    }
}
