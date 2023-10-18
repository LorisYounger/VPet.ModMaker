using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HKW.HKWViewModels.SimpleObservable;

/// <summary>
/// 可观察命令
/// </summary>
[DebuggerDisplay(
    "CanExecute = {CanExecuteProperty.Value}, EventCount =  {ExecuteEvent.GetInvocationList().Length}, AsyncEventCount = {AsyncExecuteEvent.GetInvocationList().Length}"
)]
public class ObservableCommand : ICommand
{
    /// <summary>
    /// 能执行的属性
    /// </summary>
    public ObservableValue<bool> CanExecuteProperty { get; } = new(true);

    /// <summary>
    /// 当前可执行状态
    /// </summary>
    public ObservableValue<bool> CurrentCanExecute { get; } = new(true);

    /// <inheritdoc/>
    public ObservableCommand()
    {
        CanExecuteProperty.PropertyChanged += InvokeCanExecuteChanged;
        CurrentCanExecute.PropertyChanged += InvokeCanExecuteChanged;
        CurrentCanExecute.ValueChanging += CurrentCanExecute_ValueChanging;
    }

    private bool CurrentCanExecute_ValueChanging(bool oldValue, bool newValue)
    {
        if (newValue is true && CanExecuteProperty.Value is false)
            return true;
        return false;
    }

    private void InvokeCanExecuteChanged(object? sender, PropertyChangedEventArgs e)
    {
        CanExecuteChanged?.Invoke(sender, e);
    }

    #region ICommand
    /// <summary>
    /// 能否被执行
    /// </summary>
    /// <param name="parameter">参数</param>
    /// <returns>能被执行为 <see langword="true"/> 否则为 <see langword="false"/></returns>
    public bool CanExecute(object? parameter)
    {
        return CurrentCanExecute.Value && CanExecuteProperty.Value;
    }

    /// <summary>
    /// 执行方法
    /// </summary>
    /// <param name="parameter">参数</param>
    public async void Execute(object? parameter)
    {
        ExecuteEvent?.Invoke();
        await ExecuteAsync();
    }

    /// <summary>
    /// 执行异步方法, 会在等待中关闭按钮的可执行性, 完成后恢复
    /// </summary>
    /// <returns>等待</returns>
    private async Task ExecuteAsync()
    {
        if (AsyncExecuteEvent is null)
            return;
        CurrentCanExecute.Value = false;
        foreach (
            var asyncEvent in AsyncExecuteEvent.GetInvocationList().Cast<AsyncExecuteHandler>()
        )
            await asyncEvent.Invoke();
        CurrentCanExecute.Value = true;
    }
    #endregion

    #region NotifyReceiver
    /// <summary>
    /// 添加通知属性改变后接收器
    /// <para>
    /// 添加的接口触发后会执行 <see cref="NotifyCanExecuteReceived"/>
    /// </para>
    /// <para>示例:
    /// <code><![CDATA[
    /// ObservableValue<string> value = new();
    /// ObservableCommand command = new();
    /// command.AddNotifyReceiver(value);
    /// command.NotifyCanExecuteReceived += (ref bool canExecute) =>
    /// {
    ///     canExecute = false; // trigger this
    /// };
    /// value.EnumValue = "A"; // execute this
    /// // result: value.EnumValue == "A" , command.CanExecuteProperty == false
    /// ]]>
    /// </code></para>
    /// </summary>
    /// <param name="notifies">通知属性改变后接口</param>
    public void AddNotifyReceiver(params INotifyPropertyChanged[] notifies)
    {
        foreach (var notify in notifies)
            notify.PropertyChanged += Notify_PropertyChanged;
    }

    /// <summary>
    /// 删除通知属性改变后接收器
    /// </summary>
    /// <param name="notifies">通知属性改变后接口</param>
    public void RemoveNotifyReceiver(params INotifyPropertyChanged[] notifies)
    {
        foreach (var notify in notifies)
            notify.PropertyChanged -= Notify_PropertyChanged;
    }

    private void Notify_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        var temp = CanExecuteProperty.Value;
        NotifyCanExecuteReceived?.Invoke(ref temp);
        CanExecuteProperty.Value = temp;
    }
    #endregion

    #region Event
    /// <summary>
    /// 能否执行属性改变后事件
    /// </summary>
    public event EventHandler? CanExecuteChanged;

    /// <summary>
    /// 执行事件
    /// </summary>
    public event ExecuteHandler? ExecuteEvent;

    /// <summary>
    /// 异步执行事件
    /// </summary>
    public event AsyncExecuteHandler? AsyncExecuteEvent;

    /// <summary>
    /// 可执行通知接收器事件
    /// </summary>
    public event NotifyReceivedHandler? NotifyCanExecuteReceived;
    #endregion

    #region Delegate
    /// <summary>
    /// 执行
    /// </summary>
    public delegate void ExecuteHandler();

    /// <summary>
    /// 异步执行
    /// </summary>
    /// <returns></returns>
    public delegate Task AsyncExecuteHandler();

    /// <summary>
    /// 通知接收器
    /// </summary>
    /// <param name="value">引用值</param>
    public delegate void NotifyReceivedHandler(ref bool value);
    #endregion
}
