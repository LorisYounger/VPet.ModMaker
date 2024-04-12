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
using HKW.HKWUtils.Extensions;
using LinePutScript.Localization.WPF;
using Microsoft.Win32;
using Panuon.WPF;
using Panuon.WPF.UI;
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
public partial class ModEditWindow : WindowX
{
    public static ModEditWindow Current { get; private set; } = null!;
    public ModEditWindowVM ViewModel => (ModEditWindowVM)DataContext;
    public FoodPage FoodPage { get; } = null!;
    public LowTextPage LowTextPage { get; } = null!;
    public ClickTextPage ClickTextPage { get; } = null!;
    public SelectTextPage SelectTextPage { get; } = null!;
    public PetPage PetPage { get; } = null!;
    public WorkPage WorkPage { get; } = null!;
    public MovePage MovePage { get; } = null!;
    public AnimePage AnimePage { get; } = null!;

    public ModEditWindow()
    {
        Current = this;
        InitializeComponent();
        DataContext = new ModEditWindowVM(this);
        Closing += ModEditWindow_Closing;
        Closed += ModEditWindow_Closed;
        FoodPage = new();
        LowTextPage = new();
        ClickTextPage = new();
        SelectTextPage = new();
        PetPage = new();
        WorkPage = new();
        MovePage = new();
        AnimePage = new();
    }

    /// <summary>
    /// 初始化数据
    /// </summary>
    public void InitializeData()
    {
        if (ModInfoModel.Current.I18nResource.CultureDatas.HasValue() is false)
        {
            if (
                MessageBox.Show("未添加任何文化,确定要添加文化吗?".Translate(), "", MessageBoxButton.YesNo)
                is not MessageBoxResult.Yes
            )
                return;
            ViewModel.AddCultureCommand_ExecuteCommand();
            if (
                ModInfoModel.Current.I18nResource.CultureDatas.HasValue() is false
                || MessageBox.Show(
                    "需要将文化 {0} 设为主要文化吗?".Translate(
                        ModInfoModel.Current.I18nResource.CultureDatas.First().Key.Name
                    ),
                    "",
                    MessageBoxButton.YesNo
                )
                    is not MessageBoxResult.Yes
            )
                return;
            ViewModel.SetMainCultureCommand_ExecuteCommand(
                ModInfoModel.Current.I18nResource.CultureDatas.First().Key.Name
            );
        }
    }

    private void ModEditWindow_Closing(object? sender, CancelEventArgs e)
    {
        if (
            MessageBox.Show("确认退出吗?".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.No
        )
            e.Cancel = true;
    }

    private void ModEditWindow_Closed(object? sender, EventArgs e)
    {
        ViewModel?.Close();
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
