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
using System.Windows.Shapes;
using LinePutScript.Localization.WPF;
using Microsoft.Win32;
using VPet.ModMaker.Models;
using VPet.ModMaker.ViewModels.ModEdit.FoodEdit;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.Views.ModEdit.FoodEdit;

/// <summary>
/// AddFoodWindow.xaml 的交互逻辑
/// </summary>
public partial class FoodEditWindow : Window
{
    public bool IsCancel { get; private set; } = true;

    public FoodEditWindowVM ViewModel => (FoodEditWindowVM)DataContext;

    public FoodEditWindow()
    {
        InitializeComponent();
        DataContext = new FoodEditWindowVM();
        Closed += (s, e) =>
        {
            if (IsCancel)
                ViewModel.Close();
            try
            {
                DataContext = null;
            }
            catch { }
        };
    }

    private void Button_Cancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Button_Yes_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ViewModel.Food.Id))
        {
            MessageBox.Show("Id不可为空".Translate(), "", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (ViewModel.Food.Image is null)
        {
            MessageBox.Show("图像不可为空".Translate(), "", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (
            ViewModel.OldFood?.Id != ViewModel.Food.Id
            && ModInfoModel.Current.Foods.Any(i => i.Id == ViewModel.Food.Id)
        )
        {
            MessageBox.Show("此Id已存在", "", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        IsCancel = false;
        Close();
    }
}
