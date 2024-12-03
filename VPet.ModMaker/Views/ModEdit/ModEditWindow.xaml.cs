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
                    typeof(MovePage),
                    x => (UserControl)((ModEditWindow)x).ContentControl_Move.Content
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
        if (ContentControl_Food.Content is FoodPage foodPage)
            foodPage.ViewModel.ModInfo = null!;
        ContentControl_Food.Content = null;

        if (ContentControl_ClickText.Content is ClickTextPage clickTextPage)
            clickTextPage.ViewModel.ModInfo = null!;
        ContentControl_ClickText.Content = null;

        if (ContentControl_LowText.Content is LowTextPage lowTextPage)
            lowTextPage.ViewModel.ModInfo = null!;
        ContentControl_LowText.Content = null;

        if (ContentControl_SelectText.Content is SelectTextPage selectTextPage)
            selectTextPage.ViewModel.ModInfo = null!;
        ContentControl_SelectText.Content = null;

        if (ContentControl_Pet.Content is PetPage petPage)
            petPage.ViewModel.ModInfo = null!;
        ContentControl_Pet.Content = null;

        if (ContentControl_Move.Content is MovePage movePage)
            movePage.ViewModel.ModInfo = null!;
        ContentControl_Move.Content = null;

        if (ContentControl_Work.Content is WorkPage workPage)
            workPage.ViewModel.ModInfo = null!;
        ContentControl_Work.Content = null;

        if (ContentControl_Anime.Content is AnimePage animePage)
            animePage.ViewModel.ModInfo = null!;
        ContentControl_Anime.Content = null;
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
        if (ContentControl_Food.Content is not null)
            return;
        var vm = new FoodEditVM { ModInfo = ViewModel.ModInfo };
        var page = new FoodPage() { DataContext = vm };
        ContentControl_Food.Content = page;
    }

    private void InitializeClickText()
    {
        if (ContentControl_ClickText.Content is not null)
            return;
        var vm = new ClickTextEditVM { ModInfo = ViewModel.ModInfo };
        var page = new ClickTextPage() { DataContext = vm };
        ContentControl_ClickText.Content = page;
    }

    private void InitializeLowText()
    {
        if (ContentControl_LowText.Content is not null)
            return;
        var vm = new LowTextEditVM { ModInfo = ViewModel.ModInfo };
        var page = new LowTextPage() { DataContext = vm };
        ContentControl_LowText.Content = page;
    }

    private void InitializeSelectText()
    {
        if (ContentControl_SelectText.Content is not null)
            return;
        var vm = new SelectTextEditVM { ModInfo = ViewModel.ModInfo };
        var page = new SelectTextPage() { DataContext = vm };
        ContentControl_SelectText.Content = page;
    }

    private void InitializePet()
    {
        if (ContentControl_Pet.Content is not null)
            return;
        var vm = new PetEditVM { ModInfo = ViewModel.ModInfo };
        var page = new PetPage() { DataContext = vm };
        ContentControl_Pet.Content = page;
    }

    private void InitializeWork()
    {
        if (ContentControl_Work.Content is not null)
            return;
        var vm = new WorkEditVM { ModInfo = ViewModel.ModInfo };
        var page = new WorkPage() { DataContext = vm };
        ContentControl_Work.Content = page;
    }

    private void InitializeMove()
    {
        if (ContentControl_Move.Content is not null)
            return;
        var vm = new MoveEditVM { ModInfo = ViewModel.ModInfo };
        var page = new MovePage() { DataContext = vm };
        ContentControl_Move.Content = page;
    }

    private void InitializeAnime()
    {
        if (ContentControl_Anime.Content is not null)
            return;
        var vm = new AnimeVM { ModInfo = ViewModel.ModInfo };
        var page = new AnimePage() { DataContext = vm };
        ContentControl_Anime.Content = page;
    }
}
