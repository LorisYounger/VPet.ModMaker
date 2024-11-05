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
    public FoodPage FoodPage { get; private set; } = null!;
    public FoodEditVM FoodEditVM { get; private set; } = null!;
    #endregion
    #region ClickText
    public ClickTextPage ClickTextPage { get; private set; } = null!;
    public ClickTextEditVM ClickTextEditVM { get; private set; } = null!;
    #endregion
    #region LowText
    public LowTextPage LowTextPage { get; private set; } = null!;
    public LowTextEditVM LowTextEditVM { get; private set; } = null!;
    #endregion
    #region SelectText
    public SelectTextPage SelectTextPage { get; private set; } = null!;
    public SelectTextEditVM SelectTextEditVM { get; private set; } = null!;
    #endregion
    #region Pet
    public PetPage PetPage { get; private set; } = null!;
    public PetEditVM PetEditVM { get; private set; } = null!;
    #endregion
    #region Work
    public WorkPage WorkPage { get; private set; } = null!;
    public WorkEditVM WorkEditVM { get; private set; } = null!;
    #endregion
    #region Move
    public MovePage MovePage { get; private set; } = null!;
    public MoveEditVM MoveEditVM { get; private set; } = null!;
    #endregion
    #region Anime
    public AnimePage AnimePage { get; private set; } = null!;
    public AnimeEditVM AnimeEditWindowVM { get; private set; } = null!;
    #endregion
    //public I18nEditWindow I18nEditWindow { get; } = null!;

    //public AddCulturePage AddCultureWindow { get; private set; } = null!;

    /// <inheritdoc/>
    public Dictionary<Type, Func<Window, UserControl?>> PageLocatorByType { get; } =
        new(
            [
                KeyValuePair.Create<Type, Func<Window, UserControl?>>(
                    typeof(FoodPage),
                    x => (UserControl)((ModEditWindow)x).ContentControl_Food.Content
                ),
                KeyValuePair.Create<Type, Func<Window, UserControl?>>(
                    typeof(ClickTextPage),
                    x => (UserControl)((ModEditWindow)x).ContentControl_ClickText.Content
                ),
                KeyValuePair.Create<Type, Func<Window, UserControl?>>(
                    typeof(LowTextPage),
                    x => (UserControl)((ModEditWindow)x).ContentControl_LowText.Content
                ),
                KeyValuePair.Create<Type, Func<Window, UserControl?>>(
                    typeof(SelectTextPage),
                    x => (UserControl)((ModEditWindow)x).ContentControl_SelectText.Content
                ),
                KeyValuePair.Create<Type, Func<Window, UserControl?>>(
                    typeof(PetPage),
                    x => (UserControl)((ModEditWindow)x).ContentControl_Pet.Content
                ),
                KeyValuePair.Create<Type, Func<Window, UserControl?>>(
                    typeof(WorkPage),
                    x => (UserControl)((ModEditWindow)x).ContentControl_Work.Content
                ),
                KeyValuePair.Create<Type, Func<Window, UserControl?>>(
                    typeof(AnimePage),
                    x => (UserControl)((ModEditWindow)x).ContentControl_Anime.Content
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
        ContentControl_Food_Loaded(null!, null!);
        ContentControl_ClickText_Loaded(null!, null!);
        ContentControl_LowText_Loaded(null!, null!);
        ContentControl_SelectText_Loaded(null!, null!);
        ContentControl_Pet_Loaded(null!, null!);
        ContentControl_Work_Loaded(null!, null!);
        ContentControl_Move_Loaded(null!, null!);
        ContentControl_Anime_Loaded(null!, null!);
    }

    private void ContentControl_Food_Loaded(object sender, RoutedEventArgs e)
    {
        FoodEditVM ??= new() { ModInfo = ViewModel.ModInfo };
        FoodPage ??= new() { DataContext = FoodEditVM };
        ContentControl_Food.Content ??= FoodPage;
    }

    private void ContentControl_ClickText_Loaded(object sender, RoutedEventArgs e)
    {
        ClickTextEditVM ??= new() { ModInfo = ViewModel.ModInfo };
        ClickTextPage ??= new() { DataContext = ClickTextEditVM };
        ContentControl_ClickText.Content ??= ClickTextPage;
    }

    private void ContentControl_LowText_Loaded(object sender, RoutedEventArgs e)
    {
        LowTextEditVM ??= new() { ModInfo = ViewModel.ModInfo };
        LowTextPage ??= new() { DataContext = LowTextEditVM };
        ContentControl_LowText.Content ??= LowTextPage;
    }

    private void ContentControl_SelectText_Loaded(object sender, RoutedEventArgs e)
    {
        SelectTextEditVM ??= new() { ModInfo = ViewModel.ModInfo };
        SelectTextPage ??= new() { DataContext = SelectTextEditVM };
        ContentControl_SelectText.Content ??= SelectTextPage;
    }

    private void ContentControl_Pet_Loaded(object sender, RoutedEventArgs e)
    {
        PetEditVM ??= new() { ModInfo = ViewModel.ModInfo };
        PetPage ??= new() { DataContext = PetEditVM };
        ContentControl_Pet.Content ??= PetPage;
    }

    private void ContentControl_Work_Loaded(object sender, RoutedEventArgs e)
    {
        WorkEditVM ??= new() { ModInfo = ViewModel.ModInfo };
        WorkPage ??= new() { DataContext = WorkEditVM };
        ContentControl_Work.Content ??= WorkPage;
    }

    private void ContentControl_Move_Loaded(object sender, RoutedEventArgs e)
    {
        MoveEditVM ??= new() { ModInfo = ViewModel.ModInfo };
        MovePage ??= new() { DataContext = MoveEditVM };
        ContentControl_Move.Content ??= MovePage;
    }

    private void ContentControl_Anime_Loaded(object sender, RoutedEventArgs e)
    {
        //TODO:
        //AnimeEditVM ??= new() { ModInfo = ViewModel.ModInfo };
        //AnimePage ??= new() { DataContext = AnimeEditVM };
        //ContentControl_Anime.Content ??= AnimePage;
    }
}
