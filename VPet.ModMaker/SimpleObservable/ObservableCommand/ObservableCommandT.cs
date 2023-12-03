using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HKW.HKWUtils.Observable;

/// <summary>
/// 带参数的可观察命令
/// </summary>
/// <typeparam name="T">参数类型</typeparam>
[DebuggerDisplay("\\{ObservableCommand, CanExecute = {CanExecuteProperty.Value}\\}")]
public class ObservableCommand<T> : ICommand
    where T : notnull
{
    /// <inheritdoc cref="ObservableCommand.CanExecuteProperty"/>
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

    private void CurrentCanExecute_ValueChanging(
        ObservableValue<bool> sender,
        ValueChangingEventArgs<bool> e
    )
    {
        if (e.NewValue is true && CanExecuteProperty.Value is false)
            e.Cancel = true;
        else
            e.Cancel = false;
    }

    private void InvokeCanExecuteChanged(object? sender, PropertyChangedEventArgs e)
    {
        CanExecuteChanged?.Invoke(sender, e);
    }

    #region ICommand
    /// <inheritdoc cref="ObservableCommand.CanExecute(object?)"/>
    public bool CanExecute(object? parameter)
    {
        return CurrentCanExecute.Value && CanExecuteProperty.Value;
    }

    /// <inheritdoc cref="ObservableCommand.Execute(object?)"/>
    public async void Execute(object? parameter)
    {
        ExecuteCommand?.Invoke((T)parameter!);
        await ExecuteAsync((T)parameter!);
    }

    /// <inheritdoc cref="ObservableCommand.ExecuteAsync"/>
    /// <param name="parameter">参数</param>
    private async Task ExecuteAsync(T parameter)
    {
        if (AsyncExecuteCommand is null)
            return;
        CurrentCanExecute.Value = false;
        foreach (
            var asyncEvent in AsyncExecuteCommand
                .GetInvocationList()
                .Cast<AsyncExecuteEventHandler<T>>()
        )
            await asyncEvent.Invoke(parameter);
        CurrentCanExecute.Value = true;
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
        var args = new CancelEventArgs();
        NotifyCanExecuteReceived?.Invoke(this, args);
        CanExecuteProperty.Value = args.Cancel;
    }
    #endregion

    #region Event
    /// <inheritdoc cref="ObservableCommand.CanExecuteChanged"/>
    public event EventHandler? CanExecuteChanged;

    /// <inheritdoc cref="ObservableCommand.ExecuteCommand"/>
    public event ExecuteEventHandler<T>? ExecuteCommand;

    /// <inheritdoc cref="ObservableCommand.AsyncExecuteCommand"/>
    public event AsyncExecuteEventHandler<T>? AsyncExecuteCommand;

    /// <inheritdoc cref="ObservableCommand.NotifyCanExecuteReceived"/>
    public event NotifyReceivedEventHandler? NotifyCanExecuteReceived;
    #endregion
}
