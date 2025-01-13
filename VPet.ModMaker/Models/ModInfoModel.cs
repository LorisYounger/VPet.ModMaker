using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Reactive.Linq;
using System.Windows.Media.Imaging;
using DynamicData.Binding;
using HKW.HKWMapper;
using HKW.HKWReactiveUI;
using HKW.HKWUtils;
using HKW.HKWUtils.Collections;
using HKW.HKWUtils.Observable;
using HKW.WPF;
using HKW.WPF.Extensions;
using LinePutScript;
using LinePutScript.Converter;
using ReactiveUI;
using VPet.ModMaker.Models.ModModel;
using VPet.ModMaker.Native;
using VPet.ModMaker.ViewModels;
using VPet_Simulator.Core;

namespace VPet.ModMaker.Models;

/// <summary>
/// 模组信息模型
/// </summary>
public partial class ModInfoModel : ViewModelBase
{
    /// <inheritdoc/>
    public ModInfoModel()
    {
        Pets = new([], [], m => m.FromMain is false || ShowMainPet);
        Pets.BaseList.WhenPropertyChanged(x => x.Count)
            .Subscribe(x => RaisePetDisplayedCountChange());

        this.WhenValueChanged(x => x.ShowMainPet, false)
            .Throttle(TimeSpan.FromSeconds(0.5), RxApp.TaskpoolScheduler)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ =>
            {
                var oldPet = CurrentPet;
                Pets.Refresh();
                if (CurrentPet is null)
                {
                    if (oldPet.FromMain is false)
                        CurrentPet = oldPet;
                    else
                        CurrentPet = Pets.FilteredList.FirstOrDefault();
                }
            });

        I18nResource.PropertyChanged += I18nResource_PropertyChanged;
        I18nResource.Cultures.SetChanged += Cultures_SetChanged;
        I18nResource.CultureDatas.DictionaryChanged += CultureDatas_DictionaryChanged;
        foreach (var pet in NativeData.MainPets)
        {
            // 确保ID不重复
            if (Pets.All(i => i.ID != pet.Key))
            {
                Pets.Add(pet.Value);
                this.LogX().Info("载入本体宠物 {pet}", pet.Key);
            }
            else
                this.LogX().Info("载入本体宠物失败,已存在相同ID的宠物 {pet}", pet.Key);
        }
    }

    private void CultureDatas_DictionaryChanged(
        IObservableDictionary<string, ObservableCultureDataDictionary<string, string>> sender,
        NotifyDictionaryChangeEventArgs<string, ObservableCultureDataDictionary<string, string>> e
    )
    {
        if (e.Action is DictionaryChangeAction.Add)
        {
            if (e.TryGetNewPair(out var pair) && I18nResource.CurrentCulture is not null)
            {
                var value = I18nResource.GetCurrentCultureDataOrDefault(pair.Key);
                if (string.IsNullOrWhiteSpace(value) is false)
                    return;
                I18nResource.SetCurrentCultureData(pair.Key, pair.Key);
            }
        }
    }

    /// <inheritdoc/>
    /// <param name="loader">模组载入器</param>
    public ModInfoModel(ModLoader loader)
        : this()
    {
        this.LogX().Info("载入模组, ID: {id}, 路径: {path}", loader.Name, loader.ModPath.FullName);
        SourcePath = loader.ModPath.FullName;

        LoadI18nDatas(loader);

        ID = loader.Name;
        if (loader.Intro != DescriptionID)
        {
            if (I18nResource.ReplaceCultureDataKey(loader.Intro, DescriptionID, true) is false)
                I18nResource.SetCurrentCultureData(DescriptionID, loader.Intro);
        }

        Author = loader.Author;
        GameVersion = loader.GameVer;
        ModVersion = loader.Ver;
        ItemID = loader.ItemID;
        AuthorID = loader.AuthorID;
        var imagePath = Path.Combine(loader.ModPath.FullName, "icon.png");
        Image = HKWImageUtils.LoadImageToMemory(imagePath);

        LoadFoods(loader);
        LoadClickTexts(loader);
        LoadLowTexts(loader);
        LoadSelectTexts(loader);
        LoadPets(loader);

        I18nResource.FillDefaultValue();
        I18nResource.ClearUnreferencedData();
        CurrentPet = Pets.FilteredList.FirstOrDefault()!;
    }

    private void LoadPets(ModLoader loader)
    {
        this.LogX().Info("载入宠物, 数量: {count}", loader.Pets.Count);
        // 载入模组宠物
        foreach (var pet in loader.Pets)
        {
            try
            {
                var petModel = new PetModel(pet, I18nResource);
                // 如果检测到本体存在同名宠物
                if (NativeData.MainPets.TryGetValue(petModel.ID, out var mainPet))
                {
                    // 若宠物的值为默认值并且本体同名宠物不为默认值, 则把本体宠物的值作为模组宠物的默认值
                    if (
                        petModel.TouchHeadRectangleLocation
                            == PetModel.Default.TouchHeadRectangleLocation
                        && petModel.TouchHeadRectangleLocation != mainPet.TouchHeadRectangleLocation
                    )
                        petModel.TouchHeadRectangleLocation = mainPet.TouchHeadRectangleLocation;
                    if (
                        petModel.TouchBodyRectangleLocation
                            == PetModel.Default.TouchBodyRectangleLocation
                        && petModel.TouchBodyRectangleLocation != mainPet.TouchBodyRectangleLocation
                    )
                        petModel.TouchBodyRectangleLocation = mainPet.TouchBodyRectangleLocation;
                    if (
                        petModel.TouchRaisedRectangleLocation
                            == PetModel.Default.TouchRaisedRectangleLocation
                        && petModel.TouchRaisedRectangleLocation
                            != mainPet.TouchRaisedRectangleLocation
                    )
                        petModel.TouchRaisedRectangleLocation =
                            mainPet.TouchRaisedRectangleLocation;
                    if (
                        petModel.RaisePoint == PetModel.Default.RaisePoint
                        && petModel.RaisePoint != mainPet.RaisePoint
                    )
                        petModel.RaisePoint = mainPet.RaisePoint;
                }
                Pets.Add(petModel);
                foreach (var p in pet.path)
                    LoadAnime(petModel, p);
                this.LogX().Debug("添加宠物成功, ID: {id}, 宠物名称: {name}", pet.Name, pet.PetName);
            }
            catch (Exception ex)
            {
                this.LogX().Warn(ex, "添加宠物失败, ID: {id}, 宠物名称: {name}", pet.Name, pet.PetName);
            }
        }
    }

    private void LoadSelectTexts(ModLoader loader)
    {
        this.LogX().Info("载入选择文本, 数量: {count}", loader.SelectTexts.Count);
        foreach (var selectText in loader.SelectTexts)
        {
            try
            {
                SelectTexts.Add(new(selectText, I18nResource));
                this.LogX().Debug("添加选择文本成功, ID: {id}", selectText.Text);
            }
            catch (Exception ex)
            {
                this.LogX().Warn(ex, "添加选择文本失败, ID: {id}", selectText.Text);
            }
        }
    }

    private void LoadLowTexts(ModLoader loader)
    {
        this.LogX().Info("载入低状态文本, 数量: {count}", loader.LowTexts.Count);
        foreach (var lowText in loader.LowTexts)
        {
            try
            {
                LowTexts.Add(new(lowText) { I18nResource = I18nResource });
                this.LogX().Debug("添加低状态文本成功, ID: {id}", lowText.Text);
            }
            catch (Exception ex)
            {
                this.LogX().Warn(ex, "添加低状态文本失败, ID: {id}", lowText.Text);
            }
        }
    }

    private void LoadClickTexts(ModLoader loader)
    {
        this.LogX().Info("载入点击文本, 数量: {count}", loader.ClickTexts.Count);
        foreach (var clickText in loader.ClickTexts)
        {
            try
            {
                ClickTexts.Add(new(clickText) { I18nResource = I18nResource });
                this.LogX().Debug("添加点击文本成功, ID: {id}", clickText.Text);
            }
            catch (Exception ex)
            {
                this.LogX().Warn(ex, "添加点击文本失败, ID: {id}", clickText.Text);
            }
        }
    }

    private void LoadFoods(ModLoader loader)
    {
        this.LogX().Info("载入食物, 数量: {count}", loader.Foods.Count);
        foreach (var food in loader.Foods)
        {
            try
            {
                Foods.Add(new(food, I18nResource));
                this.LogX().Debug("添加食物成功, ID: {id}", food.Name);
            }
            catch (Exception ex)
            {
                this.LogX().Warn(ex, "添加食物失败, ID: {id}", food.Name);
            }
        }
    }

    /// <summary>
    /// I18n资源
    /// </summary>
    public I18nResource<string, string> I18nResource { get; } =
        new() { FillDefaultValueToData = true, DefaultValue = string.Empty };

    /// <summary>
    /// 临时I18n资源, 用于新建的项目
    /// </summary>
    public I18nResource<string, string> TempI18nResource { get; } =
        new() { FillDefaultValueToData = true, DefaultValue = string.Empty };

    /// <summary>
    /// I18n对象
    /// </summary>
    [NotifyPropertyChangeFrom("")]
    public I18nObject<string, string> I18nObject => new(this);

    /// <summary>
    /// 名称
    /// </summary>
    [ReactiveI18nProperty(nameof(I18nResource), nameof(I18nObject), nameof(ID), true)]
    public string Name
    {
        get => I18nResource.GetCurrentCultureDataOrDefault(ID);
        set => I18nResource.SetCurrentCultureData(ID, value);
    }

    /// <summary>
    /// 详情
    /// </summary>
    [ReactiveI18nProperty(nameof(I18nResource), nameof(I18nObject), nameof(DescriptionID), true)]
    public string Description
    {
        get => I18nResource.GetCurrentCultureDataOrDefault(DescriptionID);
        set => I18nResource.SetCurrentCultureData(DescriptionID, value);
    }

    /// <summary>
    /// 自动设置食物推荐价格
    /// </summary>
    [ReactiveProperty]
    public bool AutoSetFoodPrice { get; set; }

    /// <summary>
    /// 显示本体宠物
    /// </summary>
    [ReactiveProperty]
    public bool ShowMainPet { get; set; }

    #region ModInfo
    /// <summary>
    /// 作者ID
    /// </summary>
    public long AuthorID { get; }

    /// <summary>
    /// 项目ID
    /// </summary>
    public ulong ItemID { get; }

    /// <summary>
    /// ID
    /// </summary>
    [ReactiveProperty]
    public string ID { get; set; } = string.Empty;

    /// <summary>
    /// 描述ID
    /// </summary>
    [NotifyPropertyChangeFrom(nameof(ID))]
    public string DescriptionID => $"{ID}_Description";

    /// <summary>
    /// 作者
    /// </summary>
    [ReactiveProperty]
    public string Author { get; set; } = string.Empty;

    /// <summary>
    /// 支持的游戏版本
    /// </summary>
    [ReactiveProperty]
    public int GameVersion { get; set; }

    /// <summary>
    /// 模组版本
    /// </summary>
    [ReactiveProperty]
    public int ModVersion { get; set; }

    /// <summary>
    /// 封面
    /// </summary>
    [ReactiveProperty]
    public BitmapImage? Image { get; set; }

    /// <summary>
    /// 源路径
    /// </summary>
    [ReactiveProperty]
    public string SourcePath { get; set; } = string.Empty;

    #endregion

    #region ModDatas
    /// <summary>
    /// 食物
    /// </summary>
    public ObservableList<FoodModel> Foods { get; } = [];

    /// <summary>
    /// 点击文本
    /// </summary>
    public ObservableList<ClickTextModel> ClickTexts { get; } = [];

    /// <summary>
    /// 低状态文本
    /// </summary>
    public ObservableList<LowTextModel> LowTexts { get; } = [];

    /// <summary>
    /// 选择文本
    /// </summary>
    public ObservableList<SelectTextModel> SelectTexts { get; } = [];

    /// <summary>
    /// 当前宠物
    /// </summary>
    [ReactiveProperty]
    public PetModel? CurrentPet { get; set; } = null!;

    /// <summary>
    /// 宠物
    /// </summary>
    public FilterListWrapper<
        PetModel,
        ObservableList<PetModel>,
        ObservableList<PetModel>
    > Pets { get; }

    /// <summary>
    /// 宠物显示的数量
    /// </summary>
    [NotifyPropertyChangeFrom(false, nameof(ShowMainPet))]
    public int PetDisplayedCount =>
        ShowMainPet ? Pets.Count : Pets.Count - Pets.Count(m => m.FromMain);

    #endregion
    private void I18nResource_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(I18nResource.CurrentCulture))
        {
            TempI18nResource.CurrentCulture = I18nResource.CurrentCulture;
        }
    }

    private void Cultures_SetChanged(
        IObservableSet<CultureInfo> sender,
        NotifySetChangeEventArgs<CultureInfo> e
    )
    {
        if (e.Action is SetChangeAction.Clear)
        {
            TempI18nResource.ClearCulture();
        }
        else
        {
            if (e.OldItems is not null)
            {
                foreach (var item in e.OldItems)
                {
                    TempI18nResource.RemoveCulture(item);
                }
            }
            if (e.NewItems is not null)
            {
                foreach (var item in e.NewItems)
                {
                    TempI18nResource.AddCulture(item);
                }
            }
        }
    }

    #region Load
    /// <summary>
    /// 加载宠物动画
    /// </summary>
    /// <param name="petModel">模型</param>
    /// <param name="path">路径</param>
    void LoadAnime(PetModel petModel, string path)
    {
        if (Directory.Exists(path) is false)
            return;
        var directories = Directory.GetDirectories(path);
        this.LogX().Info("载入宠物动画, 数量: {count}, 路径: {path}", directories.Length, path);
        foreach (var animeDir in directories)
        {
            var dirName = Path.GetFileName(animeDir);
            this.LogX().Debug("载入动画, 路径: {path}", animeDir);
            if (Enum.TryParse<GraphInfo.GraphType>(dirName, true, out var graphType))
            {
                if (graphType.IsHasNameAnime())
                {
                    foreach (var dir in Directory.EnumerateDirectories(animeDir))
                    {
                        try
                        {
                            var anime = new AnimeTypeModel(graphType, Path.Combine(animeDir, dir));
                            petModel.Animes.Add(anime);
                        }
                        catch (Exception ex)
                        {
                            this.LogX()
                                .Warn(ex, "{$graphType} 动画载入失败, 目标文件夹: {path}", graphType, dir);
                        }
                    }
                }
                else
                {
                    try
                    {
                        var anime = new AnimeTypeModel(graphType, animeDir);
                        petModel.Animes.Add(anime);
                    }
                    catch (Exception ex)
                    {
                        this.LogX()
                            .Warn(ex, "{$graphType} 动画载入失败, 目标文件夹: {path}", graphType, animeDir);
                    }
                }
            }
            else if (dirName.Equals("Switch", StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (var dir in Directory.EnumerateDirectories(animeDir))
                {
                    try
                    {
                        var type = Enum.Parse<GraphInfo.GraphType>(
                            $"{dirName}_{Path.GetFileName(dir)}",
                            true
                        );
                        var anime = new AnimeTypeModel(type, Path.Combine(animeDir, dir));
                        petModel.Animes.Add(anime);
                    }
                    catch (Exception ex)
                    {
                        this.LogX().Warn(ex, "\"Switch\" 动画载入失败, 目标文件夹: {path}", dir);
                    }
                }
            }
            else if (dirName.Equals("Raise", StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (var dir in Directory.EnumerateDirectories(animeDir))
                {
                    try
                    {
                        var type = Enum.Parse<GraphInfo.GraphType>(Path.GetFileName(dir), true);
                        var anime = new AnimeTypeModel(type, Path.Combine(animeDir, dir));
                        petModel.Animes.Add(anime);
                    }
                    catch (Exception ex)
                    {
                        this.LogX().Warn(ex, "\"Raise\" 动画载入失败, 目标文件夹: {path}", dir);
                    }
                }
            }
            else if (dirName.Equals("State", StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (var dir in Directory.EnumerateDirectories(animeDir))
                {
                    try
                    {
                        var type = Enum.Parse<GraphInfo.GraphType>(Path.GetFileName(dir), true);
                        var anime = new AnimeTypeModel(type, Path.Combine(animeDir, dir));
                        petModel.Animes.Add(anime);
                    }
                    catch (Exception ex)
                    {
                        this.LogX().Warn(ex, "\"State\" 动画载入失败, 目标文件夹: {path}", dir);
                    }
                }
            }
            else if (dirName.Equals("Music", StringComparison.InvariantCultureIgnoreCase))
            {
                try
                {
                    var anime = new AnimeTypeModel(GraphInfo.GraphType.Common, animeDir);
                    petModel.Animes.Add(anime);
                }
                catch (Exception ex)
                {
                    this.LogX().Warn(ex, "\"Music\" 动画载入失败, 目标文件夹: {path}", animeDir);
                }
            }
            else if (FoodAnimeTypeModel.FoodAnimeNames.Contains(dirName))
            {
                try
                {
                    var anime = new FoodAnimeTypeModel(animeDir);
                    petModel.Animes.Add(anime);
                }
                catch (Exception ex)
                {
                    this.LogX().Warn(ex, "{dirName} 动画载入失败, 目标文件夹: {path}", dirName, animeDir);
                }
            }
            else
            {
                try
                {
                    var anime = new AnimeTypeModel(GraphInfo.GraphType.Common, animeDir);
                    petModel.Animes.Add(anime);
                }
                catch (Exception ex)
                {
                    this.LogX().Warn(ex, "{anime} 动画载入失败, 目标文件夹: {path}", dirName, animeDir);
                }
            }
        }
    }

    /// <summary>
    /// 加载本地化数据
    /// </summary>
    private void LoadI18nDatas(ModLoader modLoader)
    {
        if (modLoader.I18nDatas.Count == 0)
        {
            I18nResource.AddCulture(CultureInfo.CurrentCulture);
            I18nResource.CurrentCulture = CultureInfo.CurrentCulture;
            this.LogX().Info("模组未包含本地化数据");
            return;
        }
        this.LogX().Info("载入本地化数据, 目标文化: {cultrue}", string.Join(", ", modLoader.I18nDatas.Keys));
        foreach (var cultureDatas in modLoader.I18nDatas)
        {
            CultureInfo? culture;
            try
            {
                culture = CultureInfo.GetCultureInfo(cultureDatas.Key);
            }
            catch (Exception ex)
            {
                this.LogX().Warn(ex, $"载入文化 {cultureDatas.Key} 错误, 请检查文化名称");
                continue;
            }
            I18nResource.AddCulture(culture);
            I18nResource.SetCultureDatas(culture, cultureDatas.Value);
            this.LogX()
                .Info(
                    "已载入本地化数据, 文化: {cultrue}, 数据数量: {count}",
                    cultureDatas.Key,
                    cultureDatas.Value.Count
                );
        }
        if (I18nResource.SetCurrentCulture(CultureInfo.CurrentCulture) is false)
            I18nResource.SetCurrentCulture(I18nResource.Cultures.First());
    }
    #endregion
    #region Save
    /// <summary>
    /// 保存
    /// </summary>
    public void Save()
    {
        SaveTo(SourcePath);
    }

    /// <summary>
    /// 保存至路径
    /// </summary>
    /// <param name="path">路径</param>
    public void SaveTo(string path)
    {
        this.LogX().Info("正在保存模组, 目标路径: {path}", path);
        // 保存模型信息
        SaveModInfo(path);
        // 保存模组数据
        SavePets(path);
        SaveFoods(path);
        SaveText(path);
        SaveI18nData(path, I18nResource.Cultures);
        SaveImage(path);
        this.LogX().Info("模组保存完毕");
    }

    /// <summary>
    /// 保存模型信息
    /// </summary>
    /// <param name="path">路径</param>
    private void SaveModInfo(string path)
    {
        var modInfoFile = Path.Combine(path, NativeData.InfoFileName);
        if (File.Exists(modInfoFile) is false)
            File.Create(modInfoFile).Close();

        var lps = new LPS()
        {
            new Line("vupmod", ID)
            {
                new Sub("author", Author),
                new Sub("gamever", GameVersion),
                new Sub("ver", ModVersion)
            },
            new Line("intro", DescriptionID),
            new Line("authorid", AuthorID.ToString()),
            new Line("itemid", ItemID.ToString()),
            new Line("cachedate", DateTime.Now.Date.ToString("s"))
        };
        foreach (var culture in I18nResource.Cultures)
        {
            lps.Add(
                new Line("cultureDatas", culture.Name)
                {
                    new Sub(ID, I18nResource.GetCultureDataOrDefault(culture, ID, string.Empty)),
                    new Sub(
                        DescriptionID,
                        I18nResource.GetCultureDataOrDefault(culture, DescriptionID, string.Empty)
                    ),
                }
            );
        }
        Image?.SaveToPng(Path.Combine(path, "icon.png"));
        File.WriteAllText(modInfoFile, lps.ToString());
        this.LogX().Info("模组信息保存完成, 目标文件: {file}", modInfoFile);
    }

    private void SavePets(string path)
    {
        var petPath = Path.Combine(path, "pet");
        if (Pets.Count == 0 || Pets.All(m => m.FromMain))
        {
            if (Directory.Exists(petPath))
                Directory.Delete(petPath, true);
            return;
        }
        this.LogX().Info("正在保存宠物, 数量: {count}", Pets.Count(m => m.FromMain is false));
        Directory.CreateDirectory(petPath);
        foreach (var pet in Pets)
        {
            if (pet.FromMain)
                continue;
            try
            {
                pet.Save(petPath);
                this.LogX().Info("保存宠物成功, ID: {id}, 宠物名称: {petName}", pet.ID, pet.PetName);
            }
            catch (Exception ex)
            {
                this.LogX().Info(ex, "保存宠物失败, ID: {id}, 宠物名称: {petName}", pet.ID, pet.PetName);
            }
        }
        // 如果没有一个完成保存, 则删除文件夹
        if (Directory.EnumerateFiles(petPath).Any() is false)
            Directory.Delete(petPath);
    }

    /// <summary>
    /// 保存食物
    /// </summary>
    /// <param name="path">路径</param>
    private void SaveFoods(string path)
    {
        var foodPath = Path.Combine(path, "food");
        if (Foods.Count == 0)
        {
            if (Directory.Exists(foodPath))
                Directory.Delete(foodPath, true);
            return;
        }
        this.LogX().Info("正在保存食物, 数量: {count}", Foods.Count);
        Directory.CreateDirectory(foodPath);
        var foodFile = Path.Combine(foodPath, "food.lps");
        if (File.Exists(foodFile) is false)
            File.Create(foodFile).Close();
        var lps = new LPS();
        foreach (var food in Foods)
            lps.Add(LPSConvert.SerializeObjectToLine<Line>(food.MapToFood(new()), "food"));
        File.WriteAllText(foodFile, lps.ToString());
    }

    #region SaveText
    /// <summary>
    /// 保存文本
    /// </summary>
    /// <param name="path">路径</param>
    private void SaveText(string path)
    {
        var textPath = Path.Combine(path, "text");
        if (LowTexts.Count == 0 && ClickTexts.Count == 0 && SelectTexts.Count == 0)
        {
            if (Directory.Exists(textPath))
                Directory.Delete(textPath, true);
            return;
        }
        Directory.CreateDirectory(textPath);
        SaveLowText(textPath);
        SaveClickText(textPath);
        SaveSelectText(textPath);
    }

    /// <summary>
    /// 保存选择文本
    /// </summary>
    /// <param name="path">路径</param>
    private void SaveSelectText(string path)
    {
        if (SelectTexts.Count == 0)
            return;
        this.LogX().Info("正在保存选择文本, 数量: {count}", SelectTexts.Count);
        var textFile = Path.Combine(path, "selectText.lps");
        File.Create(textFile).Close();
        var lps = new LPS();
        foreach (var text in SelectTexts)
            lps.Add(
                LPSConvert.SerializeObjectToLine<Line>(text.MapToSelectText(new()), "SelectText")
            );
        File.WriteAllText(textFile, lps.ToString());
    }

    /// <summary>
    /// 保存低状态文本
    /// </summary>
    /// <param name="path">路径</param>
    private void SaveLowText(string path)
    {
        if (LowTexts.Count == 0)
            return;
        this.LogX().Info("正在保存低状态文本, 数量: {count}", LowTexts.Count);
        var textFile = Path.Combine(path, "lowText.lps");
        File.Create(textFile).Close();
        var lps = new LPS();
        foreach (var text in LowTexts)
            lps.Add(
                LPSConvert.SerializeObjectToLine<Line>(text.MapToLowText(new()), "lowfoodtext")
            );
        File.WriteAllText(textFile, lps.ToString());
    }

    /// <summary>
    /// 保存点击文本
    /// </summary>
    /// <param name="path">路径</param>
    private void SaveClickText(string path)
    {
        if (ClickTexts.Count == 0)
            return;
        this.LogX().Info("正在保存点击文本, 数量: {count}", ClickTexts.Count);
        var textFile = Path.Combine(path, "clickText.lps");
        File.Create(textFile).Close();
        var lps = new LPS();
        foreach (var text in ClickTexts)
            lps.Add(
                LPSConvert.SerializeObjectToLine<Line>(text.MapToClickText(new()), "clicktext")
            );
        File.WriteAllText(textFile, lps.ToString());
    }
    #endregion
    /// <summary>
    /// 保存I18n资源
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="cultures">文化</param>
    private void SaveI18nData(string path, IEnumerable<CultureInfo> cultures)
    {
        I18nResource.ClearUnreferencedData();
        this.LogX().Info("正在保存本地化数据, 文化数量: {count}", I18nResource.Cultures.Count);
        var langPath = Path.Combine(path, "lang");
        Directory.CreateDirectory(langPath);
        foreach (var culture in cultures)
        {
            this.LogX()
                .Info(
                    "正在保存本地化数据 {cultrue}, 数据数量: {count}",
                    culture.Name,
                    I18nResource.CultureDatas.Count
                );
            var culturePath = Path.Combine(langPath, culture.Name);
            Directory.CreateDirectory(culturePath);
            var cultureFile = Path.Combine(culturePath, $"{culture.Name}.lps");
            File.Create(cultureFile).Close();
            var lps = new LPS();
            foreach (var datas in I18nResource.CultureDatas)
            {
                if (I18nResource.TryGetCultureData(culture, datas.Key, out var data))
                    lps.Add(new Line(datas.Key, data));
            }
            File.WriteAllText(cultureFile, lps.ToString());
        }
    }

    /// <summary>
    /// 保存图片
    /// </summary>
    /// <param name="path">路径</param>
    private void SaveImage(string path)
    {
        if (Foods.Count == 0)
            return;
        this.LogX().Info("正在保存图片, 数量: {count}", Foods.Count(x => x.Image is not null));
        var imagePath = Path.Combine(path, "image");
        if (Directory.Exists(imagePath))
            Directory.Delete(imagePath, true);
        Directory.CreateDirectory(imagePath);
        if (Foods.Count > 0)
        {
            var foodPath = Path.Combine(imagePath, "food");
            Directory.CreateDirectory(foodPath);
            foreach (var food in Foods)
            {
                food.Image?.SaveToPng(Path.Combine(foodPath, food.ID));
            }
        }
    }
    #endregion


    /// <summary>
    /// 保存为翻译模组
    /// </summary>
    /// <param name="path">路径</param>
    /// <param name="cultures">指定文化</param>
    public void SaveToTranslationMod(string path, IEnumerable<CultureInfo> cultures)
    {
        this.LogX().Info("保存为翻译模组, 路径: {path}, 翻译目标: {cultures}", string.Join(", ", cultures));
        // 保存模型信息
        SaveModInfo(path);
        var petPath = Path.Combine(path, "pet");
        if (Directory.Exists(petPath))
            Directory.Delete(petPath, true);
        var foodPath = Path.Combine(path, "food");
        if (Directory.Exists(foodPath))
            Directory.Delete(foodPath, true);
        var imagePath = Path.Combine(path, "image");
        if (Directory.Exists(imagePath))
            Directory.Delete(imagePath, true);
        var textPath = Path.Combine(path, "text");
        if (Directory.Exists(textPath))
            Directory.Delete(textPath, true);
        // 保存文化数据
        SaveI18nData(path, cultures);
        this.LogX().Info("翻译模组保存完毕");
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (_disposed)
            return;
        base.Dispose(disposing);
        if (disposing)
        {
            I18nResource.PropertyChanged -= I18nResource_PropertyChanged;
            I18nResource.Cultures.SetChanged -= Cultures_SetChanged;
            I18nResource.CultureDatas.DictionaryChanged -= CultureDatas_DictionaryChanged;
            Image?.CloseStreamWhenNoReference();
            I18nObject.Close();
            I18nResource.Clear();
            TempI18nResource.Clear();
            foreach (var food in Foods)
                food.Dispose();
            foreach (var text in ClickTexts)
                text.Dispose();
            foreach (var text in LowTexts)
                text.Dispose();
            foreach (var text in SelectTexts)
                text.Dispose();
            foreach (var pet in Pets.Where(x => x.FromMain is false))
                pet.Dispose();
        }
    }
}
