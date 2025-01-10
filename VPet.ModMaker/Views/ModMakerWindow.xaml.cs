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
        this.SetViewModel<ModMakerVM>(
            (s, e) =>
            {
                ViewModel.Close();
            }
        );
    }

    /// <summary>
    /// 视图模型
    /// </summary>
    public ModMakerVM ViewModel => (ModMakerVM)DataContext;

    /// <inheritdoc/>
    public Func<Type, FrameworkElement?>? LocatePageByType { get; }

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
