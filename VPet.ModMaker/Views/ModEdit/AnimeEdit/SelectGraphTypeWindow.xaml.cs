using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using HKW.HKWUtils.Observable;
using LinePutScript.Localization.WPF;
using VPet.ModMaker.Models;
using VPet.ModMaker.Models.ModModel;
using VPet.ModMaker.ViewModels.ModEdit;
using VPet_Simulator.Core;

namespace VPet.ModMaker.Views.ModEdit;

/// <summary>
/// SelectGraphTypeWindow.xaml 的交互逻辑
/// </summary>
public partial class SelectGraphTypeWindow : Window
{
    public SelectGraphTypeWindowVM ViewModel => (SelectGraphTypeWindowVM)DataContext;

    public SelectGraphTypeWindow()
    {
        InitializeComponent();
        DataContext = new SelectGraphTypeWindowVM();
    }

    public bool IsCancel { get; private set; } = true;

    private void Button_Cancel_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Button_Yes_Click(object? sender, RoutedEventArgs e)
    {
        IsCancel = false;
        Close();
    }
}
