using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.Models;

public class FoodModel : I18nModel<I18nFoodModel>
{
    public ObservableValue<string> Name { get; } = new();
    public ObservableValue<string> Description { get; } = new();
    public ObservableValue<Food.FoodType> Type { get; } = new();
    public ObservableValue<double> Strength { get; } = new();
    public ObservableValue<double> StrengthFood { get; } = new();
    public ObservableValue<double> StrengthDrink { get; } = new();
    public ObservableValue<double> Feeling { get; } = new();
    public ObservableValue<double> Health { get; } = new();
    public ObservableValue<double> Likability { get; } = new();
    public ObservableValue<double> Price { get; } = new();
    public ObservableValue<int> Exp { get; } = new();
    public ObservableValue<BitmapImage> Image { get; } = new();

    public FoodModel() { }

    public FoodModel(FoodModel food)
        : this()
    {
        Name.Value = food.Name.Value;
        Description.Value = food.Description.Value;
        Type.Value = food.Type.Value;
        Strength.Value = food.Strength.Value;
        StrengthFood.Value = food.StrengthFood.Value;
        StrengthDrink.Value = food.StrengthDrink.Value;
        Feeling.Value = food.Feeling.Value;
        Health.Value = food.Health.Value;
        Likability.Value = food.Likability.Value;
        Price.Value = food.Price.Value;
        Exp.Value = food.Exp.Value;
        Image.Value = Utils.LoadImageToStream(food.Image.Value);
        foreach (var item in food.I18nDatas)
        {
            I18nDatas[item.Key] = new();
            I18nDatas[item.Key].Name.Value = food.I18nDatas[item.Key].Name.Value;
            I18nDatas[item.Key].Description.Value = food.I18nDatas[item.Key].Description.Value;
        }
        CurrentI18nData.Value = I18nDatas[I18nHelper.Current.CultureName.Value];
    }

    public FoodModel(Food food)
        : this()
    {
        Name.Value = food.Name;
        Description.Value = food.Desc;
        Type.Value = food.Type;
        Strength.Value = food.Strength;
        StrengthDrink.Value = food.StrengthDrink;
        StrengthFood.Value = food.StrengthFood;
        Feeling.Value = food.Feeling;
        Health.Value = food.Health;
        Likability.Value = food.Likability;
        Price.Value = food.Price;
        Exp.Value = food.Exp;
        if (File.Exists(food.Image))
            Image.Value = Utils.LoadImageToStream(food.Image);
    }

    public Food ToFood()
    {
        return new Food()
        {
            Name = Name.Value,
            Desc = $"{Name.Value}_{nameof(Description)}",
            Type = Type.Value,
            Strength = Strength.Value,
            StrengthFood = StrengthFood.Value,
            StrengthDrink = StrengthDrink.Value,
            Feeling = Feeling.Value,
            Health = Health.Value,
            Likability = Likability.Value,
            Price = Price.Value,
            ImageSource = Image.Value,
        };
    }

    public void Close()
    {
        Image.Value?.StreamSource?.Close();
    }
}

public class I18nFoodModel
{
    public ObservableValue<string> Name { get; } = new();
    public ObservableValue<string> Description { get; } = new();
}
