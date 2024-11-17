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
/// Page_Food.xaml 的交互逻辑
/// </summary>
public partial class FoodPage : UserControl
{
    /// <summary>
    /// 视图模型
    /// </summary>
    public FoodEditVM ViewModel => (FoodEditVM)DataContext;

    /// <inheritdoc/>
    public FoodPage()
    {
        InitializeComponent();
    }

    private void DataGrid_MouseDoubleClick(object? sender, MouseButtonEventArgs e)
    {
        if (sender is not DataGrid dataGrid || dataGrid.SelectedItem is not FoodModel model)
            return;
        ViewModel.Edit(model);
    }
}
