using HKW.HKWViewModels.SimpleObservable;
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

    public ObservableValue<string> Id { get; } = new();

    public ObservableValue<string> Summary { get; } = new();
    public ObservableValue<string> Author { get; } = new();
    public ObservableValue<string> GameVersion { get; } = new();
    public ObservableValue<string> ModVersion { get; } = new();
    public ObservableValue<BitmapImage> ModImage { get; } = new();
    public ObservableCollection<FoodModel> Foods { get; } = new();
    public ObservableCollection<ClickTextModel> ClickTexts { get; } = new();
    public ObservableCollection<LowTextModel> LowTexts { get; } = new();
    public ObservableValue<string> SourcePath { get; } = new();

    public Dictionary<string, Dictionary<string, string>> OtherI18nDatas { get; } = new();

    public ModInfoModel() { }

    public ModInfoModel(ModLoader loader)
    {
        SourcePath.Value = loader.Path.FullName;
        Id.Value = loader.Name;
        var imagePath = Path.Combine(loader.Path.FullName, "icon.png");
        if (File.Exists(imagePath))
            ModImage.Value = Utils.LoadImageToStream(imagePath);
        Author.Value = loader.Author;
        GameVersion.Value = loader.GameVer.ToString();
        ModVersion.Value = loader.Ver.ToString();
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
}

public class I18nModInfoModel
{
    public ObservableValue<string> Name { get; set; } = new();
    public ObservableValue<string> Description { get; set; } = new();
}
