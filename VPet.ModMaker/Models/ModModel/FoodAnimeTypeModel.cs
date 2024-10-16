﻿using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HKW.HKWReactiveUI;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using LinePutScript;
using LinePutScript.Localization.WPF;
using VPet.ModMaker.ViewModels;
using VPet_Simulator.Core;
using static VPet_Simulator.Core.IGameSave;

namespace VPet.ModMaker.Models.ModModel;

public partial class FoodAnimeTypeModel : ViewModelBase
{
    public FoodAnimeTypeModel() { }

    /// <summary>
    /// 动作类型
    /// </summary>
    public static GraphInfo.GraphType GraphType => GraphInfo.GraphType.Common;

    /// <summary>
    /// 动画名称
    /// </summary>
    public static FrozenSet<string> FoodAnimeNames = FrozenSet.ToFrozenSet(
        ["Eat", "Drink", "Gift"],
        StringComparer.InvariantCultureIgnoreCase
    );

    /// <summary>
    /// 顶层名称
    /// </summary>
    public const string FrontLayName = "front_lay";

    /// <summary>
    /// 底层名称
    /// </summary>
    public const string BackLayName = "back_lay";

    [ReactiveProperty]
    public string ID { get; set; } = string.Empty;

    /// <summary>
    /// 名称
    /// </summary>
    [ReactiveProperty]
    public string Name { get; set; } = string.Empty;

    partial void OnNameChanged(string oldValue, string newValue)
    {
        ID = $"{GraphType}_{Name}";
    }

    /// <summary>
    /// 开心动画
    /// </summary>
    public ObservableList<FoodAnimeModel> HappyAnimes { get; } = [];

    /// <summary>
    /// 普通动画 (默认)
    /// </summary>
    public ObservableList<FoodAnimeModel> NomalAnimes { get; } = [];

    /// <summary>
    /// 低状态动画
    /// </summary>
    public ObservableList<FoodAnimeModel> PoorConditionAnimes { get; } = new();

    /// <summary>
    /// 生病动画
    /// </summary>
    public ObservableList<FoodAnimeModel> IllAnimes { get; } = new();

    public void LoadTypeAnime()
    {
        foreach (var anime in HappyAnimes)
            anime.LoadAnime();
        foreach (var anime in NomalAnimes)
            anime.LoadAnime();
        foreach (var anime in PoorConditionAnimes)
            anime.LoadAnime();
        foreach (var anime in IllAnimes)
            anime.LoadAnime();
    }

    public void Close()
    {
        foreach (var anime in HappyAnimes)
            anime.Close();
        foreach (var anime in NomalAnimes)
            anime.Close();
        foreach (var anime in PoorConditionAnimes)
            anime.Close();
        foreach (var anime in IllAnimes)
            anime.Close();
    }

    public void Clear()
    {
        HappyAnimes.Clear();
        NomalAnimes.Clear();
        PoorConditionAnimes.Clear();
        IllAnimes.Clear();
    }

    public FoodAnimeTypeModel(string path)
        : this()
    {
        Name = Path.GetFileName(path);
        var infoFiles = Directory.EnumerateFiles(
            path,
            ModMakerInfo.InfoFile,
            SearchOption.AllDirectories
        );
        if (infoFiles.Any() is false)
            throw new Exception("信息文件不存在".Translate());
        foreach (var file in infoFiles)
        {
            ParseInfoFile(Path.GetDirectoryName(file)!, file);
        }
    }

    public FoodAnimeTypeModel(FoodAnimeTypeModel model)
        : this()
    {
        ID = model.ID;
        Name = model.Name;
        foreach (var anime in model.HappyAnimes)
            HappyAnimes.Add(anime.Clone());
        foreach (var anime in model.NomalAnimes)
            NomalAnimes.Add(anime.Clone());
        foreach (var anime in model.PoorConditionAnimes)
            PoorConditionAnimes.Add(anime.Clone());
        foreach (var anime in model.IllAnimes)
            IllAnimes.Add(anime.Clone());
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

    /// <summary>
    /// 解析信息文件
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="infoPath">信息文件路径</param>
    /// <exception cref="Exception"></exception>
    public void ParseInfoFile(string path, string infoPath)
    {
        var lps = new LPS(File.ReadAllText(infoPath));
        var foodAnimeInfos = lps.FindAllLine(nameof(FoodAnimation));
        if (foodAnimeInfos.Any() is false)
            throw new Exception("信息文件\n{0}\n未包含食物动画信息".Translate(infoPath));
        var pngAnimeInfos = lps.FindAllLine(nameof(PNGAnimation))
            .Select(i => new PNGAnimeInfo(
                i.Info,
                i.Find("path").Info,
                (ModeType)Enum.Parse(typeof(ModeType), i.Find("mode").Info, true)
            ))
            .ToList();
        foreach (var foodAnimation in foodAnimeInfos)
        {
            ParseFoodAnimeInfo(path, foodAnimation, pngAnimeInfos);
        }
    }

    /// <summary>
    /// 解析食物动画信息
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="line">食物动画信息</param>
    /// <param name="pngAnimeInfos">PNG动画信息</param>
    public void ParseFoodAnimeInfo(string path, ILine line, List<PNGAnimeInfo> pngAnimeInfos)
    {
        var mode = (ModeType)Enum.Parse(typeof(ModeType), line.Find("mode").Info, true);
        if (mode is ModeType.Happy)
            AddModeAnime(path, ModeType.Happy, HappyAnimes, line, pngAnimeInfos);
        else if (mode is ModeType.Nomal)
            AddModeAnime(path, ModeType.Nomal, NomalAnimes, line, pngAnimeInfos);
        else if (mode is ModeType.PoorCondition)
            AddModeAnime(path, ModeType.PoorCondition, PoorConditionAnimes, line, pngAnimeInfos);
        else if (mode is ModeType.Ill)
            AddModeAnime(path, ModeType.Ill, IllAnimes, line, pngAnimeInfos);
    }

    /// <summary>
    /// 添加模式动画
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="mode">模式</param>
    /// <param name="foodAnimes">食物动画</param>
    /// <param name="line">食物动画信息</param>
    /// <param name="pngAnimeInfos">PNG动画信息</param>
    public void AddModeAnime(
        string path,
        ModeType mode,
        ObservableList<FoodAnimeModel> foodAnimes,
        ILine line,
        List<PNGAnimeInfo> pngAnimeInfos
    )
    {
        var anime = new FoodAnimeModel(line);
        var frontLay = line.Find("front_lay").Info;
        var backLay = line.Find("back_lay").Info;
        var frontLayAnimes = pngAnimeInfos.Where(i => i.Name == frontLay).ToList();
        var backLayAnimes = pngAnimeInfos.Where(i => i.Name == backLay).ToList();
        // 尝试获取相同模式的动画
        if (frontLayAnimes.FirstOrDefault(i => i.Mode == mode) is PNGAnimeInfo frontAnimeInfo)
        {
            anime.FrontImages = GetImages(path, frontAnimeInfo);
        }
        else
        {
            // 若没有则获取通用动画
            anime.FrontImages = GetImages(
                path,
                frontLayAnimes.First(i => i.Mode == ModeType.Nomal)
            );
        }
        if (backLayAnimes.FirstOrDefault(i => i.Mode == mode) is PNGAnimeInfo backAnimeInfo)
        {
            anime.BackImages = GetImages(path, backAnimeInfo);
        }
        else
        {
            anime.BackImages = GetImages(path, backLayAnimes.First(i => i.Mode == ModeType.Nomal));
        }
        foodAnimes.Add(anime);

        static ObservableList<ImageModel> GetImages(string path, PNGAnimeInfo pngAnimeInfo)
        {
            return new(
                Directory
                    .EnumerateFiles(Path.Combine(path, pngAnimeInfo.Path))
                    .Select(f => new ImageModel(
                        f,
                        int.Parse(Path.GetFileNameWithoutExtension(f).Split('_')[2])
                    ))
            );
        }
    }

    /// <summary>
    /// 保存
    /// </summary>
    /// <param name="path">路径</param>
    public void Save(string path)
    {
        var animePath = Path.Combine(path, Name);
        if (
            Directory.Exists(animePath)
            && HappyAnimes.HasValue() is false
            && NomalAnimes.HasValue() is false
            && PoorConditionAnimes.HasValue() is false
            && IllAnimes.HasValue() is false
        )
        {
            Directory.Delete(animePath, true);
            return;
        }
        if (HappyAnimes.Count > 0)
            SaveAnimeInfo(animePath, HappyAnimes, ModeType.Happy);
        if (NomalAnimes.Count > 0)
            SaveAnimeInfo(animePath, NomalAnimes, ModeType.Nomal);
        if (PoorConditionAnimes.Count > 0)
            SaveAnimeInfo(animePath, PoorConditionAnimes, ModeType.PoorCondition);
        if (IllAnimes.Count > 0)
            SaveAnimeInfo(animePath, IllAnimes, ModeType.Ill);
    }

    /// <summary>
    /// 保存动画信息
    /// </summary>
    /// <param name="animePath">路径</param>
    /// <param name="animes">动画</param>
    /// <param name="mode">模式</param>
    private void SaveAnimeInfo(
        string animePath,
        ObservableList<FoodAnimeModel> animes,
        ModeType mode
    )
    {
        var modeAnimePath = Path.Combine(animePath, mode.ToString());
        foreach ((var index, var anime) in animes.EnumerateIndex())
        {
            var indexPath = Path.Combine(modeAnimePath, index.ToString());
            Directory.CreateDirectory(indexPath);
            var infoFile = Path.Combine(indexPath, ModMakerInfo.InfoFile);
            var frontLayName = $"{Name.ToLower()}_{FrontLayName}_{index}";
            var backLayName = $"{Name.ToLower()}_{BackLayName}_{index}";
            SaveInfoFile(infoFile, frontLayName, backLayName, anime, mode);
            SaveImages(anime, indexPath);
        }
    }

    /// <summary>
    /// 保存图片
    /// </summary>
    /// <param name="anime">动画</param>
    /// <param name="indexPath">索引路径</param>
    /// <param name="frontLayName">顶层名称</param>
    /// <param name="backLayName">底层名称</param>
    private static void SaveImages(FoodAnimeModel anime, string indexPath)
    {
        var frontLayPath = Path.Combine(indexPath, FrontLayName);
        var backLayPath = Path.Combine(indexPath, BackLayName);
        Directory.CreateDirectory(frontLayPath);
        Directory.CreateDirectory(backLayPath);
        foreach ((var index, var image) in anime.FrontImages.EnumerateIndex())
        {
            var path = Path.Combine(frontLayPath, $"{anime.ID}_{index:000}_{image.Duration}.png");
            if (image.Image is not null)
            {
                image.Image.SaveToPng(path);
            }
            else if (Path.Exists(image.ImageFile))
            {
                File.Copy(image.ImageFile, path, true);
            }
        }
        foreach ((var index, var image) in anime.BackImages.EnumerateIndex())
        {
            var path = Path.Combine(backLayPath, $"{anime.ID}_{index:000}_{image.Duration}.png");
            if (image.Image is not null)
            {
                image.Image.SaveToPng(path);
            }
            else if (Path.Exists(image.ImageFile))
            {
                File.Copy(image.ImageFile, path, true);
            }
        }
    }

    private void SaveInfoFile(
        string infoFile,
        string frontLayName,
        string backLayName,
        FoodAnimeModel anime,
        ModeType mode
    )
    {
        var lps = new LPS()
        {
            new Line(nameof(PNGAnimation), frontLayName)
            {
                new Sub("path", FrontLayName),
                new Sub("mode", mode.ToString()),
                new Sub("graph", nameof(GraphInfo.GraphType.Common))
            },
            new Line(nameof(PNGAnimation), backLayName)
            {
                new Sub("path", BackLayName),
                new Sub("mode", mode.ToString()),
                new Sub("graph", nameof(GraphInfo.GraphType.Common))
            },
        };
        var line = new Line(nameof(FoodAnimation), Name.ToLower())
        {
            new Sub("mode", mode.ToString()),
            new Sub("graph", Name)
        };
        foreach ((var index, var foodLocation) in anime.FoodLocations.EnumerateIndex())
        {
            var sub = new Sub($"a{index}");
            sub.info = foodLocation.ToString();
            line.Add(sub);
        }
        line.Add(new Sub(FrontLayName, frontLayName));
        line.Add(new Sub(BackLayName, backLayName));
        lps.Add(line);
        File.WriteAllText(infoFile, lps.ToString());
    }
}

public class PNGAnimeInfo(string name, string path, IGameSave.ModeType mode)
{
    public string Name { get; } = name;
    public string Path { get; } = path;
    public ModeType Mode { get; } = mode;
}
