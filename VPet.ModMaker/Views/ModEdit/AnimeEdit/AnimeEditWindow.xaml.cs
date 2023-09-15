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
using VPet.ModMaker.ViewModels.ModEdit.AnimeEdit;

namespace VPet.ModMaker.Views.ModEdit.AnimeEdit;

/// <summary>
/// AnimeEditWindow.xaml 的交互逻辑
/// </summary>
public partial class AnimeEditWindow : Window
{
    public AnimeEditWindow()
    {
        InitializeComponent();
        DataContext = new AnimeEditWindowVM();
        Closed += (s, e) =>
        {
            try
            {
                DataContext = null;
            }
            catch { }
        };
    }

    public AnimeEditWindowVM ViewModel => (AnimeEditWindowVM)DataContext;

    public bool IsCancel { get; private set; } = true;

    private void Button_Cancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Button_Yes_Click(object sender, RoutedEventArgs e)
    {
        //if (string.IsNullOrEmpty(ViewModel.Work.Value.Id.Value))
        //{
        //    MessageBox.Show("Id不可为空".Translate(), "", MessageBoxButton.OK, MessageBoxImage.Warning);
        //    return;
        //}
        //if (string.IsNullOrEmpty(ViewModel.Work.Value.Graph.Value))
        //{
        //    MessageBox.Show(
        //        "指定动画Id不可为空".Translate(),
        //        "",
        //        MessageBoxButton.OK,
        //        MessageBoxImage.Warning
        //    );
        //    return;
        //}
        //if (
        //    ViewModel.OldWork?.Id.Value != ViewModel.Work.Value.Id.Value
        //    && ViewModel.CurrentPet.Works.Any(i => i.Id.Value == ViewModel.Work.Value.Id.Value)
        //)
        //{
        //    MessageBox.Show("此Id已存在".Translate(), "", MessageBoxButton.OK, MessageBoxImage.Warning);
        //    return;
        //}
        IsCancel = false;
        Close();
    }
}
