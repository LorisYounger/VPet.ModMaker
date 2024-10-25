using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using HKW.HKWReactiveUI;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using HKW.WPF.Extensions;
using LinePutScript;
using LinePutScript.Converter;
using Mapster;
using VPet.ModMaker.Native;
using VPet.ModMaker.ViewModels;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.Models;

/// <summary>
/// 食物模型
/// </summary>
public partial class FoodModel : ViewModelBase
{
    public FoodModel() { }

    public FoodModel(FoodModel model)
        : this()
    {
        model.Adapt(this);
        Image = model.Image?.CloneStream();
    }

    public FoodModel(Food food)
        : this()
    {
        food.Adapt(this);
        if (File.Exists(food.Image))
            Image = NativeUtils.LoadImageToMemoryStream(food.Image);
    }

    /// <summary>
    /// 食物类型
    /// </summary>
    public static FrozenSet<Food.FoodType> FoodTypes => EnumInfo<Food.FoodType>.Values;

    /// <summary>
    /// ID
    /// </summary>
    [AdaptMember(nameof(Food.Name))]
    [ReactiveProperty]
    public string ID { get; set; } = string.Empty;

    partial void OnIDChanged(string oldValue, string newValue)
    {
        DescriptionID = $"{ID}_{nameof(Description)}";
    }

    /// <summary>
    /// 详情Id
    /// </summary>
    [AdaptMember(nameof(Food.Desc))]
    [ReactiveProperty]
    public string DescriptionID { get; set; } = string.Empty;

    [AdaptIgnore]
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

    [NotifyPropertyChangeFrom("")]
    public I18nObject<string, string> I18nObject => new(this);

    [AdaptIgnore]
    [ReactiveI18nProperty("I18nResource", nameof(I18nObject), nameof(ID), true)]
    public string Name
    {
        get => I18nResource.GetCurrentCultureDataOrDefault(ID);
        set => I18nResource.SetCurrentCultureData(ID, value);
    }

    [AdaptIgnore]
    [ReactiveI18nProperty("I18nResource", nameof(I18nObject), nameof(DescriptionID), true)]
    public string Description
    {
        get => I18nResource.GetCurrentCultureDataOrDefault(DescriptionID);
        set => I18nResource.SetCurrentCultureData(DescriptionID, value);
    }

    /// <summary>
    /// 指定动画
    /// </summary>
    [AdaptMember(nameof(Food.Graph))]
    [ReactiveProperty]
    public string Graph { get; set; } = string.Empty;

    /// <summary>
    /// 类型
    /// </summary>
    [AdaptMember(nameof(Food.Type))]
    [ReactiveProperty]
    public Food.FoodType Type { get; set; }

    /// <summary>
    /// 体力
    /// </summary>
    [AdaptMember(nameof(Food.Strength))]
    [ReactiveProperty]
    public double Strength { get; set; }

    /// <summary>
    /// 饱食度
    /// </summary>
    [AdaptMember(nameof(Food.StrengthFood))]
    [ReactiveProperty]
    public double StrengthFood { get; set; }

    /// <summary>
    /// 口渴度
    /// </summary>
    [AdaptMember(nameof(Food.StrengthDrink))]
    [ReactiveProperty]
    public double StrengthDrink { get; set; }

    /// <summary>
    /// 心情
    /// </summary>
    [AdaptMember(nameof(Food.Feeling))]
    [ReactiveProperty]
    public double Feeling { get; set; }

    /// <summary>
    /// 健康度
    /// </summary>
    [AdaptMember(nameof(Food.Health))]
    [ReactiveProperty]
    public double Health { get; set; }

    /// <summary>
    /// 好感度
    /// </summary>
    [AdaptMember(nameof(Food.Likability))]
    [ReactiveProperty]
    public double Likability { get; set; }

    /// <summary>
    /// 价格
    /// </summary>
    [AdaptMember(nameof(Food.Price))]
    [ReactiveProperty]
    public double Price { get; set; }

    /// <summary>
    /// 经验
    /// </summary>
    [AdaptMember(nameof(Food.Exp))]
    [ReactiveProperty]
    public int Exp { get; set; }

    /// <summary>
    /// 图片
    /// </summary>
    [AdaptIgnore]
    [ReactiveProperty]
    public BitmapImage? Image { get; set; }

    /// <summary>
    /// 推荐价格
    /// </summary>
    [AdaptIgnore]
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
            x.Adapt(_food);
            return Math.Floor(_food.RealPrice);
        });

    private static readonly Food _food = new();

    public Food ToFood()
    {
        return this.Adapt<Food>();
        //return new Food()
        //{
        //    Name = ID,
        //    Desc = DescriptionID,
        //    Graph = Graph,
        //    Type = Type,
        //    Strength = Strength,
        //    StrengthFood = StrengthFood,
        //    StrengthDrink = StrengthDrink,
        //    Feeling = Feeling,
        //    Health = Health,
        //    Likability = Likability,
        //    Price = Price,
        //    Exp = Exp,
        //};
    }

    public void RefreshID()
    {
        DescriptionID = $"{ID}_{nameof(DescriptionID)}";
    }

    public void Close()
    {
        Image?.CloseStream();
        I18nResource.I18nObjects.Remove(I18nObject);
    }
}
