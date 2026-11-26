using System.Collections.Immutable;
using System.Runtime.CompilerServices;
using CommunityToolkit.Mvvm.ComponentModel;
using HKW.HKWUtils;
using HKW.HKWUtils.Observable;
using HKW.Mapper;
using VPet_Simulator.Core;

namespace VPet.ModMaker.Models;

/// <summary>
/// 移动模型
/// </summary>
[MapTarget(typeof(GraphHelper.Move), typeof(MoveModelToMoveMapConfig))]
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
    /// 移动方向类型（用于触发类型与检测类型）
    /// </summary>
    public static ImmutableArray<GraphHelper.Move.DirectionType> DirectionTypes =>
        EnumInfo<GraphHelper.Move.DirectionType>.StaticValidValues;

    /// <summary>
    /// 定位类型列表（仅包含本体支持的单一基础方向）
    /// </summary>
    public static ImmutableArray<EnumInfo<GraphHelper.Move.DirectionType>> LocateTypes { get; } =
    [
        EnumInfo<GraphHelper.Move.DirectionType>.GetInfo(GraphHelper.Move.DirectionType.None),
        EnumInfo<GraphHelper.Move.DirectionType>.GetInfo(GraphHelper.Move.DirectionType.Left),
        EnumInfo<GraphHelper.Move.DirectionType>.GetInfo(GraphHelper.Move.DirectionType.Right),
        EnumInfo<GraphHelper.Move.DirectionType>.GetInfo(GraphHelper.Move.DirectionType.Top),
        EnumInfo<GraphHelper.Move.DirectionType>.GetInfo(GraphHelper.Move.DirectionType.Bottom),
    ];

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
    public int Distance { get; set; } = 5;

    /// <summary>
    /// 间隔
    /// </summary>
    [ObservableProperty]
    public int Interval { get; set; } = 125;

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
    public int CheckLeft { get; set; } = 100;

    /// <summary>
    /// 右侧检测距离
    /// </summary>
    [ObservableProperty]
    public int CheckRight { get; set; } = 100;

    /// <summary>
    /// 上方检测距离
    /// </summary>
    [ObservableProperty]
    public int CheckTop { get; set; } = 100;

    /// <summary>
    /// 下方检测距离
    /// </summary>
    [ObservableProperty]
    public int CheckBottom { get; set; } = 100;

    /// <summary>
    /// 左侧触发距离
    /// </summary>
    [ObservableProperty]
    public int TriggerLeft { get; set; } = 100;

    /// <summary>
    /// 右侧触发距离
    /// </summary>
    [ObservableProperty]
    public int TriggerRight { get; set; } = 100;

    /// <summary>
    /// 上方触发距离
    /// </summary>
    [ObservableProperty]
    public int TriggerTop { get; set; } = 100;

    /// <summary>
    /// 下方触发距离
    /// </summary>
    [ObservableProperty]
    public int TriggerBottom { get; set; } = 100;

    /// <summary>
    /// 定位类型
    /// </summary>
    [ObservableProperty]
    public GraphHelper.Move.DirectionType LocateType { get; set; } =
        GraphHelper.Move.DirectionType.None;

    /// <summary>
    /// 触发类型
    /// </summary>
    [MapProperty(typeof(GraphHelper.Move), typeof(ObservableEnumConverter))]
    public ObservableEnum<GraphHelper.Move.DirectionType> TriggerType { get; set; } =
        new(GraphHelper.Move.DirectionType.None);

    /// <summary>
    /// 检测类型
    /// </summary>
    [MapProperty(typeof(GraphHelper.Move), typeof(ObservableEnumConverter))]
    public ObservableEnum<GraphHelper.Move.DirectionType> CheckType { get; set; } =
        new(GraphHelper.Move.DirectionType.None);

    /// <summary>
    /// 模式
    /// </summary>
    [MapProperty(typeof(GraphHelper.Move), Ignore = true)]
    public ObservableEnum<GraphHelper.Move.ModeType> ModeType { get; set; } =
        new(
            GraphHelper.Move.ModeType.Happy
                | GraphHelper.Move.ModeType.Nomal
                | GraphHelper.Move.ModeType.PoorCondition
                | GraphHelper.Move.ModeType.Ill
        );
}

/// <summary>
/// 移动模型映射配置
/// <para>规避核心库中 <see cref="GraphHelper.Move.Mode"/> 赋值时误写入 <c>checkType</c> 的问题</para>
/// </summary>
public sealed class MoveModelToMoveMapConfig : MapperConfigBase<MoveModel, GraphHelper.Move>
{
    [UnsafeAccessor(UnsafeAccessorKind.Method, Name = "set_modeType")]
    private static extern void SetModeType(GraphHelper.Move target, int value);

    public MoveModelToMoveMapConfig(MoveModel source, GraphHelper.Move target)
        : base(source, target) { }

    [MapToConfigAction(MapConfigActionMode.End)]
    public void MapToEnd()
    {
        Target.CheckType = Source.CheckType.Value;
        SetModeType(Target, (int)Source.ModeType.Value);
    }

    [MapFromConfigAction(MapConfigActionMode.End)]
    public void MapFromEnd()
    {
        Source.ModeType = new(Target.Mode);
    }
}
