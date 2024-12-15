using System.Windows.Controls;
using System.Windows.Input;
using VPet.ModMaker.Models;
using VPet.ModMaker.ViewModels.ModEdit;

namespace VPet.ModMaker.Views.ModEdit;

/// <summary>
/// MovePage.xaml 的交互逻辑
/// </summary>
public partial class MovePage : UserControl
{
    /// <inheritdoc/>
    public MovePage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 视图模型
    /// </summary>
    public MoveEditVM ViewModel => (MoveEditVM)DataContext;

    private void DataGrid_MouseDoubleClick(object? sender, MouseButtonEventArgs e)
    {
        if (sender is not DataGrid dataGrid || dataGrid.SelectedItem is not MoveModel model)
            return;
        ViewModel.Edit(model);
    }
}
