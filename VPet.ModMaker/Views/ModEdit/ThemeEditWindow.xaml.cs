using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Panuon.WPF.UI;
using VPet.ModMaker.ViewModels;

namespace VPet.ModMaker.Views;

/// <summary>
/// ThemeEditWindow.xaml 的交互逻辑
/// </summary>
public partial class ThemeEditWindow : WindowX
{
    public ThemeEditWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 视图模型
    /// </summary>
    public ThemeEditViewModel ViewModel => (ThemeEditViewModel)DataContext;

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
