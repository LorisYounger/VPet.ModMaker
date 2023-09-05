using LinePutScript;
using LinePutScript.Localization.WPF;
using System;
using System.Collections.Generic;
using System.IO;
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
using VPet.ModMaker.Models;
using VPet.ModMaker.ViewModels;
using VPet.ModMaker.Views.ModEdit;
using VPet.ModMaker.Views.ModEdit.PetEdit;

namespace VPet.ModMaker.Views;

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
        if (item.DataContext is not ModMakerHistory history)
            return;
        if (Directory.Exists(history.SourcePath) is false)
        {
            if (
                MessageBox.Show($"路径不存在, 是否删除?".Translate(), "", MessageBoxButton.YesNo)
                is MessageBoxResult.Yes
            )
            {
                ViewModel.Histories.Remove(history);
                ViewModel.ShowHistories.Value.Remove(history);
            }
        }
        var loader = new ModLoader(new(history.SourcePath));
        if (loader.SuccessLoad)
        {
            ModInfoModel.Current = new(loader);
            ViewModel.CreateNewMod();
        }
        else
            MessageBox.Show($"载入失败".Translate());
    }
}
