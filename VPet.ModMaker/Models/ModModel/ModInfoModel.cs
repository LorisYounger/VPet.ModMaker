using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using HKW.HKWReactiveUI;
using HKW.HKWUtils;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using LinePutScript;
using LinePutScript.Converter;
using LinePutScript.Localization.WPF;
using VPet.ModMaker.Models.ModModel;
using VPet.ModMaker.ViewModels;
using VPet.ModMaker.Views.ModEdit.I18nEdit;
using VPet_Simulator.Core;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.Models;

/// <summary>
/// 模组信息模型
/// </summary>
public partial class ModInfoModel : ViewModelBase
{
    public ModInfoModel()
    {
        Current = this;
        PropertyChanged += ModInfoModel_PropertyChanged;
        Pets.CollectionChanged += Pets_CollectionChanged;
        I18nResource.PropertyChanged += I18nResource_PropertyChanged;
        I18nResource.Cultures.SetChanged += Cultures_SetChanged;
        //TODO:
        //I18nResource?.I18nObjectInfos.Add(
        //    this,
        //    new I18nObjectInfo<string, string>(this, OnPropertyChanged).AddPropertyInfo(
        //        [
        //            (nameof(ID), ID, nameof(Name)),
        //            (nameof(DescriptionID), DescriptionID, nameof(Description))
        //        ],
        //        true
        //    )
        //);
        foreach (var pet in ModMakerInfo.MainPets)
        {
            // 确保ID不重复
            if (Pets.All(i => i.ID != pet.Key))
                Pets.Add(pet.Value);
        }
    }

    public ModInfoModel(ModLoader loader)
        : this()
    {
        SourcePath = loader.ModPath.FullName;
        ID = loader.Name;
        DescriptionID = loader.Intro;
        Author = loader.Author;
        GameVersion = loader.GameVer;
        ModVersion = loader.Ver;
        ItemID = loader.ItemID;
        AuthorID = loader.AuthorID;
        var imagePath = Path.Combine(loader.ModPath.FullName, "icon.png");
        if (File.Exists(imagePath))
            Image = NativeUtils.LoadImageToMemoryStream(imagePath);
        foreach (var food in loader.Foods.Where(m => string.IsNullOrWhiteSpace(m.Name) is false))
            Foods.Add(new(food) { I18nResource = I18nResource });
        foreach (
            var clickText in loader.ClickTexts.Where(m =>
                string.IsNullOrWhiteSpace(m.Text) is false
            )
        )
            ClickTexts.Add(new(clickText) { I18nResource = I18nResource });
        foreach (
            var lowText in loader.LowTexts.Where(m => string.IsNullOrWhiteSpace(m.Text) is false)
        )
            LowTexts.Add(new(lowText) { I18nResource = I18nResource });
        foreach (
            var selectText in loader.SelectTexts.Where(m =>
                string.IsNullOrWhiteSpace(m.Text) is false
            )
        )
            SelectTexts.Add(new(selectText) { I18nResource = I18nResource });

        // 载入模组宠物
        foreach (var pet in loader.Pets)
        {
            var petModel = new PetModel(pet) { I18nResource = I18nResource };
            // 如果检测到本体存在同名宠物
            if (ModMakerInfo.MainPets.TryGetValue(petModel.ID, out var mainPet))
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
                    && petModel.TouchRaisedRectangleLocation != mainPet.TouchRaisedRectangleLocation
                )
                    petModel.TouchRaisedRectangleLocation = mainPet.TouchRaisedRectangleLocation;
                if (
                    petModel.RaisePoint == PetModel.Default.RaisePoint
                    && petModel.RaisePoint != mainPet.RaisePoint
                )
                    petModel.RaisePoint = mainPet.RaisePoint;
            }
            Pets.Add(petModel);
            foreach (var p in pet.path)
                LoadAnime(petModel, p);
        }
        if (loader.I18nDatas.HasValue() is false)
            return;
        LoadI18nDatas(loader);
        RefreshAllID();
        if (I18nResource.CultureDatas.HasValue())
            RefreshID();
        I18nResource.FillDefaultValue();
    }

    private void ModInfoModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ID))
        {
            DescriptionID = $"{ID}_{nameof(DescriptionID)}";
        }
        else if (e.PropertyName == nameof(ShowMainPet))
        {
            Pets_CollectionChanged(null, null!);
        }
    }

    /// <summary>
    /// 当前
    /// </summary>
    public static ModInfoModel Current { get; set; } = new();

    /// <summary>
    /// I18n资源
    /// </summary>
    public I18nResource<string, string> I18nResource { get; } =
        new() { FillDefaultValueToNewCulture = true, DefaultValue = string.Empty };

    /// <summary>
    /// 临时I18n资源, 用于新建的项目
    /// </summary>
    public I18nResource<string, string> TempI18nResource { get; } =
        new() { FillDefaultValueToNewCulture = true, DefaultValue = string.Empty };

    #region I18nData
    public string Name
    {
        get => I18nResource.GetCurrentCultureDataOrDefault(ID);
        set => I18nResource.SetCurrentCultureData(ID, value);
    }
    public string Description
    {
        get => I18nResource.GetCurrentCultureDataOrDefault(DescriptionID);
        set => I18nResource.SetCurrentCultureData(DescriptionID, value);
    }
    #endregion

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
    public string DescriptionID { get; set; } = string.Empty;

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
    /// 宠物
    /// </summary>
    public ObservableList<PetModel> Pets { get; } = [];

    /// <summary>
    /// 宠物实际数量
    /// </summary>
    [ReactiveProperty]
    public int PetDisplayedCount { get; set; }

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

    private void Pets_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (ShowMainPet)
            PetDisplayedCount = Pets.Count;
        else
            PetDisplayedCount = Pets.Count - Pets.Count(m => m.FromMain);
    }

    public void RefreshID()
    {
        DescriptionID = $"{ID}_{nameof(DescriptionID)}";
    }

    #region Load
    /// <summary>
    /// 加载宠物动画
    /// </summary>
    /// <param name="petModel">模型</param>
    /// <param name="path">路径</param>
    static void LoadAnime(PetModel petModel, string path)
    {
        if (Directory.Exists(path) is false)
            return;
        foreach (var animeDir in Directory.EnumerateDirectories(path))
        {
            var dirName = Path.GetFileName(animeDir);
            if (Enum.TryParse<GraphInfo.GraphType>(dirName, true, out var graphType))
            {
                if (graphType.IsHasNameAnime())
                {
                    foreach (var dir in Directory.EnumerateDirectories(animeDir))
                    {
                        if (AnimeTypeModel.Create(graphType, dir) is AnimeTypeModel model1)
                            petModel.Animes.Add(model1);
                    }
                }
                else if (AnimeTypeModel.Create(graphType, animeDir) is AnimeTypeModel model)
                    petModel.Animes.Add(model);
            }
            else if (dirName.Equals("Switch", StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (var dir in Directory.EnumerateDirectories(animeDir))
                {
                    Enum.TryParse<GraphInfo.GraphType>(
                        $"{dirName}_{Path.GetFileName(dir)}",
                        true,
                        out var switchType
                    );
                    if (
                        AnimeTypeModel.Create(switchType, Path.Combine(animeDir, dir))
                        is AnimeTypeModel switchModel
                    )
                        petModel.Animes.Add(switchModel);
                }
            }
            else if (dirName.Equals("Raise", StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (var dir in Directory.EnumerateDirectories(animeDir))
                {
                    Enum.TryParse<GraphInfo.GraphType>(
                        Path.GetFileName(dir),
                        true,
                        out var switchType
                    );
                    if (
                        AnimeTypeModel.Create(switchType, Path.Combine(animeDir, dir))
                        is AnimeTypeModel switchModel
                    )
                        petModel.Animes.Add(switchModel);
                }
            }
            else if (dirName.Equals("State", StringComparison.InvariantCultureIgnoreCase))
            {
                foreach (var dir in Directory.EnumerateDirectories(animeDir))
                {
                    Enum.TryParse<GraphInfo.GraphType>(
                        Path.GetFileName(dir),
                        true,
                        out var switchType
                    );
                    if (
                        AnimeTypeModel.Create(switchType, Path.Combine(animeDir, dir))
                        is AnimeTypeModel switchModel
                    )
                        petModel.Animes.Add(switchModel);
                }
            }
            else if (dirName.Equals("Music", StringComparison.InvariantCultureIgnoreCase))
            {
                if (
                    AnimeTypeModel.Create(GraphInfo.GraphType.Common, animeDir)
                    is AnimeTypeModel model1
                )
                    petModel.Animes.Add(model1);
            }
            else if (FoodAnimeTypeModel.FoodAnimeNames.Contains(dirName))
            {
                if (FoodAnimeTypeModel.Create(animeDir) is FoodAnimeTypeModel model1)
                    petModel.FoodAnimes.Add(model1);
            }
        }
    }

    /// <summary>
    /// 加载本地化数据
    /// </summary>
    private void LoadI18nDatas(ModLoader modLoader)
    {
        foreach (var cultureDatas in modLoader.I18nDatas)
        {
            var culture = CultureInfo.GetCultureInfo(cultureDatas.Key);
            I18nResource.AddCulture(culture);
            foreach (var data in cultureDatas.Value)
                I18nResource.SetCultureData(culture, data.Key, data.Value);
        }
        if (I18nResource.SetCurrentCulture(CultureInfo.CurrentCulture) is false)
            I18nResource.SetCurrentCulture(I18nResource.Cultures.First());
    }

    public void RefreshAllID()
    {
        RefreshID();
        foreach (var food in Foods)
            food.RefreshID();
        foreach (var selectText in SelectTexts)
            selectText.RefreshID();
        foreach (var pet in Pets)
            pet.RefreshID();
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
        // 保存模型信息
        SaveModInfo(path);
        // 保存模组数据
        SavePets(path);
        SaveFoods(path);
        SaveText(path);
        SaveI18nData(path);
        SaveImage(path);
    }

    /// <summary>
    /// 保存模型信息
    /// </summary>
    /// <param name="path">路径</param>
    private void SaveModInfo(string path)
    {
        var modInfoFile = Path.Combine(path, ModMakerInfo.InfoFile);
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
        foreach (var culture in Current.I18nResource.Cultures)
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
    }

    private void SavePets(string path)
    {
        var petPath = Path.Combine(path, "pet");
        if (Pets.Count == 0 || Pets.All(m => m.CanSave() is false))
        {
            if (Directory.Exists(petPath))
                Directory.Delete(petPath, true);
            return;
        }
        Directory.CreateDirectory(petPath);
        foreach (var pet in Pets)
        {
            if (pet.CanSave())
                pet.Save(petPath);
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
        Directory.CreateDirectory(foodPath);
        var foodFile = Path.Combine(foodPath, "food.lps");
        if (File.Exists(foodFile) is false)
            File.Create(foodFile).Close();
        var lps = new LPS();
        foreach (var food in Foods)
            lps.Add(LPSConvert.SerializeObjectToLine<Line>(food.ToFood(), "food"));
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
    private void SaveSelectText(string textPath)
    {
        if (SelectTexts.Count == 0)
            return;
        var textFile = Path.Combine(textPath, "selectText.lps");
        File.Create(textFile).Close();
        var lps = new LPS();
        foreach (var text in SelectTexts)
            lps.Add(LPSConvert.SerializeObjectToLine<Line>(text.ToSelectText(), "SelectText"));
        File.WriteAllText(textFile, lps.ToString());
    }

    /// <summary>
    /// 保存低状态文本
    /// </summary>
    /// <param name="path">路径</param>
    private void SaveLowText(string textPath)
    {
        if (LowTexts.Count == 0)
            return;
        var textFile = Path.Combine(textPath, "lowText.lps");
        File.Create(textFile).Close();
        var lps = new LPS();
        foreach (var text in LowTexts)
            lps.Add(LPSConvert.SerializeObjectToLine<Line>(text.ToLowText(), "lowfoodtext"));
        File.WriteAllText(textFile, lps.ToString());
    }

    /// <summary>
    /// 保存点击文本
    /// </summary>
    /// <param name="path">路径</param>
    private void SaveClickText(string textPath)
    {
        if (ClickTexts.Count == 0)
            return;
        var textFile = Path.Combine(textPath, "clickText.lps");
        File.Create(textFile).Close();
        var lps = new LPS();
        foreach (var text in ClickTexts)
            lps.Add(LPSConvert.SerializeObjectToLine<Line>(text.ToClickText(), "clicktext"));
        File.WriteAllText(textFile, lps.ToString());
    }
    #endregion
    /// <summary>
    /// 保存I18n资源
    /// </summary>
    /// <param name="path">路径</param>
    private void SaveI18nData(string path)
    {
        var langPath = Path.Combine(path, "lang");
        Directory.CreateDirectory(langPath);
        foreach (var culture in I18nResource.Cultures)
        {
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
    /// 保存突破
    /// </summary>
    /// <param name="path">路径</param>
    private void SaveImage(string path)
    {
        if (Foods.Count == 0)
            return;
        var imagePath = Path.Combine(path, "image");
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
    /// 关闭
    /// </summary>
    public void Close()
    {
        Image?.CloseStream();
        foreach (var food in Foods)
            food.Close();
        foreach (var pet in Pets)
            pet.Close();
        Current = null!;
    }

    public void SaveTranslationMod(string path, IEnumerable<CultureInfo> cultures)
    {
        // 保存模型信息
        SaveModInfo(path);
        // 保存文化数据
        var langPath = Path.Combine(path, "lang");
        Directory.CreateDirectory(langPath);
        foreach (var culture in cultures)
        {
            var culturePath = Path.Combine(langPath, culture.Name);
            Directory.CreateDirectory(culturePath);
            var cultureFile = Path.Combine(culturePath, $"{culture}.lps");
            File.Create(cultureFile).Close();
            var lps = new LPS();
            foreach (var data in I18nResource.CultureDatas.Values)
                lps.Add(new Line(data.Key, data[culture]));
            File.WriteAllText(cultureFile, lps.ToString());
        }
    }
}
