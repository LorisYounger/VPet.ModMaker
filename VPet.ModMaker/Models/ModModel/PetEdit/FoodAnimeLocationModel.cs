using CommunityToolkit.Mvvm.ComponentModel;
using HKW.HKWUtils;
using HKW.HKWUtils.Observable;
using HKW.MVVM;
using VPet.ModMaker.ViewModels;

namespace VPet.ModMaker.Models;

/// <summary>
/// 食物图像位置模型
/// </summary>
public partial class FoodAnimeLocationModel
    : ObservableModelBase,
        ICloneable<FoodAnimeLocationModel>
{
    /// <inheritdoc/>
    public FoodAnimeLocationModel() { }

    /// <summary>
    /// 持续时间(ms)
    /// </summary>
    [ObservableProperty]
    public int Duration { get; set; } = 100;

    /// <summary>
    /// 矩形位置
    /// </summary>
    public ObservableRectangle<double> RectangleLocation { get; set; } = new();

    /// <summary>
    /// 旋转角度
    /// </summary>
    [ObservableProperty]
    public double Rotate { get; set; }

    /// <summary>
    /// 透明度
    /// </summary>
    [ObservableProperty]
    public double Opacity { get; set; } = 1.0;

    /// <inheritdoc/>
    public FoodAnimeLocationModel Clone()
    {
        var model = new FoodAnimeLocationModel
        {
            Duration = Duration,
            RectangleLocation = RectangleLocation.Clone(),
            Rotate = Rotate,
            Opacity = Opacity,
        };
        return model;
    }

    object ICloneable.Clone() => Clone();

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"{Duration}, {RectangleLocation.X}, {RectangleLocation.Y}, {RectangleLocation.Width}, {Rotate}, {Opacity}";
    }
}
