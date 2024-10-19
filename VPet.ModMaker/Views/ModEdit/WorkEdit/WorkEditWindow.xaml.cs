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
using VPet.ModMaker.Models;
using VPet.ModMaker.ViewModels.ModEdit;

namespace VPet.ModMaker.Views.ModEdit;

/// <summary>
/// WorkEditWindow.xaml 的交互逻辑
/// </summary>
public partial class WorkEditWindow : Window
{
    public bool IsCancel { get; private set; } = true;
    public WorkEditWindowVM ViewModel => (WorkEditWindowVM)DataContext;

    public WorkEditWindow()
    {
        InitializeComponent();
        this.SetDataContext<WorkEditWindowVM>();
    }

    private void Button_Cancel_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Button_Yes_Click(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ViewModel.Work.ID))
        {
            MessageBox.Show("ID不可为空".Translate(), "", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (string.IsNullOrWhiteSpace(ViewModel.Work.Graph))
        {
            MessageBox.Show(
                "指定动画ID不可为空".Translate(),
                "",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
            return;
        }
        if (
            ViewModel.OldWork?.ID != ViewModel.Work.ID
            && ViewModel.CurrentPet.Works.Any(i => i.ID == ViewModel.Work.ID)
        )
        {
            MessageBox.Show("此ID已存在".Translate(), "", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        IsCancel = false;
        Close();
    }
}
