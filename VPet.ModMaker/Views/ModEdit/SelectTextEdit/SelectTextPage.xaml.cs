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
using VPet.ModMaker.ViewModels.ModEdit.SelectTextEdit;

namespace VPet.ModMaker.Views.ModEdit.SelectTextEdit;

/// <summary>
/// SelectTextPage.xaml 的交互逻辑
/// </summary>
public partial class SelectTextPage : Page
{
    public SelectTextPageVM ViewModel => (SelectTextPageVM)DataContext;

    public SelectTextPage()
    {
        InitializeComponent();
        DataContext = new SelectTextPageVM();
    }

    private void DataGrid_MouseDoubleClick(object? sender, MouseButtonEventArgs e)
    {
        if (sender is not DataGrid dataGrid || dataGrid.SelectedItem is not SelectTextModel model)
            return;
        ViewModel.Edit(model);
    }
}
