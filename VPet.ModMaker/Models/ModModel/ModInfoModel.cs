using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using HKW.HKWUtils;
using HKW.HKWUtils.Observable;
using LinePutScript;
using LinePutScript.Converter;
using LinePutScript.Localization.WPF;
using VPet.ModMaker.Models.ModModel;
using VPet.ModMaker.Views.ModEdit.I18nEdit;
using VPet_Simulator.Core;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.Models;

/// <summary>
/// 模组信息模型
/// </summary>
public class ModInfoModel : I18nModel<I18nModInfoModel>
{
    /// <summary>
    /// 自动设置食物推荐价格
    /// </summary>
    public ObservableValue<bool> AutoSetFoodPrice { get; } = new();

    /// <summary>
    /// 不显示本体宠物
    /// </summary>
    public ObservableValue<bool> ShowMainPet { get; } = new(true);

    #region ModInfo
    /// <summary>
    /// 作者Id
    /// </summary>
    public long AuthorID { get; }

    /// <summary>
    /// 项目Id
    /// </summary>
    public ulong ItemID { get; }

    /// <summary>
    /// 当前
    /// </summary>
    public static ModInfoModel Current { get; set; } = new();

    /// <summary>
    /// Id
    /// </summary>
    public ObservableValue<string> Id { get; } = new();

    /// <summary>
    /// 描述Id
    /// </summary>
    public ObservableValue<string> DescriptionId { get; } = new();

    /// <summary>
    /// 作者
    /// </summary>
    public ObservableValue<string> Author { get; } = new();

    /// <summary>
    /// 支持的游戏版本
    /// </summary>
    public ObservableValue<int> GameVersion { get; } = new(ModMakerInfo.GameVersion);

    /// <summary>
    /// 模组版本
    /// </summary>
    public ObservableValue<int> ModVersion { get; } = new(100);

    /// <summary>
    /// 封面
    /// </summary>
    public ObservableValue<BitmapImage> Image { get; } = new();

    /// <summary>
    /// 源路径
    /// </summary>
    public ObservableValue<string> SourcePath { get; } = new();

    #endregion

    #region ModDatas
    /// <summary>
    /// 食物
    /// </summary>
    public ObservableCollection<FoodModel> Foods { get; } = new();

    /// <summary>
    /// 点击文本
    /// </summary>
    public ObservableCollection<ClickTextModel> ClickTexts { get; } = new();

    /// <summary>
    /// 低状态文本
    /// </summary>
    public ObservableCollection<LowTextModel> LowTexts { get; } = new();

    /// <summary>
    /// 选择文本
    /// </summary>
    public ObservableCollection<SelectTextModel> SelectTexts { get; } = new();

    /// <summary>
    /// 宠物
    /// </summary>
    public ObservableCollection<PetModel> Pets { get; } = new();

    /// <summary>
    /// 宠物实际数量
    /// </summary>
    public ObservableValue<int> PetDisplayedCount { get; } = new();

    /// <summary>
    /// 其它I18n数据
    /// </summary>
    public Dictionary<string, Dictionary<string, string>> OtherI18nDatas { get; } = new();

    /// <summary>
    /// 需要保存的I18n数据
    /// </summary>
    public static Dictionary<string, Dictionary<string, string>> SaveI18nDatas { get; } = new();
    #endregion
    public ModInfoModel()
    {
        DescriptionId.Value = $"{Id.Value}_{nameof(DescriptionId)}";
        Id.ValueChanged += (o, n) =>
        {
            DescriptionId.Value = $"{n}_{nameof(DescriptionId)}";
        };
        Pets.CollectionChanged += Pets_CollectionChanged;
        ShowMainPet.ValueChanged += ShowMainPet_ValueChanged;
    }

    private void ShowMainPet_ValueChanged(
        ObservableValue<bool> sender,
        ValueChangedEventArgs<bool> e
    )
    {
        Pets_CollectionChanged(null, null);
    }

    private void Pets_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (ShowMainPet.Value)
            PetDisplayedCount.Value = Pets.Count;
        else
            PetDisplayedCount.Value = Pets.Count - Pets.Count(m => m.FromMain.Value);
    }

    public ModInfoModel(ModLoader loader)
        : this()
    {
        SourcePath.Value = loader.ModPath.FullName;
        Id.Value = loader.Name;
        DescriptionId.Value = loader.Intro;
        Author.Value = loader.Author;
        GameVersion.Value = loader.GameVer;
        ModVersion.Value = loader.Ver;
        ItemID = loader.ItemID;
        AuthorID = loader.AuthorID;
        var imagePath = Path.Combine(loader.ModPath.FullName, "icon.png");
        if (File.Exists(imagePath))
            Image.Value = NativeUtils.LoadImageToMemoryStream(imagePath);
        foreach (var food in loader.Foods)
            Foods.Add(new(food));
        foreach (var clickText in loader.ClickTexts)
            ClickTexts.Add(new(clickText));
        foreach (var lowText in loader.LowTexts)
            LowTexts.Add(new(lowText));
        foreach (var selectText in loader.SelectTexts)
            SelectTexts.Add(new(selectText));

        // 载入模组宠物
        foreach (var pet in loader.Pets)
        {
            var petModel = new PetModel(pet);
            // 如果检测到本体存在同名宠物
            if (ModMakerInfo.MainPets.TryGetValue(petModel.Id.Value, out var mainPet))
            {
                // 若宠物的值为默认值并且本体同名宠物不为默认值, 则把本体宠物的值作为模组宠物的默认值
                if (
                    petModel.TouchHeadRect == PetModel.Default.TouchHeadRect
                    && petModel.TouchHeadRect != mainPet.TouchHeadRect
                )
                    petModel.TouchHeadRect.Value = mainPet.TouchHeadRect.Value;
                if (
                    petModel.TouchBodyRect == PetModel.Default.TouchBodyRect
                    && petModel.TouchBodyRect != mainPet.TouchBodyRect
                )
                    petModel.TouchBodyRect.Value = mainPet.TouchBodyRect.Value;
                if (
                    petModel.TouchRaisedRect == PetModel.Default.TouchRaisedRect
                    && petModel.TouchRaisedRect != mainPet.TouchRaisedRect
                )
                    petModel.TouchRaisedRect.Value = mainPet.TouchRaisedRect.Value;
                if (
                    petModel.RaisePoint == PetModel.Default.RaisePoint
                    && petModel.RaisePoint != mainPet.RaisePoint
                )
                    petModel.RaisePoint.Value = mainPet.RaisePoint.Value;
            }
            Pets.Add(petModel);
            foreach (var p in pet.path)
                LoadAnime(petModel, p);
        }

        //loader.Pets.First().Name = "TestMainPet";
        //Pets.Insert(0, new(loader.Pets.First(), true));

        // 插入本体宠物
        foreach (var pet in ModMakerInfo.MainPets)
        {
            // 确保Id不重复
            if (Pets.All(i => i.Id.Value != pet.Key))
                Pets.Insert(0, pet.Value);
        }

        // 载入本地化
        foreach (var lang in loader.I18nDatas)
            I18nDatas.Add(lang.Key, lang.Value);
        OtherI18nDatas = loader.OtherI18nDatas;

        LoadI18nDatas();
        RefreshAllId();
        RefreshId();
    }

    public void RefreshId()
    {
        DescriptionId.Value = $"{Id.Value}_{nameof(DescriptionId)}";
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
    private void LoadI18nDatas()
    {
        foreach (var lang in I18nDatas.Keys.Union(OtherI18nDatas.Keys))
        {
            if (I18nHelper.Current.CultureNames.Contains(lang) is false)
                I18nHelper.Current.CultureNames.Add(lang);
        }
        if (I18nHelper.Current.CultureNames.Count == 0)
            return;
        I18nHelper.Current.CultureName.Value = I18nHelper.Current.CultureNames.First();
        foreach (var i18nData in OtherI18nDatas)
        {
            LoadFoodI18nData(i18nData.Key, i18nData.Value);
            LoadLowTextI18nData(i18nData.Key, i18nData.Value);
            LoadClickTextI18nData(i18nData.Key, i18nData.Value);
            LoadSelectTextI18nData(i18nData.Key, i18nData.Value);
            LoadPetI18nData(i18nData.Key, i18nData.Value);
        }
    }

    private void LoadFoodI18nData(string key, Dictionary<string, string> i18nData)
    {
        foreach (var food in Foods)
        {
            if (food.I18nDatas.TryGetValue(key, out var data) is false)
                continue;
            if (i18nData.TryGetValue(food.Id.Value, out var name))
                data.Name.Value = name;
            if (i18nData.TryGetValue(food.DescriptionId.Value, out var description))
                data.Description.Value = description;
        }
    }

    private void LoadLowTextI18nData(string key, Dictionary<string, string> i18nData)
    {
        foreach (var lowText in LowTexts)
        {
            if (lowText.I18nDatas.TryGetValue(key, out var data) is false)
                continue;
            if (i18nData.TryGetValue(lowText.Id.Value, out var text))
                data.Text.Value = text;
        }
    }

    private void LoadClickTextI18nData(string key, Dictionary<string, string> i18nData)
    {
        foreach (var clickText in ClickTexts)
        {
            if (clickText.I18nDatas.TryGetValue(key, out var data) is false)
                continue;
            if (i18nData.TryGetValue(clickText.Id.Value, out var text))
                data.Text.Value = text;
        }
    }

    private void LoadSelectTextI18nData(string key, Dictionary<string, string> i18nData)
    {
        foreach (var selectText in SelectTexts)
        {
            if (selectText.I18nDatas.TryGetValue(key, out var data) is false)
                continue;
            if (i18nData.TryGetValue(selectText.Id.Value, out var text))
                data.Text.Value = text;
            if (i18nData.TryGetValue(selectText.ChooseId.Value, out var choose))
                data.Choose.Value = choose;
        }
    }

    private void LoadPetI18nData(string key, Dictionary<string, string> i18nData)
    {
        foreach (var pet in Pets)
        {
            if (pet.I18nDatas.TryGetValue(key, out var data) is false)
                continue;
            if (i18nData.TryGetValue(pet.Id.Value, out var name))
                data.Name.Value = name;
            if (i18nData.TryGetValue(pet.PetNameId.Value, out var petName))
                data.PetName.Value = petName;
            if (i18nData.TryGetValue(pet.DescriptionId.Value, out var description))
                data.Description.Value = description;
            foreach (var work in pet.Works)
            {
                if (work.I18nDatas.TryGetValue(key, out var workData) is false)
                    continue;
                if (i18nData.TryGetValue(work.Id.Value, out var workName))
                    workData.Name.Value = workName;
            }
        }
    }

    private void RefreshAllId()
    {
        foreach (var food in Foods)
            food.RefreshId();
        foreach (var selectText in SelectTexts)
            selectText.RefreshId();
        foreach (var pet in Pets)
            pet.RefreshId();
    }
    #endregion
    #region Save
    /// <summary>
    /// 保存
    /// </summary>
    public void Save()
    {
        SaveTo(SourcePath.Value);
    }

    /// <summary>
    /// 保存至路径
    /// </summary>
    /// <param name="path">路径</param>
    public void SaveTo(string path)
    {
        SaveI18nDatas.Clear();
        // 保存模型信息
        SaveModInfo(path);
        // 保存模组数据
        SavePets(path);
        SaveFoods(path);
        SaveText(path);
        SaveI18nData(path);
        SaveImage(path);
        SaveI18nDatas.Clear();
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

        SaveI18nDatas.Clear();
        foreach (var cultureName in I18nHelper.Current.CultureNames)
            SaveI18nDatas.Add(cultureName, new());

        var lps = new LPS()
        {
            new Line("vupmod", Id.Value)
            {
                new Sub("author", Author.Value),
                new Sub("gamever", GameVersion.Value),
                new Sub("ver", ModVersion.Value)
            },
            new Line("intro", DescriptionId.Value),
            new Line("authorid", AuthorID.ToString()),
            new Line("itemid", ItemID.ToString()),
            new Line("cachedate", DateTime.Now.Date.ToString("s"))
        };
        foreach (var cultureName in I18nHelper.Current.CultureNames)
        {
            lps.Add(
                new Line("lang", cultureName)
                {
                    new Sub(Id.Value, I18nDatas[cultureName].Name.Value),
                    new Sub(DescriptionId.Value, I18nDatas[cultureName].Description.Value),
                }
            );
        }
        Image.Value?.SaveToPng(Path.Combine(path, "icon.png"));
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
        {
            lps.Add(LPSConvert.SerializeObjectToLine<Line>(food.ToFood(), "food"));
            foreach (var cultureName in I18nHelper.Current.CultureNames)
            {
                SaveI18nDatas[cultureName]
                    .TryAdd(food.Id.Value, food.I18nDatas[cultureName].Name.Value);
                SaveI18nDatas[cultureName]
                    .TryAdd(
                        food.DescriptionId.Value,
                        food.I18nDatas[cultureName].Description.Value
                    );
            }
        }
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
        {
            lps.Add(LPSConvert.SerializeObjectToLine<Line>(text.ToSelectText(), "SelectText"));
            foreach (var cultureName in I18nHelper.Current.CultureNames)
            {
                SaveI18nDatas[cultureName]
                    .TryAdd(text.Id.Value, text.I18nDatas[cultureName].Text.Value);
                SaveI18nDatas[cultureName]
                    .TryAdd(text.ChooseId.Value, text.I18nDatas[cultureName].Choose.Value);
            }
        }
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
        {
            lps.Add(LPSConvert.SerializeObjectToLine<Line>(text.ToLowText(), "lowfoodtext"));
            foreach (var cultureName in I18nHelper.Current.CultureNames)
            {
                SaveI18nDatas[cultureName]
                    .TryAdd(text.Id.Value, text.I18nDatas[cultureName].Text.Value);
            }
        }
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
        {
            lps.Add(LPSConvert.SerializeObjectToLine<Line>(text.ToClickText(), "clicktext"));
            foreach (var cultureName in I18nHelper.Current.CultureNames)
            {
                SaveI18nDatas[cultureName]
                    .TryAdd(text.Id.Value, text.I18nDatas[cultureName].Text.Value);
            }
        }
        File.WriteAllText(textFile, lps.ToString());
    }
    #endregion
    /// <summary>
    /// 保存I18n数据
    /// </summary>
    /// <param name="path">路径</param>
    private void SaveI18nData(string path)
    {
        var langPath = Path.Combine(path, "lang");
        Directory.CreateDirectory(langPath);
        foreach (var cultureName in I18nHelper.Current.CultureNames)
        {
            var culturePath = Path.Combine(langPath, cultureName);
            Directory.CreateDirectory(culturePath);
            var cultureFile = Path.Combine(culturePath, $"{cultureName}.lps");
            File.Create(cultureFile).Close();
            var lps = new LPS();
            foreach (var data in SaveI18nDatas[cultureName])
                lps.Add(new Line(data.Key, data.Value));
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
                food.Image.Value.SaveToPng(Path.Combine(foodPath, food.Id.Value));
            }
        }
    }
    #endregion
    /// <summary>
    /// 关闭
    /// </summary>
    public void Close()
    {
        Image.Value.CloseStream();
        foreach (var food in Foods)
            food.Close();
        foreach (var pet in Pets)
            pet.Close();
        Current = null;
    }

    public void SaveTranslationMod(string path, IEnumerable<string> cultures)
    {
        // 保存模型信息
        SaveModInfo(path);
        // 保存文化数据
        var langPath = Path.Combine(path, "lang");
        Directory.CreateDirectory(langPath);
        foreach (var cultureName in cultures)
        {
            var culturePath = Path.Combine(langPath, cultureName);
            Directory.CreateDirectory(culturePath);
            var cultureFile = Path.Combine(culturePath, $"{cultureName}.lps");
            File.Create(cultureFile).Close();
            var lps = new LPS();
            foreach (var data in I18nEditWindow.Current.ViewModel.AllI18nDatas)
                lps.Add(
                    new Line(
                        data.Key,
                        data.Value.Datas[I18nHelper.Current.CultureNames.IndexOf(cultureName)].Value
                    )
                );
            File.WriteAllText(cultureFile, lps.ToString());
        }
    }
}

public class I18nModInfoModel
{
    public ObservableValue<string> Name { get; set; } = new();
    public ObservableValue<string> Description { get; set; } = new();
}
