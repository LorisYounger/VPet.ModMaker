using System.Windows.Controls;
using System.Windows.Input;
using VPet.ModMaker.Models;
using VPet.ModMaker.ViewModels;

namespace VPet.ModMaker.Views;

/// <summary>
/// PetView.xaml 的交互逻辑
/// </summary>
public partial class PetView : UserControl
{
    /// <summary>
    /// 视图模型
    /// </summary>
    public PetEditViewModel ViewModel => (PetEditViewModel)DataContext;

    /// <inheritdoc/>
    public PetView()
    {
        InitializeComponent();
    }

    private void DataGrid_MouseDoubleClick(object? sender, MouseButtonEventArgs e)
    {
        if (sender is not DataGrid dataGrid || dataGrid.SelectedItem is not PetModel model)
            return;
        ViewModel.Edit(model);
    }
}
