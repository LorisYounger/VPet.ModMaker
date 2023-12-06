using HKW.HKWUtils.Observable;

namespace VPet.ModMaker.Models;

public class ObservableRect<T>
{
    public ObservableValue<T> X { get; } = new();
    public ObservableValue<T> Y { get; } = new();
    public ObservableValue<T> Width { get; } = new();
    public ObservableValue<T> Height { get; } = new();

    public ObservableRect() { }

    public ObservableRect(T x, T y, T width, T hetght)
    {
        X.Value = x;
        Y.Value = y;
        Width.Value = width;
        Height.Value = hetght;
    }

    public void SetValue(T x, T y, T width, T hetght)
    {
        X.Value = x;
        Y.Value = y;
        Width.Value = width;
        Height.Value = hetght;
    }

    public ObservableRect<T> Copy()
    {
        var result = new ObservableRect<T>();
        result.X.Value = X.Value;
        result.Y.Value = Y.Value;
        result.Width.Value = Width.Value;
        result.Height.Value = Height.Value;
        return result;
    }
}
