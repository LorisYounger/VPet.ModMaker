using LinePutScript;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
using VPet.Plugin.ModMaker.Models;
using VPet.Plugin.ModMaker.ViewModels;
using VPet.Plugin.ModMaker.Views.ModEdit;
using VPet.Plugin.ModMaker.Views.ModEdit.PetEdit;

namespace VPet.Plugin.ModMaker.Views;

/// <summary>
/// winModMaker.xaml 的交互逻辑
/// </summary>
public partial class ModMakerWindow : Window
{
    public ModMakerWindowVM ViewModel => (ModMakerWindowVM)DataContext;
    public ModEditWindow ModEditWindow { get; set; }
    public Models.ModMaker ModMaker { get; internal set; }

    public ModMakerWindow()
    {
        InitializeComponent();
        DataContext = new ModMakerWindowVM(this);
    }

    private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is not ListBoxItem item)
            return;
        ModInfoModel.Current = (ModInfoModel)item.DataContext;
        ViewModel.CreateNewMod();
    }
}
