using System.Collections.Immutable;
using System.Numerics;
using CommunityToolkit.Mvvm.ComponentModel;
using HKW.HKWUtils;
using HKW.HKWUtils.Observable;
using HKW.Mapper;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.Models;

/// <summary>
/// 点击文本模型
/// </summary>
[MapTarget(typeof(ClickText))]
[MapTarget(typeof(ClickTextModel))]
public sealed partial class ClickTextModel : ObservableModelBase, IDisposable
{
    /// <inheritdoc/>
    public ClickTextModel(ObservableI18nResource<string, string> i18nResource)
    {
        I18nResource = i18nResource;
    }

    /// <inheritdoc/>
    /// <param name="clickText">点击文本模型</param>
    public ClickTextModel(
        ClickTextModel clickText,
        ObservableI18nResource<string, string> i18nResource
    )
    {
        this.MapFrom(clickText);
        I18nResource = i18nResource;
    }

    /// <inheritdoc/>
    /// <param name="clickText">点击文本</param>
    public ClickTextModel(ClickText clickText, ObservableI18nResource<string, string> i18nResource)
    {
        this.MapFrom(clickText);
        I18nResource = i18nResource;
    }

    /// <summary>
    /// 模式类型
    /// </summary>
    public static ImmutableArray<ClickText.ModeType> ModeTypes =>
        EnumInfo<ClickText.ModeType>.StaticValues;

    /// <summary>
    /// 日期区间
    /// </summary>
    public static ImmutableArray<ClickText.DayTime> DayTimes =>
        EnumInfo<ClickText.DayTime>.StaticValues;

    /// <summary>
    /// 工作状态
    /// </summary>
    public static ImmutableArray<VPet_Simulator.Core.Main.WorkingState> WorkingStates =>
        EnumInfo<VPet_Simulator.Core.Main.WorkingState>.StaticValues;

    /// <summary>
    /// 常规改变档位
    /// </summary>
    public static ImmutableArray<ValueChangeLevel> ValueChangeLevels =>
        ValueChangeLevelHelper.Levels;

    /// <summary>
    /// 好感度改变档位
    /// </summary>
    public static ImmutableArray<ValueChangeLevel> LikabilityChangeLevels =>
        ValueChangeLevelHelper.LikabilityLevels;

    /// <summary>
    /// ID
    /// </summary>
    [ObservableProperty]
    [MapProperty(typeof(ClickText), nameof(ClickText.Text))]
    public string ID { get; set; } = string.Empty;

    /// <summary>
    /// I18n资源
    /// </summary>
    [MapIgnoreProperty]
    [ObservableProperty]
    public ObservableI18nResource<string, string> I18nResource { get; set; }

    partial void OnI18nResourceChanged(
        ObservableI18nResource<string, string> oldValue,
        ObservableI18nResource<string, string> newValue
    )
    {
        I18nObserver.Resource = newValue;
    }

    [MapIgnoreProperty]
    public I18nObserver<ClickTextModel, string, string> I18nObserver =>
        field ??= new(I18nResource, this, (s, p) => s.OnPropertyChanged(p))
        {
            new(s => s.ID, nameof(Text)),
        };

    /// <summary>
    /// 点击文本
    /// </summary>
    [MapIgnoreProperty]
    public string Text
    {
        get => I18nObserver.GetDataOrDefault(ID);
        set => I18nObserver.SetData(ID, value);
    }

    /// <summary>
    /// 指定工作
    /// </summary>
    [ObservableProperty]
    public string Working { get; set; } = string.Empty;

    /// <summary>
    /// 宠物状态
    /// </summary>
    [MapProperty(typeof(ClickText), typeof(ObservableEnumConverter))]
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
    [ObservableProperty]
    [MapProperty(typeof(ClickText), nameof(ClickText.State))]
    public VPet_Simulator.Core.Main.WorkingState WorkingState { get; set; }

    /// <summary>
    /// 日期区间
    /// </summary>
    [MapProperty(typeof(ClickText), nameof(ClickText.DaiTime), typeof(ObservableEnumConverter))]
    public ObservableEnum<ClickText.DayTime> DayTime { get; set; } =
        new(
            ClickText.DayTime.Morning
                | ClickText.DayTime.Afternoon
                | ClickText.DayTime.Night
                | ClickText.DayTime.Midnight
        );

    /// <summary>
    /// 好感度范围
    /// </summary>
    [MapCompositeProperty(
        typeof(ClickText),
        typeof(ObservableRangeConverter<double>),
        nameof(ClickText.LikeMin),
        nameof(ClickText.LikeMax)
    )]
    public ObservableRange<double> Like { get; set; } = new(0, int.MaxValue);

    /// <summary>
    /// 健康值范围
    /// </summary>
    [MapCompositeProperty(
        typeof(ClickText),
        typeof(ObservableRangeConverter<double>),
        nameof(ClickText.HealthMin),
        nameof(ClickText.HealthMax)
    )]
    public ObservableRange<double> Health { get; set; } = new(0, int.MaxValue);

    /// <summary>
    /// 等级范围
    /// </summary>
    [MapCompositeProperty(
        typeof(ClickText),
        typeof(ObservableRangeConverter<double>),
        nameof(ClickText.LevelMin),
        nameof(ClickText.LevelMax)
    )]
    public ObservableRange<double> Level { get; set; } = new(0, int.MaxValue);

    /// <summary>
    /// 金钱范围
    /// </summary>
    [MapCompositeProperty(
        typeof(ClickText),
        typeof(ObservableRangeConverter<double>),
        nameof(ClickText.MoneyMin),
        nameof(ClickText.MoneyMax)
    )]
    public ObservableRange<double> Money { get; set; } = new(int.MinValue, int.MaxValue);

    /// <summary>
    /// 饱食度范围
    /// </summary>
    [MapCompositeProperty(
        typeof(ClickText),
        typeof(ObservableRangeConverter<double>),
        nameof(ClickText.FoodMin),
        nameof(ClickText.FoodMax)
    )]
    public ObservableRange<double> Food { get; set; } = new(0, int.MaxValue);

    /// <summary>
    /// 口渴度范围
    /// </summary>
    [MapCompositeProperty(
        typeof(ClickText),
        typeof(ObservableRangeConverter<double>),
        nameof(ClickText.DrinkMin),
        nameof(ClickText.DrinkMax)
    )]
    public ObservableRange<double> Drink { get; set; } = new(0, int.MaxValue);

    /// <summary>
    /// 心情范围
    /// </summary>
    [MapCompositeProperty(
        typeof(ClickText),
        typeof(ObservableRangeConverter<double>),
        nameof(ClickText.FeelMin),
        nameof(ClickText.FeelMax)
    )]
    public ObservableRange<double> Feel { get; set; } = new(0, int.MaxValue);

    /// <summary>
    /// 体力范围
    /// </summary>
    [MapCompositeProperty(
        typeof(ClickText),
        typeof(ObservableRangeConverter<double>),
        nameof(ClickText.StrengthMin),
        nameof(ClickText.StrengthMax)
    )]
    public ObservableRange<double> Strength { get; set; } = new(0, int.MaxValue);

    /// <summary>
    /// 好感度改变值
    /// </summary>
    [ObservableProperty]
    [MapProperty(
        typeof(ClickText),
        nameof(ClickText.Likability),
        typeof(ValueChangeLevelToDoubleConverter)
    )]
    public ValueChangeLevel ChangeLikability { get; set; }

    /// <summary>
    /// 健康度改变值
    /// </summary>
    [ObservableProperty]
    [MapProperty(
        typeof(ClickText),
        nameof(ClickText.Health),
        typeof(ValueChangeLevelToDoubleConverter)
    )]
    public ValueChangeLevel ChangeHealth { get; set; }

    /// <summary>
    /// 经验改变值
    /// </summary>
    [ObservableProperty]
    [MapProperty(
        typeof(ClickText),
        nameof(ClickText.Exp),
        typeof(ValueChangeLevelToIntConverter)
    )]
    public ValueChangeLevel ChangeExp { get; set; }

    /// <summary>
    /// 金钱改变值
    /// </summary>
    [ObservableProperty]
    [MapProperty(
        typeof(ClickText),
        nameof(ClickText.Money),
        typeof(ValueChangeLevelToDoubleConverter)
    )]
    public ValueChangeLevel ChangeMoney { get; set; }

    /// <summary>
    /// 饱腹度改变值
    /// </summary>
    [ObservableProperty]
    [MapProperty(
        typeof(ClickText),
        nameof(ClickText.StrengthFood),
        typeof(ValueChangeLevelToDoubleConverter)
    )]
    public ValueChangeLevel ChangeStrengthFood { get; set; }

    /// <summary>
    /// 口渴度改变值
    /// </summary>
    [ObservableProperty]
    [MapProperty(
        typeof(ClickText),
        nameof(ClickText.StrengthDrink),
        typeof(ValueChangeLevelToDoubleConverter)
    )]
    public ValueChangeLevel ChangeStrengthDrink { get; set; }

    /// <summary>
    /// 心情改变值
    /// </summary>
    [ObservableProperty]
    [MapProperty(
        typeof(ClickText),
        nameof(ClickText.Feeling),
        typeof(ValueChangeLevelToDoubleConverter)
    )]
    public ValueChangeLevel ChangeFeeling { get; set; }

    /// <summary>
    /// 体力改变值
    /// </summary>
    [ObservableProperty]
    [MapProperty(
        typeof(ClickText),
        nameof(ClickText.Strength),
        typeof(ValueChangeLevelToDoubleConverter)
    )]
    public ValueChangeLevel ChangeStrength { get; set; }

    /// <inheritdoc/>
    public void Dispose()
    {
        I18nObserver?.Dispose();
    }
}

public sealed class ObservableRangeConverter<T>
    : IMapCompositeConverter<ObservableRange<T>, (T Min, T Max)>
    where T : struct, INumber<T>
{
    public (T Min, T Max) Convert(ObservableRange<T> value, MapCompositeConvertArgs args) =>
        (value.Min, value.Max);

    public ObservableRange<T> ConvertBack((T Min, T Max) values, MapCompositeConvertArgs args) =>
        new(values.Min, values.Max);
}

public sealed class ObservableEnumConverter : IMapConverter<IObservableEnum, Enum>
{
    public Enum Convert(IObservableEnum value, MapConvertArgs args) => value.Value;

    public IObservableEnum ConvertBack(Enum value, MapConvertArgs args) =>
        ObservableEnum.Create(value);
}
