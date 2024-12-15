using System.Windows.Controls;
using System.Windows.Input;
using VPet.ModMaker.Models;
using VPet.ModMaker.ViewModels.ModEdit;

namespace VPet.ModMaker.Views.ModEdit;

/// <summary>
/// ClickTextPage.xaml 的交互逻辑
/// </summary>
public partial class ClickTextPage : UserControl
{
    /// <summary>
    /// 视图模型
    /// </summary>
    public ClickTextEditVM ViewModel => (ClickTextEditVM)DataContext;

    /// <inheritdoc/>
    public ClickTextPage()
    {
        InitializeComponent();
    }

    private void DataGrid_MouseDoubleClick(object? sender, MouseButtonEventArgs e)
    {
        if (sender is not DataGrid dataGrid || dataGrid.SelectedItem is not ClickTextModel model)
            return;
        ViewModel.Edit(model);
    }
}
