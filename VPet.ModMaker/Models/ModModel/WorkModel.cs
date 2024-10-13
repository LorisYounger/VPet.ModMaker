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
using HKW.HKWReactiveUI;
using HKW.HKWUtils.Observable;
using Mapster;
using VPet.ModMaker.ViewModels;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.Models;

/// <summary>
/// 工作模型
/// </summary>
public partial class WorkModel : ViewModelBase
{
    public WorkModel()
    {
        //PropertyChanged += WorkModel_PropertyChanged;
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

    /// <summary>
    /// ID
    /// </summary>
    [ReactiveProperty]
    public string ID { get; set; } = string.Empty;

    #region I18nData
    [AdaptIgnore]
    private I18nResource<string, string> _i18nResource = null!;

    [AdaptIgnore]
    public required I18nResource<string, string> I18nResource
    {
        get => _i18nResource;
        set
        {
            //TODO:
            //if (_i18nResource is not null)
            //    I18nResource.I18nObjectInfos.Remove(this);
            //_i18nResource = value;
            //InitializeI18nResource();
        }
    }

    public void InitializeI18nResource()
    {
        //I18nResource?.I18nObjectInfos.Add(
        //    this,
        //    new I18nObjectInfo<string, string>(this, OnPropertyChanged).AddPropertyInfo(
        //        nameof(ID),
        //        ID,
        //        nameof(Name),
        //        true
        //    )
        //);
    }

    [AdaptIgnore]
    public string Name
    {
        get => I18nResource.GetCurrentCultureDataOrDefault(ID);
        set => I18nResource.SetCurrentCultureData(ID, value);
    }
    #endregion


    /// <summary>
    /// 指定动画
    /// </summary>
    [ReactiveProperty]
    public string Graph { get; set; } = string.Empty;

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


    /// <summary>
    /// 收获基础
    /// </summary>
    [ReactiveProperty]
    public double MoneyBase { get; set; }

    /// <summary>
    /// 饱食度消耗
    /// </summary>
    [ReactiveProperty]
    public double StrengthFood { get; set; }

    /// <summary>
    /// 口渴度消耗
    /// </summary>
    [ReactiveProperty]
    public double StrengthDrink { get; set; }

    /// <summary>
    /// 心情消耗
    /// </summary>
    [ReactiveProperty]
    public double Feeling { get; set; }

    /// <summary>
    /// 等级倍率
    /// </summary>
    [ReactiveProperty]
    public int LevelLimit { get; set; }

    /// <summary>
    /// 时间
    /// </summary>
    [ReactiveProperty]
    public int Time { get; set; }

    /// <summary>
    /// 完成奖励倍率
    /// </summary>
    [ReactiveProperty]
    public double FinishBonus { get; set; }

    /// <summary>
    /// 是否超模
    /// </summary>
    [ReactiveProperty]
    public bool IsOverLoad { get; set; }

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


    /// <summary>
    /// 工作类型
    /// </summary>
    [ReactiveProperty]
    public VPet_Simulator.Core.GraphHelper.Work.WorkType WorkType { get; set; }

    /// <summary>
    /// 边框颜色
    /// </summary>
    [AdaptIgnore]
    [ReactiveProperty]
    public SolidColorBrush BorderBrush { get; set; } =
        new((Color)ColorConverter.ConvertFromString("#FF0290D5"));

    /// <summary>
    /// 背景色
    /// </summary>
    [AdaptIgnore]
    [ReactiveProperty]
    public SolidColorBrush Background { get; set; } =
        new((Color)ColorConverter.ConvertFromString("#FF81D4FA"));

    /// <summary>
    /// 前景色
    /// </summary>
    [AdaptIgnore]
    [ReactiveProperty]
    public SolidColorBrush Foreground { get; set; } =
        new((Color)ColorConverter.ConvertFromString("#FF0286C6"));

    /// <summary>
    /// 按钮背景色
    /// </summary>
    [AdaptIgnore]
    [ReactiveProperty]
    public SolidColorBrush ButtonBackground { get; set; } =
        new((Color)ColorConverter.ConvertFromString("#AA0286C6"));

    /// <summary>
    /// 按钮前景色
    /// </summary>
    [AdaptIgnore]
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
        //TODO:
        //I18nResource.I18nObjectInfos.Remove(this);
    }
}
