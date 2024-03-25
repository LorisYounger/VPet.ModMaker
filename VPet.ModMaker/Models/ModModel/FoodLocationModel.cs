using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HKW.HKWUtils.Observable;

namespace VPet.ModMaker.Models.ModModel;

/// <summary>
/// 食物图像模型
/// </summary>
public class FoodLocationModel : ObservableObjectX<FoodLocationModel>
{
    #region Duration
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private int _duration;

    /// <summary>
    /// 持续时间
    /// </summary>
    public int Duration
    {
        get => _duration;
        set => SetProperty(ref _duration, value);
    }
    #endregion

    /// <summary>
    /// 范围
    /// </summary>
    public ObservableRectangleLocation<double> Rect { get; set; } = new();

    /// <summary>
    /// 旋转角度
    /// </summary>
    #region Rotate
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private double _rotate;

    public double Rotate
    {
        get => _rotate;
        set => SetProperty(ref _rotate, value);
    }
    #endregion

    /// <summary>
    /// 透明度
    /// </summary>
    #region Opacity
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private double _opacity;

    public double Opacity
    {
        get => _opacity;
        set => SetProperty(ref _opacity, value);
    }
    #endregion

    public FoodLocationModel()
    {
        Rect.PropertyChangedX += (s, e) =>
        {
            Rect.Height = (int)e.NewValue;
        };
    }

    public FoodLocationModel Copy()
    {
        var model = new FoodLocationModel();
        model.Duration = Duration;
        model.Rect = new(Rect.X, Rect.Y, Rect.Width, Rect.Height);
        model.Rotate = Rotate;
        model.Opacity = Opacity;
        return model;
    }

    public override string ToString()
    {
        return $"{Duration}, {Rect.X}, {Rect.Y}, {Rect.Width}, {Rotate}, {Opacity}";
    }
}
