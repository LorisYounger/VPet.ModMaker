using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using HKW.HKWUtils;
using HKW.HKWUtils.Extensions;
using HKW.Mapper;
using HKW.MVVM;
using HKW.MVVM.SourceGenerator;
using HKW.WPF;
using HKW.WPF.Extensions;
using Microsoft.Extensions.Logging;
using VPet_Simulator.Windows.Interface;
using VPet.ModMaker.ViewModels;

namespace VPet.ModMaker.Models;

/// <summary>
/// 食物模型
/// </summary>
[MapTarget(typeof(Food))]
[MapTarget(typeof(FoodModel))]
public sealed partial class FoodModel : ObservableModelBase, IDisposable
{
    /// <inheritdoc/>
    public FoodModel(ObservableI18nResource<string, string> i18nResource)
    {
        I18nResource = i18nResource;
    }

    /// <inheritdoc/>
    /// <param name="model">食物模型</param>
    public FoodModel(FoodModel model, ObservableI18nResource<string, string> i18nResource)
    {
        this.MapFrom(model);
        I18nResource = i18nResource;
        Image = model.Image;
    }

    /// <inheritdoc/>
    /// <param name="food">食物</param>
    /// <param name="i18nResource">I18n资源</param>
    public FoodModel(Food food, ObservableI18nResource<string, string> i18nResource)
    {
        this.MapFrom(food);
        I18nResource = i18nResource;
        if (
            string.Equals(food.Desc, DescriptionID) is false
            && I18nResource.RenameKey(food.Desc, DescriptionID) is false
        )
            I18nResource.SetData(DescriptionID, food.Desc);

        if (File.Exists(food.Image))
            Image = HKWImageUtils.LoadImageToMemory(food.Image);
        else
            this.LogIfEnabled(LogLevel.Warning)
                ?.Log("获取食物图像失败, 目标路径: '{path}'", food.Image);
    }

    /// <summary>
    /// 食物类型
    /// </summary>
    public static ImmutableArray<Food.FoodType> FoodTypes => EnumInfo<Food.FoodType>.StaticValues;

    /// <summary>
    /// ID
    /// </summary>
    [MapProperty(typeof(Food), nameof(Food.Name))]
    [ObservableProperty]
    public string ID { get; set; } = string.Empty;

    /// <summary>
    /// 详情Id
    /// </summary>
    [NotifyPropertyChangeFrom(nameof(ID))]
    [MapProperty(typeof(FoodModel), Ignore = true)]
    [MapProperty(typeof(Food), nameof(Food.Desc), Direction = MapDirections.To)]
    public string DescriptionID => $"{ID}_Description";

    /// <summary>
    /// I18n资源
    /// </summary>
    [MapIgnoreProperty]
    [ObservableProperty]
    public ObservableI18nResource<string, string> I18nResource { get; set; }

    partial void OnI18nResourceChanged(
        ObservableI18nResource<string, string> oldValue,
        ObservableI18nResource<string, string> newValue
    )
    {
        I18nObserver.Resource = newValue;
    }

    [MapIgnoreProperty]
    public I18nObserver<FoodModel, string, string> I18nObserver =>
        field ??= new(I18nResource, this, (s, p) => s.OnPropertyChanged(p))
        {
            new(s => s.ID, nameof(Name)),
            new(s => s.DescriptionID, nameof(Description)),
        };

    /// <summary>
    /// 名称
    /// </summary>
    [MapIgnoreProperty]
    public string Name
    {
        get => I18nObserver.GetDataOrDefault(ID);
        set => I18nObserver.SetData(ID, value);
    }

    /// <summary>
    /// 详情
    /// </summary>
    [MapIgnoreProperty]
    public string Description
    {
        get => I18nObserver.GetDataOrDefault(DescriptionID);
        set => I18nObserver.SetData(DescriptionID, value);
    }

    /// <summary>
    /// 指定动画
    /// </summary>
    [ObservableProperty]
    public string Graph { get; set; } = string.Empty;

    /// <summary>
    /// 类型
    /// </summary>
    [ObservableProperty]
    public Food.FoodType Type { get; set; }

    /// <summary>
    /// 体力
    /// </summary>
    [ObservableProperty]
    public double Strength { get; set; }

    /// <summary>
    /// 饱食度
    /// </summary>
    [ObservableProperty]
    public double StrengthFood { get; set; }

    /// <summary>
    /// 口渴度
    /// </summary>
    [ObservableProperty]
    public double StrengthDrink { get; set; }

    /// <summary>
    /// 心情
    /// </summary>
    [ObservableProperty]
    public double Feeling { get; set; }

    /// <summary>
    /// 健康度
    /// </summary>
    [ObservableProperty]
    public double Health { get; set; }

    /// <summary>
    /// 好感度
    /// </summary>
    [ObservableProperty]
    public double Likability { get; set; }

    /// <summary>
    /// 价格
    /// </summary>
    [ObservableProperty]
    public double Price { get; set; }

    /// <summary>
    /// 经验
    /// </summary>
    [ObservableProperty]
    public int Exp { get; set; }

    /// <summary>
    /// 图片
    /// </summary>
    [MapIgnoreProperty]
    [ObservableProperty]
    public BitmapImage? Image { get; set; }

    /// <summary>
    /// 推荐价格
    /// </summary>
    [MapIgnoreProperty]
    [NotifyPropertyChangeFrom(
        nameof(Strength),
        nameof(StrengthFood),
        nameof(StrengthDrink),
        nameof(Feeling),
        nameof(Health),
        nameof(Likability),
        nameof(Exp)
    )]
    public double ReferencePrice
    {
        get
        {
            this.MapTo(_food);
            var price = Math.Floor(_food.RealPrice);
            if (AutoSetFoodPrice)
                Price = price;
            return price;
        }
    }

    [MapIgnoreProperty]
    public bool AutoSetFoodPrice { get; set; }
    private static readonly Food _food = new() { Image = string.Empty };

    public void Dispose()
    {
        I18nObserver?.Dispose();
    }
}
