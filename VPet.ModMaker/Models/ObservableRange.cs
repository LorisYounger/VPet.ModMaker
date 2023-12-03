using HKW.HKWUtils.Observable;

namespace HKW.Models;

/// <summary>
/// 可观察的范围
/// </summary>
/// <typeparam name="T">类型</typeparam>
public class ObservableRange<T>
{
    /// <summary>
    /// 最小值
    /// </summary>
    public ObservableValue<T> Min { get; } = new();

    /// <summary>
    /// 最大值
    /// </summary>
    public ObservableValue<T> Max { get; } = new();

    /// <summary>
    /// 信息
    /// </summary>
    public ObservableValue<string> Info { get; } = new();

    public ObservableRange()
    {
        Min.ValueChanged += ValueChanged;
        Max.ValueChanged += ValueChanged;
    }

    public ObservableRange(T min, T max)
        : this()
    {
        SetValue(min, max);
    }

    private void ValueChanged(ObservableValue<T> sender, ValueChangedEventArgs<T> e)
    {
        Info.Value = $"({Min.Value}, {Max.Value})";
    }

    /// <summary>
    /// 设置值
    /// </summary>
    /// <param name="min">最小值</param>
    /// <param name="max">最大值</param>
    public void SetValue(T min, T max)
    {
        Min.Value = min;
        Max.Value = max;
    }

    /// <summary>
    /// 复制
    /// </summary>
    /// <returns></returns>
    public ObservableRange<T> Copy()
    {
        return new(Min.Value, Max.Value);
    }
}
