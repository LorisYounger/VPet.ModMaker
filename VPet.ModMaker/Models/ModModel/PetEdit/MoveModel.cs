using System.Collections.Frozen;
using System.Collections.Immutable;
using CommunityToolkit.Mvvm.ComponentModel;
using HKW.HKWUtils;
using HKW.HKWUtils.Observable;
using HKW.Mapper;
using HKW.MVVM;
using VPet_Simulator.Core;
using VPet.ModMaker.ViewModels;

namespace VPet.ModMaker.Models;

/// <summary>
/// 移动模型
/// </summary>
[MapTarget(typeof(GraphHelper.Move))]
[MapTarget(typeof(MoveModel))]
public partial class MoveModel : ObservableModelBase
{
    /// <inheritdoc/>
    public MoveModel() { }

    /// <inheritdoc/>
    /// <param name="model">移动模型</param>
    public MoveModel(MoveModel model)
    {
        this.MapFrom(model);
    }

    /// <inheritdoc/>
    /// <param name="move">移动</param>
    public MoveModel(GraphHelper.Move move)
    {
        this.MapFrom(move);
    }

    /// <summary>
    /// 移动类型
    /// </summary>
    public static ImmutableArray<GraphHelper.Move.DirectionType> DirectionTypes =>
        EnumInfo<GraphHelper.Move.DirectionType>.StaticValidValues;

    /// <summary>
    /// 模式类型
    /// </summary>
    public static ImmutableArray<GraphHelper.Move.ModeType> ModeTypes =>
        EnumInfo<GraphHelper.Move.ModeType>.StaticValues;

    ///// <summary>
    ///// ID
    ///// </summary>
    //[ObservableProperty]
    //public string ID { get; set; } = string.Empty;

    /// <summary>
    /// 指定动画
    /// </summary>
    [ObservableProperty]
    public string Graph { get; set; } = string.Empty;

    /// <summary>
    /// 移动距离
    /// </summary>
    [ObservableProperty]
    public int Distance { get; set; }

    /// <summary>
    /// 间隔
    /// </summary>
    [ObservableProperty]
    public int Interval { get; set; }

    /// <summary>
    /// 定位长度
    /// </summary>
    [ObservableProperty]
    public int LocateLength { get; set; }

    /// <summary>
    /// X速度
    /// </summary>
    [ObservableProperty]
    public int SpeedX { get; set; }

    /// <summary>
    /// Y速度
    /// </summary>
    [ObservableProperty]
    public int SpeedY { get; set; }

    /// <summary>
    /// 左侧检测距离
    /// </summary>
    [ObservableProperty]
    public int CheckLeft { get; set; }

    /// <summary>
    /// 右侧检测距离
    /// </summary>
    [ObservableProperty]
    public int CheckRight { get; set; }

    /// <summary>
    /// 上方检测距离
    /// </summary>
    [ObservableProperty]
    public int CheckTop { get; set; }

    /// <summary>
    /// 下方检测距离
    /// </summary>
    [ObservableProperty]
    public int CheckBottom { get; set; }

    /// <summary>
    /// 左侧触发距离
    /// </summary>
    [ObservableProperty]
    public int TriggerLeft { get; set; }

    /// <summary>
    /// 右侧触发距离
    /// </summary>
    [ObservableProperty]
    public int TriggerRight { get; set; }

    /// <summary>
    /// 上方触发距离
    /// </summary>
    [ObservableProperty]
    public int TriggerTop { get; set; }

    /// <summary>
    /// 下方触发距离
    /// </summary>
    [ObservableProperty]
    public int TriggerBottom { get; set; }

    /// <summary>
    /// 定位类型
    /// </summary>
    [MapProperty(typeof(GraphHelper.Move), typeof(ObservableEnumConverter))]
    public ObservableEnum<GraphHelper.Move.DirectionType> LocateType { get; set; } =
        new(GraphHelper.Move.DirectionType.None);

    /// <summary>
    /// 触发类型
    /// </summary>
    [MapProperty(typeof(GraphHelper.Move), typeof(ObservableEnumConverter))]
    public ObservableEnum<GraphHelper.Move.DirectionType> TriggerType { get; set; } =
        new(GraphHelper.Move.DirectionType.None);

    /// <summary>
    /// 模式
    /// </summary>
    [MapProperty(
        typeof(GraphHelper.Move),
        nameof(GraphHelper.Move.Mode),
        typeof(ObservableEnumConverter)
    )]
    public ObservableEnum<GraphHelper.Move.ModeType> ModeType { get; set; } =
        new(
            GraphHelper.Move.ModeType.Happy
                | GraphHelper.Move.ModeType.Nomal
                | GraphHelper.Move.ModeType.PoorCondition
                | GraphHelper.Move.ModeType.Ill
        );
}
