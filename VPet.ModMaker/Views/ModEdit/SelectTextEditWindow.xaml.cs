using System.Windows;
using Panuon.WPF.UI;
using VPet.ModMaker.ViewModels;

namespace VPet.ModMaker.Views;

/// <summary>
/// SelectTextWindow.xaml 的交互逻辑
/// </summary>
public partial class SelectTextEditWindow : WindowX
{
    /// <inheritdoc/>
    public SelectTextEditWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 视图模型
    /// </summary>
    public SelectTextEditViewModel ViewModel => (SelectTextEditViewModel)DataContext;

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
