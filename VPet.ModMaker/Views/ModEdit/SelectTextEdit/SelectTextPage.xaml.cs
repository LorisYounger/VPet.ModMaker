using System.Windows.Controls;
using System.Windows.Input;
using VPet.ModMaker.Models;
using VPet.ModMaker.ViewModels.ModEdit;

namespace VPet.ModMaker.Views.ModEdit;

/// <summary>
/// SelectTextPage.xaml 的交互逻辑
/// </summary>
public partial class SelectTextPage : UserControl
{
    /// <summary>
    /// 视图模型
    /// </summary>
    public SelectTextEditVM ViewModel => (SelectTextEditVM)DataContext;

    /// <inheritdoc/>
    public SelectTextPage()
    {
        InitializeComponent();
    }

    private void DataGrid_MouseDoubleClick(object? sender, MouseButtonEventArgs e)
    {
        if (sender is not DataGrid dataGrid || dataGrid.SelectedItem is not SelectTextModel model)
            return;
        ViewModel.Edit(model);
    }
}
