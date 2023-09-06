using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HKW.HKWViewModels.SimpleObservable;

/// <summary>
/// 可观察命令
/// </summary>
public class ObservableCommand : ICommand
{
    /// <summary>
    /// 能执行的属性
    /// </summary>
    public ObservableValue<bool> CanExecuteProperty { get; } = new(true);

    /// <summary>
    /// 等待异步执行完成
    /// </summary>
    private readonly ObservableValue<bool> r_waiting = new(false);

    /// <inheritdoc/>
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

    /// <summary>
    /// 能否被执行
    /// </summary>
    /// <param name="parameter">参数</param>
    /// <returns>能被执行为 <see langword="true"/> 否则为 <see langword="false"/></returns>
    public bool CanExecute(object? parameter)
    {
        if (r_waiting.Value is true)
            return false;
        return CanExecuteProperty.Value;
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
        r_waiting.Value = true;
        foreach (
            var asyncEvent in AsyncExecuteEvent.GetInvocationList().Cast<AsyncExecuteHandler>()
        )
            await asyncEvent.Invoke();
        r_waiting.Value = false;
    }

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
    /// 执行
    /// </summary>
    public delegate void ExecuteHandler();

    /// <summary>
    /// 异步执行
    /// </summary>
    /// <returns></returns>
    public delegate Task AsyncExecuteHandler();
}
