using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace VPet.ModMaker.Models;

/// <summary>
/// 工作模型
/// </summary>
public class WorkModel : I18nModel<I18nWorkModel>
{
    /// <summary>
    /// 工作类型
    /// </summary>
    public static ObservableCollection<VPet_Simulator.Core.GraphHelper.Work.WorkType> WorkTypes { get; } =
        new(
            Enum.GetValues(typeof(VPet_Simulator.Core.GraphHelper.Work.WorkType))
                .Cast<VPet_Simulator.Core.GraphHelper.Work.WorkType>()
        );

    /// <summary>
    /// Id
    /// </summary>
    public ObservableValue<string> Id { get; } = new();

    /// <summary>
    /// 指定动画
    /// </summary>
    public ObservableValue<string> Graph { get; } = new();

    /// <summary>
    /// 收获倍率
    /// </summary>
    public ObservableValue<double> MoneyLevel { get; } = new();

    /// <summary>
    /// 收获基础
    /// </summary>
    public ObservableValue<double> MoneyBase { get; } = new();

    /// <summary>
    /// 饱食度消耗
    /// </summary>
    public ObservableValue<double> StrengthFood { get; } = new();

    /// <summary>
    /// 口渴度消耗
    /// </summary>
    public ObservableValue<double> StrengthDrink { get; } = new();

    /// <summary>
    /// 心情消耗
    /// </summary>
    public ObservableValue<double> Feeling { get; } = new();

    /// <summary>
    /// 等级倍率
    /// </summary>
    public ObservableValue<int> LevelLimit { get; } = new();

    /// <summary>
    /// 时间
    /// </summary>
    public ObservableValue<int> Time { get; } = new();

    /// <summary>
    /// 完成奖励倍率
    /// </summary>
    public ObservableValue<double> FinishBonus { get; } = new();

    /// <summary>
    /// 是否超模
    /// </summary>
    public ObservableValue<bool> IsOverLoad { get; } = new();

    /// <summary>
    /// 类型
    /// </summary>
    public ObservableValue<VPet_Simulator.Core.GraphHelper.Work.WorkType> WorkType { get; } =
        new(VPet_Simulator.Core.GraphHelper.Work.WorkType.Work);

    /// <summary>
    /// 边框颜色
    /// </summary>
    public ObservableValue<SolidColorBrush> BorderBrush { get; } =
        new(new((Color)ColorConverter.ConvertFromString("#FF0290D5")));

    /// <summary>
    /// 背景色
    /// </summary>
    public ObservableValue<SolidColorBrush> Background { get; } =
        new(new((Color)ColorConverter.ConvertFromString("#FF81D4FA")));

    /// <summary>
    /// 前景色
    /// </summary>
    public ObservableValue<SolidColorBrush> Foreground { get; } =
        new(new((Color)ColorConverter.ConvertFromString("#FF0286C6")));

    /// <summary>
    /// 按钮背景色
    /// </summary>
    public ObservableValue<SolidColorBrush> ButtonBackground { get; } =
        new(new((Color)ColorConverter.ConvertFromString("#AA0286C6")));

    /// <summary>
    /// 按钮前景色
    /// </summary>
    public ObservableValue<SolidColorBrush> ButtonForeground { get; } =
        new(new((Color)ColorConverter.ConvertFromString("#FFFFFFFF")));

    /// <summary>
    /// X位置
    /// </summary>
    public ObservableValue<double> Left { get; } = new(100);

    /// <summary>
    /// Y位置
    /// </summary>
    public ObservableValue<double> Top { get; } = new(160);

    /// <summary>
    /// 宽度
    /// </summary>
    public ObservableValue<double> Width { get; } = new(300);

    public WorkModel()
    {
        IsOverLoad.AddNotifyReceiver(
            WorkType,
            MoneyBase,
            MoneyLevel,
            StrengthFood,
            StrengthDrink,
            Feeling,
            LevelLimit,
            FinishBonus
        );
        IsOverLoad.NotifyReceived += (ref bool v) =>
        {
            v = VPet_Simulator.Windows.Interface.ExtensionFunction.IsOverLoad(ToWork());
        };
    }

    public WorkModel(WorkModel model)
        : this()
    {
        WorkType.Value = model.WorkType.Value;
        Id.Value = model.Id.Value;
        Graph.Value = model.Graph.Value;
        MoneyLevel.Value = model.MoneyLevel.Value;
        MoneyBase.Value = model.MoneyBase.Value;
        StrengthFood.Value = model.StrengthFood.Value;
        StrengthDrink.Value = model.StrengthDrink.Value;
        Feeling.Value = model.Feeling.Value;
        LevelLimit.Value = model.LevelLimit.Value;
        Time.Value = model.Time.Value;
        FinishBonus.Value = model.FinishBonus.Value;

        BorderBrush.Value = model.BorderBrush.Value;
        Background.Value = model.Background.Value;
        ButtonBackground.Value = model.ButtonBackground.Value;
        ButtonForeground.Value = model.ButtonForeground.Value;
        Foreground.Value = model.Foreground.Value;

        Left.Value = model.Left.Value;
        Top.Value = model.Top.Value;
        Width.Value = model.Width.Value;

        foreach (var item in model.I18nDatas)
            I18nDatas[item.Key] = item.Value.Copy();
        CurrentI18nData.Value = I18nDatas[I18nHelper.Current.CultureName.Value];
    }

    public WorkModel(VPet_Simulator.Core.GraphHelper.Work work)
        : this()
    {
        WorkType.Value = work.Type;
        Id.Value = work.Name;
        Graph.Value = work.Graph;
        MoneyLevel.Value = work.MoneyLevel;
        MoneyBase.Value = work.MoneyBase;
        StrengthFood.Value = work.StrengthFood;
        StrengthDrink.Value = work.StrengthDrink;
        Feeling.Value = work.Feeling;
        LevelLimit.Value = work.LevelLimit;
        Time.Value = work.Time;
        FinishBonus.Value = work.FinishBonus;

        BorderBrush.Value = new((Color)ColorConverter.ConvertFromString("#FF" + work.BorderBrush));
        Background.Value = new((Color)ColorConverter.ConvertFromString("#FF" + work.Background));
        Foreground.Value = new((Color)ColorConverter.ConvertFromString("#FF" + work.Foreground));
        ButtonBackground.Value = new(
            (Color)ColorConverter.ConvertFromString("#AA" + work.ButtonBackground)
        );
        ButtonForeground.Value = new(
            (Color)ColorConverter.ConvertFromString("#FF" + work.ButtonForeground)
        );

        Left.Value = work.Left;
        Top.Value = work.Top;
        Width.Value = work.Width;
    }

    public VPet_Simulator.Core.GraphHelper.Work ToWork()
    {
        return new()
        {
            Type = WorkType.Value,
            Name = Id.Value,
            Graph = Graph.Value,
            MoneyLevel = MoneyLevel.Value,
            MoneyBase = MoneyBase.Value,
            StrengthFood = StrengthFood.Value,
            StrengthDrink = StrengthDrink.Value,
            Feeling = Feeling.Value,
            LevelLimit = LevelLimit.Value,
            Time = Time.Value,
            FinishBonus = FinishBonus.Value,
            //
            BorderBrush = BorderBrush.Value.ToString().Substring(3),
            Background = Background.Value.ToString().Substring(3),
            ButtonBackground = ButtonBackground.Value.ToString().Substring(3),
            ButtonForeground = ButtonForeground.Value.ToString().Substring(3),
            Foreground = Foreground.Value.ToString().Substring(3),
            //
            Left = Left.Value,
            Top = Top.Value,
            Width = Width.Value,
        };
    }
}

public class I18nWorkModel
{
    public ObservableValue<string> Name { get; } = new();

    public I18nWorkModel Copy()
    {
        var result = new I18nWorkModel();
        result.Name.Value = Name.Value;
        return result;
    }
}
