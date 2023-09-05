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

namespace VPet.ModMaker.Views.ModEdit.SelectTextEdit;

/// <summary>
/// SelectTextPage.xaml 的交互逻辑
/// </summary>
public partial class SelectTextPage : Page
{
    public SelectTextPage()
    {
        InitializeComponent();
    }

    private void DataGrid_ClickText_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        //if (
        //    sender is not DataGrid dataGrid || dataGrid.SelectedItem is not ClickTextModel clickText
        //)
        //    return;
        //ViewModel.EditClickText(clickText);
    }
}
