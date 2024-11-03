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
using Panuon.WPF.UI;
using VPet.ModMaker.Models;
using VPet.ModMaker.ViewModels.ModEdit;

namespace VPet.ModMaker.Views.ModEdit;

/// <summary>
/// WorkEditWindow.xaml 的交互逻辑
/// </summary>
public partial class WorkEditWindow : Window
{
    public WorkEditVM ViewModel => (WorkEditVM)DataContext;

    public WorkEditWindow()
    {
        InitializeComponent();
    }

    private void Button_Cancel_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Button_Yes_Click(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ViewModel.Work.ID))
        {
            MessageBoxX.Show(
                "ID不可为空".Translate(),
                "数据错误".Translate(),
                MessageBoxButton.OK,
                MessageBoxIcon.Warning
            );
            return;
        }
        if (string.IsNullOrWhiteSpace(ViewModel.Work.Graph))
        {
            MessageBoxX.Show(
                "指定动画ID不可为空".Translate(),
                "数据错误".Translate(),
                MessageBoxButton.OK,
                MessageBoxIcon.Warning
            );
            return;
        }
        if (
            ViewModel.OldWork?.ID != ViewModel.Work.ID
            && ViewModel.CurrentPet.Works.Any(i => i.ID == ViewModel.Work.ID)
        )
        {
            MessageBoxX.Show(
                "此ID已存在".Translate(),
                "数据错误".Translate(),
                MessageBoxButton.OK,
                MessageBoxIcon.Warning
            );
            return;
        }
        Close();
    }
}
