using HKW.HKWViewModels.SimpleObservable;
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
    /// Id
    /// </summary>
    public ObservableValue<string> Id { get; } = new();

    /// <summary>
    /// 名称
    /// </summary>
    public ObservableValue<string> Name { get; } = new();

    /// <summary>
    /// 动作类型
    /// </summary>
    public GraphInfo.GraphType GraphType => GraphInfo.GraphType.Common;

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
        HappyAnimes.CollectionChanged += Animes_CollectionChanged;
        //Name.ValueChanged += (_, _) =>
        //{
        //    Id.Value = $"{GraphType.Value}_{Name.Value}";
        //};
    }

    private void Animes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.NewItems is not null)
        {
            foreach (var model in e.NewItems.Cast<FoodAnimeModel>())
            {
                SetImagesPathValueChanged(model);
            }
        }
        if (e.OldItems is not null)
        {
            foreach (var model in e.OldItems.Cast<FoodAnimeModel>())
            {
                SetImagesPathValueChanged(model);
            }
        }
    }

    private void SetImagesPathValueChanged(FoodAnimeModel model)
    {
        model.FrontImagesPath.ValueChanged += (o, n) =>
        {
            if (n is null)
                return;
            if (n.Mode.Value is GameSave.ModeType.Happy)
                model.FrontImages = HappyAnimes[n.Index.Value].FrontImages;
            else if (n.Mode.Value is GameSave.ModeType.Nomal)
                model.FrontImages = NomalAnimes[n.Index.Value].FrontImages;
            else if (n.Mode.Value is GameSave.ModeType.PoorCondition)
                model.FrontImages = PoorConditionAnimes[n.Index.Value].FrontImages;
            else if (n.Mode.Value is GameSave.ModeType.Ill)
                model.FrontImages = IllAnimes[n.Index.Value].FrontImages;
        };
        model.BackImagesPath.ValueChanged += (o, n) =>
        {
            if (n is null)
                return;
            if (n.Mode.Value is GameSave.ModeType.Happy)
                model.BackImages = HappyAnimes[n.Index.Value].BackImages;
            else if (n.Mode.Value is GameSave.ModeType.Nomal)
                model.BackImages = NomalAnimes[n.Index.Value].BackImages;
            else if (n.Mode.Value is GameSave.ModeType.PoorCondition)
                model.BackImages = PoorConditionAnimes[n.Index.Value].BackImages;
            else if (n.Mode.Value is GameSave.ModeType.Ill)
                model.BackImages = IllAnimes[n.Index.Value].BackImages;
        };
    }

    public FoodAnimeTypeModel(string path)
        : this()
    {
        Name.Value = Path.GetFileName(path);
        var infoFile = Path.Combine(path, ModMakerInfo.InfoFile);
        if (
            Directory.EnumerateFiles(path, ModMakerInfo.InfoFile, SearchOption.AllDirectories).Any()
            is false
        )
            throw new Exception("信息文件\n{0}\n不存在".Translate(infoFile));
        if (File.Exists(infoFile))
            ParseInfoFile(path, infoFile);
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

    public void ParseInfoFile(string path, string infoPath)
    {
        var lps = new LPS(infoPath);
        var foodAnimeInfos = lps.FindAllLineInfo(nameof(FoodAnimation));
        if (foodAnimeInfos.Any() is false)
            throw new Exception("信息文件\n{0}\n未包含食物动画信息".Translate(infoPath));
        var pngAnimeInfos = lps.FindAllLineInfo(nameof(PNGAnimation))
            .Select(
                i =>
                    new PNGAnimeInfo(
                        i.Info,
                        i.Find("infoPath").Info,
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
                                int.Parse(Path.GetFileNameWithoutExtension(i).Split('_')[1])
                            )
                    )
            );
        }
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
