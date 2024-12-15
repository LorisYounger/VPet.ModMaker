using System.Windows;
using Panuon.WPF.UI;
using VPet.ModMaker.ViewModels.ModEdit;

namespace VPet.ModMaker.Views.ModEdit;

/// <summary>
/// SelectGraphTypeWindow.xaml 的交互逻辑
/// </summary>
public partial class SelectGraphTypeWindow : WindowX
{
    /// <summary>
    /// 视图模型
    /// </summary>
    public SelectGraphTypeVM ViewModel => (SelectGraphTypeVM)DataContext;

    /// <inheritdoc/>
    public SelectGraphTypeWindow()
    {
        InitializeComponent();
        DataContext = new SelectGraphTypeVM();
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
