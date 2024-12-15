using System.Windows;
using Panuon.WPF.UI;
using VPet.ModMaker.ViewModels.ModEdit;

namespace VPet.ModMaker.Views.ModEdit;

/// <summary>
/// Window_AddLang.xaml 的交互逻辑
/// </summary>
public partial class AddCultureWindow : WindowX
{
    /// <summary>
    /// 视图模型
    /// </summary>
    public AddCultureVM ViewModel => (AddCultureVM)DataContext;

    /// <inheritdoc/>
    public AddCultureWindow()
    {
        InitializeComponent();
        TextBox_Lang.Focus();
        TextBox_Lang.Dispatcher.InvokeAsync(TextBox_Lang.SelectAll);
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
