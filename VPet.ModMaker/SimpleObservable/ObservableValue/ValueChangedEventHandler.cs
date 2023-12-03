namespace HKW.HKWUtils.Observable;

/// <summary>
/// 值改变后事件
/// </summary>
/// <param name="sender">发送者</param>
/// <param name="e">参数</param>
public delegate void ValueChangedEventHandler<T>(
    ObservableValue<T> sender,
    ValueChangedEventArgs<T> e
);
