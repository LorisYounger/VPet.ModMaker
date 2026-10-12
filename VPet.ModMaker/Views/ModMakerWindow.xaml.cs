using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HKW.WPF.Extensions;
using HKW.WPF.MVVMDialogs;
using Panuon.WPF.UI;
using VPet.ModMaker.Models;
using VPet.ModMaker.ViewModels;
using VPet.ModMaker.Views;

namespace VPet.ModMaker.Views;

/// <summary>
/// ModMakerWindow.xaml 的交互逻辑
/// </summary>
public partial class ModMakerWindow : WindowX
{
    /// <summary>
    /// 视图模型
    /// </summary>
    public ModMakerViewModel ViewModel => (ModMakerViewModel)DataContext;

    /// <inheritdoc/>
    public ModMakerWindow()
    {
        InitializeComponent();
        //NativeUtils.ClipboardSetText = Clipboard.SetText;
    }

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
#if DEBUG
        GC.Collect();
#endif
    }
}
