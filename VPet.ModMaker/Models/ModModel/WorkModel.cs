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
using HKW.HKWUtils.Observable;
using Mapster;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.Models;

/// <summary>
/// 工作模型
/// </summary>
public class WorkModel : ObservableObjectX
{
    public WorkModel()
    {
        PropertyChanged += WorkModel_PropertyChanged;
    }

    private static readonly FrozenSet<string> _notifyIsOverLoad = FrozenSet.ToFrozenSet(
        [
            nameof(WorkType),
            nameof(MoneyBase),
            nameof(StrengthFood),
            nameof(StrengthDrink),
            nameof(Feeling),
            nameof(Feeling),
            nameof(LevelLimit),
            nameof(FinishBonus)
        ]
    );

    private void WorkModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is not null && _notifyIsOverLoad.Contains(e.PropertyName))
        {
            IsOverLoad = VPet_Simulator.Windows.Interface.ExtensionFunction.IsOverLoad(ToWork());
        }
    }

    public WorkModel(WorkModel model)
        : this()
    {
        WorkType = model.WorkType;
        ID = model.ID;
        Graph = model.Graph;
        //MoneyLevel = model.MoneyLevel;
        MoneyBase = model.MoneyBase;
        StrengthFood = model.StrengthFood;
        StrengthDrink = model.StrengthDrink;
        Feeling = model.Feeling;
        LevelLimit = model.LevelLimit;
        Time = model.Time;
        FinishBonus = model.FinishBonus;

        BorderBrush = model.BorderBrush;
        Background = model.Background;
        ButtonBackground = model.ButtonBackground;
        ButtonForeground = model.ButtonForeground;
        Foreground = model.Foreground;
        Left = model.Left;
        Top = model.Top;
        Width = model.Width;

        //Image = model.Image?.CloneStream();
    }

    public WorkModel(VPet_Simulator.Core.GraphHelper.Work work)
        : this()
    {
        WorkType = work.Type;
        ID = work.Name;
        Graph = work.Graph;
        //MoneyLevel = work.MoneyLevel;
        MoneyBase = work.MoneyBase;
        StrengthFood = work.StrengthFood;
        StrengthDrink = work.StrengthDrink;
        Feeling = work.Feeling;
        LevelLimit = work.LevelLimit;
        Time = work.Time;
        FinishBonus = work.FinishBonus;

        BorderBrush = new((Color)ColorConverter.ConvertFromString("#FF" + work.BorderBrush));
        Background = new((Color)ColorConverter.ConvertFromString("#FF" + work.Background));
        Foreground = new((Color)ColorConverter.ConvertFromString("#FF" + work.Foreground));
        ButtonBackground = new(
            (Color)ColorConverter.ConvertFromString("#AA" + work.ButtonBackground)
        );
        ButtonForeground = new(
            (Color)ColorConverter.ConvertFromString("#FF" + work.ButtonForeground)
        );
        Left = work.Left;
        Top = work.Top;
        Width = work.Width;
    }

    /// <summary>
    /// 工作类型
    /// </summary>
    public static FrozenSet<VPet_Simulator.Core.GraphHelper.Work.WorkType> WorkTypes { get; } =
        Enum.GetValues<VPet_Simulator.Core.GraphHelper.Work.WorkType>().ToFrozenSet();

    #region ID
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _id = string.Empty;

    /// <summary>
    /// ID
    /// </summary>
    public string ID
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }
    #endregion

    #region I18nData
    [AdaptIgnore]
    private I18nResource<string, string> _i18nResource = null!;

    [AdaptIgnore]
    public required I18nResource<string, string> I18nResource
    {
        get => _i18nResource;
        set
        {
            if (_i18nResource is not null)
                I18nResource.I18nObjectInfos.Remove(this);
            _i18nResource = value;
            InitializeI18nResource();
        }
    }

    public void InitializeI18nResource()
    {
        I18nResource?.I18nObjectInfos.Add(
            this,
            new I18nObjectInfo<string, string>(this, OnPropertyChanged).AddPropertyInfo(
                nameof(ID),
                ID,
                nameof(Name),
                true
            )
        );
    }

    [AdaptIgnore]
    public string Name
    {
        get => I18nResource.GetCurrentCultureDataOrDefault(ID);
        set => I18nResource.SetCurrentCultureData(ID, value);
    }
    #endregion

    #region Graph
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _graph = string.Empty;

    /// <summary>
    /// 指定动画
    /// </summary>
    public string Graph
    {
        get => _graph;
        set => SetProperty(ref _graph, value);
    }
    #endregion

    //#region MoneyLevel
    //[DebuggerBrowsable(DebuggerBrowsableState.Never)]
    //private double _moneyLevel;

    ///// <summary>
    ///// 收获倍率
    ///// </summary>
    //public double MoneyLevel
    //{
    //    get => _moneyLevel;
    //    set => SetProperty(ref _moneyLevel, value);
    //}
    //#endregion

    #region MoneyBase
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private double _moneyBase;

    /// <summary>
    /// 收获基础
    /// </summary>
    public double MoneyBase
    {
        get => _moneyBase;
        set => SetProperty(ref _moneyBase, value);
    }
    #endregion

    #region StrengthFood
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private double _strengthFood;

    /// <summary>
    /// 饱食度消耗
    /// </summary>
    public double StrengthFood
    {
        get => _strengthFood;
        set => SetProperty(ref _strengthFood, value);
    }
    #endregion

    #region StrengthDrink
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private double _strengthDrink;

    /// <summary>
    /// 口渴度消耗
    /// </summary>
    public double StrengthDrink
    {
        get => _strengthDrink;
        set => SetProperty(ref _strengthDrink, value);
    }
    #endregion

    #region Feeling
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private double _feeling;

    /// <summary>
    /// 心情消耗
    /// </summary>
    public double Feeling
    {
        get => _feeling;
        set => SetProperty(ref _feeling, value);
    }
    #endregion

    #region LevelLimit
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private int _levelLimit;

    /// <summary>
    /// 等级倍率
    /// </summary>
    public int LevelLimit
    {
        get => _levelLimit;
        set => SetProperty(ref _levelLimit, value);
    }
    #endregion

    #region Time
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private int _time;

    /// <summary>
    /// 时间
    /// </summary>
    public int Time
    {
        get => _time;
        set => SetProperty(ref _time, value);
    }
    #endregion

    #region FinishBonus
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private double _finishBonus;

    /// <summary>
    /// 完成奖励倍率
    /// </summary>
    public double FinishBonus
    {
        get => _finishBonus;
        set => SetProperty(ref _finishBonus, value);
    }
    #endregion

    #region IsOverLoad
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private bool _isOverLoad;

    /// <summary>
    /// 是否超模
    /// </summary>
    public bool IsOverLoad
    {
        get => _isOverLoad;
        set => SetProperty(ref _isOverLoad, value);
    }
    #endregion

    //#region Image
    //[DebuggerBrowsable(DebuggerBrowsableState.Never)]
    //private BitmapImage? _image;

    ///// <summary>
    ///// 图片
    ///// </summary>
    //public BitmapImage? Image
    //{
    //    get => _image;
    //    set => SetProperty(ref _image, value);
    //}
    //#endregion

    #region WorkType
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private VPet_Simulator.Core.GraphHelper.Work.WorkType _workType;

    /// <summary>
    /// 工作类型
    /// </summary>
    public VPet_Simulator.Core.GraphHelper.Work.WorkType WorkType
    {
        get => _workType;
        set => SetProperty(ref _workType, value);
    }
    #endregion

    #region BorderBrush
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private SolidColorBrush _borderBrush =
        new((Color)ColorConverter.ConvertFromString("#FF0290D5"));

    /// <summary>
    /// 边框颜色
    /// </summary>
    [AdaptIgnore]
    public SolidColorBrush BorderBrush
    {
        get => _borderBrush;
        set => SetProperty(ref _borderBrush, value);
    }
    #endregion

    #region Background
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private SolidColorBrush _background = new((Color)ColorConverter.ConvertFromString("#FF81D4FA"));

    /// <summary>
    /// 背景色
    /// </summary>
    [AdaptIgnore]
    public SolidColorBrush Background
    {
        get => _background;
        set => SetProperty(ref _background, value);
    }
    #endregion

    #region Foreground
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private SolidColorBrush _foreground = new((Color)ColorConverter.ConvertFromString("#FF0286C6"));

    /// <summary>
    /// 前景色
    /// </summary>
    [AdaptIgnore]
    public SolidColorBrush Foreground
    {
        get => _foreground;
        set => SetProperty(ref _foreground, value);
    }
    #endregion

    #region ButtonBackground
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private SolidColorBrush _buttonBackground =
        new((Color)ColorConverter.ConvertFromString("#AA0286C6"));

    /// <summary>
    /// 按钮背景色
    /// </summary>
    [AdaptIgnore]
    public SolidColorBrush ButtonBackground
    {
        get => _buttonBackground;
        set => SetProperty(ref _buttonBackground, value);
    }
    #endregion

    #region ButtonForeground
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private SolidColorBrush _buttonForeground =
        new((Color)ColorConverter.ConvertFromString("#FFFFFFFF"));

    /// <summary>
    /// 按钮前景色
    /// </summary>
    [AdaptIgnore]
    public SolidColorBrush ButtonForeground
    {
        get => _buttonForeground;
        set => SetProperty(ref _buttonForeground, value);
    }
    #endregion
    #region Left
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private double _left;

    /// <summary>
    /// X位置
    /// </summary>
    public double Left
    {
        get => _left;
        set => SetProperty(ref _left, value);
    }
    #endregion

    #region Top
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private double _top;

    /// <summary>
    /// Y位置
    /// </summary>
    public double Top
    {
        get => _top;
        set => SetProperty(ref _top, value);
    }
    #endregion

    #region Width
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private double _width;

    /// <summary>
    /// 宽度
    /// </summary>
    public double Width
    {
        get => _width;
        set => SetProperty(ref _width, value);
    }
    #endregion

    public void FixOverLoad()
    {
        var work = ToWork();
        work.FixOverLoad();
        work.Adapt(this);
    }

    public VPet_Simulator.Core.GraphHelper.Work ToWork()
    {
        return new()
        {
            Type = WorkType,
            Name = ID,
            Graph = Graph,
            //MoneyLevel = MoneyLevel,
            MoneyBase = MoneyBase,
            StrengthFood = StrengthFood,
            StrengthDrink = StrengthDrink,
            Feeling = Feeling,
            LevelLimit = LevelLimit,
            Time = Time,
            FinishBonus = FinishBonus,
            //
            BorderBrush = BorderBrush.ToString()[3..],
            Background = Background.ToString()[3..],
            ButtonBackground = ButtonBackground.ToString()[3..],
            ButtonForeground = ButtonForeground.ToString()[3..],
            Foreground = Foreground.ToString()[3..],
            //
            Left = Left,
            Top = Top,
            Width = Width,
        };
    }

    public void Close()
    {
        //Image?.CloseStream();
        I18nResource.I18nObjectInfos.Remove(this);
    }
}
