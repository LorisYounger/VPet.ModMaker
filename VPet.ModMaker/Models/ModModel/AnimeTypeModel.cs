using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using HKW.HKWReactiveUI;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using LinePutScript.Localization.WPF;
using VPet.ModMaker.Native;
using VPet.ModMaker.ViewModels;
using VPet_Simulator.Core;
using static VPet_Simulator.Core.GraphInfo;
using static VPet_Simulator.Core.IGameSave;

namespace VPet.ModMaker.Models.ModModel;

/// <summary>
/// 多类型动画模型
/// </summary>
public partial class AnimeTypeModel : ViewModelBase, IAnimeModel
{
    /// <inheritdoc/>
    public AnimeTypeModel() { }

    /// <inheritdoc/>
    /// <param name="model">模型</param>
    public AnimeTypeModel(AnimeTypeModel model)
        : this()
    {
        ID = model.ID;
        Name = model.Name;
        GraphType = model.GraphType;
        foreach (var anime in model.HappyAnimes)
            HappyAnimes.Add(anime.Clone());
        foreach (var anime in model.NomalAnimes)
            NomalAnimes.Add(anime.Clone());
        foreach (var anime in model.PoorConditionAnimes)
            PoorConditionAnimes.Add(anime.Clone());
        foreach (var anime in model.IllAnimes)
            IllAnimes.Add(anime.Clone());
    }

    /// <inheritdoc/>
    /// <param name="graphType">动画类型</param>
    /// <param name="path">路径</param>
    public AnimeTypeModel(GraphInfo.GraphType graphType, string path)
    {
        Name = Path.GetFileName(path);
        // 为带有名字的类型设置ID
        if (graphType.IsHasNameAnime())
            ID = $"{graphType}_{Name}";
        else
            ID = graphType.ToString();
        GraphType = graphType;
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
            throw new Exception("未知动画类型, 目标路径 {0}".Translate(path));
    }

    /// <summary>
    /// 动作类型
    /// </summary>
    public static FrozenSet<GraphInfo.GraphType> GraphTypes { get; } =
        Enum.GetValues<GraphInfo.GraphType>().ToFrozenSet();

    /// <summary>
    /// 动画类型
    /// </summary>
    public static FrozenSet<GraphInfo.AnimatType> AnimatTypes { get; } =
        Enum.GetValues<GraphInfo.AnimatType>().ToFrozenSet();

    /// <summary>
    /// 模式类型
    /// </summary>
    public static FrozenSet<ModeType> ModeTypes { get; } = Enum.GetValues<ModeType>().ToFrozenSet();

    /// <summary>
    /// 含有名称的动作列表
    /// </summary>
    public static FrozenSet<GraphInfo.GraphType> HasNameAnimes { get; } =
        FrozenSet.ToFrozenSet(
            [
                GraphInfo.GraphType.Common,
                GraphInfo.GraphType.Work,
                GraphInfo.GraphType.Idel,
                GraphInfo.GraphType.Move,
                GraphInfo.GraphType.Say
            ]
        );

    /// <summary>
    /// 含有不同动画类型的动作列表
    /// </summary>
    public static FrozenSet<GraphInfo.GraphType> HasMultiTypeAnimes { get; } =
        FrozenSet.ToFrozenSet(
            [
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
            ]
        );

    #region Property
    /// <summary>
    /// ID
    /// </summary>
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
    /// 动作类型
    /// </summary>
    [ReactiveProperty]
    public GraphInfo.GraphType GraphType { get; set; }
    #endregion

    /// <summary>
    /// 开心动画
    /// </summary>
    public ObservableList<AnimeModel> HappyAnimes { get; } = [];

    /// <summary>
    /// 普通动画 (默认)
    /// </summary>
    public ObservableList<AnimeModel> NomalAnimes { get; } = [];

    /// <summary>
    /// 低状态动画
    /// </summary>
    public ObservableList<AnimeModel> PoorConditionAnimes { get; } = [];

    /// <summary>
    /// 生病动画
    /// </summary>
    public ObservableList<AnimeModel> IllAnimes { get; } = [];

    /// <summary>
    /// 载入动画
    /// </summary>
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

    /// <summary>
    /// 关闭动画
    /// </summary>
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

    /// <summary>
    /// 清除动画
    /// </summary>
    public void Clear()
    {
        Close();
        HappyAnimes.Clear();
        NomalAnimes.Clear();
        PoorConditionAnimes.Clear();
        IllAnimes.Clear();
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
            //if (Directory.EnumerateDirectories(dir).Any())
            //{
            //    LoadMultiType(dir);
            //}
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
                // 判断 Happy_A 类型文件夹
                if (type is AnimatType.Single)
                {
                    var temp = GetAnimatType(modeName[0]);
                    if (temp is not AnimatType.Single)
                    {
                        modeName = typeName;
                        type = temp;
                    }
                    else if (
                        modeName.Length > 1
                        && (
                            Enum.TryParse<ModeType>(typeName, out var imode)
                            || Enum.TryParse<ModeType>(modeName, out imode)
                        )
                    )
                    {
                        foreach (var idir in Directory.EnumerateDirectories(dir))
                        {
                            var idirName = Path.GetFileName(idir)
                                .Split('_', StringSplitOptions.RemoveEmptyEntries)[0];
                            AddAnimeForModeType(idir, imode, GetAnimatType(idirName[0]));
                        }
                        continue;
                    }
                }

                var mode = Enum.Parse<ModeType>(Path.GetFileName(modeName), true);
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
        ObservableList<AnimeModel> collection,
        string path,
        GraphInfo.AnimatType animatType = AnimatType.Single
    )
    {
        if (Directory.EnumerateFiles(path).Any())
        {
            // 如果没有文件夹 则载入全部文件
            var animeModel = new AnimeModel(path);
            animeModel.AnimeType = animatType;
            collection.Add(animeModel);
        }
        else
        {
            // 否则遍历文件夹
            foreach (var imagesDir in Directory.EnumerateDirectories(path))
            {
                var animeModel = new AnimeModel(imagesDir);
                animeModel.AnimeType = animatType;
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
            GraphType
            is GraphInfo.GraphType.Default
                or GraphInfo.GraphType.Shutdown
                or GraphInfo.GraphType.StartUP
        )
            SaveDefault(path);
        else if (
            GraphType
            is GraphInfo.GraphType.Touch_Head
                or GraphInfo.GraphType.Touch_Body
                or GraphInfo.GraphType.Sleep
        )
            SaveMultiType(path);
        else if (
            GraphType
            is GraphInfo.GraphType.Switch_Up
                or GraphInfo.GraphType.Switch_Down
                or GraphInfo.GraphType.Switch_Thirsty
                or GraphInfo.GraphType.Switch_Hunger
        )
            SaveSwitch(path);
        else if (
            GraphType is GraphInfo.GraphType.Raised_Dynamic or GraphInfo.GraphType.Raised_Static
        )
            SaveRaised(path);
        else if (GraphType is GraphInfo.GraphType.StateONE or GraphInfo.GraphType.StateTWO)
            SaveState(path);
        else if (GraphType is GraphInfo.GraphType.Common)
            SaveCommon(path);
        else if (GraphType.IsHasNameAnime())
            SaveHasNameAnime(path);
    }

    /// <summary>
    /// 保存为带有名称的动画样式
    /// </summary>
    /// <param name="path">路径</param>
    void SaveHasNameAnime(string path)
    {
        var animeTypePath = Path.Combine(path, GraphType.ToString());
        Directory.CreateDirectory(animeTypePath);
        var animePath = Path.Combine(animeTypePath, Name);
        Directory.CreateDirectory(animePath);
        SaveWithModeType(animePath);
    }

    /// <summary>
    /// 保存为通用样式
    /// </summary>
    /// <param name="path">路径</param>
    void SaveCommon(string path)
    {
        var animePath = Path.Combine(path, Name);
        Directory.CreateDirectory(animePath);
        SaveWithModeType(animePath);
    }

    /// <summary>
    /// 保存为 <see cref="GraphInfo.GraphType.StateONE"/> 或 <see cref="GraphInfo.GraphType.StateTWO"/> 样式
    /// </summary>
    /// <param name="path">路径</param>
    void SaveState(string path)
    {
        var animePath = Path.Combine(path, "State");
        Directory.CreateDirectory(animePath);
        SaveMultiType(animePath);
    }

    /// <summary>
    /// 保存为 <see cref="GraphInfo.GraphType.Raised_Dynamic"/> 或 <see cref="GraphInfo.GraphType.Raised_Static"/> 样式
    /// </summary>
    /// <param name="path">路径</param>
    void SaveRaised(string path)
    {
        var animePath = Path.Combine(path, "Raise");
        Directory.CreateDirectory(animePath);
        if (GraphType is GraphInfo.GraphType.Raised_Dynamic)
            SaveDefault(animePath);
        else if (GraphType is GraphInfo.GraphType.Raised_Static)
            SaveMultiType(animePath);
    }

    /// <summary>
    /// 保存为 <see cref="GraphInfo.GraphType.Switch_Up"/> 或 <see cref="GraphInfo.GraphType.Switch_Down"/> 或 <see cref="GraphInfo.GraphType.Switch_Thirsty"/> 或 <see cref="GraphInfo.GraphType.Switch_Hunger"/>
    /// </summary>
    /// <param name="path">路径</param>
    void SaveSwitch(string path)
    {
        var animePath = Path.Combine(path, "Switch");
        Directory.CreateDirectory(animePath);
        var switchName = GraphType.ToString().Split(NativeUtils.Separator).Last();
        SaveWithAnimeType(Path.Combine(animePath, switchName));
    }

    /// <summary>
    /// 保存成默认样式
    /// </summary>
    /// <param name="path"></param>
    void SaveDefault(string path)
    {
        var animePath = Path.Combine(path, GraphType.ToString());
        Directory.CreateDirectory(animePath);
        SaveWithAnimeType(animePath);
    }

    /// <summary>
    /// 保存成多类型样式
    /// </summary>
    /// <param name="path"></param>
    void SaveMultiType(string path)
    {
        var animePath = Path.Combine(path, GraphType.ToString());
        Directory.CreateDirectory(animePath);
        SaveWithModeType(animePath);
    }

    /// <summary>
    /// 保存为 Happy/A/0 的路径样式
    /// </summary>
    /// <param name="path"></param>
    void SaveWithModeType(string path)
    {
        if (HappyAnimes.Count > 0)
        {
            var modePath = Path.Combine(path, nameof(ModeType.Happy));
            SaveAnimes(modePath, HappyAnimes);
        }
        if (NomalAnimes.Count > 0)
        {
            var modePath = Path.Combine(path, nameof(ModeType.Nomal));
            SaveAnimes(modePath, NomalAnimes);
        }
        if (PoorConditionAnimes.Count > 0)
        {
            var modePath = Path.Combine(path, nameof(ModeType.PoorCondition));
            SaveAnimes(modePath, PoorConditionAnimes);
        }
        if (IllAnimes.Count > 0)
        {
            var modePath = Path.Combine(path, nameof(ModeType.Ill));
            SaveAnimes(modePath, IllAnimes);
        }

        static void SaveAnimes(string animePath, ObservableList<AnimeModel> animes)
        {
            Directory.CreateDirectory(animePath);
            var countA = 0;
            var countB = 0;
            var countC = 0;
            foreach (var anime in animes)
            {
                if (anime.AnimeType is GraphInfo.AnimatType.A_Start)
                {
                    var animatTypePath = Path.Combine(animePath, "A");
                    Directory.CreateDirectory(animatTypePath);
                    SaveImages(Path.Combine(animatTypePath, countA.ToString()), anime);
                    countA++;
                }
                else if (anime.AnimeType is GraphInfo.AnimatType.B_Loop)
                {
                    var animatTypePath = Path.Combine(animePath, "B");
                    Directory.CreateDirectory(animatTypePath);
                    SaveImages(Path.Combine(animatTypePath, countB.ToString()), anime);
                    countB++;
                }
                else if (anime.AnimeType is GraphInfo.AnimatType.C_End)
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
    void SaveWithAnimeType(string animePath)
    {
        if (HappyAnimes.Count > 0)
        {
            var modePath = Path.Combine(animePath, nameof(ModeType.Happy));
            SaveAnimes(modePath, HappyAnimes);
        }
        if (NomalAnimes.Count > 0)
        {
            var modePath = Path.Combine(animePath, nameof(ModeType.Nomal));
            SaveAnimes(modePath, NomalAnimes);
        }
        if (PoorConditionAnimes.Count > 0)
        {
            var modePath = Path.Combine(animePath, nameof(ModeType.PoorCondition));
            SaveAnimes(modePath, PoorConditionAnimes);
        }
        if (IllAnimes.Count > 0)
        {
            var modePath = Path.Combine(animePath, nameof(ModeType.Ill));
            SaveAnimes(modePath, IllAnimes);
        }
        static void SaveAnimes(string animePath, ObservableList<AnimeModel> animes)
        {
            Directory.CreateDirectory(animePath);
            foreach ((var index, var anime) in animes.EnumerateIndex())
                SaveImages(Path.Combine(animePath, index.ToString()), anime);
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
        foreach ((var index, var image) in model.Images.EnumerateIndex())
        {
            var path = Path.Combine(imagesPath, $"{model.ID}_{index:000}_{image.Duration}.png");
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
    #endregion
}
