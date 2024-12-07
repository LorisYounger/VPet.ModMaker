using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Wpf;
using HKW.WPF.MVVMDialogs;
using Microsoft.Extensions.Logging;
using ReactiveUI;
using Splat;
using Splat.NLog;
using VPet.ModMaker.Resources;
using VPet.ModMaker.ViewModels;

namespace VPet.ModMaker;

/// <summary>
/// 控制反转
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// 注册
    /// </summary>
    /// <param name="build"></param>
    public static void Register(IMutableDependencyResolver build)
    {
        //build.Register<ModMakerWindowVM>(() => new());
    }

    /// <summary>
    /// 初始化
    /// </summary>
    public static void Initialize()
    {
        var build = Locator.CurrentMutable;
        build.RegisterLazySingleton<IDialogService>(
            () =>
                new DialogService(
                    new DialogManagerX(
                        viewLocator: Locator.Current.GetService<HanumanInstitute.MvvmDialogs.IViewLocator>()
                    ),
                    viewModelFactory: x => Locator.Current.GetService(x)
                )
        );
        build.RegisterLazySingleton<HanumanInstitute.MvvmDialogs.IViewLocator>(
            () => new ViewLocator()
        );
        // 重设日志设置文件位置
        NLog.LogManager.Configuration = new NLog.Config.XmlLoggingConfiguration(
            Path.Combine(NativeData.ModMakerBaseDirectory, "NLog.config")
        );
        build.UseNLogWithWrappingFullLogger();

        Register(build);

        build.InitializeSplat();
        build.InitializeReactiveUI();
    }
}
