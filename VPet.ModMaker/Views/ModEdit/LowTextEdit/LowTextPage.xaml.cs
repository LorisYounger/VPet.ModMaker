using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.Views.ModEdit;

/// <summary>
/// Page_LowText.xaml 的交互逻辑
/// </summary>
public partial class LowTextPage : UserControl
{
    public LowTextEditVM ViewModel => (LowTextEditVM)DataContext;

    public LowTextPage()
    {
        InitializeComponent();
    }

    private void DataGrid_LowText_MouseDoubleClick(object? sender, MouseButtonEventArgs e)
    {
        if (sender is not DataGrid dataGrid || dataGrid.SelectedItem is not LowTextModel lowText)
            return;
        ViewModel.Edit(lowText);
    }
}
