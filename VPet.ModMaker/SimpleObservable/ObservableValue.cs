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
        // 若全部事件取消改变 则取消改变
        var args = new ValueChangingEventArgs<T>(oldValue, newValue);
        ValueChanging?.Invoke(this, args);
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
    public override bool Equals(object? obj)
    {
        return Equals(obj as ObservableValue<T>);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Guid.GetHashCode();
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
    public event ValueChangingEventHandler? ValueChanging;

    /// <summary>
    /// 值改变后事件
    /// </summary>
    public event ValueChangedEventHandler? ValueChanged;

    /// <summary>
    /// 通知接收事件
    /// </summary>
    public event NotifySenderPropertyChangedHandler? SenderPropertyChanged;

    #endregion

    #region Delegate
    /// <summary>
    /// 值改变前事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">参数</param>
    public delegate void ValueChangingEventHandler(
        ObservableValue<T> sender,
        ValueChangingEventArgs<T> e
    );

    /// <summary>
    /// 值改变后事件
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="e">参数</param>
    public delegate void ValueChangedEventHandler(
        ObservableValue<T> sender,
        ValueChangedEventArgs<T> e
    );

    /// <summary>
    /// 通知发送者属性改变接收器
    /// </summary>
    /// <param name="source">源</param>
    /// <param name="sender">发送者</param>
    public delegate void NotifySenderPropertyChangedHandler(
        ObservableValue<T> source,
        INotifyPropertyChanged? sender
    );
    #endregion
}

/// <summary>
/// 值改变前事件参数
/// </summary>
/// <typeparam name="T">值类型</typeparam>
public class ValueChangingEventArgs<T> : CancelEventArgs
{
    /// <summary>
    /// 旧值
    /// </summary>
    public T? OldValue { get; }

    /// <summary>
    /// 新值
    /// </summary>
    public T? NewValue { get; }

    /// <inheritdoc/>
    /// <param name="oldValue">旧值</param>
    /// <param name="newValue">新值</param>
    public ValueChangingEventArgs(T? oldValue, T? newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }
}

/// <summary>
/// 值改变后事件参数
/// </summary>
/// <typeparam name="T">值类型</typeparam>
public class ValueChangedEventArgs<T> : EventArgs
{
    /// <summary>
    /// 旧值
    /// </summary>
    public T OldValue { get; }

    /// <summary>
    /// 新值
    /// </summary>
    public T NewValue { get; }

    /// <inheritdoc/>
    /// <param name="oldValue">旧值</param>
    /// <param name="newValue">新值</param>
    public ValueChangedEventArgs(T oldValue, T newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }
}
