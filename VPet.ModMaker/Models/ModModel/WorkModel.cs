using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HKW.HKWMapper;
using HKW.HKWReactiveUI;
using HKW.HKWUtils;
using HKW.HKWUtils.Observable;
using VPet.ModMaker.ViewModels;
using VPet_Simulator.Core;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.Models;

/// <summary>
/// 工作模型
/// </summary>
[MapTo(
    typeof(GraphHelper.Work),
    MapperConfig = typeof(WorkModelMapToWorkConfig),
    ScrutinyMode = true
)]
[MapFrom(typeof(GraphHelper.Work), MapperConfig = typeof(WorkModelMapFromWorkConfig))]
[MapFrom(typeof(WorkModel), MapperConfig = typeof(WorkModelMapFromWorkModelConfig))]
public partial class WorkModel : ViewModelBase
{
    /// <inheritdoc/>
    public WorkModel() { }

    /// <inheritdoc/>
    /// <param name="model">工作模型</param>
    public WorkModel(WorkModel model)
        : this()
    {
        this.MapFromWorkModel(model);
    }

    /// <inheritdoc/>
    /// <param name="work">工作</param>
    public WorkModel(GraphHelper.Work work)
        : this()
    {
        this.MapFromWork(work);
    }

    /// <summary>
    /// 工作类型
    /// </summary>
    public static FrozenSet<GraphHelper.Work.WorkType> WorkTypes =>
        EnumInfo<GraphHelper.Work.WorkType>.Values;

    /// <summary>
    /// ID
    /// </summary>
    [WorkModelMapToWorkProperty(nameof(GraphHelper.Work.Name))]
    [WorkModelMapFromWorkProperty(nameof(GraphHelper.Work.Name))]
    [ReactiveProperty]
    public string ID { get; set; } = string.Empty;

    /// <summary>
    /// I18n资源
    /// </summary>
    [MapIgnoreProperty]
    [ReactiveProperty]
    public required I18nResource<string, string> I18nResource { get; set; }

    partial void OnI18nResourceChanged(
        I18nResource<string, string> oldValue,
        I18nResource<string, string> newValue
    )
    {
        oldValue?.I18nObjects.Remove(I18nObject);
        newValue?.I18nObjects.Add(I18nObject);
    }

    /// <summary>
    /// I18n对象
    /// </summary>
    [MapIgnoreProperty]
    [NotifyPropertyChangeFrom("")]
    public I18nObject<string, string> I18nObject => new(this);

    /// <summary>
    /// 名称
    /// </summary>
    [MapIgnoreProperty]
    [ReactiveI18nProperty(nameof(I18nResource), nameof(I18nObject), nameof(ID), true)]
    public string Name
    {
        get => I18nResource.GetCurrentCultureDataOrDefault(ID);
        set => I18nResource.SetCurrentCultureData(ID, value);
    }

    /// <summary>
    /// 指定动画
    /// </summary>
    [WorkModelMapToWorkProperty(nameof(GraphHelper.Work.Graph))]
    [WorkModelMapFromWorkProperty(nameof(GraphHelper.Work.Graph))]
    [ReactiveProperty]
    public string Graph { get; set; } = string.Empty;

    ///// <summary>
    ///// 收获倍率
    ///// </summary>
    //[ReactiveProperty]
    //public double MoneyLevel { get; set; }

    /// <summary>
    /// 收获基础
    /// </summary>
    [WorkModelMapToWorkProperty(nameof(GraphHelper.Work.MoneyBase))]
    [WorkModelMapFromWorkProperty(nameof(GraphHelper.Work.MoneyBase))]
    [ReactiveProperty]
    public double MoneyBase { get; set; }

    /// <summary>
    /// 饱食度消耗
    /// </summary>
    [WorkModelMapToWorkProperty(nameof(GraphHelper.Work.StrengthFood))]
    [WorkModelMapFromWorkProperty(nameof(GraphHelper.Work.StrengthFood))]
    [ReactiveProperty]
    public double StrengthFood { get; set; }

    /// <summary>
    /// 口渴度消耗
    /// </summary>
    [WorkModelMapToWorkProperty(nameof(GraphHelper.Work.StrengthDrink))]
    [WorkModelMapFromWorkProperty(nameof(GraphHelper.Work.StrengthDrink))]
    [ReactiveProperty]
    public double StrengthDrink { get; set; }

    /// <summary>
    /// 心情消耗
    /// </summary>
    [WorkModelMapToWorkProperty(nameof(GraphHelper.Work.Feeling))]
    [WorkModelMapFromWorkProperty(nameof(GraphHelper.Work.Feeling))]
    [ReactiveProperty]
    public double Feeling { get; set; }

    /// <summary>
    /// 等级倍率
    /// </summary>
    [WorkModelMapToWorkProperty(nameof(GraphHelper.Work.LevelLimit))]
    [WorkModelMapFromWorkProperty(nameof(GraphHelper.Work.LevelLimit))]
    [ReactiveProperty]
    public int LevelLimit { get; set; }

    /// <summary>
    /// 时间
    /// </summary>
    [WorkModelMapToWorkProperty(nameof(GraphHelper.Work.Time))]
    [WorkModelMapFromWorkProperty(nameof(GraphHelper.Work.Time))]
    [ReactiveProperty]
    public int Time { get; set; }

    /// <summary>
    /// 完成奖励倍率
    /// </summary>
    [WorkModelMapToWorkProperty(nameof(GraphHelper.Work.FinishBonus))]
    [WorkModelMapFromWorkProperty(nameof(GraphHelper.Work.FinishBonus))]
    [ReactiveProperty]
    public double FinishBonus { get; set; }

    /// <summary>
    /// 是否超模
    /// </summary>
    [MapIgnoreProperty]
    [NotifyPropertyChangeFrom(
        nameof(WorkType),
        nameof(MoneyBase),
        nameof(StrengthFood),
        nameof(StrengthDrink),
        nameof(Feeling),
        nameof(Feeling),
        nameof(LevelLimit),
        nameof(FinishBonus)
    )]
    public bool IsOverLoad => ExtensionFunction.IsOverLoad(this.MapToWork(_work));

    ///// <summary>
    ///// 图片
    ///// </summary>
    //[ReactiveProperty]
    //public BitmapImage? Image { get; set; }

    /// <summary>
    /// 工作类型
    /// </summary>
    [WorkModelMapToWorkProperty(nameof(GraphHelper.Work.Type))]
    [WorkModelMapFromWorkProperty(nameof(GraphHelper.Work.Type))]
    [ReactiveProperty]
    public VPet_Simulator.Core.GraphHelper.Work.WorkType WorkType { get; set; }

    /// <summary>
    /// 边框颜色
    /// </summary>
    [ReactiveProperty]
    public SolidColorBrush BorderBrush { get; set; } =
        new((Color)ColorConverter.ConvertFromString("#FF0290D5"));

    /// <summary>
    /// 背景色
    /// </summary>
    [ReactiveProperty]
    public SolidColorBrush Background { get; set; } =
        new((Color)ColorConverter.ConvertFromString("#FF81D4FA"));

    /// <summary>
    /// 前景色
    /// </summary>
    [ReactiveProperty]
    public SolidColorBrush Foreground { get; set; } =
        new((Color)ColorConverter.ConvertFromString("#FF0286C6"));

    /// <summary>
    /// 按钮背景色
    /// </summary>
    [ReactiveProperty]
    public SolidColorBrush ButtonBackground { get; set; } =
        new((Color)ColorConverter.ConvertFromString("#AA0286C6"));

    /// <summary>
    /// 按钮前景色
    /// </summary>
    [ReactiveProperty]
    public SolidColorBrush ButtonForeground { get; set; } =
        new((Color)ColorConverter.ConvertFromString("#FFFFFFFF"));

    /// <summary>
    /// X位置
    /// </summary>
    [ReactiveProperty]
    public double Left { get; set; }

    /// <summary>
    /// Y位置
    /// </summary>
    [ReactiveProperty]
    public double Top { get; set; }

    /// <summary>
    /// 宽度
    /// </summary>
    [ReactiveProperty]
    public double Width { get; set; }

    private readonly GraphHelper.Work _work = new();

    /// <summary>
    /// 修复超模
    /// </summary>
    public void FixOverLoad()
    {
        this.MapToWork(_work);
        _work.FixOverLoad();
        this.MapFromWork(_work);
    }

    /// <inheritdoc/>
    public void Close()
    {
        I18nResource.I18nObjects.Remove(I18nObject);
        I18nObject.Close();
    }
}

internal class WorkModelMapFromWorkModelConfig : MapperConfig<WorkModel, WorkModel>
{
    public WorkModelMapFromWorkModelConfig()
    {
        AddMap(
            x => x.BorderBrush,
            (s, t) =>
            {
                s.BorderBrush = t.BorderBrush.CloneCurrentValue();
            }
        );
        AddMap(
            x => x.Background,
            (s, t) =>
            {
                s.Background = t.Background.CloneCurrentValue();
            }
        );
        AddMap(
            x => x.Foreground,
            (s, t) =>
            {
                s.Foreground = t.Foreground.CloneCurrentValue();
            }
        );
        AddMap(
            x => x.ButtonBackground,
            (s, t) =>
            {
                s.ButtonBackground = t.ButtonBackground.CloneCurrentValue();
            }
        );
        AddMap(
            x => x.ButtonForeground,
            (s, t) =>
            {
                s.ButtonForeground = t.ButtonForeground.CloneCurrentValue();
            }
        );
    }
}

internal class WorkModelMapFromWorkConfig : MapperConfig<WorkModel, GraphHelper.Work>
{
    public WorkModelMapFromWorkConfig()
    {
        AddMap(
            x => x.BorderBrush,
            (s, t) =>
            {
                s.BorderBrush = new((Color)ColorConverter.ConvertFromString("#FF" + t.BorderBrush));
            }
        );
        AddMap(
            x => x.Background,
            (s, t) =>
            {
                s.Background = new((Color)ColorConverter.ConvertFromString("#FF" + t.Background));
            }
        );
        AddMap(
            x => x.Foreground,
            (s, t) =>
            {
                s.Foreground = new((Color)ColorConverter.ConvertFromString("#FF" + t.Foreground));
            }
        );
        AddMap(
            x => x.ButtonBackground,
            (s, t) =>
            {
                s.ButtonBackground = new(
                    (Color)ColorConverter.ConvertFromString("#FF" + t.ButtonBackground)
                );
            }
        );
        AddMap(
            x => x.ButtonForeground,
            (s, t) =>
            {
                s.ButtonForeground = new(
                    (Color)ColorConverter.ConvertFromString("#FF" + t.ButtonForeground)
                );
            }
        );
        AddMap(
            x => x.Left,
            (s, t) =>
            {
                s.Left = t.Left;
            }
        );
        AddMap(
            x => x.Top,
            (s, t) =>
            {
                s.Top = t.Top;
            }
        );
        AddMap(
            x => x.Width,
            (s, t) =>
            {
                s.Width = t.Width;
            }
        );
    }
}

internal class WorkModelMapToWorkConfig : MapperConfig<WorkModel, GraphHelper.Work>
{
    public WorkModelMapToWorkConfig()
    {
        AddMap(
            x => x.BorderBrush,
            (s, t) =>
            {
                t.BorderBrush = s.BorderBrush.ToString()[3..];
            }
        );
        AddMap(
            x => x.Background,
            (s, t) =>
            {
                t.Background = s.Background.ToString()[3..];
            }
        );
        AddMap(
            x => x.ButtonBackground,
            (s, t) =>
            {
                t.ButtonBackground = s.ButtonBackground.ToString()[3..];
            }
        );
        AddMap(
            x => x.ButtonForeground,
            (s, t) =>
            {
                t.ButtonForeground = s.ButtonForeground.ToString()[3..];
            }
        );
        AddMap(
            x => x.Foreground,
            (s, t) =>
            {
                t.Foreground = s.Foreground.ToString()[3..];
            }
        );
        AddMap(
            x => x.Left,
            (s, t) =>
            {
                t.Left = s.Left;
            }
        );
        AddMap(
            x => x.Top,
            (s, t) =>
            {
                t.Top = s.Top;
            }
        );
        AddMap(
            x => x.Width,
            (s, t) =>
            {
                t.Width = s.Width;
            }
        );
    }
}
