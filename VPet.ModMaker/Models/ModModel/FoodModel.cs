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
using HKW.HKWUtils.Observable;
using LinePutScript;
using LinePutScript.Converter;
using Mapster;
using VPet.ModMaker.ViewModels;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.Models;

/// <summary>
/// 食物模型
/// </summary>
public partial class FoodModel : ViewModelBase
{
    public FoodModel()
    {
        //PropertyChangedX += FoodModel_PropertyChangedX;
    }

    private static readonly FrozenSet<string> _notifyReferencePrice = FrozenSet.ToFrozenSet(
        [
            nameof(Strength),
            nameof(StrengthFood),
            nameof(StrengthDrink),
            nameof(Feeling),
            nameof(Health),
            nameof(Likability),
            nameof(Exp)
        ]
    );

    //TODO:
    //private void FoodModel_PropertyChangedX(object? sender, PropertyChangedXEventArgs e)
    //{
    //    if (e.PropertyName is not null && _notifyReferencePrice.Contains(e.PropertyName))
    //    {
    //        this.Adapt(_food);
    //        ReferencePrice = Math.Floor(_food.RealPrice);
    //    }
    //}

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
    public static ObservableList<Food.FoodType> FoodTypes { get; } =
        new(Enum.GetValues(typeof(Food.FoodType)).Cast<Food.FoodType>());

    /// <summary>
    /// ID
    /// </summary>
    [AdaptMember(nameof(Food.Name))]
    [ReactiveProperty]
    public string ID { get; set; } = string.Empty;

    partial void OnIDChanged(string oldValue, string newValue)
    {
        DescriptionID = $"{ID}_{nameof(DescriptionID)}";
    }

    /// <summary>
    /// 详情Id
    /// </summary>
    [AdaptMember(nameof(Food.Desc))]
    [ReactiveProperty]
    public string DescriptionID { get; set; } = string.Empty;

    #region I18nData

    [AdaptIgnore]
    private I18nResource<string, string> _i18nResource = null!;

    [AdaptIgnore]
    public required I18nResource<string, string> I18nResource
    {
        get => _i18nResource;
        set
        {
            // TODO:
            //if (_i18nResource is not null)
            //    I18nResource.I18nObjectInfos.Remove(this);
            //_i18nResource = value;
            //InitializeI18nResource();
        }
    }

    public void InitializeI18nResource()
    {
        // TODO:
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
    }

    [AdaptIgnore]
    public string Name
    {
        get => I18nResource.GetCurrentCultureDataOrDefault(ID);
        set => I18nResource.SetCurrentCultureData(ID, value);
    }

    [AdaptIgnore]
    public string Description
    {
        get => I18nResource.GetCurrentCultureDataOrDefault(DescriptionID);
        set => I18nResource.SetCurrentCultureData(DescriptionID, value);
    }
    #endregion


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
    [ReactiveProperty]
    public double ReferencePrice { get; set; }

    private readonly Food _food = new();

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
        //TODO:
        //I18nResource.I18nObjectInfos.Remove(this);
    }
}
