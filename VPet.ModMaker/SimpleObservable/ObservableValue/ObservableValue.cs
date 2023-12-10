using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace HKW.HKWUtils.Observable;

/// <summary>
/// 可观察值
/// </summary>
/// <typeparam name="T"></typeparam>
[DebuggerDisplay("\\{ObservableValue, Value = {Value}\\}")]
public class ObservableValue<T>
    : INotifyPropertyChanging,
        INotifyPropertyChanged,
        IEquatable<ObservableValue<T>>
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private T _value = default!;

    /// <summary>
    /// 值
    /// </summary>
    public T Value
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
    /// 包含值
    /// </summary>
    public bool HasValue => Value != null;

    /// <summary>
    /// 分组
    /// </summary>
    public ObservableValueGroup<T>? Group { get; internal set; }

    /// <summary>
    /// 唯一标识符
    /// </summary>
    public Guid Guid { get; } = Guid.NewGuid();

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
        PropertyChanging?.Invoke(this, new(nameof(Value)));
        var args = new ValueChangingEventArgs<T>(oldValue, newValue);
        ValueChanging?.Invoke(this, args);
        // 取消改变后通知UI更改
        if (args.Cancel)
            PropertyChanged?.Invoke(this, new(nameof(Value)));
        return args.Cancel;
    }

    /// <summary>
    /// 通知属性改变后
    /// </summary>
    /// <param name="oldValue">旧值</param>
    /// <param name="newValue">新值</param>
    private void NotifyPropertyChanged(T oldValue, T newValue)
    {
        PropertyChanged?.Invoke(this, new(nameof(Value)));
        ValueChanged?.Invoke(this, new(oldValue, newValue));
    }
    #endregion

    #region NotifySender
    /// <summary>
    /// 通知发送者
    /// </summary>
    public IReadOnlyCollection<INotifyPropertyChanged> NotifySenders => _notifySenders;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly HashSet<INotifyPropertyChanged> _notifySenders = new();

    /// <summary>
    /// 添加通知发送者
    /// <para>
    /// 添加的发送者改变后会执行 <see cref="SenderPropertyChanged"/>
    /// </para>
    /// <para>示例:
    /// <code><![CDATA[
    /// ObservableValue<string> value1 = new();
    /// ObservableValue<string> value2 = new();
    /// value2.AddNotifySender(value1);
    /// value2.SenderPropertyChanged += (source, sender) =>
    /// {
    ///     source.Value = sender.Value;
    /// };
    /// value1.Value = "A";
    /// // value1.Value == "A", value2.Value == "A"
    /// ]]>
    /// </code></para>
    /// </summary>
    /// <param name="items">发送者</param>
    public void AddNotifySender(params INotifyPropertyChanged[] items)
    {
        foreach (var item in items)
        {
            item.PropertyChanged -= NotifySenderPropertyChanged;
            item.PropertyChanged += NotifySenderPropertyChanged;
            _notifySenders.Add(item);
        }
    }

    /// <summary>
    /// 删除通知发送者
    /// </summary>
    /// <param name="items">发送者</param>
    public void RemoveNotifySender(params INotifyPropertyChanged[] items)
    {
        foreach (var item in items)
        {
            item.PropertyChanged -= NotifySenderPropertyChanged;
            _notifySenders.Remove(item);
        }
    }

    /// <summary>
    /// 清空通知发送者
    /// </summary>
    public void ClearNotifySender()
    {
        foreach (var sender in _notifySenders)
            sender.PropertyChanged -= NotifySenderPropertyChanged;
        _notifySenders.Clear();
    }

    private void NotifySenderPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        SenderPropertyChanged?.Invoke(this, (INotifyPropertyChanged)sender!);
    }
    #endregion

    #region Other
    /// <inheritdoc/>
    public override string ToString()
    {
        return Value?.ToString() ?? string.Empty;
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Value?.GetHashCode() ?? 0;
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is ObservableValue<T> value
            && EqualityComparer<T>.Default.Equals(Value, value.Value);
    }

    /// <inheritdoc/>
    public bool Equals(ObservableValue<T>? other)
    {
        return other is ObservableValue<T> value
            && EqualityComparer<T>.Default.Equals(Value, value.Value);
    }

    /// <inheritdoc/>
    public static bool operator ==(ObservableValue<T> a, ObservableValue<T> b)
    {
        return EqualityComparer<T>.Default.Equals(a.Value, b.Value);
    }

    /// <inheritdoc/>
    public static bool operator !=(ObservableValue<T> a, ObservableValue<T> b)
    {
        return (a == b) is not true;
    }
    #endregion

    #region Event

    /// <summary>
    /// 属性改变前事件
    /// </summary>
    public event PropertyChangingEventHandler? PropertyChanging;

    /// <summary>
    /// 属性改变后事件
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// 值改变前事件
    /// </summary>
    public event ValueChangingEventHandler<T>? ValueChanging;

    /// <summary>
    /// 值改变后事件
    /// </summary>
    public event ValueChangedEventHandler<T>? ValueChanged;

    /// <summary>
    /// 通知接收事件
    /// </summary>
    public event NotifySenderPropertyChangedHandler<T>? SenderPropertyChanged;
    #endregion
}
