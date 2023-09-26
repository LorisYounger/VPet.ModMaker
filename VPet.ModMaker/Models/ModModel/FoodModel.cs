using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.Models;

public class FoodModel : I18nModel<I18nFoodModel>
{
    public static ObservableCollection<Food.FoodType> FoodTypes { get; } =
        new(Enum.GetValues(typeof(Food.FoodType)).Cast<Food.FoodType>());

    public ObservableValue<string> Id { get; } = new();
    public ObservableValue<string> DescriptionId { get; } = new();
    public ObservableValue<string> Graph { get; } = new();
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

    public FoodModel()
    {
        DescriptionId.Value = $"{Id.Value}_{nameof(DescriptionId)}";
        Id.ValueChanged += (o, n) =>
        {
            DescriptionId.Value = $"{n}_{nameof(DescriptionId)}";
        };
    }

    public FoodModel(FoodModel model)
        : this()
    {
        Id.Value = model.Id.Value;
        DescriptionId.Value = model.DescriptionId.Value;
        Graph.Value = model.Graph.Value;
        Type.Value = model.Type.Value;
        Strength.Value = model.Strength.Value;
        StrengthFood.Value = model.StrengthFood.Value;
        StrengthDrink.Value = model.StrengthDrink.Value;
        Feeling.Value = model.Feeling.Value;
        Health.Value = model.Health.Value;
        Likability.Value = model.Likability.Value;
        Price.Value = model.Price.Value;
        Exp.Value = model.Exp.Value;
        Image.Value = model.Image.Value;
        foreach (var item in model.I18nDatas)
            I18nDatas[item.Key] = item.Value.Copy();
        CurrentI18nData.Value = I18nDatas[I18nHelper.Current.CultureName.Value];
    }

    public FoodModel(Food food)
        : this()
    {
        Id.Value = food.Name;
        DescriptionId.Value = food.Desc;
        Graph.Value = food.Graph;
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
            Image.Value = Utils.LoadImageToMemoryStream(food.Image);
    }

    public Food ToFood()
    {
        return new Food()
        {
            Name = Id.Value,
            Desc = DescriptionId.Value,
            Graph = Graph.Value,
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

    public I18nFoodModel Copy()
    {
        var result = new I18nFoodModel();
        result.Name.Value = Name.Value;
        result.Description.Value = Description.Value;
        return result;
    }
}
