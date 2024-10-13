using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HKW.HKWReactiveUI;
using HKW.HKWUtils.Observable;
using VPet.ModMaker.ViewModels;

namespace VPet.ModMaker.Models.ModModel;

/// <summary>
/// 食物图像位置模型
/// </summary>
public partial class FoodAnimeLocationModel : ViewModelBase, ICloneable<FoodAnimeLocationModel>
{
    public FoodAnimeLocationModel() { }

    /// <summary>
    /// 持续时间(ms)
    /// </summary>
    [ReactiveProperty]
    public int Duration { get; set; } = 100;

    /// <summary>
    /// 矩形位置
    /// </summary>
    public ObservableRectangle<double> RectangleLocation { get; set; } = new();

    /// <summary>
    /// 旋转角度
    /// </summary>
    [ReactiveProperty]
    public double Rotate { get; set; }

    /// <summary>
    /// 透明度
    /// </summary>
    [ReactiveProperty]
    public double Opacity { get; set; } = 1.0;

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
