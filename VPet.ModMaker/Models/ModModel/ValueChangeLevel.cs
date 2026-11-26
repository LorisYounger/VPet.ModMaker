using System.Collections.Immutable;
using HKW.Mapper;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.Models;

/// <summary>
/// 数值改变档位
/// </summary>
public enum ValueChangeLevel
{
    /// <summary>
    /// ---
    /// </summary>
    Minus3 = -3,

    /// <summary>
    /// --
    /// </summary>
    Minus2 = -2,

    /// <summary>
    /// -
    /// </summary>
    Minus1 = -1,

    /// <summary>
    /// 无
    /// </summary>
    None = 0,

    /// <summary>
    /// +
    /// </summary>
    Plus1 = 1,

    /// <summary>
    /// ++
    /// </summary>
    Plus2 = 2,

    /// <summary>
    /// +++
    /// </summary>
    Plus3 = 3,
}

/// <summary>
/// 数值改变档位辅助类
/// </summary>
public static class ValueChangeLevelHelper
{
    /// <summary>
    /// 常规改变档位（--- ~ +++）
    /// </summary>
    public static ImmutableArray<ValueChangeLevel> Levels { get; } =
    [
        ValueChangeLevel.Minus3,
        ValueChangeLevel.Minus2,
        ValueChangeLevel.Minus1,
        ValueChangeLevel.None,
        ValueChangeLevel.Plus1,
        ValueChangeLevel.Plus2,
        ValueChangeLevel.Plus3,
    ];

    /// <summary>
    /// 好感度改变档位（-- ~ ++）
    /// </summary>
    public static ImmutableArray<ValueChangeLevel> LikabilityLevels { get; } =
    [
        ValueChangeLevel.Minus2,
        ValueChangeLevel.Minus1,
        ValueChangeLevel.None,
        ValueChangeLevel.Plus1,
        ValueChangeLevel.Plus2,
    ];

    internal readonly record struct TierRule(
        double Level1Max,
        double Level2Max,
        double Level1Value,
        double Level2Value,
        double Level3Value,
        bool AllowLevel3 = true
    )
    {
        public double ToValue(ValueChangeLevel level) =>
            level switch
            {
                ValueChangeLevel.Minus3 => -(AllowLevel3 ? Level3Value : Level2Value),
                ValueChangeLevel.Minus2 => -Level2Value,
                ValueChangeLevel.Minus1 => -Level1Value,
                ValueChangeLevel.None => 0,
                ValueChangeLevel.Plus1 => Level1Value,
                ValueChangeLevel.Plus2 => Level2Value,
                ValueChangeLevel.Plus3 => AllowLevel3 ? Level3Value : Level2Value,
                _ => 0,
            };

        public ValueChangeLevel ToLevel(double value)
        {
            var abs = Math.Truncate(Math.Abs(value));
            if (abs < 1)
                return ValueChangeLevel.None;

            var tier =
                abs <= Level1Max ? 1
                : !AllowLevel3 || abs <= Level2Max ? 2
                : 3;

            return (ValueChangeLevel)(value < 0 ? -tier : tier);
        }
    }

    // 心情: 1-7 (±4), 8-26 (±17), 27-63 (±45)
    internal static TierRule FeelingRule { get; } = new(7, 26, 4, 17, 45);

    // 经验值/体力: 1-15 (±8), 16-80 (±48), 81-255 (±168)
    internal static TierRule ExpAndStrengthRule { get; } = new(15, 80, 8, 48, 168);

    // 饱腹度: 1-3 (±2), 4-8 (±6), 9-15 (±12)
    internal static TierRule StrengthFoodRule { get; } = new(3, 8, 2, 6, 12);

    // 口渴度: 1-5 (±3), 6-15 (±10), 16-31 (±23)
    internal static TierRule StrengthDrinkRule { get; } = new(5, 15, 3, 10, 23);

    // 健康: 1 (±1), 2 (±2), 3 (±3)
    internal static TierRule HealthRule { get; } = new(1, 2, 1, 2, 3);

    // 好感度: 1 (±1), 2 (±2), 无 +++/---（≥3 为 +++++）
    internal static TierRule LikabilityRule { get; } = new(1, 2, 1, 2, 2, AllowLevel3: false);

    // 金钱: +/- = ±1, ++/-- = ±5, +++/--- = ±10
    internal static TierRule MoneyRule { get; } = new(4, 9, 1, 5, 10);

    internal static TierRule GetRule(string propertyName) =>
        propertyName switch
        {
            nameof(ClickText.Feeling) => FeelingRule,
            nameof(ClickText.Exp) => ExpAndStrengthRule,
            nameof(ClickText.Strength) => ExpAndStrengthRule,
            nameof(ClickText.StrengthFood) => StrengthFoodRule,
            nameof(ClickText.StrengthDrink) => StrengthDrinkRule,
            nameof(ClickText.Health) => HealthRule,
            nameof(ClickText.Likability) => LikabilityRule,
            nameof(ClickText.Money) => MoneyRule,
            _ => throw new ArgumentOutOfRangeException(
                nameof(propertyName),
                propertyName,
                "未配置数值改变档位规则的属性"
            ),
        };
}

/// <summary>
/// 数值改变档位与 double 互转映射转换器
/// </summary>
public sealed class ValueChangeLevelToDoubleConverter : IMapConverter<ValueChangeLevel, double>
{
    public double Convert(ValueChangeLevel value, MapConvertArgs args) =>
        ValueChangeLevelHelper.GetRule(args.TargetPropertyName).ToValue(value);

    public ValueChangeLevel ConvertBack(double value, MapConvertArgs args) =>
        ValueChangeLevelHelper.GetRule(args.TargetPropertyName).ToLevel(value);
}

/// <summary>
/// 数值改变档位与 int 互转映射转换器
/// </summary>
public sealed class ValueChangeLevelToIntConverter : IMapConverter<ValueChangeLevel, int>
{
    public int Convert(ValueChangeLevel value, MapConvertArgs args) =>
        (int)ValueChangeLevelHelper.GetRule(args.TargetPropertyName).ToValue(value);

    public ValueChangeLevel ConvertBack(int value, MapConvertArgs args) =>
        ValueChangeLevelHelper.GetRule(args.TargetPropertyName).ToLevel(value);
}
