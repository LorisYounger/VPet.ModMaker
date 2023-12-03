using HKW.HKWUtils.Observable;
using HKW.Models;
using LinePutScript;
using LinePutScript.Localization.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPet_Simulator.Core;

namespace VPet.ModMaker.Models.ModModel;

public class FoodAnimeTypeModel
{
    /// <summary>
    /// 动作类型
    /// </summary>
    public static GraphInfo.GraphType GraphType => GraphInfo.GraphType.Common;

    /// <summary>
    /// 动画名称
    /// </summary>
    public static HashSet<string> FoodAnimeNames =
        new(StringComparer.InvariantCultureIgnoreCase) { "Eat", "Drink", "Gift", };

    /// <summary>
    /// 顶层名称
    /// </summary>
    public const string FrontLayName = "front_lay";

    /// <summary>
    /// 底层名称
    /// </summary>
    public const string BackLayName = "back_lay";

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
        Name.ValueChanged += (_, _) =>
        {
            Id.Value = $"{GraphType}_{Name.Value}";
        };
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
        Name.Value = Path.GetFileName(path);
        var infoFiles = Directory.EnumerateFiles(
            path,
            ModMakerInfo.InfoFile,
            SearchOption.AllDirectories
        );
        if (infoFiles.Any() is false)
            throw new Exception("信息文件不存在".Translate());
        foreach (var file in infoFiles)
        {
            ParseInfoFile(Path.GetDirectoryName(file), file);
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
            .Select(
                i =>
                    new PNGAnimeInfo(
                        i.Info,
                        i.Find("path").Info,
                        (GameSave.ModeType)
                            Enum.Parse(typeof(GameSave.ModeType), i.Find("mode").Info, true)
                    )
            )
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
        var mode = (GameSave.ModeType)
            Enum.Parse(typeof(GameSave.ModeType), line.Find("mode").Info, true);
        if (mode is GameSave.ModeType.Happy)
            AddModeAnime(path, GameSave.ModeType.Happy, HappyAnimes, line, pngAnimeInfos);
        else if (mode is GameSave.ModeType.Nomal)
            AddModeAnime(path, GameSave.ModeType.Nomal, NomalAnimes, line, pngAnimeInfos);
        else if (mode is GameSave.ModeType.PoorCondition)
            AddModeAnime(
                path,
                GameSave.ModeType.PoorCondition,
                PoorConditionAnimes,
                line,
                pngAnimeInfos
            );
        else if (mode is GameSave.ModeType.Ill)
            AddModeAnime(path, GameSave.ModeType.Ill, IllAnimes, line, pngAnimeInfos);
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
        GameSave.ModeType mode,
        ObservableCollection<FoodAnimeModel> foodAnimes,
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
                frontLayAnimes.First(i => i.Mode == GameSave.ModeType.Nomal)
            );
        }
        if (backLayAnimes.FirstOrDefault(i => i.Mode == mode) is PNGAnimeInfo backAnimeInfo)
        {
            anime.BackImages = GetImages(path, backAnimeInfo);
        }
        else
        {
            anime.BackImages = GetImages(
                path,
                backLayAnimes.First(i => i.Mode == GameSave.ModeType.Nomal)
            );
        }
        foodAnimes.Add(anime);

        static ObservableCollection<ImageModel> GetImages(string path, PNGAnimeInfo pngAnimeInfo)
        {
            return new(
                Directory
                    .EnumerateFiles(Path.Combine(path, pngAnimeInfo.Path))
                    .Select(
                        i =>
                            new ImageModel(
                                Utils.LoadImageToMemoryStream(i),
                                int.Parse(Path.GetFileNameWithoutExtension(i).Split('_')[2])
                            )
                    )
            );
        }
    }

    /// <summary>
    /// 保存
    /// </summary>
    /// <param name="path">路径</param>
    public void Save(string path)
    {
        var animePath = Path.Combine(path, Name.Value);
        if (
            Directory.Exists(animePath)
            && HappyAnimes.Count == 0
            && NomalAnimes.Count == 0
            && PoorConditionAnimes.Count == 0
            && IllAnimes.Count == 0
        )
        {
            Directory.Delete(animePath, true);
            return;
        }
        if (HappyAnimes.Count > 0)
            SaveAnimeInfo(animePath, HappyAnimes, GameSave.ModeType.Happy);
        if (NomalAnimes.Count > 0)
            SaveAnimeInfo(animePath, NomalAnimes, GameSave.ModeType.Nomal);
        if (PoorConditionAnimes.Count > 0)
            SaveAnimeInfo(animePath, PoorConditionAnimes, GameSave.ModeType.PoorCondition);
        if (IllAnimes.Count > 0)
            SaveAnimeInfo(animePath, IllAnimes, GameSave.ModeType.Ill);
    }

    /// <summary>
    /// 保存动画信息
    /// </summary>
    /// <param name="animePath">路径</param>
    /// <param name="animes">动画</param>
    /// <param name="mode">模式</param>
    private void SaveAnimeInfo(
        string animePath,
        ObservableCollection<FoodAnimeModel> animes,
        GameSave.ModeType mode
    )
    {
        var modeAnimePath = Path.Combine(animePath, mode.ToString());
        foreach (var anime in animes.EnumerateIndex())
        {
            var indexPath = Path.Combine(modeAnimePath, anime.Index.ToString());
            Directory.CreateDirectory(indexPath);
            var infoFile = Path.Combine(indexPath, ModMakerInfo.InfoFile);
            var frontLayName = $"{Name.Value.ToLower()}_{FrontLayName}_{anime.Index}";
            var backLayName = $"{Name.Value.ToLower()}_{BackLayName}_{anime.Index}";
            SaveInfoFile(infoFile, frontLayName, backLayName, anime.Value, mode);
            SaveImages(anime.Value, indexPath);
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
        foreach (var frontImage in anime.FrontImages.EnumerateIndex())
        {
            frontImage.Value.Image.Value.SaveToPng(
                Path.Combine(
                    frontLayPath,
                    $"{anime.Id.Value}_{frontImage.Index:000}_{frontImage.Value.Duration.Value}.png"
                )
            );
        }
        foreach (var backImage in anime.BackImages.EnumerateIndex())
        {
            backImage.Value.Image.Value.SaveToPng(
                Path.Combine(
                    backLayPath,
                    $"{anime.Id.Value}_{backImage.Index:000}_{backImage.Value.Duration.Value}.png"
                )
            );
        }
    }

    private void SaveInfoFile(
        string infoFile,
        string frontLayName,
        string backLayName,
        FoodAnimeModel anime,
        GameSave.ModeType mode
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
        var line = new Line(nameof(FoodAnimation), Name.Value.ToLower())
        {
            new Sub("mode", mode.ToString()),
            new Sub("graph", Name.Value)
        };
        foreach (var foodLocation in anime.FoodLocations.EnumerateIndex())
        {
            var sub = new Sub($"a{foodLocation.Index}");
            sub.info = foodLocation.Value.ToString();
            line.Add(sub);
        }
        line.Add(new Sub(FrontLayName, frontLayName));
        line.Add(new Sub(BackLayName, backLayName));
        lps.Add(line);
        File.WriteAllText(infoFile, lps.ToString());
    }
}

public class PNGAnimeInfo
{
    public string Name { get; }
    public string Path { get; }
    public GameSave.ModeType Mode { get; }

    public PNGAnimeInfo(string name, string path, GameSave.ModeType mode)
    {
        Name = name;
        Path = path;
        Mode = mode;
    }
}
