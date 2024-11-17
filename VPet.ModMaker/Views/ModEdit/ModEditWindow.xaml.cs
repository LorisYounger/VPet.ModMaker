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
    /// <inheritdoc/>
    public ModEditWindow()
    {
        InitializeComponent();
        this.SetViewModel<ModEditVM>();
        Closing += ModEditWindow_Closing;
    }

    /// <summary>
    /// 视图模型
    /// </summary>
    public ModEditVM ViewModel => (ModEditVM)DataContext;

    #region Food
    /// <summary>
    /// 食物页面
    /// </summary>
    public FoodPage FoodPage { get; private set; } = null!;

    /// <summary>
    /// 食物编辑视图模型
    /// </summary>
    public FoodEditVM FoodEditVM { get; private set; } = null!;
    #endregion
    #region ClickText
    /// <summary>
    /// 点击文本页面
    /// </summary>
    public ClickTextPage ClickTextPage { get; private set; } = null!;

    /// <summary>
    /// 点击文本编辑视图模型
    /// </summary>
    public ClickTextEditVM ClickTextEditVM { get; private set; } = null!;
    #endregion
    #region LowText
    /// <summary>
    /// 低状态文本页面
    /// </summary>
    public LowTextPage LowTextPage { get; private set; } = null!;

    /// <summary>
    /// 低状态文本编辑视图模型
    /// </summary>
    public LowTextEditVM LowTextEditVM { get; private set; } = null!;
    #endregion
    #region SelectText
    /// <summary>
    /// 选择文本页面
    /// </summary>
    public SelectTextPage SelectTextPage { get; private set; } = null!;

    /// <summary>
    /// 选择文本编辑视图模型
    /// </summary>
    public SelectTextEditVM SelectTextEditVM { get; private set; } = null!;
    #endregion
    #region Pet
    /// <summary>
    /// 宠物页面
    /// </summary>
    public PetPage PetPage { get; private set; } = null!;

    /// <summary>
    /// 宠物编辑视图模型
    /// </summary>
    public PetEditVM PetEditVM { get; private set; } = null!;
    #endregion
    #region Work
    /// <summary>
    /// 工作页面
    /// </summary>
    public WorkPage WorkPage { get; private set; } = null!;

    /// <summary>
    /// 工作编辑视图模型
    /// </summary>
    public WorkEditVM WorkEditVM { get; private set; } = null!;
    #endregion
    #region Move
    /// <summary>
    /// 移动页面
    /// </summary>
    public MovePage MovePage { get; private set; } = null!;

    /// <summary>
    /// 移动编辑视图模型
    /// </summary>
    public MoveEditVM MoveEditVM { get; private set; } = null!;
    #endregion
    #region Anime
    /// <summary>
    /// 动画页面
    /// </summary>
    public AnimePage AnimePage { get; private set; } = null!;

    /// <summary>
    /// 动画编辑视图模型
    /// </summary>
    public AnimeVM AnimeVM { get; private set; } = null!;
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
        ViewModel.Close();
        ResetPage();
        MessageBus.Current.SendMessage<ModInfoModel?>(null);
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

    /// <summary>
    /// 显示页面
    /// </summary>
    /// <param name="index"></param>
    public void ShowTab(int index)
    {
        TabControl_Main.SelectedIndex = index;
    }

    /// <summary>
    /// 初始化所有页面
    /// </summary>
    public void InitializePages()
    {
        InitializeFood();
        InitializeClickText();
        InitializeLowText();
        InitializeSelectText();
        InitializePet();
        InitializeWork();
        InitializeMove();
        InitializeAnime();
    }

    /// <summary>
    /// 重置页面
    /// </summary>
    public void ResetPage()
    {
        if (FoodEditVM is not null)
            FoodEditVM.ModInfo = null!;
        if (ClickTextEditVM is not null)
            ClickTextEditVM.ModInfo = null!;
        if (LowTextEditVM is not null)
            LowTextEditVM.ModInfo = null!;
        if (SelectTextEditVM is not null)
            SelectTextEditVM.ModInfo = null!;
        if (PetEditVM is not null)
            PetEditVM.ModInfo = null!;
        if (MoveEditVM is not null)
            MoveEditVM.ModInfo = null!;
        if (WorkEditVM is not null)
            WorkEditVM.ModInfo = null!;
        if (AnimeVM is not null)
            AnimeVM.ModInfo = null!;
    }

    private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (IsLoaded is false)
            return;
        if (sender is not TabControl tabControl)
            return;
        if (tabControl.SelectedIndex == 0)
            InitializeFood();
        else if (tabControl.SelectedIndex == 1)
            InitializeClickText();
        else if (tabControl.SelectedIndex == 2)
            InitializeLowText();
        else if (tabControl.SelectedIndex == 3)
            InitializeSelectText();
        else if (tabControl.SelectedIndex == 4)
            InitializePet();
        else if (tabControl.SelectedIndex == 5)
            InitializeWork();
        else if (tabControl.SelectedIndex == 6)
            InitializeMove();
        else if (tabControl.SelectedIndex == 7)
            InitializeAnime();
    }

    private void InitializeFood()
    {
        FoodEditVM ??= new();
        FoodEditVM.ModInfo = ViewModel.ModInfo;
        FoodPage ??= new() { DataContext = FoodEditVM };
        ContentControl_Food.Content ??= FoodPage;
    }

    private void InitializeClickText()
    {
        ClickTextEditVM ??= new();
        ClickTextEditVM.ModInfo = ViewModel.ModInfo;
        ClickTextPage ??= new() { DataContext = ClickTextEditVM };
        ContentControl_ClickText.Content ??= ClickTextPage;
    }

    private void InitializeLowText()
    {
        LowTextEditVM ??= new();
        LowTextEditVM.ModInfo = ViewModel.ModInfo;
        LowTextPage ??= new() { DataContext = LowTextEditVM };
        ContentControl_LowText.Content ??= LowTextPage;
    }

    private void InitializeSelectText()
    {
        SelectTextEditVM ??= new();
        SelectTextEditVM.ModInfo = ViewModel.ModInfo;
        SelectTextPage ??= new() { DataContext = SelectTextEditVM };
        ContentControl_SelectText.Content ??= SelectTextPage;
    }

    private void InitializePet()
    {
        PetEditVM ??= new();
        PetEditVM.ModInfo = ViewModel.ModInfo;
        PetPage ??= new() { DataContext = PetEditVM };
        ContentControl_Pet.Content ??= PetPage;
    }

    private void InitializeWork()
    {
        WorkEditVM ??= new();
        WorkEditVM.ModInfo = ViewModel.ModInfo;
        WorkPage ??= new() { DataContext = WorkEditVM };
        ContentControl_Work.Content ??= WorkPage;
    }

    private void InitializeMove()
    {
        MoveEditVM ??= new();
        MoveEditVM.ModInfo = ViewModel.ModInfo;
        MovePage ??= new() { DataContext = MoveEditVM };
        ContentControl_Move.Content ??= MovePage;
    }

    private void InitializeAnime()
    {
        AnimeVM ??= new() { ModInfo = ViewModel.ModInfo };
        AnimePage ??= new() { DataContext = AnimeVM };
        ContentControl_Anime.Content ??= AnimePage;
    }
}
