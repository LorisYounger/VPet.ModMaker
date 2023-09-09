using HKW.HKWViewModels.SimpleObservable;
using LinePutScript;
using LinePutScript.Converter;
using LinePutScript.Localization.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.Models;

public class ModInfoModel : I18nModel<I18nModInfoModel>
{
    public const string ModInfoFile = "info.lps";
    public static ModInfoModel Current { get; set; } = new();

    public ObservableValue<string> Id { get; } = new();
    public ObservableValue<string> DescriptionId { get; } = new();
    public ObservableValue<string> Author { get; } = new();
    public ObservableValue<string> GameVersion { get; } = new();
    public ObservableValue<string> ModVersion { get; } = new();
    public ObservableValue<BitmapImage> Image { get; } = new();
    public ObservableValue<string> SourcePath { get; } = new();

    public ObservableCollection<FoodModel> Foods { get; } = new();
    public ObservableCollection<ClickTextModel> ClickTexts { get; } = new();
    public ObservableCollection<LowTextModel> LowTexts { get; } = new();
    public ObservableCollection<SelectTextModel> SelectTexts { get; } = new();
    public ObservableCollection<PetModel> Pets { get; } = new();

    public Dictionary<string, Dictionary<string, string>> OtherI18nDatas { get; } = new();

    public ModInfoModel()
    {
        DescriptionId.Value = $"{Id.Value}_{nameof(DescriptionId)}";
        Id.ValueChanged += (o, n) =>
        {
            DescriptionId.Value = $"{n}_{nameof(DescriptionId)}";
        };
    }

    public ModInfoModel(ModLoader loader)
        : this()
    {
        SourcePath.Value = loader.Path.FullName;
        Id.Value = loader.Name;
        DescriptionId.Value = loader.Intro;
        Author.Value = loader.Author;
        GameVersion.Value = loader.GameVer.ToString();
        ModVersion.Value = loader.Ver.ToString();
        var imagePath = Path.Combine(loader.Path.FullName, "icon.png");
        if (File.Exists(imagePath))
            Image.Value = Utils.LoadImageToStream(imagePath);
        foreach (var food in loader.Foods)
            Foods.Add(new(food));
        foreach (var clickText in loader.ClickTexts)
            ClickTexts.Add(new(clickText));
        foreach (var lowText in loader.LowTexts)
            LowTexts.Add(new(lowText));
        foreach (var selectText in loader.SelectTexts)
            SelectTexts.Add(new(selectText));
        foreach (var pet in loader.Pets)
            Pets.Add(new(pet));

        foreach (var lang in loader.I18nDatas)
            I18nDatas.Add(lang.Key, lang.Value);
        OtherI18nDatas = loader.OtherI18nDatas;

        LoadI18nData();
    }

    private void LoadI18nData()
    {
        foreach (var lang in I18nDatas)
        {
            if (I18nHelper.Current.CultureNames.Contains(lang.Key) is false)
                I18nHelper.Current.CultureNames.Add(lang.Key);
        }
        if (I18nHelper.Current.CultureNames.Count > 0)
        {
            I18nHelper.Current.CultureName.Value = I18nHelper.Current.CultureNames.First();
            foreach (var i18nData in OtherI18nDatas)
            {
                foreach (var food in Foods)
                {
                    var foodI18n = food.I18nDatas[i18nData.Key];
                    if (i18nData.Value.TryGetValue(food.Id.Value, out var name))
                        foodI18n.Name.Value = name;
                    if (i18nData.Value.TryGetValue(food.DescriptionId.Value, out var description))
                        foodI18n.Description.Value = description;
                }
                foreach (var lowText in LowTexts)
                {
                    var lowTextI18n = lowText.I18nDatas[i18nData.Key];
                    if (i18nData.Value.TryGetValue(lowText.Id.Value, out var text))
                        lowTextI18n.Text.Value = text;
                }
                foreach (var clickText in ClickTexts)
                {
                    var clickTextI18n = clickText.I18nDatas[i18nData.Key];
                    if (i18nData.Value.TryGetValue(clickText.Id.Value, out var text))
                        clickTextI18n.Text.Value = text;
                }
                foreach (var selectText in SelectTexts)
                {
                    var selectTextI18n = selectText.I18nDatas[i18nData.Key];
                    if (i18nData.Value.TryGetValue(selectText.Id.Value, out var text))
                        selectTextI18n.Text.Value = text;
                    if (i18nData.Value.TryGetValue(selectText.ChooseId.Value, out var choose))
                        selectTextI18n.Choose.Value = choose;
                }
                foreach (var pet in Pets)
                {
                    var petI18n = pet.I18nDatas[i18nData.Key];
                    if (i18nData.Value.TryGetValue(pet.Id.Value, out var name))
                        petI18n.Name.Value = name;
                    if (i18nData.Value.TryGetValue(pet.PetNameId.Value, out var petName))
                        petI18n.PetName.Value = petName;
                    if (i18nData.Value.TryGetValue(pet.DescriptionId.Value, out var description))
                        petI18n.Description.Value = description;
                    foreach (var work in pet.Works)
                    {
                        var workI18n = work.I18nDatas[i18nData.Key];
                        if (i18nData.Value.TryGetValue(work.Id.Value, out var workName))
                            workI18n.Name.Value = workName;
                    }
                }
            }
        }
    }

    public void Save()
    {
        SaveTo(SourcePath.Value);
    }

    public void SaveTo(string path)
    {
        var modInfoFile = Path.Combine(path, ModInfoFile);
        if (File.Exists(modInfoFile) is false)
            File.Create(modInfoFile).Close();
        //var lps = new LpsDocument(File.ReadAllText(modInfoFile));
        var lps = new LPS()
        {
            new Line("vupmod", Id.Value)
            {
                new Sub("author", Author.Value),
                new Sub("gamever", GameVersion.Value),
                new Sub("ver", ModVersion.Value)
            },
            new Line("intro", DescriptionId.Value),
            new Line("authorid", "0"),
            new Line("itemid", "0"),
            new Line("cachedate", DateTime.Now.Date.ToString())
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
        var imagePath = Utils.GetImageSourceFile(Image.Value);
        var targetImagePath = Path.Combine(path, Path.GetFileName(imagePath));
        if (imagePath != targetImagePath)
            File.Copy(imagePath, targetImagePath, true);
        //lps.FindLine("vupmod").Info = Id.Value;
        //lps.FindLine("intro").Info = DescriptionId.Value;
        //lps.FindSub("gamever").Info = GameVersion.Value;
        //lps.FindSub("ver").Info = ModVersion.Value;
        //lps.FindSub("author").Info = Author.Value;
        //lps.FindorAddLine("authorid").InfoToInt64 = 0;
        //lps.FindorAddLine("itemid").info = "0";
        File.WriteAllText(modInfoFile, lps.ToString());
        SaveFoods(path);
        SaveText(path);
        SaveLang(path);
        SaveImage(path);
    }

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
        }
        File.WriteAllText(textFile, lps.ToString());
    }

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
        }
        File.WriteAllText(textFile, lps.ToString());
    }

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
        }
        File.WriteAllText(textFile, lps.ToString());
    }

    private void SaveLang(string path)
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
            foreach (var food in Foods)
            {
                lps.Add(new Line(food.Id.Value, food.I18nDatas[cultureName].Name.Value));
                lps.Add(
                    new Line(
                        food.DescriptionId.Value,
                        food.I18nDatas[cultureName].Description.Value
                    )
                );
            }
            foreach (var text in LowTexts)
            {
                lps.Add(new Line(text.Id.Value, text.I18nDatas[cultureName].Text.Value));
            }
            foreach (var text in ClickTexts)
            {
                lps.Add(new Line(text.Id.Value, text.I18nDatas[cultureName].Text.Value));
            }
            foreach (var text in SelectTexts)
            {
                lps.Add(new Line(text.Id.Value, text.I18nDatas[cultureName].Text.Value));
                lps.Add(new Line(text.ChooseId.Value, text.I18nDatas[cultureName].Choose.Value));
            }
            File.WriteAllText(cultureFile, lps.ToString());
        }
    }

    private void SaveImage(string path)
    {
        var imagePath = Path.Combine(path, "image");
        Directory.CreateDirectory(imagePath);
        if (Foods.Count > 0)
        {
            var foodPath = Path.Combine(imagePath, "food");
            Directory.CreateDirectory(foodPath);
            foreach (var food in Foods)
            {
                var foodImagePath = Utils.GetImageSourceFile(food.Image.Value);
                var targetImagePath = Path.Combine(
                    foodPath,
                    $"{food.Id.Value}{Path.GetExtension(foodImagePath)}"
                );
                if (foodImagePath != targetImagePath)
                    File.Copy(foodImagePath, targetImagePath, true);
            }
        }
    }
}

public class I18nModInfoModel
{
    public ObservableValue<string> Name { get; set; } = new();
    public ObservableValue<string> Description { get; set; } = new();
}
