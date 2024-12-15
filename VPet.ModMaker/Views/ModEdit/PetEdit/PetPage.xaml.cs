using System.Windows.Controls;
using System.Windows.Input;
using VPet.ModMaker.Models;
using VPet.ModMaker.ViewModels.ModEdit;

namespace VPet.ModMaker.Views.ModEdit;

/// <summary>
/// PetPage.xaml 的交互逻辑
/// </summary>
public partial class PetPage : UserControl
{
    /// <summary>
    /// 视图模型
    /// </summary>
    public PetEditVM ViewModel => (PetEditVM)DataContext;

    /// <inheritdoc/>
    public PetPage()
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
