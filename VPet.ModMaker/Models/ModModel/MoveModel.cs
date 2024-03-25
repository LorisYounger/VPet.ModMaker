using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HKW.HKWUtils.Observable;
using VPet_Simulator.Core;

namespace VPet.ModMaker.Models;

/// <summary>
/// 移动模型
/// </summary>
public class MoveModel : ObservableObjectX<MoveModel>
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

    //#region Id
    //[DebuggerBrowsable(DebuggerBrowsableState.Never)]
    //private string _Id;

    //public string Id { get => _Id; set => SetProperty(ref _Id, value); }
    //#endregion
    #region Graph
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _graph;

    /// <summary>
    /// 指定动画
    /// </summary>
    public string Graph
    {
        get => _graph;
        set => SetProperty(ref _graph, value);
    }
    #endregion

    #region Distance
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private int _distance;

    /// <summary>
    /// 移动距离
    /// </summary>
    public int Distance
    {
        get => _distance;
        set => SetProperty(ref _distance, value);
    }
    #endregion

    #region Interval
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private int _interval;

    /// <summary>
    /// 间隔
    /// </summary>
    public int Interval
    {
        get => _interval;
        set => SetProperty(ref _interval, value);
    }
    #endregion

    #region LocateLength
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private int _locateLength;

    /// <summary>
    /// 定位长度
    /// </summary>
    public int LocateLength
    {
        get => _locateLength;
        set => SetProperty(ref _locateLength, value);
    }
    #endregion

    #region SpeedX
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private int _speedX;

    /// <summary>
    /// X速度
    /// </summary>
    public int SpeedX
    {
        get => _speedX;
        set => SetProperty(ref _speedX, value);
    }
    #endregion

    #region SpeedY
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private int _speedY;

    /// <summary>
    /// Y速度
    /// </summary>
    public int SpeedY
    {
        get => _speedY;
        set => SetProperty(ref _speedY, value);
    }
    #endregion

    #region CheckLeft
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private int _checkLeft;

    /// <summary>
    /// 左侧检测距离
    /// </summary>
    public int CheckLeft
    {
        get => _checkLeft;
        set => SetProperty(ref _checkLeft, value);
    }
    #endregion

    #region CheckRight
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private int _checkRight;

    /// <summary>
    /// 右侧检测距离
    /// </summary>
    public int CheckRight
    {
        get => _checkRight;
        set => SetProperty(ref _checkRight, value);
    }
    #endregion

    #region CheckTop
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private int _checkTop;

    /// <summary>
    /// 上方检测距离
    /// </summary>
    public int CheckTop
    {
        get => _checkTop;
        set => SetProperty(ref _checkTop, value);
    }
    #endregion

    #region CheckBottom
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private int _checkBottom;

    /// <summary>
    /// 下方检测距离
    /// </summary>
    public int CheckBottom
    {
        get => _checkBottom;
        set => SetProperty(ref _checkBottom, value);
    }
    #endregion

    #region TriggerLeft
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private int _triggerLeft;

    /// <summary>
    /// 左侧触发距离
    /// </summary>
    public int TriggerLeft
    {
        get => _triggerLeft;
        set => SetProperty(ref _triggerLeft, value);
    }
    #endregion

    #region TriggerRight
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private int _triggerRight;

    /// <summary>
    /// 右侧触发距离
    /// </summary>
    public int TriggerRight
    {
        get => _triggerRight;
        set => SetProperty(ref _triggerRight, value);
    }
    #endregion

    #region TriggerTop
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private int _triggerTop;

    /// <summary>
    /// 上方触发距离
    /// </summary>
    public int TriggerTop
    {
        get => _triggerTop;
        set => SetProperty(ref _triggerTop, value);
    }
    #endregion

    #region TriggerBottom
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private int _triggerBottom;

    /// <summary>
    /// 下方触发距离
    /// </summary>
    public int TriggerBottom
    {
        get => _triggerBottom;
        set => SetProperty(ref _triggerBottom, value);
    }
    #endregion

    /// <summary>
    /// 定位类型
    /// </summary>
    public ObservableEnumCommand<GraphHelper.Move.DirectionType> LocateType { get; } =
        new(GraphHelper.Move.DirectionType.None);

    /// <summary>
    /// 触发类型
    /// </summary>
    public ObservableEnumCommand<GraphHelper.Move.DirectionType> TriggerType { get; } =
        new(GraphHelper.Move.DirectionType.None);

    /// <summary>
    /// 模式
    /// </summary>
    public ObservableEnumCommand<GraphHelper.Move.ModeType> ModeType { get; } =
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
