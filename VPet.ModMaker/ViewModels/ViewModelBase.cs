using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HKW.HKWReactiveUI;
using Splat;
using Splat.NLog;

namespace VPet.ModMaker.ViewModels;

/// <summary>
/// 视图模型基类
/// </summary>
public partial class ViewModelBase : ReactiveObjectX { }

internal static class LogResolver
{
    private const int MaxCacheSize = 16;
    private static readonly MemoizingMRUCache<Type, global::NLog.Logger> _loggerCache =
        new((type, _) => global::NLog.LogManager.GetLogger(type.ToString()), MaxCacheSize);

    public static global::NLog.Logger Resolve(Type type) => _loggerCache.Get(type, null);
}
