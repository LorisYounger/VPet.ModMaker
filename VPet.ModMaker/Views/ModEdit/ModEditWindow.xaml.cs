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
using HKW.WPF.Extensions;
using HKW.WPF.MVVMDialogs;
using LinePutScript.Localization.WPF;
using Microsoft.Win32;
using Panuon.WPF;
using Panuon.WPF.UI;
using ReactiveUI;
using VPet.ModMaker.Models;
using VPet.ModMaker.ViewModels.ModEdit;
using VPet.ModMaker.Views.ModEdit;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.Views.ModEdit;

/// <summary>
/// winModInfo.xaml 的交互逻辑
/// </summary>
public partial class ModEditWindow : WindowX, IPageLocator
{
    public ModEditWindow()
    {
        InitializeComponent();
        this.SetViewModel<ModEditVM>();
        Closing += ModEditWindow_Closing;

        //Closed += ModEditWindow_Closed;


        //LowTextPage = new();
        //ClickTextPage = new();
        //SelectTextPage = new();
        //PetPage = new();
        //WorkPage = new();
        //MovePage = new();
        //AnimePage = new();
        //I18nEditWindow = new();
    }

    /// <summary>
    /// 视图模型
    /// </summary>
    public ModEditVM ViewModel => (ModEditVM)DataContext;
    #region Food
    public FoodPage FoodPage { get; set; } = null!;
    public FoodEditWindow FoodEditWindow { get; set; } = null!;
    public FoodEditVM FoodEditVM { get; set; } = null!;
    #endregion
    public LowTextPage LowTextPage { get; } = null!;
    public ClickTextPage ClickTextPage { get; } = null!;
    public SelectTextPage SelectTextPage { get; } = null!;
    public PetPage PetPage { get; } = null!;
    public WorkPage WorkPage { get; } = null!;
    public MovePage MovePage { get; } = null!;
    public AnimePage AnimePage { get; } = null!;
    public I18nEditWindow I18nEditWindow { get; } = null!;

    public AddCulturePage AddCultureWindow { get; private set; } = null!;

    /// <inheritdoc/>
    public Dictionary<Type, Func<Window, Page?>> PageLocatorByType { get; } =
        new(
            [
                KeyValuePair.Create<Type, Func<Window, Page?>>(
                    typeof(FoodPage),
                    x => (Page)((ModEditWindow)x).Frame_Food.Content
                )
            ]
        );

    private void ModEditWindow_Closing(object? sender, CancelEventArgs e)
    {
        if (ViewModel.ModInfo is null)
            return;
        if (
            MessageBoxX.Show("确认退出吗?".Translate(), "退出编辑".Translate(), MessageBoxButton.YesNo)
            is not MessageBoxResult.Yes
        )
        {
            this.SkipNextClose();
            return;
        }
        MessageBus.Current.SendMessage<ModInfoModel>(null!);
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
            //I18nEditWindow.CloseX();
        }
        catch { }
    }

    public void InitializePage()
    {
        Frame_Food_Loaded(null!, null!);
    }

    private void Frame_Food_Loaded(object sender, RoutedEventArgs e)
    {
        FoodEditVM ??= new() { ModInfo = ViewModel.ModInfo };
        FoodPage ??= new() { DataContext = FoodEditVM };
        Frame_Food.Content ??= FoodPage;
    }
}
