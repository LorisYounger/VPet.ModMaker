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
using VPet.ModMaker.ViewModels.ModEdit.MoveEdit;

namespace VPet.ModMaker.Views.ModEdit.MoveEdit;

/// <summary>
/// MoveEditWindow.xaml 的交互逻辑
/// </summary>
public partial class MoveEditWindow : Window
{
    public bool IsCancel { get; private set; } = true;
    public MoveEditWindowVM ViewModel => (MoveEditWindowVM)DataContext;

    public MoveEditWindow()
    {
        InitializeComponent();
        DataContext = new MoveEditWindowVM();
        Closed += (s, e) =>
        {
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
        if (string.IsNullOrEmpty(ViewModel.Move.Value.Graph.Value))
        {
            MessageBox.Show(
                "指定动画Id不可为空".Translate(),
                "",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
            return;
        }
        IsCancel = false;
        Close();
    }
}
