using LinePutScript.Localization.WPF;
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
using VPet.ModMaker.Models;
using VPet.ModMaker.ViewModels.ModEdit.WorkEdit;

namespace VPet.ModMaker.Views.ModEdit.WorkEdit;

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
        DataContext = new WorkEditWindowVM();
    }

    private void Button_Cancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Button_Yes_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrEmpty(ViewModel.Work.Value.Id.Value))
        {
            MessageBox.Show("Id不可为空".Translate(), "", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (string.IsNullOrEmpty(ViewModel.Work.Value.Graph.Value))
        {
            MessageBox.Show(
                "指定动画Id不可为空".Translate(),
                "",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
            return;
        }
        if (
            ViewModel.OldWork?.Id.Value != ViewModel.Work.Value.Id.Value
            && ViewModel.CurrentPet.Works.Any(i => i.Id.Value == ViewModel.Work.Value.Id.Value)
        )
        {
            MessageBox.Show("此Id已存在".Translate(), "", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        IsCancel = false;
        Close();
    }
}
