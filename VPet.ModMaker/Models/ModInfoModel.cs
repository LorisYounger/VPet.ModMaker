using HKW.HKWViewModels.SimpleObservable;
using LinePutScript;
using LinePutScript.Converter;
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
    public static ModInfoModel Current { get; set; }

    public ObservableValue<string> Name { get; } = new();
    public ObservableValue<string> Description { get; } = new();
    public ObservableValue<string> Summary { get; } = new();
    public ObservableValue<string> Author { get; } = new();
    public ObservableValue<string> GameVersion { get; } = new();
    public ObservableValue<string> ModVersion { get; } = new();
    public ObservableValue<BitmapImage> Image { get; } = new();
    public ObservableCollection<FoodModel> Foods { get; } = new();
    public ObservableCollection<ClickTextModel> ClickTexts { get; } = new();
    public ObservableCollection<LowTextModel> LowTexts { get; } = new();
    public ObservableValue<string> SourcePath { get; } = new();

    public Dictionary<string, Dictionary<string, string>> OtherI18nDatas { get; } = new();

    public ModInfoModel() { }

    public ModInfoModel(ModLoader loader)
    {
        SourcePath.Value = loader.Path.FullName;
        Name.Value = loader.Name;
        Description.Value = loader.Intro;
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
        Summary.Value = GetSummary();
        foreach (var lang in loader.I18nDatas)
            I18nDatas.Add(lang.Key, lang.Value);
        OtherI18nDatas = loader.OtherI18nDatas;
    }

    public string GetSummary()
    {
        return $@"包含以下内容:
食物: {Foods.Count}
点击文本: {ClickTexts.Count}
低状态文本: {LowTexts.Count}";
    }

    public const string ModInfoFile = "info.lps";

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
            new Line("vupmod", Name.Value)
            {
                new Sub("author", Author.Value),
                new Sub("gamever", GameVersion.Value),
                new Sub("ver", ModVersion.Value)
            },
            new Line("intro", Description.Value),
            new Line("authorid", "0"),
            new Line("itemid", "0"),
            new Line("cachedate", DateTime.Now.Date.ToString())
        };
        foreach (var cultureName in I18nHelper.Current.CultureNames)
        {
            lps.Add(
                new Line("lang", cultureName)
                {
                    new Sub(Name.Value, I18nDatas[cultureName].Name.Value),
                    new Sub(Description.Value, I18nDatas[cultureName].Description.Value),
                }
            );
        }
        var imagePath = Utils.GetImageSourceFile(Image.Value);
        var targetImagePath = Path.Combine(path, Path.GetFileName(imagePath));
        if (imagePath != targetImagePath)
            File.Copy(imagePath, targetImagePath, true);
        //lps.FindLine("vupmod").Info = Name.Value;
        //lps.FindLine("intro").Info = Description.Value;
        //lps.FindSub("gamever").Info = GameVersion.Value;
        //lps.FindSub("ver").Info = ModVersion.Value;
        //lps.FindSub("author").Info = Author.Value;
        //lps.FindorAddLine("authorid").InfoToInt64 = 0;
        //lps.FindorAddLine("itemid").info = "0";
        File.WriteAllText(modInfoFile, lps.ToString());
        SaveFoods(path);
        SaveLang(path);
        SaveImage(path);
    }

    public void SaveFoods(string path)
    {
        if (Foods.Count == 0)
            return;
        var foodPath = Path.Combine(path, "food");
        Directory.CreateDirectory(foodPath);
        var foodFile = Path.Combine(foodPath, "food.lps");
        if (File.Exists(foodFile) is false)
            File.Create(foodFile).Close();
        var lps = new LPS();
        foreach (var food in Foods)
            lps.Add(LPSConvert.SerializeObjectToLine<Line>(food.ToFood(), "food"));
        File.WriteAllText(foodFile, lps.ToString());
    }

    public void SaveLang(string path)
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
                lps.Add(new Line(food.Name.Value, food.I18nDatas[cultureName].Name.Value));
                lps.Add(
                    new Line(food.Description.Value, food.I18nDatas[cultureName].Description.Value)
                );
            }
            if (lps.Count > 0)
                File.WriteAllText(cultureFile, lps.ToString());
        }
    }

    public void SaveImage(string path)
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
                var targetImagePath = Path.Combine(foodPath, Path.GetFileName(foodImagePath));
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
