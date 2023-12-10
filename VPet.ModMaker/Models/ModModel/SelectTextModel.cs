using HKW.HKWUtils.Observable;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPet.ModMaker.Models;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.Models;

/// <summary>
/// 选择文本模型
/// </summary>
public class SelectTextModel : I18nModel<I18nSelectTextModel>
{
    /// <summary>
    /// 模式类型
    /// </summary>
    public static ObservableCollection<ClickText.ModeType> ModeTypes => ClickTextModel.ModeTypes;

    /// <summary>
    /// 标签
    /// </summary>
    public ObservableValue<string> Tags { get; } = new();

    /// <summary>
    /// 跳转标签
    /// </summary>
    public ObservableValue<string> ToTags { get; } = new();

    /// <summary>
    /// Id
    /// </summary>
    public ObservableValue<string> Id { get; } = new();

    /// <summary>
    /// 选择Id
    /// </summary>
    public ObservableValue<string> ChooseId { get; } = new();

    /// <summary>
    /// 宠物状态
    /// </summary>
    public ObservableEnumFlags<ClickText.ModeType> Mode { get; } =
        new(
            ClickText.ModeType.Happy
                | ClickText.ModeType.Nomal
                | ClickText.ModeType.PoorCondition
                | ClickText.ModeType.Ill
        );

    /// <summary>
    /// 好感度
    /// </summary>
    public ObservableRange<double> Like { get; } = new(0, int.MaxValue);

    /// <summary>
    /// 健康度
    /// </summary>
    public ObservableRange<double> Health { get; } = new(0, int.MaxValue);

    /// <summary>
    /// 等级
    /// </summary>
    public ObservableRange<double> Level { get; } = new(0, int.MaxValue);

    /// <summary>
    /// 金钱
    /// </summary>
    public ObservableRange<double> Money { get; } = new(int.MinValue, int.MaxValue);

    /// <summary>
    /// 饱食度
    /// </summary>
    public ObservableRange<double> Food { get; } = new(0, int.MaxValue);

    /// <summary>
    /// 口渴度
    /// </summary>
    public ObservableRange<double> Drink { get; } = new(0, int.MaxValue);

    /// <summary>
    /// 心情值
    /// </summary>
    public ObservableRange<double> Feel { get; } = new(0, int.MaxValue);

    /// <summary>
    /// 体力值
    /// </summary>
    public ObservableRange<double> Strength { get; } = new(0, int.MaxValue);

    public SelectTextModel()
    {
        ChooseId.Value = $"{Id.Value}_{nameof(ChooseId)}";
        Id.ValueChanged += (o, n) =>
        {
            ChooseId.Value = $"{n}_{nameof(ChooseId)}";
        };
    }

    public SelectTextModel(SelectTextModel model)
        : this()
    {
        Id.Value = model.Id.Value;
        Mode.EnumValue.Value = model.Mode.EnumValue.Value;
        Tags.Value = model.Tags.Value;
        ToTags.Value = model.ToTags.Value;
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
        Id.Value = text.Text;
        ChooseId.Value = text.Choose ?? string.Empty;
        Mode.EnumValue.Value = text.Mode;
        Tags.Value = text.Tags is null ? string.Empty : string.Join(", ", text.Tags);
        ToTags.Value = text.ToTags is null ? string.Empty : string.Join(", ", text.ToTags);
        Like = new(text.LikeMin, text.LikeMax);
        Health = new(text.HealthMin, text.HealthMax);
        Level = new(text.LevelMin, text.LevelMax);
        Money = new(text.MoneyMin, text.MoneyMax);
        Food = new(text.FoodMin, text.FoodMax);
        Drink = new(text.DrinkMin, text.DrinkMax);
        Feel = new(text.FeelMin, text.FeelMax);
        Strength = new(text.StrengthMin, text.StrengthMax);
    }

    private readonly static char[] rs_splitChar = { ',', ' ' };

    public SelectText ToSelectText()
    {
        return new()
        {
            Text = Id.Value,
            Choose = ChooseId.Value,
            Mode = Mode.EnumValue.Value,
            Tags = new(Tags.Value.Split(rs_splitChar, StringSplitOptions.RemoveEmptyEntries)),
            ToTags = new(ToTags.Value.Split(rs_splitChar, StringSplitOptions.RemoveEmptyEntries)),
            LikeMax = Like.Max,
            LikeMin = Like.Min,
            HealthMin = Health.Min,
            HealthMax = Health.Max,
            LevelMin = Level.Min,
            LevelMax = Level.Max,
            MoneyMin = Money.Min,
            MoneyMax = Money.Max,
            FoodMin = Food.Min,
            FoodMax = Food.Max,
            DrinkMin = Drink.Min,
            DrinkMax = Drink.Max,
            FeelMin = Feel.Min,
            FeelMax = Feel.Max,
            StrengthMin = Strength.Min,
            StrengthMax = Strength.Max,
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
