using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HKW.HKWViewModels.SimpleObservable;

/// <summary>
/// 带参数的可观察命令
/// </summary>
/// <typeparam name="T">参数类型</typeparam>
public class ObservableCommand<T> : ICommand
    where T : notnull
{
    /// <inheritdoc cref="ObservableCommand.CanExecuteProperty"/>
    public ObservableValue<bool> CanExecuteProperty { get; } = new(true);

    /// <inheritdoc cref="ObservableCommand.r_waiting"/>
    private readonly ObservableValue<bool> r_waiting = new(false);

    /// <inheritdoc />
    public ObservableCommand()
    {
        CanExecuteProperty.PropertyChanged += InvokeCanExecuteChanged;
        r_waiting.PropertyChanged += InvokeCanExecuteChanged;
    }

    private void InvokeCanExecuteChanged(
        object? sender,
        System.ComponentModel.PropertyChangedEventArgs e
    )
    {
        CanExecuteChanged?.Invoke(sender, e);
    }

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

    /// <inheritdoc cref="ObservableCommand.CanExecuteChanged"/>
    public event EventHandler? CanExecuteChanged;

    /// <inheritdoc cref="ObservableCommand.ExecuteEvent"/>
    public event ExecuteHandler? ExecuteEvent;

    /// <inheritdoc cref="ObservableCommand.AsyncExecuteEvent"/>
    public event AsyncExecuteHandler? AsyncExecuteEvent;

    /// <inheritdoc cref="ObservableCommand.ExecuteHandler"/>
    /// <param name="value">值</param>
    public delegate void ExecuteHandler(T value);

    /// <inheritdoc cref="ObservableCommand.AsyncExecuteHandler"/>
    /// <param name="value">值</param>
    public delegate Task AsyncExecuteHandler(T value);
}
