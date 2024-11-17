using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HKW.HKWMapper;
using HKW.HKWReactiveUI;
using HKW.HKWUtils.Observable;
using VPet.ModMaker.ViewModels;
using VPet_Simulator.Core;

namespace VPet.ModMaker.Models;

/// <summary>
/// 移动模型
/// </summary>
[MapTo(typeof(GraphHelper.Move), MapperConfig = typeof(MoveModelMapToMoveConfig))]
[MapFrom(typeof(GraphHelper.Move), MapperConfig = typeof(MoveModelMapFromMoveConfig))]
[MapFrom(typeof(MoveModel), MapperConfig = typeof(MoveModelMapFromMoveModelConfig))]
public partial class MoveModel : ViewModelBase
{
    /// <inheritdoc/>
    public MoveModel() { }

    /// <inheritdoc/>
    /// <param name="model">移动模型</param>
    public MoveModel(MoveModel model)
        : this()
    {
        this.MapFromMoveModel(model);
    }

    /// <inheritdoc/>
    /// <param name="move">移动</param>
    public MoveModel(GraphHelper.Move move)
        : this()
    {
        this.MapFromMove(move);
    }

    /// <summary>
    /// 移动类型
    /// </summary>
    public static FrozenSet<GraphHelper.Move.DirectionType> DirectionTypes =>
        EnumInfo<GraphHelper.Move.DirectionType>.Values;

    /// <summary>
    /// 模式类型
    /// </summary>
    public static FrozenSet<GraphHelper.Move.ModeType> ModeTypes =>
        EnumInfo<GraphHelper.Move.ModeType>.Values;

    //#region Id
    //[DebuggerBrowsable(DebuggerBrowsableState.Never)]
    //private string _Id;

    //public string Id { get => _Id; set => SetProperty(ref _Id, value); }
    //#endregion

    /// <summary>
    /// 指定动画
    /// </summary>
    [MoveModelMapToMoveProperty(nameof(GraphHelper.Move.Graph))]
    [MoveModelMapFromMoveProperty(nameof(GraphHelper.Move.Graph))]
    [ReactiveProperty]
    public string Graph { get; set; } = string.Empty;

    /// <summary>
    /// 移动距离
    /// </summary>
    [MoveModelMapToMoveProperty(nameof(GraphHelper.Move.Distance))]
    [MoveModelMapFromMoveProperty(nameof(GraphHelper.Move.Distance))]
    [ReactiveProperty]
    public int Distance { get; set; }

    /// <summary>
    /// 间隔
    /// </summary>
    [MoveModelMapToMoveProperty(nameof(GraphHelper.Move.Interval))]
    [MoveModelMapFromMoveProperty(nameof(GraphHelper.Move.Interval))]
    [ReactiveProperty]
    public int Interval { get; set; }

    /// <summary>
    /// 定位长度
    /// </summary>
    [MoveModelMapToMoveProperty(nameof(GraphHelper.Move.LocateLength))]
    [MoveModelMapFromMoveProperty(nameof(GraphHelper.Move.LocateLength))]
    [ReactiveProperty]
    public int LocateLength { get; set; }

    /// <summary>
    /// X速度
    /// </summary>
    [MoveModelMapToMoveProperty(nameof(GraphHelper.Move.SpeedX))]
    [MoveModelMapFromMoveProperty(nameof(GraphHelper.Move.SpeedX))]
    [ReactiveProperty]
    public int SpeedX { get; set; }

    /// <summary>
    /// Y速度
    /// </summary>
    [MoveModelMapToMoveProperty(nameof(GraphHelper.Move.SpeedY))]
    [MoveModelMapFromMoveProperty(nameof(GraphHelper.Move.SpeedY))]
    [ReactiveProperty]
    public int SpeedY { get; set; }

    /// <summary>
    /// 左侧检测距离
    /// </summary>
    [MoveModelMapToMoveProperty(nameof(GraphHelper.Move.CheckLeft))]
    [MoveModelMapFromMoveProperty(nameof(GraphHelper.Move.CheckLeft))]
    [ReactiveProperty]
    public int CheckLeft { get; set; }

    /// <summary>
    /// 右侧检测距离
    /// </summary>
    [MoveModelMapToMoveProperty(nameof(GraphHelper.Move.CheckRight))]
    [MoveModelMapFromMoveProperty(nameof(GraphHelper.Move.CheckRight))]
    [ReactiveProperty]
    public int CheckRight { get; set; }

    /// <summary>
    /// 上方检测距离
    /// </summary>
    [MoveModelMapToMoveProperty(nameof(GraphHelper.Move.CheckTop))]
    [MoveModelMapFromMoveProperty(nameof(GraphHelper.Move.CheckTop))]
    [ReactiveProperty]
    public int CheckTop { get; set; }

    /// <summary>
    /// 下方检测距离
    /// </summary>
    [MoveModelMapToMoveProperty(nameof(GraphHelper.Move.CheckBottom))]
    [MoveModelMapFromMoveProperty(nameof(GraphHelper.Move.CheckBottom))]
    [ReactiveProperty]
    public int CheckBottom { get; set; }

    /// <summary>
    /// 左侧触发距离
    /// </summary>
    [MoveModelMapToMoveProperty(nameof(GraphHelper.Move.TriggerLeft))]
    [MoveModelMapFromMoveProperty(nameof(GraphHelper.Move.TriggerLeft))]
    [ReactiveProperty]
    public int TriggerLeft { get; set; }

    /// <summary>
    /// 右侧触发距离
    /// </summary>
    [MoveModelMapToMoveProperty(nameof(GraphHelper.Move.TriggerRight))]
    [MoveModelMapFromMoveProperty(nameof(GraphHelper.Move.TriggerRight))]
    [ReactiveProperty]
    public int TriggerRight { get; set; }

    /// <summary>
    /// 上方触发距离
    /// </summary>
    [MoveModelMapToMoveProperty(nameof(GraphHelper.Move.TriggerTop))]
    [MoveModelMapFromMoveProperty(nameof(GraphHelper.Move.TriggerTop))]
    [ReactiveProperty]
    public int TriggerTop { get; set; }

    /// <summary>
    /// 下方触发距离
    /// </summary>
    [MoveModelMapToMoveProperty(nameof(GraphHelper.Move.TriggerBottom))]
    [MoveModelMapFromMoveProperty(nameof(GraphHelper.Move.TriggerBottom))]
    [ReactiveProperty]
    public int TriggerBottom { get; set; }

    /// <summary>
    /// 定位类型
    /// </summary>
    public ObservableEnum<GraphHelper.Move.DirectionType> LocateType { get; } =
        new(GraphHelper.Move.DirectionType.None);

    /// <summary>
    /// 触发类型
    /// </summary>
    public ObservableEnum<GraphHelper.Move.DirectionType> TriggerType { get; } =
        new(GraphHelper.Move.DirectionType.None);

    /// <summary>
    /// 模式
    /// </summary>
    public ObservableEnum<GraphHelper.Move.ModeType> ModeType { get; } =
        new(
            GraphHelper.Move.ModeType.Happy
                | GraphHelper.Move.ModeType.Nomal
                | GraphHelper.Move.ModeType.PoorCondition
                | GraphHelper.Move.ModeType.Ill
        );
}

internal class MoveModelMapToMoveConfig : MapperConfig<MoveModel, GraphHelper.Move>
{
    public MoveModelMapToMoveConfig()
    {
        AddMap(
            x => x.LocateType,
            (s, t) =>
            {
                t.LocateType = s.LocateType.Value;
            }
        );
        AddMap(
            x => x.TriggerType,
            (s, t) =>
            {
                t.TriggerType = s.TriggerType.Value;
            }
        );
        AddMap(
            x => x.ModeType,
            (s, t) =>
            {
                t.Mode = s.ModeType.Value;
            }
        );
    }
}

internal class MoveModelMapFromMoveConfig : MapperConfig<MoveModel, GraphHelper.Move>
{
    public MoveModelMapFromMoveConfig()
    {
        AddMap(
            x => x.LocateType,
            (s, t) =>
            {
                s.LocateType.Value = t.LocateType;
            }
        );
        AddMap(
            x => x.TriggerType,
            (s, t) =>
            {
                s.TriggerType.Value = t.TriggerType;
            }
        );
        AddMap(
            x => x.ModeType,
            (s, t) =>
            {
                s.ModeType.Value = t.Mode;
            }
        );
    }
}

internal class MoveModelMapFromMoveModelConfig : MapperConfig<MoveModel, MoveModel>
{
    public MoveModelMapFromMoveModelConfig()
    {
        AddMap(
            x => x.LocateType,
            (s, t) =>
            {
                s.LocateType.Value = t.LocateType.Value;
            }
        );
        AddMap(
            x => x.TriggerType,
            (s, t) =>
            {
                s.TriggerType.Value = t.TriggerType.Value;
            }
        );
        AddMap(
            x => x.ModeType,
            (s, t) =>
            {
                s.ModeType.Value = t.ModeType.Value;
            }
        );
    }
}
