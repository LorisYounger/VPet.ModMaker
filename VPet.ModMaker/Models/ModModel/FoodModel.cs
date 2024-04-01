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
using CommunityToolkit.Mvvm.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using HKW.HKWUtils.Observable;
using LinePutScript;
using LinePutScript.Converter;
using Mapster;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.Models;

/// <summary>
/// 食物模型
/// </summary>
public class FoodModel : I18nModel<I18nFoodModel>
{
    public FoodModel()
    {
        PropertyChangedX += FoodModel_PropertyChangedX;
    }

    private static FrozenSet<string> _notifyReferencePrice = FrozenSet.ToFrozenSet(
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

    private void FoodModel_PropertyChangedX(object? sender, PropertyChangedXEventArgs e)
    {
        if (e.PropertyName is not null && _notifyReferencePrice.Contains(e.PropertyName))
        {
            this.Adapt(_food);
            ReferencePrice = Math.Floor(_food.RealPrice);
        }
    }

    public FoodModel(FoodModel model)
        : this()
    {
        model.Adapt(this);
        Image = model.Image?.CloneStream();
        foreach (var item in model.I18nDatas)
            I18nDatas[item.Key] = item.Value.Clone();
        CurrentI18nData = I18nDatas[I18nHelper.Current.CultureName];
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

    #region ID
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _id = string.Empty;

    /// <summary>
    /// ID
    /// </summary>
    [AdaptMember(nameof(Food.Name))]
    public string ID
    {
        get => _id;
        set
        {
            if (SetProperty(ref _id, value) is false)
                return;
            DescriptionID = $"{ID}_{nameof(DescriptionID)}";
        }
    }
    #endregion

    #region DescriptionID
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _descriptionID = string.Empty;

    /// <summary>
    /// 详情Id
    /// </summary>
    [AdaptMember(nameof(Food.Desc))]
    public string DescriptionID
    {
        get => _descriptionID;
        set => SetProperty(ref _descriptionID, value);
    }
    #endregion

    #region Graph
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _graph = string.Empty;

    /// <summary>
    /// 指定动画
    /// </summary>
    [AdaptMember(nameof(Food.Graph))]
    public string Graph
    {
        get => _graph;
        set => SetProperty(ref _graph, value);
    }
    #endregion

    #region Type
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private Food.FoodType _type;

    /// <summary>
    /// 类型
    /// </summary>
    [AdaptMember(nameof(Food.Type))]
    public Food.FoodType Type
    {
        get => _type;
        set => SetProperty(ref _type, value);
    }
    #endregion

    #region Strength
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private double _strength;

    /// <summary>
    /// 体力
    /// </summary>
    [AdaptMember(nameof(Food.Strength))]
    public double Strength
    {
        get => _strength;
        set => SetProperty(ref _strength, value);
    }
    #endregion

    #region StrengthFood
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private double _strengthFood;

    /// <summary>
    /// 饱食度
    /// </summary>
    [AdaptMember(nameof(Food.StrengthFood))]
    public double StrengthFood
    {
        get => _strengthFood;
        set => SetProperty(ref _strengthFood, value);
    }
    #endregion

    #region StrengthDrink
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private double _strengthDrink;

    /// <summary>
    /// 口渴度
    /// </summary>
    [AdaptMember(nameof(Food.StrengthDrink))]
    public double StrengthDrink
    {
        get => _strengthDrink;
        set => SetProperty(ref _strengthDrink, value);
    }
    #endregion

    #region Feeling
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private double _feeling;

    /// <summary>
    /// 心情
    /// </summary>
    [AdaptMember(nameof(Food.Feeling))]
    public double Feeling
    {
        get => _feeling;
        set => SetProperty(ref _feeling, value);
    }
    #endregion

    #region Health
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private double _health;

    /// <summary>
    /// 健康度
    /// </summary>
    [AdaptMember(nameof(Food.Health))]
    public double Health
    {
        get => _health;
        set => SetProperty(ref _health, value);
    }
    #endregion

    #region Likability
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private double _likability;

    /// <summary>
    /// 好感度
    /// </summary>
    [AdaptMember(nameof(Food.Likability))]
    public double Likability
    {
        get => _likability;
        set => SetProperty(ref _likability, value);
    }
    #endregion

    #region Price
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private double _price;

    /// <summary>
    /// 价格
    /// </summary>
    [AdaptMember(nameof(Food.Price))]
    public double Price
    {
        get => _price;
        set => SetProperty(ref _price, value);
    }
    #endregion

    #region Exp
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private int _exp;

    /// <summary>
    /// 经验
    /// </summary>
    [AdaptMember(nameof(Food.Exp))]
    public int Exp
    {
        get => _exp;
        set => SetProperty(ref _exp, value);
    }
    #endregion

    #region Image
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private BitmapImage? _image;

    /// <summary>
    /// 图片
    /// </summary>
    [AdaptIgnore]
    public BitmapImage? Image
    {
        get => _image;
        set => SetProperty(ref _image, value);
    }
    #endregion

    #region ReferencePrice
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private double _ReferencePrice;

    /// <summary>
    /// 推荐价格
    /// </summary>
    [AdaptIgnore]
    public double ReferencePrice
    {
        get => _ReferencePrice;
        set => SetProperty(ref _ReferencePrice, value);
    }
    #endregion

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

    public void RefreshId()
    {
        DescriptionID = $"{ID}_{nameof(DescriptionID)}";
    }

    public void Close()
    {
        Image?.CloseStream();
    }
}

public class I18nFoodModel : ObservableObjectX, ICloneable<I18nFoodModel>
{
    #region Name
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _name = string.Empty;

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }
    #endregion

    #region Description
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _description = string.Empty;

    public string Description
    {
        get => _description;
        set => SetProperty(ref _description, value);
    }
    #endregion

    public I18nFoodModel Clone() => this.Adapt<I18nFoodModel>();

    object ICloneable.Clone() => Clone();
}
