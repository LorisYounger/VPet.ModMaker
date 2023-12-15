using System.ComponentModel;

namespace HKW.HKWUtils.Observable;

/// <summary>
/// 属性改变前事件参数
/// </summary>
public class PropertyChangingXEventArgs : CancelEventArgs
{
    /// <summary>
    /// 属性名
    /// </summary>
    public string PropertyName { get; }

    /// <summary>
    /// 旧值
    /// </summary>
    public object? OldValue { get; }

    /// <summary>
    /// 新值
    /// </summary>
    public object? NewValue { get; }

    /// <inheritdoc/>
    /// <param name="propertyName">属性名</param>
    /// <param name="oldValue">旧值</param>
    /// <param name="newValue">新值</param>
    public PropertyChangingXEventArgs(string propertyName, object? oldValue, object? newValue)
    {
        PropertyName = propertyName;
        OldValue = oldValue;
        NewValue = newValue;
    }

#if  NETCOREAPP2_0_OR_GREATER
    /// <summary>
    /// 获取值
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <returns>(旧值, 新值)</returns>
    public (T oldValue, T newValye) GetValue<T>()
    {
        return ((T)OldValue!, (T)NewValue!)!;
    }
#else
    /// <summary>
    /// 获取值
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <returns>(旧值, 新值)</returns>
    public ValueInfo<T> GetValue<T>()
    {
        return new((T)OldValue!, (T)NewValue!)!;
    }
#endif
}
