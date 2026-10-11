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

        var s = EditorViews.FindVisualChild<ScrollViewer>();
        s.ScrollToHorizontalOffset(s.HorizontalOffset - e.Delta);
        e.Handled = true;
    }
}
