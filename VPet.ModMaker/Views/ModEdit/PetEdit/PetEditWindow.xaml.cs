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
/// PetEditWindow.xaml 的交互逻辑
/// </summary>
public partial class PetEditWindow : Window
{
    public PetEditVM ViewModel => (PetEditVM)DataContext;
    public bool IsCancel { get; private set; } = true;

    public PetEditWindow()
    {
        InitializeComponent();
        //DataContext = new PetEditWindowVM();
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

    private void Button_Cancel_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Button_Yes_Click(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ViewModel.Pet.ID))
        {
            MessageBox.Show("ID不可为空".Translate(), "", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (string.IsNullOrWhiteSpace(ViewModel.Pet.Name))
        {
            MessageBox.Show("名称不可为空".Translate(), "", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (string.IsNullOrWhiteSpace(ViewModel.Pet.PetName))
        {
            MessageBox.Show(
                "宠物名称不可为空".Translate(),
                "",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
            return;
        }
        if (
            ViewModel.OldPet?.ID != ViewModel.Pet.ID
            && ViewModel.ModInfo.Pets.Any(i => i.ID == ViewModel.Pet.ID)
        )
        {
            MessageBox.Show("此ID已存在", "", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        IsCancel = false;
        Close();
    }
}
