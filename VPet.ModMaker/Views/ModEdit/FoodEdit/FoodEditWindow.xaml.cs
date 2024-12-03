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
using HKW.MVVMDialogs;
using HKW.WPF.Extensions;
using LinePutScript.Localization.WPF;
using Microsoft.Win32;
using Panuon.WPF.UI;
using VPet.ModMaker.Models;
using VPet.ModMaker.ViewModels.ModEdit;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.Views.ModEdit;

/// <summary>
/// AddFoodWindow.xaml 的交互逻辑
/// </summary>
public partial class FoodEditWindow : WindowX
{
    /// <inheritdoc/>
    public FoodEditWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 视图模型
    /// </summary>
    public FoodEditVM ViewModel => (FoodEditVM)DataContext;

    private void Button_Cancel_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Button_Yes_Click(object? sender, RoutedEventArgs e)
    {
        ViewModel.DialogResult = true;
        Close();
    }
}
