using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPet.ModMaker.Models;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.Models;

public class SelectTextModel : I18nModel<I18nSelectTextModel>
{
    public static ObservableCollection<ClickText.ModeType> ModeTypes => ClickTextModel.ModeTypes;

    //public ObservableValue<int> Exp { get; } = new();
    //public ObservableValue<double> Money { get; } = new();
    //public ObservableValue<double> Strength { get; } = new();
    //public ObservableValue<double> StrengthFood { get; } = new();
    //public ObservableValue<double> StrengthDrink { get; } = new();
    //public ObservableValue<double> Feeling { get; } = new();
    //public ObservableValue<double> Health { get; } = new();
    //public ObservableValue<double> Likability { get; } = new();

    public ObservableValue<string> Tags { get; } = new();
    public ObservableValue<string> ToTags { get; } = new();

    public ObservableValue<string> Name { get; } = new();
    public ObservableValue<string> Choose { get; } = new();
    public ObservableValue<ClickText.ModeType> Mode { get; } = new();

    //public ObservableValue<string> Working { get; } = new();
    //public ObservableValue<VPet_Simulator.Core.Main.WorkingState> WorkingState { get; } = new();
    //public ObservableValue<ClickText.DayTime> DayTime { get; } = new();

    public ObservableRange<double> Like { get; } = new(0, int.MaxValue);
    public ObservableRange<double> Health { get; } = new(0, int.MaxValue);
    public ObservableRange<double> Level { get; } = new(0, int.MaxValue);
    public ObservableRange<double> Money { get; } = new(int.MinValue, int.MaxValue);
    public ObservableRange<double> Food { get; } = new(0, int.MaxValue);
    public ObservableRange<double> Drink { get; } = new(0, int.MaxValue);
    public ObservableRange<double> Feel { get; } = new(0, int.MaxValue);
    public ObservableRange<double> Strength { get; } = new(0, int.MaxValue);

    public SelectTextModel()
    {
        Choose.Value = $"{Name.Value}_{nameof(Choose)}";
        Name.ValueChanged += (v) =>
        {
            Choose.Value = $"{v}_{nameof(Choose)}";
        };
    }

    public SelectTextModel(SelectTextModel model)
        : this()
    {
        Name.Value = model.Name.Value;
        Mode.Value = model.Mode.Value;
        Tags.Value = model.Tags.Value;
        ToTags.Value = model.ToTags.Value;
        //Working.Value = model.Working.Value;
        //WorkingState.Value = model.WorkingState.Value;
        //DayTime.Value = model.DayTime.Value;
        Like = model.Like.Copy();
        Health = model.Health.Copy();
        Level = model.Level.Copy();
        Money = model.Money.Copy();
        Food = model.Food.Copy();
        Drink = model.Drink.Copy();
        Feel = model.Feel.Copy();
        Strength = model.Strength.Copy();

        foreach (var item in model.I18nDatas)
            I18nDatas[item.Key] = item.Value.Copy();
        CurrentI18nData.Value = I18nDatas[I18nHelper.Current.CultureName.Value];
    }

    public SelectTextModel(SelectText text)
        : this()
    {
        Name.Value = text.Text;
        Choose.Value = text.Choose ?? string.Empty;
        Mode.Value = text.Mode;
        Tags.Value = text.Tags is null ? string.Empty : string.Join(", ", text.Tags);
        ToTags.Value = text.ToTags is null ? string.Empty : string.Join(", ", text.ToTags);
        //Working.Value = text.Working;
        //WorkingState.Value = text.State;
        //DayTime.Value = text.DaiTime;
        Like.SetValue(text.LikeMin, text.LikeMax);
        Health.SetValue(text.HealthMin, text.HealthMax);
        Level.SetValue(text.LevelMin, text.LevelMax);
        Money.SetValue(text.MoneyMin, text.MoneyMax);
        Food.SetValue(text.FoodMin, text.FoodMax);
        Drink.SetValue(text.DrinkMin, text.DrinkMax);
        Feel.SetValue(text.FeelMin, text.FeelMax);
        Strength.SetValue(text.StrengthMin, text.StrengthMax);
    }

    private readonly static char[] rs_splitChar = { ',', ' ' };

    public SelectText ToSelectText()
    {
        return new()
        {
            Text = Name.Value,
            Choose = Choose.Value,
            Mode = Mode.Value,
            Tags = new(Tags.Value.Split(rs_splitChar, StringSplitOptions.RemoveEmptyEntries)),
            ToTags = new(ToTags.Value.Split(rs_splitChar, StringSplitOptions.RemoveEmptyEntries)),
            //Working = Working.Value,
            //State = WorkingState.Value,
            //DaiTime = DayTime.Value,
            LikeMax = Like.Max.Value,
            LikeMin = Like.Min.Value,
            HealthMin = Health.Min.Value,
            HealthMax = Health.Max.Value,
            LevelMin = Level.Min.Value,
            LevelMax = Level.Max.Value,
            MoneyMin = Money.Min.Value,
            MoneyMax = Money.Max.Value,
            FoodMin = Food.Min.Value,
            FoodMax = Food.Max.Value,
            DrinkMin = Drink.Min.Value,
            DrinkMax = Drink.Max.Value,
            FeelMin = Feel.Min.Value,
            FeelMax = Feel.Max.Value,
            StrengthMin = Strength.Min.Value,
            StrengthMax = Strength.Max.Value,
        };
    }
}

public class I18nSelectTextModel
{
    public ObservableValue<string> Choose { get; } = new();
    public ObservableValue<string> Text { get; } = new();

    public I18nSelectTextModel Copy()
    {
        var result = new I18nSelectTextModel();
        result.Text.Value = Text.Value;
        result.Choose.Value = Choose.Value;
        return result;
    }
}
