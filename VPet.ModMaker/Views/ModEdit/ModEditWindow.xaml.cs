using LinePutScript.Localization.WPF;
using Microsoft.Win32;
using Panuon.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.IO;
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
using VPet.ModMaker.ViewModels.ModEdit;
using VPet.ModMaker.Views.ModEdit.AnimeEdit;
using VPet.ModMaker.Views.ModEdit.ClickTextEdit;
using VPet.ModMaker.Views.ModEdit.FoodEdit;
using VPet.ModMaker.Views.ModEdit.LowTextEdit;
using VPet.ModMaker.Views.ModEdit.MoveEdit;
using VPet.ModMaker.Views.ModEdit.PetEdit;
using VPet.ModMaker.Views.ModEdit.SelectTextEdit;
using VPet.ModMaker.Views.ModEdit.WorkEdit;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.Views.ModEdit;

/// <summary>
/// winModInfo.xaml 的交互逻辑
/// </summary>
public partial class ModEditWindow : Window
{
    public ModEditWindowVM ViewModel => (ModEditWindowVM)DataContext;
    public FoodPage FoodPage { get; } = new();
    public LowTextPage LowTextPage { get; } = new();
    public ClickTextPage ClickTextPage { get; } = new();
    public SelectTextPage SelectTextPage { get; } = new();
    public PetPage PetPage { get; } = new();
    public WorkPage WorkPage { get; } = new();
    public MovePage MovePage { get; } = new();
    public AnimePage AnimePage { get; } = new();

    public ModEditWindow()
    {
        InitializeComponent();
        DataContext = new ModEditWindowVM(this);
        Closing += ModEditWindow_Closing;
        Closed += ModEditWindow_Closed;
    }

    /// <summary>
    /// 初始化数据
    /// </summary>
    public void InitializeData()
    {
        if (I18nHelper.Current.CultureNames.Count == 0)
        {
            if (
                MessageBox.Show("未添加任何文化,确定要添加文化吗?".Translate(), "", MessageBoxButton.YesNo)
                is not MessageBoxResult.Yes
            )
                return;
            ViewModel.AddCulture();
            if (
                I18nHelper.Current.CultureNames.Count == 0
                || MessageBox.Show(
                    "需要将文化 {0} 设为主要文化吗?".Translate(I18nHelper.Current.CultureNames.First()),
                    "",
                    MessageBoxButton.YesNo
                )
                    is not MessageBoxResult.Yes
            )
                return;
            ViewModel.SetMainCulture(I18nHelper.Current.CultureNames.First());
        }
    }

    private void ModEditWindow_Closing(object sender, CancelEventArgs e)
    {
        if (
            MessageBox.Show("确认退出吗?".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.No
        )
            e.Cancel = true;
    }

    private void ModEditWindow_Closed(object sender, EventArgs e)
    {
        ViewModel.Close();
        try
        {
            DataContext = null;
            FoodPage.DataContext = null;
            LowTextPage.DataContext = null;
            ClickTextPage.DataContext = null;
            SelectTextPage.DataContext = null;
            PetPage.DataContext = null;
            WorkPage.DataContext = null;
            MovePage.DataContext = null;
            AnimePage.DataContext = null;
        }
        catch { }
    }
}
