using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using HKW.HKWUtils;
using HKW.Mapper;
using HKW.MVVM;
using HKW.MVVM.SourceGenerator;
using VPet_Simulator.Core;
using VPet_Simulator.Windows.Interface;
using VPet.ModMaker.ViewModels;

namespace VPet.ModMaker.Models;

/// <summary>
/// 工作模型
/// </summary>
[MapTarget(typeof(GraphHelper.Work), typeof(WorkModelToWorkMapConfig))]
[MapTarget(typeof(WorkModel))]
public sealed partial class WorkModel : ObservableModelBase, IDisposable
{
    /// <inheritdoc/>
    public WorkModel(ObservableI18nResource<string, string> i18nResource)
    {
        I18nResource = i18nResource;
    }

    /// <inheritdoc/>
    /// <param name="model">工作模型</param>
    public WorkModel(WorkModel model, ObservableI18nResource<string, string> i18nResource)
    {
        this.MapFrom(model);
        I18nResource = i18nResource;
    }

    /// <inheritdoc/>
    /// <param name="work">工作</param>
    public WorkModel(GraphHelper.Work work, ObservableI18nResource<string, string> i18nResource)
    {
        this.MapFrom(work);
        I18nResource = i18nResource;
    }

    /// <summary>
    /// 工作类型
    /// </summary>
    public static ImmutableArray<GraphHelper.Work.WorkType> WorkTypes =>
        EnumInfo<GraphHelper.Work.WorkType>.StaticValues;

    /// <summary>
    /// ID
    /// </summary>
    [ObservableProperty]
    [MapProperty(typeof(GraphHelper.Work), nameof(GraphHelper.Work.Name))]
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
    public I18nObserver<WorkModel, string, string> I18nObserver =>
        field ??= new(I18nResource, this, (s, p) => s.OnPropertyChanged(p))
        {
            new(s => s.ID, nameof(Name)),
        };

    /// <summary>
    /// 名称
    /// </summary>
    [MapIgnoreProperty]
    public string Name
    {
        get => I18nObserver.GetDataOrDefault(ID);
        set => I18nObserver.SetData(ID, value);
    }

    /// <summary>
    /// 指定动画
    /// </summary>
    [ObservableProperty]
    public string Graph { get; set; } = string.Empty;

    ///// <summary>
    ///// 收获倍率
    ///// </summary>
    //[ObservableProperty]
    //public double MoneyLevel { get; set; }

    /// <summary>
    /// 收获基础
    /// </summary>
    [ObservableProperty]
    public double MoneyBase { get; set; }

    /// <summary>
    /// 饱食度消耗
    /// </summary>
    [ObservableProperty]
    public double StrengthFood { get; set; }

    /// <summary>
    /// 口渴度消耗
    /// </summary>
    [ObservableProperty]
    public double StrengthDrink { get; set; }

    /// <summary>
    /// 心情消耗
    /// </summary>
    [ObservableProperty]
    public double Feeling { get; set; }

    /// <summary>
    /// 等级倍率
    /// </summary>
    [ObservableProperty]
    public int LevelLimit { get; set; }

    /// <summary>
    /// 时间
    /// </summary>
    [ObservableProperty]
    public int Time { get; set; }

    /// <summary>
    /// 完成奖励倍率
    /// </summary>
    [ObservableProperty]
    public double FinishBonus { get; set; }

    /// <summary>
    /// 是否超模
    /// </summary>
    [MapIgnoreProperty]
    [NotifyPropertyChangeFrom(
        nameof(Type),
        nameof(MoneyBase),
        nameof(StrengthFood),
        nameof(StrengthDrink),
        nameof(Feeling),
        nameof(LevelLimit),
        nameof(FinishBonus)
    )]
    public bool IsOverLoad => ExtensionFunction.IsOverLoad(this.MapTo(_work));

    /// <summary>
    /// 工作类型
    /// </summary>
    [ObservableProperty]
    public VPet_Simulator.Core.GraphHelper.Work.WorkType Type { get; set; }

    public static SolidColorBrush DefaultBorderBrush { get; } =
        new((Color)ColorConverter.ConvertFromString("#FF0290D5"));

    /// <summary>
    /// 边框颜色
    /// </summary>
    [ObservableProperty]
    [MapProperty(typeof(GraphHelper.Work), Ignore = true)]
    [MapProperty(typeof(WorkModel), typeof(BrushToBrushConverter))]
    public SolidColorBrush BorderBrush { get; set; } = DefaultBorderBrush.Clone();
    public static SolidColorBrush DefaultBackground { get; } =
        new((Color)ColorConverter.ConvertFromString("#FF81D4FA"));

    /// <summary>
    /// 背景色
    /// </summary>
    [ObservableProperty]
    [MapProperty(typeof(GraphHelper.Work), Ignore = true)]
    [MapProperty(typeof(WorkModel), typeof(BrushToBrushConverter))]
    public SolidColorBrush Background { get; set; } = DefaultBackground.Clone();
    public static SolidColorBrush DefaultForeground { get; } =
        new((Color)ColorConverter.ConvertFromString("#FF0286C6"));

    /// <summary>
    /// 前景色
    /// </summary>
    [ObservableProperty]
    [MapProperty(typeof(GraphHelper.Work), Ignore = true)]
    [MapProperty(typeof(WorkModel), typeof(BrushToBrushConverter))]
    public SolidColorBrush Foreground { get; set; } = DefaultForeground.Clone();
    public static SolidColorBrush DefaultButtonBackground { get; } =
        new((Color)ColorConverter.ConvertFromString("#AA0286C6"));

    /// <summary>
    /// 按钮背景色
    /// </summary>
    [ObservableProperty]
    [MapProperty(typeof(GraphHelper.Work), Ignore = true)]
    [MapProperty(typeof(WorkModel), typeof(BrushToBrushConverter))]
    public SolidColorBrush ButtonBackground { get; set; } = DefaultButtonBackground.Clone();
    public static SolidColorBrush DefaultButtonForeground { get; } =
        new((Color)ColorConverter.ConvertFromString("#FFFFFFFF"));

    /// <summary>
    /// 按钮前景色
    /// </summary>
    [ObservableProperty]
    [MapProperty(typeof(GraphHelper.Work), Ignore = true)]
    [MapProperty(typeof(WorkModel), typeof(BrushToBrushConverter))]
    public SolidColorBrush ButtonForeground { get; set; } = DefaultButtonForeground.Clone();

    /// <summary>
    /// X位置
    /// </summary>
    [ObservableProperty]
    [MapProperty(typeof(GraphHelper.Work), Ignore = true)]
    public double Left { get; set; }

    /// <summary>
    /// Y位置
    /// </summary>
    [ObservableProperty]
    [MapProperty(typeof(GraphHelper.Work), Ignore = true)]
    public double Top { get; set; }

    /// <summary>
    /// 宽度
    /// </summary>
    [ObservableProperty]
    [MapProperty(typeof(GraphHelper.Work), Ignore = true)]
    public double Width { get; set; }

    private readonly GraphHelper.Work _work = new();

    /// <summary>
    /// 修复超模
    /// </summary>
    public void FixOverLoad()
    {
        this.MapTo(_work);
        _work.FixOverLoad();
        this.MapFrom(_work);
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        I18nObserver?.Dispose();
    }
}

public sealed class BrushToBrushConverter : IMapConverter<SolidColorBrush, SolidColorBrush>
{
    private static readonly SolidColorBrush _default = new(
        new()
        {
            A = byte.MaxValue,
            R = byte.MaxValue,
            G = byte.MaxValue,
            B = byte.MaxValue,
        }
    );

    public SolidColorBrush Convert(SolidColorBrush value, MapConvertArgs args)
    {
        if (value is null)
            return _default.Clone();
        return value.Clone();
    }

    public SolidColorBrush ConvertBack(SolidColorBrush value, MapConvertArgs args)
    {
        if (value is null)
            return _default.Clone();
        return value.Clone();
    }
}

public sealed class WorkModelToWorkMapConfig : MapperConfigBase<WorkModel, GraphHelper.Work>
{
    public WorkModelToWorkMapConfig(WorkModel source, GraphHelper.Work target)
        : base(source, target) { }

    [MapToConfigAction(MapConfigActionMode.End)]
    public void MapToEnd()
    {
        Target.BorderBrush = Source.BorderBrush.ToString()[3..];
        Target.Background = Source.Background.ToString()[3..];
        Target.Foreground = Source.Foreground.ToString()[3..];
        Target.ButtonBackground = Source.ButtonBackground.ToString()[3..];
        Target.ButtonForeground = Source.ButtonForeground.ToString()[3..];
        Target.Left = Source.Left;
        Target.Top = Source.Top;
        Target.Width = Source.Width;
    }

    [MapFromConfigAction(MapConfigActionMode.End)]
    public void MapFromEnd()
    {
        Source.BorderBrush = new(
            (Color)ColorConverter.ConvertFromString("#FF" + Target.BorderBrush)
        );
        Source.Background = new((Color)ColorConverter.ConvertFromString("#FF" + Target.Background));
        Source.Foreground = new((Color)ColorConverter.ConvertFromString("#FF" + Target.Foreground));
        Source.ButtonBackground = new(
            (Color)ColorConverter.ConvertFromString("#AA" + Target.ButtonBackground)
        );
        Source.ButtonForeground = new(
            (Color)ColorConverter.ConvertFromString("#FF" + Target.ButtonForeground)
        );
        Source.Left = Target.Left;
        Source.Top = Target.Top;
        Source.Width = Target.Width;
    }
}
