using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HKW.HKWViewModels.SimpleObservable;

/// <summary>
/// 可观察值
/// </summary>
/// <typeparam name="T"></typeparam>
[DebuggerDisplay("\\{ObservableValue, Value = {Value}\\}")]
public class ObservableValue<T> : ObservableValue, IEquatable<ObservableValue<T>>
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private T _value = default!;

    /// <inheritdoc cref=" ObservableValue.Value"/>
    public new T Value
    {
        get => _value;
        set
        {
            if (_value?.Equals(value) is true)
                return;
            var oldValue = _value;
            if (NotifyPropertyChanging(oldValue, value))
                return;
            _value = value;
            NotifyPropertyChanged(oldValue, value);
        }
    }

    /// <summary>
    /// 分组
    /// </summary>
    public ObservableValueGroup<T>? Group { get; internal set; }

    #region Ctor
    /// <inheritdoc/>
    public ObservableValue() { }

    /// <inheritdoc/>
    /// <param name="value">初始值</param>
    public ObservableValue(T value)
    {
        _value = value;
    }
    #endregion

    #region NotifyProperty
    /// <summary>
    /// 通知属性改变前
    /// </summary>
    /// <param name="oldValue">旧值</param>
    /// <param name="newValue">新值</param>
    /// <returns>取消改变</returns>
    private bool NotifyPropertyChanging(T oldValue, T newValue)
    {
        NotifyPropertyChanging();
        var cancel = false;
        // 若全部事件取消改变 则取消改变
        ValueChanging?.Invoke(oldValue, newValue, ref cancel);
        return cancel;
    }

    /// <summary>
    /// 通知属性改变后
    /// </summary>
    /// <param name="oldValue">旧值</param>
    /// <param name="newValue">新值</param>
    private void NotifyPropertyChanged(T oldValue, T newValue)
    {
        base.NotifyPropertyChanged(oldValue!, newValue!);
        ValueChanged?.Invoke(oldValue, newValue);
    }
    #endregion

    #region NotifySender
    /// <summary>
    /// 通知发送者
    /// </summary>
    public new ICollection<ObservableValue<T>> NotifySenders => _notifySenders.Values;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Dictionary<Guid, ObservableValue<T>> _notifySenders = new();

    /// <inheritdoc cref=" ObservableValue.AddNotifySender(ObservableValue[])"/>
    public void AddNotifySender(params ObservableValue<T>[] items)
    {
        foreach (var item in items)
        {
            item.PropertyChanged += NotifySenderPropertyChanged;
            _notifySenders.Add(item.Guid, item);
        }
    }

    /// <inheritdoc cref=" ObservableValue.RemoveNotifySender(ObservableValue[])"/>
    public void RemoveNotifySender(params ObservableValue<T>[] items)
    {
        foreach (var item in items)
        {
            item.PropertyChanged -= NotifySenderPropertyChanged;
            _notifySenders.Remove(item.Guid);
        }
    }

    /// <inheritdoc cref=" ObservableValue.ClearNotifySender"/>
    public new void ClearNotifySender()
    {
        foreach (var sender in _notifySenders.Values)
            sender.PropertyChanged -= NotifySenderPropertyChanged;
        _notifySenders.Clear();
    }

    private void NotifySenderPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        NotifySenderPropertyChanged(this, sender);
    }
    #endregion

    #region Other
    /// <inheritdoc/>
    public override string ToString()
    {
        return Value?.ToString() ?? string.Empty;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return Equals(obj as ObservableValue<T>);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Value?.GetHashCode() ?? 0;
    }

    /// <inheritdoc/>
    public bool Equals(ObservableValue<T>? other)
    {
        return Guid.Equals(other?.Guid) is true;
    }

    /// <summary>
    /// 判断 <see cref="Value"/> 相等
    /// </summary>
    /// <param name="value1">左值</param>
    /// <param name="value2">右值</param>
    /// <returns>相等为 <see langword="true"/> 否则为 <see langword="false"/></returns>
    public static bool operator ==(ObservableValue<T> value1, ObservableValue<T> value2)
    {
        return value1.Value?.Equals(value2.Value) is true;
    }

    /// <summary>
    /// 判断 <see cref="Value"/> 不相等
    /// </summary>
    /// <param name="value1">左值</param>
    /// <param name="value2">右值</param>
    /// <returns>不相等为 <see langword="true"/> 否则为 <see langword="false"/></returns>
    public static bool operator !=(ObservableValue<T> value1, ObservableValue<T> value2)
    {
        return value1.Value?.Equals(value2.Value) is not true;
    }

    #endregion

    #region Event
    /// <summary>
    /// 值改变前事件
    /// </summary>
    public event ValueChangingEventHandler? ValueChanging;

    /// <summary>
    /// 值改变后事件
    /// </summary>
    public event ValueChangedEventHandler? ValueChanged;

    #endregion

    #region Delegate
    /// <summary>
    /// 值改变事件
    /// </summary>
    /// <param name="oldValue">旧值</param>
    /// <param name="newValue">新值</param>
    /// <param name="cancel">取消</param>
    public delegate void ValueChangingEventHandler(T oldValue, T newValue, ref bool cancel);

    /// <summary>
    /// 值改变后事件
    /// </summary>
    /// <param name="oldValue">旧值</param>
    /// <param name="newValue">新值</param>
    public delegate void ValueChangedEventHandler(T oldValue, T newValue);
    #endregion
}
