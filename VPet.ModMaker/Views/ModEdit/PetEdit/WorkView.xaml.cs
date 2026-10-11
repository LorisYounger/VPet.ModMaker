using System.Windows.Controls;
using System.Windows.Input;
using VPet.ModMaker.Models;
using VPet.ModMaker.ViewModels;

namespace VPet.ModMaker.Views;

/// <summary>
/// WorkView.xaml 的交互逻辑
/// </summary>
public partial class WorkView : UserControl
{
    /// <summary>
    /// 视图模型
    /// </summary>
    public WorkEditViewModel ViewModel => (WorkEditViewModel)DataContext;

    /// <inheritdoc/>
    public WorkView()
    {
        InitializeComponent();
    }

    private void DataGrid_MouseDoubleClick(object? sender, MouseButtonEventArgs e)
    {
        if (sender is not DataGrid dataGrid || dataGrid.SelectedItem is not WorkModel model)
            return;
        ViewModel.Edit(model);
    }
}
