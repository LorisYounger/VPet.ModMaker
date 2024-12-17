using HKW.HKWReactiveUI;
using NLog;
using Splat;

namespace VPet.ModMaker.ViewModels;

/// <summary>
/// 视图模型基类
/// </summary>
public partial class ViewModelBase : ReactiveObjectX { }

internal static class LogResolver
{
    private const int MaxCacheSize = 16;
    public static NLog.LogFactory LogFactory { get; } = new();

    private static readonly MemoizingMRUCache<Type, NLog.Logger> _loggerCache =
        new((type, _) => LogFactory.GetLogger(type.ToString()), MaxCacheSize);

    public static NLog.Logger Resolve(Type type) => _loggerCache.Get(type, null);
}
