using System.Windows.Controls;
using System.Windows.Input;
using VPet.ModMaker.Models;
using VPet.ModMaker.ViewModels.ModEdit;

namespace VPet.ModMaker.Views.ModEdit;

/// <summary>
/// Page_Food.xaml 的交互逻辑
/// </summary>
public partial class FoodPage : UserControl
{
    /// <summary>
    /// 视图模型
    /// </summary>
    public FoodEditVM ViewModel => (FoodEditVM)DataContext;

    /// <inheritdoc/>
    public FoodPage()
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
