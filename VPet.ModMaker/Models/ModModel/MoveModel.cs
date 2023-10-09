using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPet_Simulator.Core;

namespace VPet.ModMaker.Models;

/// <summary>
/// 移动模型
/// </summary>
public class MoveModel
{
    /// <summary>
    /// 移动类型
    /// </summary>
    public static ObservableCollection<GraphHelper.Move.DirectionType> DirectionTypes { get; } =
        new(
            Enum.GetValues(typeof(GraphHelper.Move.DirectionType))
                .Cast<GraphHelper.Move.DirectionType>()
        );

    /// <summary>
    /// 模式类型
    /// </summary>
    public static ObservableCollection<GraphHelper.Move.ModeType> ModeTypes { get; } =
        new(Enum.GetValues(typeof(GraphHelper.Move.ModeType)).Cast<GraphHelper.Move.ModeType>());

    //public ObservableValue<string> Id { get; } = new();
    /// <summary>
    /// 指定动画
    /// </summary>
    public ObservableValue<string> Graph { get; } = new();

    /// <summary>
    /// 移动距离
    /// </summary>
    public ObservableValue<int> Distance { get; } = new(5);

    /// <summary>
    /// 间隔
    /// </summary>
    public ObservableValue<int> Interval { get; } = new(125);

    /// <summary>
    /// 定位长度
    /// </summary>
    public ObservableValue<int> LocateLength { get; } = new();

    /// <summary>
    /// X速度
    /// </summary>
    public ObservableValue<int> SpeedX { get; } = new();

    /// <summary>
    /// Y速度
    /// </summary>
    public ObservableValue<int> SpeedY { get; } = new();

    /// <summary>
    /// 左侧检测距离
    /// </summary>
    public ObservableValue<int> CheckLeft { get; } = new(100);

    /// <summary>
    /// 右侧检测距离
    /// </summary>
    public ObservableValue<int> CheckRight { get; } = new(100);

    /// <summary>
    /// 上方检测距离
    /// </summary>
    public ObservableValue<int> CheckTop { get; } = new(100);

    /// <summary>
    /// 下方检测距离
    /// </summary>
    public ObservableValue<int> CheckBottom { get; } = new(100);

    /// <summary>
    /// 左侧触发距离
    /// </summary>
    public ObservableValue<int> TriggerLeft { get; } = new(100);

    /// <summary>
    /// 右侧触发距离
    /// </summary>
    public ObservableValue<int> TriggerRight { get; } = new(100);

    /// <summary>
    /// 上方触发距离
    /// </summary>
    public ObservableValue<int> TriggerTop { get; } = new(100);

    /// <summary>
    /// 下方触发距离
    /// </summary>
    public ObservableValue<int> TriggerBottom { get; } = new(100);

    /// <summary>
    /// 定位类型
    /// </summary>
    public ObservableEnumFlags<GraphHelper.Move.DirectionType> LocateType { get; } =
        new(GraphHelper.Move.DirectionType.None);

    /// <summary>
    /// 触发类型
    /// </summary>
    public ObservableEnumFlags<GraphHelper.Move.DirectionType> TriggerType { get; } =
        new(GraphHelper.Move.DirectionType.None);

    /// <summary>
    /// 模式
    /// </summary>
    public ObservableEnumFlags<GraphHelper.Move.ModeType> ModeType { get; } =
        new(
            GraphHelper.Move.ModeType.Happy
                | GraphHelper.Move.ModeType.Nomal
                | GraphHelper.Move.ModeType.PoorCondition
                | GraphHelper.Move.ModeType.Ill
        );

    public MoveModel() { }

    public MoveModel(MoveModel model)
        : this()
    {
        //Id.EnumValue = model.Id.EnumValue;
        Graph.Value = model.Graph.Value;
        Distance.Value = model.Distance.Value;
        Interval.Value = model.Interval.Value;
        CheckLeft.Value = model.CheckLeft.Value;
        CheckRight.Value = model.CheckRight.Value;
        CheckTop.Value = model.CheckTop.Value;
        CheckBottom.Value = model.CheckBottom.Value;
        SpeedX.Value = model.SpeedX.Value;
        SpeedY.Value = model.SpeedY.Value;
        LocateLength.Value = model.LocateLength.Value;
        TriggerLeft.Value = model.TriggerLeft.Value;
        TriggerRight.Value = model.TriggerRight.Value;
        TriggerTop.Value = model.TriggerTop.Value;
        TriggerBottom.Value = model.TriggerBottom.Value;
        LocateType.EnumValue.Value = model.LocateType.EnumValue.Value;
        TriggerType.EnumValue.Value = model.TriggerType.EnumValue.Value;
        ModeType.EnumValue.Value = model.ModeType.EnumValue.Value;
    }

    public MoveModel(GraphHelper.Move move)
        : this()
    {
        //Id.EnumValue = move.Id.EnumValue;
        Graph.Value = move.Graph;
        Distance.Value = move.Distance;
        Interval.Value = move.Interval;
        CheckLeft.Value = move.CheckLeft;
        CheckRight.Value = move.CheckRight;
        CheckTop.Value = move.CheckTop;
        CheckBottom.Value = move.CheckBottom;
        SpeedX.Value = move.SpeedX;
        SpeedY.Value = move.SpeedY;
        LocateLength.Value = move.LocateLength;
        TriggerLeft.Value = move.TriggerLeft;
        TriggerRight.Value = move.TriggerRight;
        TriggerTop.Value = move.TriggerTop;
        TriggerBottom.Value = move.TriggerBottom;
        LocateType.EnumValue.Value = move.LocateType;
        TriggerType.EnumValue.Value = move.TriggerType;
        ModeType.EnumValue.Value = move.Mode;
    }

    public GraphHelper.Move ToMove()
    {
        return new()
        {
            Graph = Graph.Value,
            Distance = Distance.Value,
            Interval = Interval.Value,
            CheckLeft = CheckLeft.Value,
            CheckRight = CheckRight.Value,
            CheckTop = CheckTop.Value,
            CheckBottom = CheckBottom.Value,
            SpeedX = SpeedX.Value,
            SpeedY = SpeedY.Value,
            LocateLength = LocateLength.Value,
            TriggerLeft = TriggerLeft.Value,
            TriggerRight = TriggerRight.Value,
            TriggerTop = TriggerTop.Value,
            TriggerBottom = TriggerBottom.Value,
            LocateType = LocateType.EnumValue.Value,
            TriggerType = TriggerType.EnumValue.Value,
            Mode = ModeType.EnumValue.Value,
        };
    }
}
