using System.Windows;
using Panuon.WPF.UI;
using VPet.ModMaker.ViewModels.ModEdit;

namespace VPet.ModMaker.Views.ModEdit;

/// <summary>
/// PetEditWindow.xaml 的交互逻辑
/// </summary>
public partial class PetEditWindow : WindowX
{
    /// <inheritdoc/>
    public PetEditWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 视图模型
    /// </summary>
    public PetEditVM ViewModel => (PetEditVM)DataContext;

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
