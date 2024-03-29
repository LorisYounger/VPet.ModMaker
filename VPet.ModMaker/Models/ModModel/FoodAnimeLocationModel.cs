using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HKW.HKWUtils.Observable;

namespace VPet.ModMaker.Models.ModModel;

/// <summary>
/// 食物图像位置模型
/// </summary>
public class FoodAnimeLocationModel
    : ObservableObjectX<FoodAnimeLocationModel>,
        ICloneable<FoodAnimeLocationModel>
{
    public FoodAnimeLocationModel() { }

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
    public ObservableRectangleLocation<double> RectangleLocation { get; set; } = new();

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
    public FoodAnimeLocationModel Clone()
    {
        var model = new FoodAnimeLocationModel
        {
            Duration = Duration,
            RectangleLocation = new(
                RectangleLocation.X,
                RectangleLocation.Y,
                RectangleLocation.Width,
                RectangleLocation.Width
            ),
            Rotate = Rotate,
            Opacity = Opacity
        };
        return model;
    }

    object ICloneable.Clone() => Clone();

    public override string ToString()
    {
        return $"{Duration}, {RectangleLocation.X}, {RectangleLocation.Y}, {RectangleLocation.Width}, {Rotate}, {Opacity}";
    }
}
