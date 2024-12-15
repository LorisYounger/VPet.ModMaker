using System.Windows;
using Panuon.WPF.UI;
using VPet.ModMaker.ViewModels.ModEdit;

namespace VPet.ModMaker.Views.ModEdit;

/// <summary>
/// MoveEditWindow.xaml 的交互逻辑
/// </summary>
public partial class MoveEditWindow : WindowX
{
    /// <inheritdoc/>
    public MoveEditWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 视图模型
    /// </summary>
    public MoveEditVM ViewModel => (MoveEditVM)DataContext;

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
