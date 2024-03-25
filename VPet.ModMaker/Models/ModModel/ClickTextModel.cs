using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HKW.HKWUtils;
using HKW.HKWUtils.Observable;
using LinePutScript.Converter;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.Models;

/// <summary>
/// 点击文本模型
/// </summary>
public class ClickTextModel : I18nModel<I18nClickTextModel>
{
    /// <summary>
    /// 模式类型
    /// </summary>
    public static ObservableCollection<ClickText.ModeType> ModeTypes { get; } =
        new(Enum.GetValues(typeof(ClickText.ModeType)).Cast<ClickText.ModeType>());

    /// <summary>
    /// 日期区间
    /// </summary>
    public static ObservableCollection<ClickText.DayTime> DayTimes { get; } =
        new(Enum.GetValues(typeof(ClickText.DayTime)).Cast<ClickText.DayTime>());

    /// <summary>
    /// 工作状态
    /// </summary>
    public static ObservableCollection<VPet_Simulator.Core.Main.WorkingState> WorkingStates { get; } =
        new(
            Enum.GetValues(typeof(VPet_Simulator.Core.Main.WorkingState))
                .Cast<VPet_Simulator.Core.Main.WorkingState>()
        );

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

    #region Working
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _working = string.Empty;

    /// <summary>
    /// 指定工作
    /// </summary>
    public string Working
    {
        get => _working;
        set => SetProperty(ref _working, value);
    }
    #endregion

    /// <summary>
    /// 宠物状态
    /// </summary>
    public ObservableEnumCommand<ClickText.ModeType> Mode { get; } =
        new(
            ClickText.ModeType.Happy
                | ClickText.ModeType.Nomal
                | ClickText.ModeType.PoorCondition
                | ClickText.ModeType.Ill
        );

    #region WorkingState
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private VPet_Simulator.Core.Main.WorkingState _WorkingState;

    /// <summary>
    /// 行动状态
    /// </summary>
    public VPet_Simulator.Core.Main.WorkingState WorkingState
    {
        get => _WorkingState;
        set => SetProperty(ref _WorkingState, value);
    }
    #endregion

    /// <summary>
    /// 日期区间
    /// </summary>
    public ObservableEnumCommand<ClickText.DayTime> DayTime { get; } =
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

    public ClickTextModel() { }

    public ClickTextModel(ClickTextModel clickText)
        : this()
    {
        Id = clickText.Id;
        Mode.Value = clickText.Mode.Value;
        Working = clickText.Working;
        WorkingState = clickText.WorkingState;
        DayTime.Value = clickText.DayTime.Value;
        Like = clickText.Like.Clone();
        Health = clickText.Health.Clone();
        Level = clickText.Level.Clone();
        Money = clickText.Money.Clone();
        Food = clickText.Food.Clone();
        Drink = clickText.Drink.Clone();
        Feel = clickText.Feel.Clone();
        Strength = clickText.Strength.Clone();
        foreach (var item in clickText.I18nDatas)
            I18nDatas[item.Key] = item.Value.Copy();
        CurrentI18nData = I18nDatas[I18nHelper.Current.CultureName];
    }

    public ClickTextModel(ClickText clickText)
        : this()
    {
        Id = clickText.Text;
        Mode.Value = clickText.Mode;
        Working = clickText.Working;
        WorkingState = clickText.State;
        DayTime.Value = clickText.DaiTime;
        Like = new(clickText.LikeMin, clickText.LikeMax);
        Health = new(clickText.HealthMin, clickText.HealthMax);
        Level = new(clickText.LevelMin, clickText.LevelMax);
        Money = new(clickText.MoneyMin, clickText.MoneyMax);
        Food = new(clickText.FoodMin, clickText.FoodMax);
        Drink = new(clickText.DrinkMin, clickText.DrinkMax);
        Feel = new(clickText.FeelMin, clickText.FeelMax);
        Strength = new(clickText.StrengthMin, clickText.StrengthMax);
    }

    public ClickText ToClickText()
    {
        return new()
        {
            Text = Id,
            Mode = Mode.Value,
            Working = Working,
            State = WorkingState,
            DaiTime = DayTime.Value,
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

public class I18nClickTextModel : ObservableObjectX<I18nClickTextModel>
{
    #region Text
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _text = string.Empty;

    public string Text
    {
        get => _text;
        set => SetProperty(ref _text, value);
    }
    #endregion

    public I18nClickTextModel Copy()
    {
        var result = new I18nClickTextModel();
        result.Text = Text;
        return result;
    }
}
