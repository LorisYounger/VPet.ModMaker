using HKW.HKWUtils.Observable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPet.ModMaker.Models.ModModel;

/// <summary>
/// 食物图像模型
/// </summary>
public class FoodLocationModel
{
    /// <summary>
    /// 持续时间
    /// </summary>
    public ObservableValue<int> Duration { get; } = new(100);

    /// <summary>
    /// 范围
    /// </summary>
    public ObservableRect<double> Rect { get; } = new();

    /// <summary>
    /// 旋转角度
    /// </summary>
    public ObservableValue<double> Rotate { get; } = new();

    /// <summary>
    /// 透明度
    /// </summary>
    public ObservableValue<double> Opacity { get; } = new(100);

    public FoodLocationModel()
    {
        Rect.Width.ValueChanged += (s, e) =>
        {
            Rect.Height.Value = e.NewValue;
        };
    }

    public FoodLocationModel Copy()
    {
        var model = new FoodLocationModel();
        model.Duration.Value = Duration.Value;
        model.Rect.SetValue(Rect.X.Value, Rect.Y.Value, Rect.Width.Value, Rect.Height.Value);
        model.Rotate.Value = Rotate.Value;
        model.Opacity.Value = Opacity.Value;
        return model;
    }

    public override string ToString()
    {
        return $"{Duration.Value},{Rect.X.Value},{Rect.Y.Value},{Rect.Width.Value},{Rotate.Value},{Opacity.Value}";
    }
}
