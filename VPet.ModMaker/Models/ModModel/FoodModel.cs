using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using HKW.HKWMapper;
using HKW.HKWReactiveUI;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using HKW.WPF;
using HKW.WPF.Extensions;
using LinePutScript;
using LinePutScript.Converter;
using Splat;
using VPet.ModMaker.Native;
using VPet.ModMaker.ViewModels;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.Models;

/// <summary>
/// 食物模型
/// </summary>
[MapTo(typeof(Food))]
[MapFrom(typeof(Food))]
[MapFrom(typeof(FoodModel))]
public partial class FoodModel : ViewModelBase
{
    /// <inheritdoc/>
    public FoodModel() { }

    /// <inheritdoc/>
    /// <param name="model">食物模型</param>
    public FoodModel(FoodModel model)
    {
        this.MapFromFoodModel(model);
        Image = model.Image?.AddReferenceCount();
    }

    /// <inheritdoc/>
    /// <param name="food">食物</param>
    /// <param name="i18nResource">I18n资源</param>
    [SetsRequiredMembers]
    public FoodModel(Food food, I18nResource<string, string> i18nResource)
    {
        this.MapFromFood(food);
        if (food.Desc != DescriptionID)
            i18nResource.ReplaceCultureDataKey(food.Desc, DescriptionID);
        I18nResource = i18nResource;
        if (File.Exists(food.Image))
            Image = HKWImageUtils.LoadImageToMemory(food.Image, this);
        else
            this.Log().Warn("获取食物图像失败, 目标路径: {path}", food.Image);
    }

    /// <summary>
    /// 食物类型
    /// </summary>
    public static FrozenSet<Food.FoodType> FoodTypes => EnumInfo<Food.FoodType>.Values;

    /// <summary>
    /// ID
    /// </summary>
    [FoodModelMapToFoodProperty(nameof(Food.Name))]
    [FoodModelMapFromFoodProperty(nameof(Food.Name))]
    [ReactiveProperty]
    public string ID { get; set; } = string.Empty;

    /// <summary>
    /// 详情Id
    /// </summary>
    [FoodModelMapToFoodProperty(nameof(Food.Desc))]
    [NotifyPropertyChangeFrom(nameof(ID))]
    public string DescriptionID => $"{ID}_Description";

    /// <summary>
    /// I18n资源
    /// </summary>
    [MapIgnoreProperty]
    [ReactiveProperty]
    public required I18nResource<string, string> I18nResource { get; set; }

    partial void OnI18nResourceChanged(
        I18nResource<string, string> oldValue,
        I18nResource<string, string> newValue
    )
    {
        oldValue?.I18nObjects.Remove(I18nObject);
        newValue?.I18nObjects.Add(I18nObject);
    }

    /// <summary>
    /// I18n对象
    /// </summary>
    [NotifyPropertyChangeFrom("")]
    public I18nObject<string, string> I18nObject => new(this);

    /// <summary>
    /// 名称
    /// </summary>
    [MapIgnoreProperty]
    [ReactiveI18nProperty("I18nResource", nameof(I18nObject), nameof(ID), true)]
    public string Name
    {
        get => I18nResource.GetCurrentCultureDataOrDefault(ID);
        set => I18nResource.SetCurrentCultureData(ID, value);
    }

    /// <summary>
    /// 详情
    /// </summary>
    [MapIgnoreProperty]
    [ReactiveI18nProperty("I18nResource", nameof(I18nObject), nameof(DescriptionID), true)]
    public string Description
    {
        get => I18nResource.GetCurrentCultureDataOrDefault(DescriptionID);
        set => I18nResource.SetCurrentCultureData(DescriptionID, value);
    }

    /// <summary>
    /// 指定动画
    /// </summary>
    [FoodModelMapToFoodProperty(nameof(Food.Graph))]
    [FoodModelMapFromFoodProperty(nameof(Food.Graph))]
    [ReactiveProperty]
    public string Graph { get; set; } = string.Empty;

    /// <summary>
    /// 类型
    /// </summary>
    [FoodModelMapToFoodProperty(nameof(Food.Type))]
    [FoodModelMapFromFoodProperty(nameof(Food.Type))]
    [ReactiveProperty]
    public Food.FoodType Type { get; set; }

    /// <summary>
    /// 体力
    /// </summary>
    [FoodModelMapToFoodProperty(nameof(Food.Strength))]
    [FoodModelMapFromFoodProperty(nameof(Food.Strength))]
    [ReactiveProperty]
    public double Strength { get; set; }

    /// <summary>
    /// 饱食度
    /// </summary>
    [FoodModelMapToFoodProperty(nameof(Food.StrengthFood))]
    [FoodModelMapFromFoodProperty(nameof(Food.StrengthFood))]
    [ReactiveProperty]
    public double StrengthFood { get; set; }

    /// <summary>
    /// 口渴度
    /// </summary>
    [FoodModelMapToFoodProperty(nameof(Food.StrengthDrink))]
    [FoodModelMapFromFoodProperty(nameof(Food.StrengthDrink))]
    [ReactiveProperty]
    public double StrengthDrink { get; set; }

    /// <summary>
    /// 心情
    /// </summary>
    [FoodModelMapToFoodProperty(nameof(Food.Feeling))]
    [FoodModelMapFromFoodProperty(nameof(Food.Feeling))]
    [ReactiveProperty]
    public double Feeling { get; set; }

    /// <summary>
    /// 健康度
    /// </summary>
    [FoodModelMapToFoodProperty(nameof(Food.Health))]
    [FoodModelMapFromFoodProperty(nameof(Food.Health))]
    [ReactiveProperty]
    public double Health { get; set; }

    /// <summary>
    /// 好感度
    /// </summary>
    [FoodModelMapToFoodProperty(nameof(Food.Likability))]
    [FoodModelMapFromFoodProperty(nameof(Food.Likability))]
    [ReactiveProperty]
    public double Likability { get; set; }

    /// <summary>
    /// 价格
    /// </summary>
    [FoodModelMapToFoodProperty(nameof(Food.Price))]
    [FoodModelMapFromFoodProperty(nameof(Food.Price))]
    [ReactiveProperty]
    public double Price { get; set; }

    /// <summary>
    /// 经验
    /// </summary>
    [FoodModelMapToFoodProperty(nameof(Food.Exp))]
    [FoodModelMapFromFoodProperty(nameof(Food.Exp))]
    [ReactiveProperty]
    public int Exp { get; set; }

    /// <summary>
    /// 图片
    /// </summary>
    [MapIgnoreProperty]
    [ReactiveProperty]
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
    public double ReferencePrice =>
        this.To(x =>
        {
            x.MapToFood(_food);
            return Math.Floor(_food.RealPrice);
        });

    private static readonly Food _food = new() { Image = string.Empty };

    /// <summary>
    /// 关闭
    /// </summary>
    public void Close()
    {
        Image?.CloseStreamWhenNoReference();
        I18nResource.I18nObjects.Remove(I18nObject);
        I18nObject.Close();
    }
}
