using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HKW.HKWReactiveUI;
using HKW.WPF.Extensions;
using HKW.WPF.MVVMDialogs;
using Panuon.WPF.UI;
using ReactiveUI;
using VPet.ModMaker.Models;
using VPet.ModMaker.ViewModels;
using VPet.ModMaker.Views.ModEdit;

namespace VPet.ModMaker.Views;

/// <summary>
/// winModMaker.xaml 的交互逻辑
/// </summary>
public partial class ModMakerWindow : WindowX, IPageLocator, IEnableLogger<ViewModelBase>
{
    /// <inheritdoc/>
    public ModMakerWindow()
    {
        InitializeComponent();
        NativeUtils.ClipboardSetText = Clipboard.SetText;
        IDisposable? bus = null;
        this.SetViewModel<ModMakerVM>(
            (s, e) =>
            {
                ViewModel.Close();
                bus?.Dispose();
            }
        );
        bus = MessageBus
            .Current.Listen<ModInfoModel?>()
            .Subscribe(x =>
            {
                if (x is null)
                {
                    GC.Collect();
                    this.ShowOrActivate();
                    ModEditWindow.Hide();
                    if (string.IsNullOrEmpty(ViewModel.ModInfo?.SourcePath) is false)
                        ViewModel.AddHistory(ViewModel.ModInfo);
                    ModEditWindow.ViewModel.ModInfo = ViewModel.ModInfo = null!;
                    ModEditWindow.ShowTab(-1);
                }
                else
                {
                    GC.Collect();
                    this.Hide();
                    ModEditWindow.ShowOrActivate();
                    ModEditWindow.SetLocationToCenter();
                    ModEditWindow.ViewModel.ModInfo = x;
                    ModEditWindow.ShowTab(0);

#if !RELEASE
                    //ModEditWindow.InitializePages();
#endif
                }
            });
    }

    /// <summary>
    /// 视图模型
    /// </summary>
    public ModMakerVM ViewModel => (ModMakerVM)DataContext;

    /// <inheritdoc/>
    public Func<Type, FrameworkElement?>? LocatePageByType { get; }

    #region ModEditWindow
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ModEditWindow? _modEditWindow;

    /// <summary>
    /// 单例窗口
    /// </summary>
    public ModEditWindow ModEditWindow => _modEditWindow ??= new ModEditWindow().MaskClose(this);
    #endregion

    private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is not ListBoxItem item)
            return;
        if (item.DataContext is not ModMakeHistory history)
            return;
        ViewModel.LoadHistory(history);
    }

    private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
#if !RELEASE
        GC.Collect();
#endif
    }
}
