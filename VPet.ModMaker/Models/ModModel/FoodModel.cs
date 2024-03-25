using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using HKW.HKWUtils.Observable;
using LinePutScript;
using LinePutScript.Converter;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.Models;

/// <summary>
/// 食物模型
/// </summary>
public class FoodModel : I18nModel<I18nFoodModel>
{
    /// <summary>
    /// 食物类型
    /// </summary>
    public static ObservableCollection<Food.FoodType> FoodTypes { get; } =
        new(Enum.GetValues(typeof(Food.FoodType)).Cast<Food.FoodType>());

    #region Id
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _id = string.Empty;

    /// <summary>
    /// Id
    /// </summary>
    public string Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }
    #endregion

    #region DescriptionId
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _descriptionId = string.Empty;

    /// <summary>
    /// 详情Id
    /// </summary>
    public string DescriptionId
    {
        get => _descriptionId;
        set => SetProperty(ref _descriptionId, value);
    }
    #endregion

    #region Graph
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _graph = string.Empty;

    /// <summary>
    /// 指定动画
    /// </summary>
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
    public Food.FoodType Type
    {
        get => _type;
        set => SetProperty(ref _type, value);
    }
    #endregion

    /// <summary>
    /// 体力
    /// </summary>
    #region Strength
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private double _Strength;

    public double Strength
    {
        get => _Strength;
        set => SetProperty(ref _Strength, value);
    }
    #endregion

    /// <summary>
    /// 饱食度
    /// </summary>
    #region StrengthFood
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private double _StrengthFood;

    public double StrengthFood
    {
        get => _StrengthFood;
        set => SetProperty(ref _StrengthFood, value);
    }
    #endregion

    /// <summary>
    /// 口渴度
    /// </summary>
    #region StrengthDrink
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private double _StrengthDrink;

    public double StrengthDrink
    {
        get => _StrengthDrink;
        set => SetProperty(ref _StrengthDrink, value);
    }
    #endregion

    /// <summary>
    /// 心情
    /// </summary>
    #region Feeling
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private double _Feeling;

    public double Feeling
    {
        get => _Feeling;
        set => SetProperty(ref _Feeling, value);
    }
    #endregion

    /// <summary>
    /// 健康度
    /// </summary>
    #region Health
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private double _Health;

    public double Health
    {
        get => _Health;
        set => SetProperty(ref _Health, value);
    }
    #endregion

    /// <summary>
    /// 好感度
    /// </summary>
    #region Likability
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private double _Likability;

    public double Likability
    {
        get => _Likability;
        set => SetProperty(ref _Likability, value);
    }
    #endregion

    /// <summary>
    /// 价格
    /// </summary>
    #region Price
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private double _Price;

    public double Price
    {
        get => _Price;
        set => SetProperty(ref _Price, value);
    }
    #endregion

    /// <summary>
    /// 经验
    /// </summary>
    #region Exp
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private int _Exp;

    public int Exp
    {
        get => _Exp;
        set => SetProperty(ref _Exp, value);
    }
    #endregion

    /// <summary>
    /// 图片
    /// </summary>
    #region Image
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private BitmapImage _Image;

    public BitmapImage Image
    {
        get => _Image;
        set => SetProperty(ref _Image, value);
    }
    #endregion

    #region ReferencePrice
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private double _ReferencePrice;

    public double ReferencePrice
    {
        get => _ReferencePrice;
        set => SetProperty(ref _ReferencePrice, value);
    }
    #endregion

    private readonly Food _food = new();

    public FoodModel()
    {
        DescriptionId = $"{Id}_{nameof(DescriptionId)}";
        //TODO
        //Id.ValueChanged += (s, e) =>
        //{
        //    DescriptionId.Value = $"{e.NewValue}_{nameof(DescriptionId)}";
        //};
        //ReferencePrice.AddNotifySender(
        //    Strength,
        //    StrengthFood,
        //    StrengthDrink,
        //    Feeling,
        //    Health,
        //    Likability,
        //    Exp
        //);
        //ReferencePrice.SenderPropertyChanged += (s, _) =>
        //{
        //    s.Value = Math.Floor(SetValueToFood(_food).RealPrice);
        //};
    }

    public FoodModel(FoodModel model)
        : this()
    {
        Id = model.Id;
        DescriptionId = model.DescriptionId;
        Graph = model.Graph;
        Type = model.Type;
        Strength = model.Strength;
        StrengthFood = model.StrengthFood;
        StrengthDrink = model.StrengthDrink;
        Feeling = model.Feeling;
        Health = model.Health;
        Likability = model.Likability;
        Price = model.Price;
        Exp = model.Exp;
        Image = model.Image.Copy();
        foreach (var item in model.I18nDatas)
            I18nDatas[item.Key] = item.Value.Copy();
        CurrentI18nData = I18nDatas[I18nHelper.Current.CultureName];
    }

    public FoodModel(Food food)
        : this()
    {
        Id = food.Name;
        DescriptionId = food.Desc;
        Graph = food.Graph;
        Type = food.Type;
        Strength = food.Strength;
        StrengthDrink = food.StrengthDrink;
        StrengthFood = food.StrengthFood;
        Feeling = food.Feeling;
        Health = food.Health;
        Likability = food.Likability;
        Price = food.Price;
        Exp = food.Exp;
        if (File.Exists(food.Image))
            Image = NativeUtils.LoadImageToMemoryStream(food.Image);
    }

    public Food ToFood()
    {
        return new Food()
        {
            Name = Id,
            Desc = DescriptionId,
            Graph = Graph,
            Type = Type,
            Strength = Strength,
            StrengthFood = StrengthFood,
            StrengthDrink = StrengthDrink,
            Feeling = Feeling,
            Health = Health,
            Likability = Likability,
            Price = Price,
            Exp = Exp,
        };
    }

    public Food SetValueToFood(Food food)
    {
        food.Strength = Strength;
        food.StrengthFood = StrengthFood;
        food.StrengthDrink = StrengthDrink;
        food.Feeling = Feeling;
        food.Health = Health;
        food.Likability = Likability;
        food.Exp = Exp;
        return food;
    }

    public void RefreshId()
    {
        DescriptionId = $"{Id}_{nameof(DescriptionId)}";
    }

    public void Close()
    {
        Image.CloseStream();
    }
}

public class I18nFoodModel : ObservableObjectX<I18nFoodModel>
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

    public I18nFoodModel Copy()
    {
        var result = new I18nFoodModel();
        result.Name = Name;
        result.Description = Description;
        return result;
    }
}
