using System.Collections;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Wpf;
using HKW.HKWUtils;
using HKW.MVVM;
using HKW.MVVM.SourceGenerator;
using HKW.WPF.Extensions;
using LinePutScript.Localization.WPF;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Extensions.Logging;
using Panuon.WPF.UI;
using VPet.ModMaker.Models;
using VPet.ModMaker.Resources;
using VPet.ModMaker.ViewModels;
using VPet.ModMaker.Views;

namespace VPet.ModMaker;

internal partial class AppServices : DIConfigurationBase
{
    public StrongViewLocator ViewLocator { get; } = new();

    public NLog.Config.LoggingConfiguration LoggingConfiguration { get; }

    public ILoggerFactory LogFactory { get; }

    public AppServices()
    {
        Directory.CreateDirectory(ModMaker.ModMakerBaseDirectory);

        EnumInfo.DefaultToString = x =>
        {
            if (x.IsFlaggable is false || x.IsNone)
            {
                return $"{x.EnumType.Name}_{x.Value}".Translate();
            }
            else
            {
                return string.Join(
                    ", ",
                    x.GetFlagInfos().Select(static i => $"{i.EnumType.Name}_{i.Value}".Translate())
                );
            }
        };
        LoggingConfiguration = new NLog.Config.XmlLoggingConfiguration(
            new StreamReader(NativeResources.GetStream(NativeResources.NLogConfig))
        );
        LoggingConfiguration.LogFactory.Configuration = LoggingConfiguration;
        LoggingConfiguration
            .LogFactory.Setup()
            .SetupLogFactory(ext =>
            {
                ext.AddCallSiteHiddenAssembly(
                    typeof(Microsoft.Extensions.Logging.ILogger).Assembly
                );
                ext.AddCallSiteHiddenAssembly(typeof(HKW.MVVM.ObservableObjectEx).Assembly);
            });
        LogFactory = new NLogLoggerFactory(new(), LoggingConfiguration.LogFactory);
        LogHost.RegisterDomain<ViewModelBase>(LogFactory);
        LogHost.RegisterDomain<ObservableModelBase>(LogFactory);
        LogHost.RegisterDomain<ModMakeHistory>(LogFactory);
    }

    protected override void Configure(IServiceCollection services)
    {
        services.AddSingleton<IDialogService>(sp => new DialogService(
            new DialogManagerX(ViewLocator, dialogFactory: new DialogFactory()),
            viewModelFactory: x => sp.GetService(x)
        ));
        services.AddSingleton<ILoggerFactory>(_ => LogFactory);
        services.AddSingleton<NLog.Config.LoggingConfiguration>(_ => LoggingConfiguration);
        RegisterMVVM<ModMakerViewModel, ModMakerWindow>();
        RegisterMVVM<ModEditViewModel, ModEditWindow>();
        RegisterMVVM<FoodEditViewModel, FoodEditWindow>();
        RegisterMVVM<ClickTextEditViewModel, ClickTextEditWindow>();
        RegisterMVVM<LowTextEditViewModel, LowTextEditWindow>();
        RegisterMVVM<SelectTextEditViewModel, SelectTextEditWindow>();
        RegisterMVVM<ThemeEditViewModel, ThemeEditWindow>();
        Register<FontEditViewModel>();

        RegisterMVVM<PetEditViewModel, PetEditWindow>();
        RegisterMVVM<WorkEditViewModel, WorkEditWindow>();
        RegisterMVVM<MoveEditViewModel, MoveEditWindow>();
        RegisterMVVM<AnimeViewModel, AnimeView>();
        RegisterMVVM<SelectGraphTypeViewModel, SelectGraphTypeWindow>();
        RegisterMVVM<AnimeEditViewModel, AnimeEditWindow>();
        RegisterMVVM<FoodAnimeEditViewModel, FoodAnimeEditWindow>();

        RegisterMVVM<I18nEditViewModel, I18nEditWindow>();
        RegisterMVVM<AddCultureViewModel, AddCultureWindow>();
        RegisterMVVM<SaveTranslationModViewModel, SaveTranslationModWindow>();
    }

    [DICustomServiceRegistrar(nameof(TViewModel))]
    public void RegisterMVVM<TViewModel, TView>()
        where TViewModel : class, INotifyPropertyChanged
        where TView : Control, new()
    {
        ViewLocator.Register<TViewModel, TView>();
    }

    private static readonly Lock NLogConfigLock = new();

    public static void SetDebugMode(NLog.Config.LoggingConfiguration configuration, bool enabled)
    {
        ArgumentNullException.ThrowIfNull(configuration);
        lock (NLogConfigLock)
        {
            var target =
                configuration.FindTargetByName("Normal") as NLog.Targets.FileTarget
                ?? throw new InvalidOperationException("FileTarget 'Normal' was not found.");

            const string infoLayout =
                "[${callsite:methodName=false:includeNamespace=false}] ${level} ${message}"
                + "${onexception:${newline}${exception:format=tostring}}";

            const string debugLayout =
                "[${callsite}] ${level} ${message}"
                + "${onexception:${newline}${exception:format=tostring}}";

            target.Layout = new NLog.Layouts.SimpleLayout(enabled ? debugLayout : infoLayout);
            foreach (
                var rule in configuration.LoggingRules.Where(rule => rule.Targets.Contains(target))
            )
            {
                rule.SetLoggingLevels(
                    enabled ? NLog.LogLevel.Debug : NLog.LogLevel.Info,
                    NLog.LogLevel.Fatal
                );
            }

            configuration.LogFactory.ReconfigExistingLoggers();
        }
    }
}

internal class DialogManagerX : DialogManager
{
    /// <inheritdoc cref="DialogManager.DialogManager(IViewLocator?, IDialogFactory?, ILogger{DialogManager}?, Dispatcher?)"/>
    public DialogManagerX(
        IViewLocator? viewLocator = null,
        IDialogFactory? dialogFactory = null,
        ILogger<DialogManager>? logger = null,
        Dispatcher? dispatcher = null
    )
        : base(viewLocator, dialogFactory, logger, dispatcher) { }

    private static IEnumerable<Window> Windows => Application.Current.Windows.Cast<Window>();

    public override IView? FindViewByViewModel(INotifyPropertyChanged viewModel)
    {
        var view = base.FindViewByViewModel(viewModel);
        // 如果找到的窗口未激活, 则寻找其它窗口
        if (view is not null && view.RefObj is Window w && w.IsActive)
        {
            return view;
        }
        var window = Windows.FirstOrDefault(w => w.IsActive);
        // 从激活的窗口寻找子视图
        var c = window?.FindView(viewModel);
        if (c is not null)
            return window.AsWrapper();
        return view;
    }
}

internal static class MVVMDialogExtensions
{
    private static readonly Dictionary<Type, INotifyPropertyChanged> _displayedViewModelByType = [];
    extension(IDialogService dialogService)
    {
        /// <summary>
        /// 显示单例
        /// </summary>
        /// <typeparam name="TViewModel">视图模型类型</typeparam>
        /// <param name="ownerViewModel">主视图模型</param>
        /// <param name="viewModel">视图模型</param>
        public void ShowInstance<TViewModel>(
            INotifyPropertyChanged? ownerViewModel,
            TViewModel? viewModel = null
        )
            where TViewModel : class, INotifyPropertyChanged
        {
            if (_displayedViewModelByType.TryGetValue(typeof(TViewModel), out var value))
            {
                var view = dialogService.DialogManager.FindViewByViewModel(value);
                if (view?.RefObj is Window window)
                {
                    window.RestoredAndAlert();
                }
                return;
            }
            _displayedViewModelByType[typeof(TViewModel)] = (
                viewModel ??= dialogService.CreateViewModel<TViewModel>()
            );
            dialogService.Show(ownerViewModel, viewModel);
            var v = dialogService.DialogManager.FindViewByViewModel(viewModel);
            if (v?.RefObj is Window w)
                w.Closed += W_Closed;

            void W_Closed(object? sender, EventArgs e)
            {
                _displayedViewModelByType.Remove(typeof(TViewModel));
                w.Closed -= W_Closed;
            }
        }

        /// <summary>
        /// 关闭单例
        /// </summary>
        /// <typeparam name="TViewModel">视图模型类型</typeparam>
        public void CloseInstance<TViewModel>()
            where TViewModel : INotifyPropertyChanged
        {
            if (_displayedViewModelByType.Remove(typeof(TViewModel), out var viewModel) is false)
                return;
            dialogService.Close(viewModel);
        }

        public TViewModel CreateViewModel<TViewModel>(Action<TViewModel> action)
        {
            ArgumentNullException.ThrowIfNull(action);
            var vm = dialogService.CreateViewModel<TViewModel>();
            action(vm);
            return vm;
        }

        /// <summary>
        /// 显示消息框
        /// </summary>
        /// <param name="ownerViewModel">所有者视图模型</param>
        /// <param name="text">消息</param>
        /// <param name="title">标题</param>
        /// <param name="button">按钮</param>
        /// <param name="icon">图标</param>
        /// <returns>结果</returns>
        public bool? ShowMessageBox(
            INotifyPropertyChanged ownerViewModel,
            string text,
            string title = "",
            HanumanInstitute.MvvmDialogs.FrameworkDialogs.MessageBoxButton button =
                HanumanInstitute.MvvmDialogs.FrameworkDialogs.MessageBoxButton.Ok,
            HanumanInstitute.MvvmDialogs.FrameworkDialogs.MessageBoxImage icon =
                HanumanInstitute.MvvmDialogs.FrameworkDialogs.MessageBoxImage.None
        )
        {
            var owner = dialogService.DialogManager.FindViewByViewModel(ownerViewModel)!;
            return MessageBoxX
                .Show((Window)owner.RefObj, text, title, button.ToWPFButton(), icon.ToPUIIcon())
                .ToResult();
        }

        /// <summary>
        /// 显示等待窗口
        /// </summary>
        /// <param name="ownerViewModel">所有者视图模型</param>
        /// <param name="message">消息</param>
        /// <param name="canCancel">可以取消</param>
        /// <returns>等待窗口处理程序</returns>
        public PendingHandler ShowPendingBox(
            INotifyPropertyChanged ownerViewModel,
            string message,
            bool canCancel = false
        )
        {
            var owner = dialogService.DialogManager.FindViewByViewModel(ownerViewModel)!;
            var handler = PendingBox.Show((Window)owner.RefObj, message, canCancel);
            return new(handler);
        }

        /// <summary>
        /// 当视图模型的视图关闭后行动
        /// </summary>
        /// <typeparam name="TViewModel">视图模型类型</typeparam>
        /// <param name="viewModel">视图模型</param>
        /// <param name="action">行动</param>
        public void WhenClosed<TViewModel>(TViewModel viewModel, Action<TViewModel> action)
            where TViewModel : INotifyPropertyChanged
        {
            var owner = dialogService.DialogManager.FindViewByViewModel(viewModel)!;
            var window = (Window)owner.RefObj;
            window.Closed += Window_Closed;
            void Window_Closed(object? sender, EventArgs e)
            {
                action(viewModel);
                window.Closed -= Window_Closed;
            }
        }
    }

    /// <summary>
    /// 转换为结果
    /// </summary>
    /// <param name="result">提示框结果</param>
    /// <returns>结果</returns>
    public static bool? ToResult(this MessageBoxResult result)
    {
        return result switch
        {
            MessageBoxResult.OK => true,
            MessageBoxResult.Yes => true,
            MessageBoxResult.No => false,
            MessageBoxResult.Cancel => null,
            _ => null,
        };
    }

    /// <summary>
    /// 转换为PUI图标
    /// </summary>
    /// <param name="icon">图标</param>
    /// <returns>PUI图标</returns>
    public static MessageBoxIcon ToPUIIcon(
        this HanumanInstitute.MvvmDialogs.FrameworkDialogs.MessageBoxImage icon
    )
    {
        return icon switch
        {
            HanumanInstitute.MvvmDialogs.FrameworkDialogs.MessageBoxImage.None =>
                MessageBoxIcon.None,
            HanumanInstitute.MvvmDialogs.FrameworkDialogs.MessageBoxImage.Information =>
                MessageBoxIcon.Info,
            HanumanInstitute.MvvmDialogs.FrameworkDialogs.MessageBoxImage.Warning =>
                MessageBoxIcon.Warning,
            HanumanInstitute.MvvmDialogs.FrameworkDialogs.MessageBoxImage.Error =>
                MessageBoxIcon.Error,
            _ => MessageBoxIcon.None,
        };
    }

    /// <summary>
    /// 转换为WPF按钮
    /// </summary>
    /// <param name="button">MVVM窗口按钮</param>
    /// <returns>WPF按钮</returns>
    public static MessageBoxButton ToWPFButton(
        this HanumanInstitute.MvvmDialogs.FrameworkDialogs.MessageBoxButton button
    )
    {
        return button switch
        {
            HanumanInstitute.MvvmDialogs.FrameworkDialogs.MessageBoxButton.Ok =>
                MessageBoxButton.OK,
            HanumanInstitute.MvvmDialogs.FrameworkDialogs.MessageBoxButton.OkCancel =>
                MessageBoxButton.OKCancel,
            HanumanInstitute.MvvmDialogs.FrameworkDialogs.MessageBoxButton.YesNo =>
                MessageBoxButton.YesNo,
            HanumanInstitute.MvvmDialogs.FrameworkDialogs.MessageBoxButton.YesNoCancel =>
                MessageBoxButton.YesNoCancel,
            _ => MessageBoxButton.OK,
        };
    }
}

internal static class NativeUtils
{
    public const char Underscore = '_';

    //private static readonly ImmutableHashSet<string> _singleTypeCommonAnimes =
    //    ImmutableHashSet.Create(StringComparer.CurrentCultureIgnoreCase, "BDay");

    //public static bool IsSingleTypeCommonAnime(string animeName)
    //{
    //    return _singleTypeCommonAnimes.Contains(animeName);
    //}

    /// <summary>
    /// 解码像素宽度
    /// </summary>
    public const int DecodePixelWidth = 250;

    /// <summary>
    /// 解码像素高度
    /// </summary>
    public const int DecodePixelHeight = 250;

    /// <summary>
    /// 载入图片到流
    /// </summary>
    /// <param name="imagePath">图片路径</param>
    /// <returns>图片</returns>
    public static BitmapImage LoadImageToStream(string imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath) || File.Exists(imagePath) is false)
            return null!;
        BitmapImage bitmapImage = new();
        bitmapImage.BeginInit();
        try
        {
            bitmapImage.StreamSource = new StreamReader(imagePath).BaseStream;
        }
        finally
        {
            bitmapImage.EndInit();
        }
        return bitmapImage;
    }

    /// <summary>
    /// 载入图片至内存流
    /// </summary>
    /// <param name="imagePath">图片路径</param>
    /// <returns></returns>
    public static BitmapImage LoadImageToMemoryStream(string imagePath)
    {
        BitmapImage bitmapImage = new();
        bitmapImage.BeginInit();
        try
        {
            var bytes = File.ReadAllBytes(imagePath);
            bitmapImage.StreamSource = new MemoryStream(bytes);
            bitmapImage.DecodePixelWidth = DecodePixelWidth;
        }
        finally
        {
            bitmapImage.EndInit();
        }
        return bitmapImage;
    }

    /// <summary>
    /// 载入图片至内存流
    /// </summary>
    /// <param name="imageStream">图片流</param>
    /// <returns></returns>
    public static BitmapImage LoadImageToMemoryStream(Stream imageStream)
    {
        BitmapImage bitmapImage = new();
        bitmapImage.BeginInit();
        try
        {
            bitmapImage.StreamSource = imageStream;
            bitmapImage.DecodePixelWidth = DecodePixelWidth;
        }
        finally
        {
            bitmapImage.EndInit();
        }
        return bitmapImage;
    }

    /// <summary>
    /// 打开文件
    /// </summary>
    /// <param name="filePath">文件路径</param>
    public static void OpenLink(string filePath)
    {
        System
            .Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo(filePath) { UseShellExecute = true }
            )
            ?.Close();
    }

    /// <summary>
    /// 从资源管理器打开文件
    /// </summary>
    /// <param name="filePath">文件路径</param>
    public static void OpenFileFromExplorer(string filePath)
    {
        System
            .Diagnostics.Process.Start("Explorer", $"/select,{Path.GetFullPath(filePath)}")
            ?.Close();
    }
}

internal class LPSTranslateConverter : System.Windows.Data.IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value?.ToString() is string str)
            return str.Translate();
        return null!;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

public sealed class PendingHandler : IPendingHandler
{
    public PendingHandler(IPendingHandler handler)
    {
        _handler = handler;
    }

    private readonly IPendingHandler _handler;

    public event CancelEventHandler Cancelling
    {
        add { _handler.Cancelling += value; }
        remove { _handler.Cancelling -= value; }
    }

    public event EventHandler Closed
    {
        add { _handler.Closed += value; }
        remove { _handler.Closed -= value; }
    }

    public void Close()
    {
        _handler.Close();
    }

    public void Hide()
    {
        _handler.Hide();
    }

    public void Show()
    {
        _handler.Show();
    }

    public void UpdateMessage(string message)
    {
        _handler.UpdateMessage(message);
    }
}
