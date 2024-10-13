using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HKW.HKWReactiveUI;
using HKW.HKWUtils.Observable;
using VPet.ModMaker.ViewModels;
using VPet_Simulator.Core;

namespace VPet.ModMaker.Models;

/// <summary>
/// 移动模型
/// </summary>
public partial class MoveModel : ViewModelBase
{
    public MoveModel() { }

    public MoveModel(MoveModel model)
        : this()
    {
        //Id.EnumValue = model.Id.EnumValue;
        Graph = model.Graph;
        Distance = model.Distance;
        Interval = model.Interval;
        CheckLeft = model.CheckLeft;
        CheckRight = model.CheckRight;
        CheckTop = model.CheckTop;
        CheckBottom = model.CheckBottom;
        SpeedX = model.SpeedX;
        SpeedY = model.SpeedY;
        LocateLength = model.LocateLength;
        TriggerLeft = model.TriggerLeft;
        TriggerRight = model.TriggerRight;
        TriggerTop = model.TriggerTop;
        TriggerBottom = model.TriggerBottom;
        LocateType.Value = model.LocateType.Value;
        TriggerType.Value = model.TriggerType.Value;
        ModeType.Value = model.ModeType.Value;
    }

    public MoveModel(GraphHelper.Move move)
        : this()
    {
        //Id.EnumValue = move.Id.EnumValue;
        Graph = move.Graph;
        Distance = move.Distance;
        Interval = move.Interval;
        CheckLeft = move.CheckLeft;
        CheckRight = move.CheckRight;
        CheckTop = move.CheckTop;
        CheckBottom = move.CheckBottom;
        SpeedX = move.SpeedX;
        SpeedY = move.SpeedY;
        LocateLength = move.LocateLength;
        TriggerLeft = move.TriggerLeft;
        TriggerRight = move.TriggerRight;
        TriggerTop = move.TriggerTop;
        TriggerBottom = move.TriggerBottom;
        LocateType.Value = move.LocateType;
        TriggerType.Value = move.TriggerType;
        ModeType.Value = move.Mode;
    }

    /// <summary>
    /// 移动类型
    /// </summary>
    public static FrozenSet<GraphHelper.Move.DirectionType> DirectionTypes { get; } =
        Enum.GetValues<GraphHelper.Move.DirectionType>().ToFrozenSet();

    /// <summary>
    /// 模式类型
    /// </summary>
    public static FrozenSet<GraphHelper.Move.ModeType> ModeTypes { get; } =
        Enum.GetValues<GraphHelper.Move.ModeType>().ToFrozenSet();

    //#region Id
    //[DebuggerBrowsable(DebuggerBrowsableState.Never)]
    //private string _Id;

    //public string Id { get => _Id; set => SetProperty(ref _Id, value); }
    //#endregion

    /// <summary>
    /// 指定动画
    /// </summary>
    [ReactiveProperty]
    public string Graph { get; set; } = string.Empty;

    /// <summary>
    /// 移动距离
    /// </summary>
    [ReactiveProperty]
    public int Distance { get; set; }

    /// <summary>
    /// 间隔
    /// </summary>
    [ReactiveProperty]
    public int Interval { get; set; }

    /// <summary>
    /// 定位长度
    /// </summary>
    [ReactiveProperty]
    public int LocateLength { get; set; }

    /// <summary>
    /// X速度
    /// </summary>
    [ReactiveProperty]
    public int SpeedX { get; set; }

    /// <summary>
    /// Y速度
    /// </summary>
    [ReactiveProperty]
    public int SpeedY { get; set; }

    /// <summary>
    /// 左侧检测距离
    /// </summary>
    [ReactiveProperty]
    public int CheckLeft { get; set; }

    /// <summary>
    /// 右侧检测距离
    /// </summary>
    [ReactiveProperty]
    public int CheckRight { get; set; }

    /// <summary>
    /// 上方检测距离
    /// </summary>
    [ReactiveProperty]
    public int CheckTop { get; set; }

    /// <summary>
    /// 下方检测距离
    /// </summary>
    [ReactiveProperty]
    public int CheckBottom { get; set; }

    /// <summary>
    /// 左侧触发距离
    /// </summary>
    [ReactiveProperty]
    public int TriggerLeft { get; set; }

    /// <summary>
    /// 右侧触发距离
    /// </summary>
    [ReactiveProperty]
    public int TriggerRight { get; set; }

    /// <summary>
    /// 上方触发距离
    /// </summary>
    [ReactiveProperty]
    public int TriggerTop { get; set; }

    /// <summary>
    /// 下方触发距离
    /// </summary>
    [ReactiveProperty]
    public int TriggerBottom { get; set; }

    /// <summary>
    /// 定位类型
    /// </summary>
    public ObservableEnum<GraphHelper.Move.DirectionType> LocateType { get; } =
        new(GraphHelper.Move.DirectionType.None, (v, f) => v |= f, (v, f) => v &= f);

    /// <summary>
    /// 触发类型
    /// </summary>
    public ObservableEnum<GraphHelper.Move.DirectionType> TriggerType { get; } =
        new(GraphHelper.Move.DirectionType.None, (v, f) => v |= f, (v, f) => v &= f);

    /// <summary>
    /// 模式
    /// </summary>
    public ObservableEnum<GraphHelper.Move.ModeType> ModeType { get; } =
        new(
            GraphHelper.Move.ModeType.Happy
                | GraphHelper.Move.ModeType.Nomal
                | GraphHelper.Move.ModeType.PoorCondition
                | GraphHelper.Move.ModeType.Ill,
            (v, f) => v |= f,
            (v, f) => v &= f
        );

    public GraphHelper.Move ToMove()
    {
        return new()
        {
            Graph = Graph,
            Distance = Distance,
            Interval = Interval,
            CheckLeft = CheckLeft,
            CheckRight = CheckRight,
            CheckTop = CheckTop,
            CheckBottom = CheckBottom,
            SpeedX = SpeedX,
            SpeedY = SpeedY,
            LocateLength = LocateLength,
            TriggerLeft = TriggerLeft,
            TriggerRight = TriggerRight,
            TriggerTop = TriggerTop,
            TriggerBottom = TriggerBottom,
            LocateType = LocateType.Value,
            TriggerType = TriggerType.Value,
            Mode = ModeType.Value,
        };
    }
}
