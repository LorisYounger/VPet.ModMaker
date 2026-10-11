using System.Windows.Controls;
using System.Windows.Input;
using VPet.ModMaker.Models;
using VPet.ModMaker.ViewModels;

namespace VPet.ModMaker.Views;

/// <summary>
/// SelectTextView.xaml 的交互逻辑
/// </summary>
public partial class SelectTextView : UserControl
{
    /// <summary>
    /// 视图模型
    /// </summary>
    public SelectTextEditViewModel ViewModel => (SelectTextEditViewModel)DataContext;

    /// <inheritdoc/>
    public SelectTextView()
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
