using HKW.HKWViewModels.SimpleObservable;

namespace VPet.ModMaker.Models;

public class ObservableRange<T>
{
    public ObservableValue<T> Min { get; } = new();
    public ObservableValue<T> Max { get; } = new();

    public ObservableValue<string> Info { get; } = new();

    public ObservableRange()
    {
        Min.ValueChanged += ValueChanged;
        Max.ValueChanged += ValueChanged;
    }

    private void ValueChanged(T oldValue, T newValue)
    {
        Info.Value = $"({Min.Value}, {Max.Value})";
    }

    public ObservableRange(T min, T max)
        : this()
    {
        SetValue(min, max);
    }

    public void SetValue(T min, T max)
    {
        Min.Value = min;
        Max.Value = max;
    }

    public ObservableRange<T> Copy()
    {
        return new(Min.Value, Max.Value);
    }
}
