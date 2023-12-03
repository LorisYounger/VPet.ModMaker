using System.ComponentModel;

namespace HKW.HKWUtils.Observable;

/// <summary>
/// 通知发送者属性改变接收器
/// </summary>
/// <param name="sender">发送者</param>
/// <param name="eventSender">属性改变事件发送者</param>
public delegate void NotifySenderPropertyChangedHandler<T>(
    ObservableValue<T> sender,
    INotifyPropertyChanged? eventSender
);
