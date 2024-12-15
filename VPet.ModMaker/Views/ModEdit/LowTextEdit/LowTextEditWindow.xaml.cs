using System.Windows;
using Panuon.WPF.UI;
using VPet.ModMaker.ViewModels.ModEdit;

namespace VPet.ModMaker.Views.ModEdit;

/// <summary>
/// Window_AddLowText.xaml 的交互逻辑
/// </summary>
public partial class LowTextEditWindow : WindowX
{
    /// <inheritdoc/>
    public LowTextEditWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 视图模型
    /// </summary>
    public LowTextEditVM ViewModel => (LowTextEditVM)DataContext;

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
