using HKW.HKWViewModels.SimpleObservable;
using LinePutScript;
using LinePutScript.Localization.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPet_Simulator.Core;

namespace VPet.ModMaker.Models.ModModel;

public class FoodAnimeTypeModel
{
    /// <summary>
    /// Id
    /// </summary>
    public ObservableValue<string> Id { get; } = new();

    /// <summary>
    /// 名称
    /// </summary>
    public ObservableValue<string> Name { get; } = new();

    /// <summary>
    /// 开心动画
    /// </summary>
    public ObservableCollection<FoodAnimeModel> HappyAnimes { get; } = new();

    /// <summary>
    /// 普通动画 (默认)
    /// </summary>
    public ObservableCollection<FoodAnimeModel> NomalAnimes { get; } = new();

    /// <summary>
    /// 低状态动画
    /// </summary>
    public ObservableCollection<FoodAnimeModel> PoorConditionAnimes { get; } = new();

    /// <summary>
    /// 生病动画
    /// </summary>
    public ObservableCollection<FoodAnimeModel> IllAnimes { get; } = new();

    public FoodAnimeTypeModel()
    {
        //Name.ValueChanged += (_, _) =>
        //{
        //    Id.Value = $"{GraphType.Value}_{Name.Value}";
        //};
    }

    public FoodAnimeTypeModel(string path)
        : this()
    {
        Name.Value = Path.GetFileName(path);
        var infoFile = Path.Combine(path, ModMakerInfo.InfoFile);
        if (File.Exists(infoFile) is false)
            throw new Exception("信息文件\n{0}\n不存在".Translate(infoFile));
        var lps = new LPS(infoFile);
        var foodAnime = lps.FindAllLineInfo(nameof(FoodAnimation));
        if (foodAnime.Any() is false)
            throw new Exception("信息文件\n{0}\n未包含食物动画信息".Translate(infoFile));
        var anime = lps.FindAllLineInfo(nameof(PNGAnimation));
        foreach (var foodAnimation in foodAnime)
        {
            ParseFoodAnimeInfo(foodAnimation);
        }
    }

    public FoodAnimeTypeModel(FoodAnimeTypeModel model)
        : this()
    {
        Id.Value = model.Id.Value;
        Name.Value = model.Name.Value;
        foreach (var anime in model.HappyAnimes)
            HappyAnimes.Add(anime.Copy());
        foreach (var anime in model.NomalAnimes)
            NomalAnimes.Add(anime.Copy());
        foreach (var anime in model.PoorConditionAnimes)
            PoorConditionAnimes.Add(anime.Copy());
        foreach (var anime in model.IllAnimes)
            IllAnimes.Add(anime.Copy());
    }

    /// <summary>
    /// 创建动画类型模型
    /// </summary>
    /// <param name="graphType">动作类型</param>
    /// <param name="path">路径</param>
    /// <returns></returns>
    public static FoodAnimeTypeModel? Create(string path)
    {
        try
        {
            var model = new FoodAnimeTypeModel(path);
            return model;
        }
        catch
        {
            return null;
        }
    }

    public void ParseFoodAnimeInfo(ILine line)
    {
        var mode = (GameSave.ModeType)Enum.Parse(typeof(GameSave.ModeType), line.Find("mode").Info);
        if (mode is GameSave.ModeType.Happy)
            AddModeAnime(HappyAnimes, line);
        else if (mode is GameSave.ModeType.Nomal)
            AddModeAnime(NomalAnimes, line);
        else if (mode is GameSave.ModeType.PoorCondition)
            AddModeAnime(PoorConditionAnimes, line);
        else if (mode is GameSave.ModeType.Ill)
            AddModeAnime(IllAnimes, line);
    }

    public void AddModeAnime(ObservableCollection<FoodAnimeModel> foodAnimes, ILine line)
    {
        foodAnimes.Add(new(line));
    }
}
