using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using HKW.HKWUtils.Observable;
using VPet_Simulator.Core;
using static VPet_Simulator.Core.GraphInfo;
using static VPet_Simulator.Core.IGameSave;

namespace VPet.ModMaker.Models.ModModel;

public class AnimeTypeModel
{
    /// <summary>
    /// 动作类型
    /// </summary>
    public static ObservableCollection<GraphInfo.GraphType> GraphTypes { get; } =
        new(Enum.GetValues<GraphInfo.GraphType>());

    /// <summary>
    /// 动画类型
    /// </summary>
    public static ObservableCollection<GraphInfo.AnimatType> AnimatTypes { get; } =
        new(Enum.GetValues(typeof(GraphInfo.AnimatType)).Cast<GraphInfo.AnimatType>());

    /// <summary>
    /// 模式类型
    /// </summary>
    public static ObservableCollection<ModeType> ModeTypes { get; } =
        new(Enum.GetValues(typeof(ModeType)).Cast<ModeType>());

    /// <summary>
    /// 含有名称的动作列表
    /// </summary>
    public static HashSet<GraphInfo.GraphType> HasNameAnimes { get; } =
        new()
        {
            GraphInfo.GraphType.Common,
            GraphInfo.GraphType.Work,
            GraphInfo.GraphType.Idel,
            GraphInfo.GraphType.Move,
            GraphInfo.GraphType.Say
        };

    /// <summary>
    /// 含有不同动画类型的动作列表
    /// </summary>
    public static HashSet<GraphInfo.GraphType> HasMultiTypeAnimes { get; } =
        new()
        {
            GraphInfo.GraphType.Touch_Head,
            GraphInfo.GraphType.Touch_Body,
            GraphInfo.GraphType.Sleep,
            GraphInfo.GraphType.Raised_Static,
            GraphInfo.GraphType.StateONE,
            GraphInfo.GraphType.StateTWO,
            GraphInfo.GraphType.Common,
            GraphInfo.GraphType.Work,
            GraphInfo.GraphType.Idel,
            GraphInfo.GraphType.Move,
            GraphInfo.GraphType.Say
        };

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
    public ObservableValue<GraphInfo.GraphType> GraphType { get; } = new();

    /// <summary>
    /// 开心动画
    /// </summary>
    public ObservableCollection<AnimeModel> HappyAnimes { get; } = new();

    /// <summary>
    /// 普通动画 (默认)
    /// </summary>
    public ObservableCollection<AnimeModel> NomalAnimes { get; } = new();

    /// <summary>
    /// 低状态动画
    /// </summary>
    public ObservableCollection<AnimeModel> PoorConditionAnimes { get; } = new();

    /// <summary>
    /// 生病动画
    /// </summary>
    public ObservableCollection<AnimeModel> IllAnimes { get; } = new();

    public AnimeTypeModel()
    {
        Name.ValueChanged += (_, _) =>
        {
            Id.Value = $"{GraphType.Value}_{Name.Value}";
        };
    }

    public AnimeTypeModel(AnimeTypeModel model)
        : this()
    {
        Id.Value = model.Id.Value;
        Name.Value = model.Name.Value;
        GraphType.Value = model.GraphType.Value;
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
    public static AnimeTypeModel? Create(GraphInfo.GraphType graphType, string path)
    {
        try
        {
            var model = new AnimeTypeModel(graphType, path);
            return model;
        }
        catch
        {
            return null;
        }
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

    public AnimeTypeModel(GraphInfo.GraphType graphType, string path)
    {
        Name.Value = Path.GetFileName(path);
        // 为带有名字的类型设置Id
        if (graphType.IsHasNameAnime())
            Id.Value = $"{graphType}_{Name.Value}";
        else
            Id.Value = graphType.ToString();
        GraphType.Value = graphType;
        if (
            graphType
            is GraphInfo.GraphType.Default
                or GraphInfo.GraphType.Shutdown
                or GraphInfo.GraphType.StartUP
                or GraphInfo.GraphType.Switch_Up
                or GraphInfo.GraphType.Switch_Down
                or GraphInfo.GraphType.Switch_Thirsty
                or GraphInfo.GraphType.Switch_Hunger
                or GraphInfo.GraphType.Raised_Dynamic
        )
            LoadDefault(path);
        else if (
            graphType
            is GraphInfo.GraphType.Touch_Head
                or GraphInfo.GraphType.Touch_Body
                or GraphInfo.GraphType.Sleep
                or GraphInfo.GraphType.Raised_Static
                or GraphInfo.GraphType.StateONE
                or GraphInfo.GraphType.StateTWO
        )
            LoadMultiType(path);
        else if (graphType.IsHasNameAnime())
            LoadMultiType(path);
        else
            throw new Exception();
    }

    #region Load
    /// <summary>
    /// 默认载入方式 (只有一个动画类型 <see cref="GraphInfo.AnimatType.Single"/>)
    /// </summary>
    /// <param name="path">路径</param>
    private void LoadDefault(string path)
    {
        foreach (var dir in Directory.EnumerateDirectories(path))
        {
            var dirName = Path.GetFileName(dir);
            if (
                dirName.Contains(
                    nameof(ModeType.Happy),
                    StringComparison.InvariantCultureIgnoreCase
                )
            )
            {
                AddAnime(HappyAnimes, dir);
            }
            else if (
                dirName.Contains(
                    nameof(ModeType.Nomal),
                    StringComparison.InvariantCultureIgnoreCase
                )
            )
            {
                AddAnime(NomalAnimes, dir);
            }
            else if (
                dirName.Contains(
                    nameof(ModeType.PoorCondition),
                    StringComparison.InvariantCultureIgnoreCase
                )
            )
            {
                AddAnime(PoorConditionAnimes, dir);
            }
            else if (
                dirName.Contains(nameof(ModeType.Ill), StringComparison.InvariantCultureIgnoreCase)
            )
            {
                AddAnime(IllAnimes, dir);
            }
        }
        if (Directory.EnumerateFiles(path).Any())
        {
            AddAnime(NomalAnimes, path);
        }
    }

    /// <summary>
    /// 有多个动画类型的载入方式
    /// </summary>
    /// <param name="path">路径</param>
    private void LoadMultiType(string path)
    {
        foreach (var dir in Directory.EnumerateDirectories(path))
        {
            var dirName = Path.GetFileName(dir);
            var dirInfo = dirName.Split(
                NativeUtils.Separator,
                StringSplitOptions.RemoveEmptyEntries
            );
            if (dirInfo.Length == 3)
            {
                // 判断 A_1_Happy 类型文件夹
                var typeName = dirInfo[0];
                var modeName = dirInfo[2];
                var type = GetAnimatType(typeName[0]);
                var mode = (ModeType)Enum.Parse(typeof(ModeType), Path.GetFileName(modeName), true);
                AddAnimeForModeType(dir, mode, type);
            }
            else if (dirInfo.Length == 2)
            {
                // 判断 A_Happy 类型文件夹
                var typeName = dirInfo[0];
                var modeName = dirInfo[1];
                var type = GetAnimatType(typeName[0]);
                var mode = (ModeType)Enum.Parse(typeof(ModeType), Path.GetFileName(modeName), true);
                AddAnimeForModeType(dir, mode, type);
            }
            else if (Enum.TryParse<ModeType>(dirName, true, out var mode))
            {
                // 判断 Happy/A 型文件夹
                foreach (var typePath in Directory.EnumerateDirectories(dir))
                {
                    var type = GetAnimatType(
                        Path.GetFileName(typePath).Split(NativeUtils.Separator).First()[0]
                    );
                    AddAnimeForModeType(typePath, mode, type);
                }
            }
            else
            {
                var type = GetAnimatType(dirName[0]);
                // 判断 A/Happy 文件夹
                if (Directory.EnumerateDirectories(dir).Any())
                {
                    foreach (var modePath in Directory.EnumerateDirectories(dir))
                    {
                        mode = (ModeType)
                            Enum.Parse(
                                typeof(ModeType),
                                Path.GetFileName(modePath).Split(NativeUtils.Separator).First(),
                                true
                            );
                        AddAnimeForModeType(modePath, mode, type);
                    }
                }
                else
                {
                    AddAnimeForModeType(dir, ModeType.Nomal, type);
                }
            }
        }
    }

    /// <summary>
    /// 添加动画到不同模式
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="modeType">模式类型</param>
    /// <param name="animeType">动画类型</param>
    private void AddAnimeForModeType(string path, ModeType modeType, GraphInfo.AnimatType animeType)
    {
        if (modeType is ModeType.Happy)
        {
            AddAnime(HappyAnimes, path, animeType);
        }
        else if (modeType is ModeType.Nomal)
        {
            AddAnime(NomalAnimes, path, animeType);
        }
        else if (modeType is ModeType.PoorCondition)
        {
            AddAnime(PoorConditionAnimes, path, animeType);
        }
        else if (modeType is ModeType.Ill)
        {
            AddAnime(IllAnimes, path, animeType);
        }
    }

    private static GraphInfo.AnimatType GetAnimatType(char c)
    {
        return c switch
        {
            'A' => GraphInfo.AnimatType.A_Start,
            'B' => GraphInfo.AnimatType.B_Loop,
            'C' => GraphInfo.AnimatType.C_End,
            _ => GraphInfo.AnimatType.Single,
        };
    }

    /// <summary>
    /// 添加动画至动画列表
    /// </summary>
    /// <param name="collection">动画列表</param>
    /// <param name="path">路径</param>
    /// <param name="animatType">动画类型</param>
    private static void AddAnime(
        ObservableCollection<AnimeModel> collection,
        string path,
        GraphInfo.AnimatType animatType = AnimatType.Single
    )
    {
        if (Directory.EnumerateFiles(path).Any())
        {
            // 如果没有文件夹 则载入全部文件
            var animeModel = new AnimeModel(path);
            animeModel.AnimeType.Value = animatType;
            collection.Add(animeModel);
        }
        else
        {
            // 否则遍历文件夹
            foreach (var imagesDir in Directory.EnumerateDirectories(path))
            {
                var animeModel = new AnimeModel(imagesDir);
                animeModel.AnimeType.Value = animatType;
                collection.Add(animeModel);
            }
        }
    }
    #endregion
    #region Save
    /// <summary>
    /// 保存至指定路径
    /// </summary>
    /// <param name="path">路径</param>
    public void Save(string path)
    {
        if (
            GraphType.Value
            is GraphInfo.GraphType.Default
                or GraphInfo.GraphType.Shutdown
                or GraphInfo.GraphType.StartUP
        )
            SaveDefault(path, this);
        else if (
            GraphType.Value
            is GraphInfo.GraphType.Touch_Head
                or GraphInfo.GraphType.Touch_Body
                or GraphInfo.GraphType.Sleep
        )
            SaveMultiType(path, this);
        else if (
            GraphType.Value
            is GraphInfo.GraphType.Switch_Up
                or GraphInfo.GraphType.Switch_Down
                or GraphInfo.GraphType.Switch_Thirsty
                or GraphInfo.GraphType.Switch_Hunger
        )
            SaveSwitch(path, this);
        else if (
            GraphType.Value
            is GraphInfo.GraphType.Raised_Dynamic
                or GraphInfo.GraphType.Raised_Static
        )
            SaveRaised(path, this);
        else if (GraphType.Value is GraphInfo.GraphType.StateONE or GraphInfo.GraphType.StateTWO)
            SaveState(path, this);
        else if (GraphType.Value is GraphInfo.GraphType.Common)
            SaveCommon(path, this);
        else if (GraphType.Value.IsHasNameAnime())
            SaveHasNameAnime(path, this);
    }

    /// <summary>
    /// 保存为带有名称的动画样式
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="animeTypeModel">动画模型</param>
    void SaveHasNameAnime(string path, AnimeTypeModel animeTypeModel)
    {
        var animeTypePath = Path.Combine(path, animeTypeModel.GraphType.Value.ToString());
        Directory.CreateDirectory(animeTypePath);
        var animePath = Path.Combine(animeTypePath, animeTypeModel.Name.Value);
        Directory.CreateDirectory(animePath);
        SaveWithModeType(animePath, animeTypeModel);
    }

    /// <summary>
    /// 保存为通用样式
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="animeTypeModel">模型</param>
    void SaveCommon(string path, AnimeTypeModel animeTypeModel)
    {
        var animePath = Path.Combine(path, animeTypeModel.Name.Value);
        Directory.CreateDirectory(animePath);
        SaveWithModeType(animePath, animeTypeModel);
    }

    /// <summary>
    /// 保存为 <see cref="GraphInfo.GraphType.StateONE"/> 或 <see cref="GraphInfo.GraphType.StateTWO"/> 样式
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="animeTypeModel">模型</param>
    void SaveState(string path, AnimeTypeModel animeTypeModel)
    {
        var animePath = Path.Combine(path, "State");
        Directory.CreateDirectory(animePath);
        SaveMultiType(animePath, animeTypeModel);
    }

    /// <summary>
    /// 保存为 <see cref="GraphInfo.GraphType.Raised_Dynamic"/> 或 <see cref="GraphInfo.GraphType.Raised_Static"/> 样式
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="animeTypeModel">模型</param>
    void SaveRaised(string path, AnimeTypeModel animeTypeModel)
    {
        var animePath = Path.Combine(path, "Raise");
        Directory.CreateDirectory(animePath);
        if (animeTypeModel.GraphType.Value is GraphInfo.GraphType.Raised_Dynamic)
            SaveDefault(animePath, animeTypeModel);
        else if (animeTypeModel.GraphType.Value is GraphInfo.GraphType.Raised_Static)
            SaveMultiType(animePath, animeTypeModel);
    }

    /// <summary>
    /// 保存为 <see cref="GraphInfo.GraphType.Switch_Up"/> 或 <see cref="GraphInfo.GraphType.Switch_Down"/> 或 <see cref="GraphInfo.GraphType.Switch_Thirsty"/> 或 <see cref="GraphInfo.GraphType.Switch_Hunger"/>
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="animeTypeModel">模型</param>
    void SaveSwitch(string path, AnimeTypeModel animeTypeModel)
    {
        var animePath = Path.Combine(path, "Switch");
        Directory.CreateDirectory(animePath);
        var switchName = animeTypeModel.GraphType.ToString().Split(NativeUtils.Separator).Last();
        SaveWithAnimeType(Path.Combine(animePath, switchName), animeTypeModel);
    }

    /// <summary>
    /// 保存成默认样式
    /// </summary>
    /// <param name="path"></param>
    /// <param name="animeTypeModel"></param>
    static void SaveDefault(string path, AnimeTypeModel animeTypeModel)
    {
        var animePath = Path.Combine(path, animeTypeModel.GraphType.ToString());
        Directory.CreateDirectory(animePath);
        SaveWithAnimeType(animePath, animeTypeModel);
    }

    /// <summary>
    /// 保存成多类型样式
    /// </summary>
    /// <param name="path"></param>
    /// <param name="animeTypeModel"></param>
    static void SaveMultiType(string path, AnimeTypeModel animeTypeModel)
    {
        var animePath = Path.Combine(path, animeTypeModel.GraphType.ToString());
        Directory.CreateDirectory(animePath);
        SaveWithModeType(animePath, animeTypeModel);
    }

    /// <summary>
    /// 保存为 Happy/A/0 的路径样式
    /// </summary>
    /// <param name="path"></param>
    /// <param name="animeTypeModel"></param>
    static void SaveWithModeType(string path, AnimeTypeModel animeTypeModel)
    {
        if (animeTypeModel.HappyAnimes.Count > 0)
        {
            var modePath = Path.Combine(path, nameof(ModeType.Happy));
            SaveAnimes(modePath, animeTypeModel.HappyAnimes);
        }
        if (animeTypeModel.NomalAnimes.Count > 0)
        {
            var modePath = Path.Combine(path, nameof(ModeType.Nomal));
            SaveAnimes(modePath, animeTypeModel.NomalAnimes);
        }
        if (animeTypeModel.PoorConditionAnimes.Count > 0)
        {
            var modePath = Path.Combine(path, nameof(ModeType.PoorCondition));
            SaveAnimes(modePath, animeTypeModel.PoorConditionAnimes);
        }
        if (animeTypeModel.IllAnimes.Count > 0)
        {
            var modePath = Path.Combine(path, nameof(ModeType.Ill));
            SaveAnimes(modePath, animeTypeModel.IllAnimes);
        }

        static void SaveAnimes(string animePath, ObservableCollection<AnimeModel> animes)
        {
            Directory.CreateDirectory(animePath);
            var countA = 0;
            var countB = 0;
            var countC = 0;
            foreach (var anime in animes)
            {
                if (anime.AnimeType.Value is GraphInfo.AnimatType.A_Start)
                {
                    var animatTypePath = Path.Combine(animePath, "A");
                    Directory.CreateDirectory(animatTypePath);
                    SaveImages(Path.Combine(animatTypePath, countA.ToString()), anime);
                    countA++;
                }
                else if (anime.AnimeType.Value is GraphInfo.AnimatType.B_Loop)
                {
                    var animatTypePath = Path.Combine(animePath, "B");
                    Directory.CreateDirectory(animatTypePath);
                    SaveImages(Path.Combine(animatTypePath, countB.ToString()), anime);
                    countB++;
                }
                else if (anime.AnimeType.Value is GraphInfo.AnimatType.C_End)
                {
                    var animatTypePath = Path.Combine(animePath, "C");
                    Directory.CreateDirectory(animatTypePath);
                    SaveImages(Path.Combine(animatTypePath, countC.ToString()), anime);
                    countC++;
                }
            }
        }
    }

    /// <summary>
    /// 保存为 Happy/0 的路径样式
    /// </summary>
    /// <param name="animePath"></param>
    /// <param name="animeType"></param>
    static void SaveWithAnimeType(string animePath, AnimeTypeModel animeType)
    {
        if (animeType.HappyAnimes.Count > 0)
        {
            var modePath = Path.Combine(animePath, nameof(ModeType.Happy));
            SaveAnimes(modePath, animeType.HappyAnimes);
        }
        if (animeType.NomalAnimes.Count > 0)
        {
            var modePath = Path.Combine(animePath, nameof(ModeType.Nomal));
            SaveAnimes(modePath, animeType.NomalAnimes);
        }
        if (animeType.PoorConditionAnimes.Count > 0)
        {
            var modePath = Path.Combine(animePath, nameof(ModeType.PoorCondition));
            SaveAnimes(modePath, animeType.PoorConditionAnimes);
        }
        if (animeType.IllAnimes.Count > 0)
        {
            var modePath = Path.Combine(animePath, nameof(ModeType.Ill));
            SaveAnimes(modePath, animeType.IllAnimes);
        }
        static void SaveAnimes(string animePath, ObservableCollection<AnimeModel> animes)
        {
            Directory.CreateDirectory(animePath);
            foreach (var anime in animes.EnumerateIndex())
                SaveImages(Path.Combine(animePath, anime.Index.ToString()), anime.Value);
        }
    }

    /// <summary>
    /// 保存图片
    /// </summary>
    /// <param name="imagesPath"></param>
    /// <param name="model"></param>
    static void SaveImages(string imagesPath, AnimeModel model)
    {
        Directory.CreateDirectory(imagesPath);
        foreach (var image in model.Images.EnumerateIndex())
        {
            image.Value.Image.Value.SaveToPng(
                Path.Combine(
                    imagesPath,
                    $"{model.Id.Value}_{image.Index:000}_{image.Value.Duration.Value}.png"
                )
            );
        }
    }
    #endregion
}
