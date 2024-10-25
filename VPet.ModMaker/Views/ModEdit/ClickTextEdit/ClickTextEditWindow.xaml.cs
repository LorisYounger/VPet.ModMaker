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
using HKW.WPF.Extensions;
using LinePutScript.Localization.WPF;
using VPet.ModMaker.Models;
using VPet.ModMaker.ViewModels.ModEdit;

namespace VPet.ModMaker.Views.ModEdit;

/// <summary>
/// ClickTextWindow.xaml 的交互逻辑
/// </summary>
public partial class ClickTextEditWindow : Window
{
    public bool IsCancel { get; private set; } = true;
    public ClickTextEditVM ViewModel => (ClickTextEditVM)DataContext;

    public ClickTextEditWindow()
    {
        InitializeComponent();
        this.SetViewModel<ClickTextEditVM>();
    }

    private void Button_Cancel_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Button_Yes_Click(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ViewModel.ClickText.ID))
        {
            MessageBox.Show("ID不可为空".Translate(), "", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (
            ViewModel.OldClickText?.ID != ViewModel.ClickText.ID
            && ViewModel.ModInfo.ClickTexts.Any(i => i.ID == ViewModel.ClickText.ID)
        )
        {
            MessageBox.Show("此ID已存在".Translate(), "", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (string.IsNullOrWhiteSpace(ViewModel.ClickText.Text))
        {
            MessageBox.Show("文本不可为空".Translate(), "", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        IsCancel = false;
        Close();
    }
}
