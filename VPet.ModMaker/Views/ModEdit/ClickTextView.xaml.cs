using System.Windows.Controls;
using System.Windows.Input;
using VPet.ModMaker.Models;
using VPet.ModMaker.ViewModels;

namespace VPet.ModMaker.Views;

/// <summary>
/// ClickTextView.xaml 的交互逻辑
/// </summary>
public partial class ClickTextView : UserControl
{
    /// <summary>
    /// 视图模型
    /// </summary>
    public ClickTextEditViewModel ViewModel => (ClickTextEditViewModel)DataContext;

    /// <inheritdoc/>
    public ClickTextView()
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
