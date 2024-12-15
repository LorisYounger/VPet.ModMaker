using System.Windows.Controls;
using System.Windows.Input;
using VPet.ModMaker.Models;
using VPet.ModMaker.ViewModels.ModEdit;

namespace VPet.ModMaker.Views.ModEdit;

/// <summary>
/// WorkPage.xaml 的交互逻辑
/// </summary>
public partial class WorkPage : UserControl
{
    /// <summary>
    /// 视图模型
    /// </summary>
    public WorkEditVM ViewModel => (WorkEditVM)DataContext;

    /// <inheritdoc/>
    public WorkPage()
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
