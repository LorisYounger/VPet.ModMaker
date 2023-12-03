using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HKW.HKWUtils.Observable;

/// <summary>
/// 异步执行命令事件
/// </summary>
public delegate Task AsyncExecuteEventHandler();

/// <summary>
/// 异步执行命令事件
/// </summary>
/// <param name="parameter">值</param>
public delegate Task AsyncExecuteEventHandler<T>(T parameter);
