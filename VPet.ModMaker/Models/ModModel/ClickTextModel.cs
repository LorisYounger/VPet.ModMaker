using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HKW.HKWMapper;
using HKW.HKWReactiveUI;
using HKW.HKWUtils;
using HKW.HKWUtils.Observable;
using LinePutScript.Converter;
using VPet.ModMaker.ViewModels;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.Models;

/// <summary>
/// 点击文本模型
/// </summary>
[MapTo(typeof(ClickText), MapperConfig = typeof(ClickTextModelMapToClickTextConfig))]
[MapFrom(typeof(ClickText), MapperConfig = typeof(ClickTextModelMapFromClickTextConfig))]
[MapFrom(typeof(ClickTextModel), MapperConfig = typeof(ClickTextModelMapFromClickTextModelConfig))]
public partial class ClickTextModel : ViewModelBase
{
    /// <inheritdoc/>
    public ClickTextModel() { }

    /// <inheritdoc/>
    /// <param name="clickText">点击文本模型</param>
    public ClickTextModel(ClickTextModel clickText)
        : this()
    {
        this.MapFromClickTextModel(clickText);
    }

    /// <inheritdoc/>
    /// <param name="clickText">点击文本</param>
    public ClickTextModel(ClickText clickText)
        : this()
    {
        this.MapFromClickText(clickText);
    }

    private readonly ClickText _clickText = new();

    /// <summary>
    /// 转化为点击文本
    /// </summary>
    /// <returns>点击文本</returns>
    public ClickText ToClickText()
    {
        return this.MapToClickText(_clickText);
    }

    /// <summary>
    /// 模式类型
    /// </summary>
    public static FrozenSet<ClickText.ModeType> ModeTypes => EnumInfo<ClickText.ModeType>.Values;

    /// <summary>
    /// 日期区间
    /// </summary>
    public static FrozenSet<ClickText.DayTime> DayTimes => EnumInfo<ClickText.DayTime>.Values;

    /// <summary>
    /// 工作状态
    /// </summary>
    public static FrozenSet<VPet_Simulator.Core.Main.WorkingState> WorkingStates =>
        EnumInfo<VPet_Simulator.Core.Main.WorkingState>.Values;

    /// <summary>
    /// ID
    /// </summary>
    [ClickTextModelMapToClickTextProperty(nameof(ClickText.Text))]
    [ClickTextModelMapFromClickTextProperty(nameof(ClickText.Text))]
    [ReactiveProperty]
    public string ID { get; set; } = string.Empty;

    /// <summary>
    /// 本地化资源
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
    /// 本地化对象
    /// </summary>
    [NotifyPropertyChangeFrom("")]
    public I18nObject<string, string> I18nObject => new(this);

    /// <summary>
    /// 点击文本
    /// </summary>
    [MapIgnoreProperty]
    [ReactiveI18nProperty(nameof(I18nResource), nameof(I18nObject), nameof(ID), true)]
    public string Text
    {
        get => I18nResource.GetCurrentCultureDataOrDefault(ID);
        set => I18nResource.SetCurrentCultureData(ID, value);
    }

    /// <summary>
    /// 指定工作
    /// </summary>
    [ClickTextModelMapToClickTextProperty(nameof(ClickText.Working))]
    [ClickTextModelMapFromClickTextProperty(
        nameof(ClickText.Working),
        MapWhenRValueNotNullOrDefault = true
    )]
    [ReactiveProperty]
    public string Working { get; set; } = string.Empty;

    /// <summary>
    /// 宠物状态
    /// </summary>
    public ObservableEnum<ClickText.ModeType> Mode { get; set; } =
        new(
            ClickText.ModeType.Happy
                | ClickText.ModeType.Nomal
                | ClickText.ModeType.PoorCondition
                | ClickText.ModeType.Ill
        );

    /// <summary>
    /// 行动状态
    /// </summary>
    [ClickTextModelMapToClickTextProperty(nameof(ClickText.State))]
    [ClickTextModelMapFromClickTextProperty(nameof(ClickText.State))]
    [ReactiveProperty]
    public VPet_Simulator.Core.Main.WorkingState WorkingState { get; set; }

    /// <summary>
    /// 日期区间
    /// </summary>
    public ObservableEnum<ClickText.DayTime> DayTime { get; set; } =
        new(
            ClickText.DayTime.Morning
                | ClickText.DayTime.Afternoon
                | ClickText.DayTime.Night
                | ClickText.DayTime.Midnight
        );

    /// <summary>
    /// 好感度
    /// </summary>
    public ObservableRange<double> Like { get; } = new(0, int.MaxValue);

    /// <summary>
    /// 健康值
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
    /// 心情
    /// </summary>
    public ObservableRange<double> Feel { get; } = new(0, int.MaxValue);

    /// <summary>
    /// 体力
    /// </summary>
    public ObservableRange<double> Strength { get; } = new(0, int.MaxValue);

    /// <summary>
    /// 关闭
    /// </summary>
    public void Close()
    {
        I18nResource.I18nObjects.Remove(I18nObject);
        I18nObject.Close();
    }
}

internal class ClickTextModelMapFromClickTextModelConfig
    : MapperConfig<ClickTextModel, ClickTextModel>
{
    public ClickTextModelMapFromClickTextModelConfig()
    {
        AddMap(
            x => x.Mode,
            (s, t) =>
            {
                s.Mode = t.Mode.Clone();
            }
        );
        AddMap(
            x => x.Like,
            (s, t) =>
            {
                s.Like.SetValue(t.Like);
            }
        );
        AddMap(
            x => x.Health,
            (s, t) =>
            {
                s.Health.SetValue(t.Health);
            }
        );
        AddMap(
            x => x.Level,
            (s, t) =>
            {
                s.Level.SetValue(t.Level);
            }
        );
        AddMap(
            x => x.Money,
            (s, t) =>
            {
                s.Money.SetValue(t.Money);
            }
        );
        AddMap(
            x => x.Food,
            (s, t) =>
            {
                s.Food.SetValue(t.Food);
            }
        );
        AddMap(
            x => x.Drink,
            (s, t) =>
            {
                s.Drink.SetValue(t.Drink);
            }
        );
        AddMap(
            x => x.Feel,
            (s, t) =>
            {
                s.Feel.SetValue(t.Feel);
            }
        );
        AddMap(
            x => x.Strength,
            (s, t) =>
            {
                s.Strength.SetValue(t.Strength);
            }
        );
    }
}

internal class ClickTextModelMapToClickTextConfig : MapperConfig<ClickTextModel, ClickText>
{
    public ClickTextModelMapToClickTextConfig()
    {
        AddMap(
            x => x.Mode,
            (s, t) =>
            {
                t.Mode = s.Mode.Value;
            }
        );
        AddMap(
            x => x.Like,
            (s, t) =>
            {
                t.LikeMax = s.Like.Max;
                t.LikeMin = s.Like.Min;
            }
        );
        AddMap(
            x => x.Health,
            (s, t) =>
            {
                t.HealthMax = s.Health.Max;
                t.HealthMin = s.Health.Min;
            }
        );
        AddMap(
            x => x.Level,
            (s, t) =>
            {
                t.LevelMax = s.Level.Max;
                t.LevelMin = s.Level.Min;
            }
        );
        AddMap(
            x => x.Money,
            (s, t) =>
            {
                t.MoneyMax = s.Money.Max;
                t.MoneyMin = s.Money.Min;
            }
        );
        AddMap(
            x => x.Food,
            (s, t) =>
            {
                t.FoodMax = s.Food.Max;
                t.FoodMin = s.Food.Min;
            }
        );
        AddMap(
            x => x.Drink,
            (s, t) =>
            {
                t.DrinkMax = s.Drink.Max;
                t.DrinkMin = s.Drink.Min;
            }
        );
        AddMap(
            x => x.Feel,
            (s, t) =>
            {
                t.FeelMax = s.Feel.Max;
                t.FeelMin = s.Feel.Min;
            }
        );
        AddMap(
            x => x.Strength,
            (s, t) =>
            {
                t.StrengthMax = s.Strength.Max;
                t.StrengthMin = s.Strength.Min;
            }
        );
    }
}

internal class ClickTextModelMapFromClickTextConfig : MapperConfig<ClickTextModel, ClickText>
{
    public ClickTextModelMapFromClickTextConfig()
    {
        AddMap(
            x => x.Mode,
            (s, t) =>
            {
                s.Mode = new(t.Mode);
            }
        );
        AddMap(
            x => x.Like,
            (s, t) =>
            {
                s.Like.SetValue(t.LikeMin, t.LikeMax);
            }
        );
        AddMap(
            x => x.Health,
            (s, t) =>
            {
                s.Health.SetValue(t.HealthMin, t.HealthMax);
            }
        );
        AddMap(
            x => x.Level,
            (s, t) =>
            {
                s.Level.SetValue(t.LevelMin, t.LevelMax);
            }
        );
        AddMap(
            x => x.Money,
            (s, t) =>
            {
                s.Money.SetValue(t.MoneyMin, t.MoneyMax);
            }
        );
        AddMap(
            x => x.Food,
            (s, t) =>
            {
                s.Food.SetValue(t.FoodMin, t.FoodMax);
            }
        );
        AddMap(
            x => x.Drink,
            (s, t) =>
            {
                s.Drink.SetValue(t.DrinkMin, t.DrinkMax);
            }
        );
        AddMap(
            x => x.Feel,
            (s, t) =>
            {
                s.Feel.SetValue(t.FeelMin, t.FeelMax);
            }
        );
        AddMap(
            x => x.Strength,
            (s, t) =>
            {
                s.Strength.SetValue(t.StrengthMin, t.StrengthMax);
            }
        );
    }
}
