using System.Windows.Controls;
using System.Windows.Input;
using VPet.ModMaker.Models;
using VPet.ModMaker.ViewModels;

namespace VPet.ModMaker.Views;

/// <summary>
/// ThemeView.xaml 的交互逻辑
/// </summary>
public partial class ThemeView : UserControl
{
    /// <summary>
    /// 视图模型
    /// </summary>
    public ThemeEditViewModel ViewModel => (ThemeEditViewModel)DataContext;

    /// <inheritdoc/>
    public ThemeView()
    {
        InitializeComponent();
    }

    private void DataGrid_MouseDoubleClick(object? sender, MouseButtonEventArgs e)
    {
        if (sender is not DataGrid dataGrid || dataGrid.SelectedItem is not ThemeModel model)
            return;
        ViewModel.Edit(model);
    }
}
