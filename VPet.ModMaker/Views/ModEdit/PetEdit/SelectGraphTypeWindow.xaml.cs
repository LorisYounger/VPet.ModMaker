using System.Windows;
using Panuon.WPF.UI;
using VPet.ModMaker.ViewModels;

namespace VPet.ModMaker.Views;

/// <summary>
/// SelectGraphTypeWindow.xaml 的交互逻辑
/// </summary>
public partial class SelectGraphTypeWindow : WindowX
{
    /// <summary>
    /// 视图模型
    /// </summary>
    public SelectGraphTypeViewModel ViewModel => (SelectGraphTypeViewModel)DataContext;

    /// <inheritdoc/>
    public SelectGraphTypeWindow()
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
