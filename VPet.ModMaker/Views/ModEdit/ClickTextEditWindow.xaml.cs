using System.Windows;
using Panuon.WPF.UI;
using VPet.ModMaker.ViewModels;

namespace VPet.ModMaker.Views;

/// <summary>
/// ClickTextWindow.xaml 的交互逻辑
/// </summary>
public partial class ClickTextEditWindow : WindowX
{
    /// <summary>
    /// 视图模型
    /// </summary>
    public ClickTextEditViewModel ViewModel => (ClickTextEditViewModel)DataContext;

    /// <inheritdoc/>
    public ClickTextEditWindow()
    {
        InitializeComponent();
    }

    private void Button_Cancel_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Button_Yes_Click(object? sender, RoutedEventArgs e)
    {
        ViewModel.DialogResult = true;
        Close();
    }
}
