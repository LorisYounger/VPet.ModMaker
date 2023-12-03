using System;

namespace HKW.HKWUtils.Observable;

/// <summary>
/// 值改变后事件参数
/// </summary>
/// <typeparam name="T">值类型</typeparam>
public class ValueChangedEventArgs<T> : EventArgs
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
    public ValueChangedEventArgs(T oldValue, T newValue)
    {
        OldValue = oldValue;
        NewValue = newValue;
    }
}
