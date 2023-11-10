using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;

namespace HKW.HKWViewModels.SimpleObservable;

/// <summary>
/// 可观察值
/// </summary>
[DebuggerDisplay("\\{ObservableValue, Value = {Value}\\}")]
public class ObservableValue
    : INotifyPropertyChanging,
        INotifyPropertyChanged,
        IEquatable<ObservableValue>
{
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private object _value = default!;

    /// <summary>
    /// 当前值
    /// </summary>
    public object Value
    {
        get => _value;
        set
        {
            if (_value?.Equals(value) is true)
                return;
            NotifyPropertyChanging();
            var oldValue = _value;
            _value = value;
            NotifyPropertyChanged(oldValue!, value);
        }
    }

    /// <summary>
    /// 包含值
    /// </summary>
    public bool HasValue => Value != null;

    /// <summary>
    /// 唯一标识符
    /// </summary>
    public Guid Guid { get; } = Guid.NewGuid();

    #region Ctor
    /// <inheritdoc/>
    public ObservableValue() { }

    /// <inheritdoc/>
    /// <param name="value">初始值</param>
    public ObservableValue(object value)
    {
        _value = value;
    }
    #endregion

    #region NotifyProperty
    /// <summary>
    /// 通知属性改变前
    /// </summary>
    /// <returns>取消改变</returns>
    protected void NotifyPropertyChanging()
    {
        PropertyChanging?.Invoke(this, new(nameof(Value)));
    }

    /// <summary>
    /// 通知属性改变后
    /// </summary>
    /// <param name="oldValue">旧值</param>
    /// <param name="newValue">新值</param>
    protected void NotifyPropertyChanged(object oldValue, object newValue)
    {
        PropertyChanged?.Invoke(this, new(nameof(Value)));
        if (oldValue is null || newValue is null)
            PropertyChanged?.Invoke(this, new(nameof(HasValue)));
    }
    #endregion

    #region NotifySender
    /// <summary>
    /// 通知发送者
    /// </summary>
    public ICollection<ObservableValue> NotifySenders => _notifySenders.Values;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Dictionary<Guid, ObservableValue> _notifySenders = new();

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
    /// value2.NotifyReceived += (source, sender) =>
    /// {
    ///     source.Value = sender.Value;
    /// };
    /// value1.Value = "A";
    /// // value1.Value == "A", value2.Value == "A"
    /// ]]>
    /// </code></para>
    /// </summary>
    /// <param name="items">发送者</param>
    public void AddNotifySender(params ObservableValue[] items)
    {
        foreach (var item in items)
        {
            item.PropertyChanged += Notify_SenderPropertyChanged;
            _notifySenders.Add(item.Guid, item);
        }
    }

    /// <summary>
    /// 添加通知发送者
    /// <para>
    /// 注: 此方法添加的发送者不会被记录到 <see cref="NotifySenders"/> 中
    /// </para>
    /// </summary>
    /// <param name="notices">发送者</param>
    public void AddNotifySender(params INotifyPropertyChanged[] notices)
    {
        foreach (var item in notices)
        {
            item.PropertyChanged += Notify_SenderPropertyChanged;
        }
    }

    /// <summary>
    /// 删除通知发送者
    /// </summary>
    /// <param name="items">发送者</param>
    public void RemoveNotifySender(params ObservableValue[] items)
    {
        foreach (var item in items)
        {
            item.PropertyChanged -= Notify_SenderPropertyChanged;
            _notifySenders.Remove(item.Guid);
        }
    }

    /// <summary>
    /// 删除通知发送者
    /// <para>
    /// 注: 此方法删除的发送者不会从 <see cref="NotifySenders"/> 中删除
    /// </para>
    /// </summary>
    /// <param name="notices">发送者</param>
    public void RemoveNotifySender(params INotifyPropertyChanged[] notices)
    {
        foreach (var item in notices)
        {
            item.PropertyChanged -= Notify_SenderPropertyChanged;
        }
    }

    /// <summary>
    /// 清空通知发送者
    /// </summary>
    public void ClearNotifySender()
    {
        foreach (var sender in _notifySenders.Values)
            sender.PropertyChanged -= Notify_SenderPropertyChanged;
        _notifySenders.Clear();
    }

    private void Notify_SenderPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        NotifySenderPropertyChanged(this, sender);
    }

    /// <summary>
    /// 通知接收事件
    /// </summary>
    /// <param name="source">源</param>
    /// <param name="sender">发送者</param>
    protected void NotifySenderPropertyChanged(ObservableValue source, object? sender)
    {
        SenderPropertyChanged?.Invoke(source, sender);
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
        return Equals(obj as ObservableValue);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        return Value?.GetHashCode() ?? 0;
    }

    /// <inheritdoc/>
    public bool Equals(ObservableValue? other)
    {
        return Guid.Equals(other?.Guid) is true;
    }

    /// <summary>
    /// 判断 <see cref="Value"/> 相等
    /// </summary>
    /// <param name="value1">左值</param>
    /// <param name="value2">右值</param>
    /// <returns>相等为 <see langword="true"/> 否则为 <see langword="false"/></returns>
    public static bool operator ==(ObservableValue value1, ObservableValue value2)
    {
        return value1.Value?.Equals(value2.Value) is true;
    }

    /// <summary>
    /// 判断 <see cref="Value"/> 不相等
    /// </summary>
    /// <param name="value1">左值</param>
    /// <param name="value2">右值</param>
    /// <returns>不相等为 <see langword="true"/> 否则为 <see langword="false"/></returns>
    public static bool operator !=(ObservableValue value1, ObservableValue value2)
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
    /// 通知接收事件
    /// </summary>
    public event NotifySenderPropertyChangedHandler? SenderPropertyChanged;
    #endregion

    #region Delegate
    /// <summary>
    /// 通知发送者属性改变接收器
    /// </summary>
    /// <param name="source">源</param>
    /// <param name="sender">发送者</param>
    public delegate void NotifySenderPropertyChangedHandler(ObservableValue source, object? sender);
    #endregion
}
