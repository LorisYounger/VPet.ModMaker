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
/// 带参数的可观察命令
/// </summary>
/// <typeparam name="T">参数类型</typeparam>
[DebuggerDisplay(
    "CanExecute = {CanExecuteProperty.Value}, EventCount =  {ExecuteEvent.GetInvocationList().Length}, AsyncEventCount = {AsyncExecuteEvent.GetInvocationList().Length}"
)]
public class ObservableCommand<T> : ICommand
    where T : notnull
{
    /// <inheritdoc cref="ObservableCommand.CanExecuteProperty"/>
    public ObservableValue<bool> CanExecuteProperty { get; } = new(true);

    /// <inheritdoc cref="ObservableCommand.r_waiting"/>
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly ObservableValue<bool> r_waiting = new(false);

    /// <inheritdoc />
    public ObservableCommand()
    {
        CanExecuteProperty.PropertyChanged += InvokeCanExecuteChanged;
        r_waiting.PropertyChanged += InvokeCanExecuteChanged;
    }

    private void InvokeCanExecuteChanged(object? sender, PropertyChangedEventArgs e)
    {
        CanExecuteChanged?.Invoke(sender, e);
    }

    #region ICommand
    /// <inheritdoc cref="ObservableCommand.CanExecute(object?)"/>
    public bool CanExecute(object? parameter)
    {
        if (r_waiting.Value is true)
            return false;
        return CanExecuteProperty.Value;
    }

    /// <inheritdoc cref="ObservableCommand.Execute(object?)"/>
    public async void Execute(object? parameter)
    {
        ExecuteEvent?.Invoke((T?)parameter!);
        await ExecuteAsync((T?)parameter!);
    }

    /// <inheritdoc cref="ObservableCommand.ExecuteAsync"/>
    /// <param name="parameter">参数</param>
    private async Task ExecuteAsync(T parameter)
    {
        if (AsyncExecuteEvent is null)
            return;
        r_waiting.Value = true;
        foreach (
            var asyncEvent in AsyncExecuteEvent.GetInvocationList().Cast<AsyncExecuteHandler>()
        )
            await asyncEvent.Invoke(parameter);
        r_waiting.Value = false;
    }
    #endregion

    #region NotifyReceiver
    /// <inheritdoc cref="ObservableCommand.AddNotifyReceiver(INotifyPropertyChanged[])"/>
    public void AddNotifyReceiver(params INotifyPropertyChanged[] notifies)
    {
        foreach (var notify in notifies)
            notify.PropertyChanged += Notify_PropertyChanged;
    }

    /// <inheritdoc cref="ObservableCommand.RemoveNotifyReceiver(INotifyPropertyChanged[])"/>
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
    /// <inheritdoc cref="ObservableCommand.CanExecuteChanged"/>
    public event EventHandler? CanExecuteChanged;

    /// <inheritdoc cref="ObservableCommand.ExecuteEvent"/>
    public event ExecuteHandler? ExecuteEvent;

    /// <inheritdoc cref="ObservableCommand.AsyncExecuteEvent"/>
    public event AsyncExecuteHandler? AsyncExecuteEvent;

    /// <inheritdoc cref="ObservableCommand.NotifyCanExecuteReceived"/>
    public event NotifyReceivedHandler? NotifyCanExecuteReceived;
    #endregion

    #region Delegate
    /// <inheritdoc cref="ObservableCommand.ExecuteHandler"/>
    /// <param name="value">值</param>
    public delegate void ExecuteHandler(T value);

    /// <inheritdoc cref="ObservableCommand.AsyncExecuteHandler"/>
    /// <param name="value">值</param>
    public delegate Task AsyncExecuteHandler(T value);

    /// <summary>
    /// 通知接收器
    /// </summary>
    /// <param name="value">引用值</param>
    public delegate void NotifyReceivedHandler(ref bool value);
    #endregion
}
