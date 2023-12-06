using HKW.HKWUtils.Observable;

namespace VPet.ModMaker.Models;

public class ObservablePoint<T>
{
    public ObservableValue<T> X { get; } = new();
    public ObservableValue<T> Y { get; } = new();

    public ObservablePoint() { }

    public ObservablePoint(T x, T y)
    {
        X.Value = x;
        Y.Value = y;
    }

    public void SetValue(T x, T y)
    {
        X.Value = x;
        Y.Value = y;
    }

    public ObservablePoint<T> Copy()
    {
        var result = new ObservablePoint<T>();
        result.X.Value = X.Value;
        result.Y.Value = Y.Value;
        return result;
    }
}
