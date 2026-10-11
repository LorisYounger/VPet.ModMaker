using System.Windows.Controls;
using System.Windows.Input;
using VPet.ModMaker.Models;
using VPet.ModMaker.ViewModels;

namespace VPet.ModMaker.Views;

/// <summary>
/// FoodView.xaml 的交互逻辑
/// </summary>
public partial class FoodView : UserControl
{
    /// <summary>
    /// 视图模型
    /// </summary>
    public FoodEditViewModel ViewModel => (FoodEditViewModel)DataContext;

    /// <inheritdoc/>
    public FoodView()
    {
        InitializeComponent();
    }

    private void DataGrid_MouseDoubleClick(object? sender, MouseButtonEventArgs e)
    {
        if (sender is not DataGrid dataGrid || dataGrid.SelectedItem is not FoodModel model)
            return;
        ViewModel.Edit(model);
    }
}
