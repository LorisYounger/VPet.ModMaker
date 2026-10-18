using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HKW.WPF.Extensions;
using HKW.WPF.MVVMDialogs;
using LinePutScript.Localization.WPF;
using Panuon.WPF.UI;
using VPet.ModMaker.Models;
using VPet.ModMaker.ViewModels;

namespace VPet.ModMaker.Views;

/// <summary>
/// ModEditWindow.xaml 的交互逻辑
/// </summary>
public partial class ModEditWindow : WindowX
{
    /// <summary>
    /// 视图模型
    /// </summary>
    public ModEditViewModel ViewModel => (ModEditViewModel)DataContext;

    /// <inheritdoc/>
    public ModEditWindow()
    {
        InitializeComponent();
    }

    private void ListBox_Pages_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        // 横向滚动
        if (e.Delta == 0)
            return;

        var s = Views.FindVisualChild<ScrollViewer>();
        s.ScrollToHorizontalOffset(s.HorizontalOffset - e.Delta);
        e.Handled = true;
    }

    private void Label_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
#if DEBUG
        GC.Collect();
#endif
    }

    private void LayoutRoot_MouseMove(object sender, MouseEventArgs e)
    {
        var position = e.GetPosition(Grid_Collapse);

        var isInCollapseArea =
            position.X >= 0
            && position.X <= Grid_Collapse.ActualWidth
            && position.Y >= 0
            && position.Y <= Grid_Collapse.ActualHeight;

        ToggleButton_Collapse.Visibility = isInCollapseArea
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void LayoutRoot_MouseLeave(object sender, MouseEventArgs e)
    {
        ToggleButton_Collapse.Visibility = Visibility.Collapsed;
    }

    private void ToggleButton_Collapse_Click(object sender, RoutedEventArgs e)
    {
        Grid_ModInfo.Width = ToggleButton_Collapse.IsChecked is true ? 0 : 256;
    }
}
