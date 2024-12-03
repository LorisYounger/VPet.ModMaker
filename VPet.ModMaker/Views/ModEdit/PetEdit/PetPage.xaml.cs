using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
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
