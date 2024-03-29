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
using VPet.ModMaker.ViewModels.ModEdit.PetEdit;

namespace VPet.ModMaker.Views.ModEdit.PetEdit;

/// <summary>
/// PetPage.xaml 的交互逻辑
/// </summary>
public partial class PetPage : Page
{
    public PetPageVM ViewModel => (PetPageVM)DataContext;

    public PetPage()
    {
        InitializeComponent();
        DataContext = new PetPageVM();
    }

    private void DataGrid_MouseDoubleClick(object? sender, MouseButtonEventArgs e)
    {
        if (sender is not DataGrid dataGrid || dataGrid.SelectedItem is not PetModel model)
            return;
        ViewModel.EditCommand_ExecuteCommand(model);
    }
}
