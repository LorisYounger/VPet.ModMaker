using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using HKW.HKWReactiveUI;
using HKW.WPF.Extensions;
using HKW.WPF.MVVMDialogs;
using HKW.WPF.Utils;
using LinePutScript.Localization.WPF;
using Panuon.WPF.UI;
using ReactiveUI;
using VPet.ModMaker.Models;
using VPet.ModMaker.ViewModels.ModEdit;

namespace VPet.ModMaker.Views.ModEdit;

/// <summary>
/// winModInfo.xaml 的交互逻辑
/// </summary>
public partial class ModEditWindow : WindowX, IPageLocator, IDisposables
{
    /// <inheritdoc/>
    public ModEditWindow()
    {
        InitializeComponent();
        this.SetViewModel<ModEditVM>();
        ListBox_Pages.ItemsSource = PageByType.Values;
        Closing += ModEditWindow_Closing;
    }

    /// <summary>
    /// 处理列表
    /// </summary>
    public List<IDisposable> Disposables { get; private set; } = [];

    /// <summary>
    /// 视图模型
    /// </summary>
    public ModEditVM ViewModel => (ModEditVM)DataContext;

    private Func<Type, FrameworkElement?>? _locatePageByType;

    /// <inheritdoc/>
    public Func<Type, FrameworkElement?>? LocatePageByType =>
        _locatePageByType ??= t => PageByType[t].Control;

    private Dictionary<Type, ControlWrapper<UserControl>>? _pages;

    /// <inheritdoc/>
    public Dictionary<Type, ControlWrapper<UserControl>> PageByType =>
        _pages ??= new()
        {
            [typeof(FoodPage)] = new(w =>
            {
                var vm = new FoodEditVM() { ModInfo = ViewModel.ModInfo };
                var page = new FoodPage() { DataContext = vm };

                ViewModel
                    .ModInfo.WhenAnyValue(x => x.Foods.Count)
                    .Subscribe(_ => w.RefreshDisplayText())
                    .Record(this);
                return page;
            })
            {
                DisplayTextAction = _ => "食物".Translate() + $" ({ViewModel.ModInfo.Foods.Count})"
            },
            [typeof(ClickTextPage)] = new(w =>
            {
                var vm = new ClickTextEditVM() { ModInfo = ViewModel.ModInfo };
                var page = new ClickTextPage() { DataContext = vm };

                ViewModel
                    .ModInfo.WhenAnyValue(x => x.ClickTexts.Count)
                    .Subscribe(_ => w.RefreshDisplayText())
                    .Record(this);
                return page;
            })
            {
                DisplayTextAction = _ =>
                    "点击文本".Translate() + $" ({ViewModel.ModInfo.ClickTexts.Count})"
            },
            [typeof(LowTextPage)] = new(w =>
            {
                var vm = new LowTextEditVM() { ModInfo = ViewModel.ModInfo };
                var page = new LowTextPage() { DataContext = vm };

                ViewModel
                    .ModInfo.WhenAnyValue(x => x.LowTexts.Count)
                    .Subscribe(x => w.RefreshDisplayText())
                    .Record(this);
                return page;
            })
            {
                DisplayTextAction = _ =>
                    "低状态文本".Translate() + $" ({ViewModel.ModInfo.LowTexts.Count})"
            },
            [typeof(SelectTextPage)] = new(w =>
            {
                var vm = new SelectTextEditVM() { ModInfo = ViewModel.ModInfo };
                var page = new SelectTextPage() { DataContext = vm };

                ViewModel
                    .ModInfo.WhenAnyValue(x => x.SelectTexts.Count)
                    .Subscribe(x => w.RefreshDisplayText())
                    .Record(this);
                return page;
            })
            {
                DisplayTextAction = _ =>
                    "选择文本".Translate() + $" ({ViewModel.ModInfo.SelectTexts.Count})"
            },
            [typeof(PetPage)] = new(w =>
            {
                var vm = new PetEditVM() { ModInfo = ViewModel.ModInfo };
                var page = new PetPage() { DataContext = vm };

                ViewModel
                    .ModInfo.WhenAnyValue(x => x.Pets.FilteredList.Count)
                    .Subscribe(x => w.RefreshDisplayText())
                    .Record(this);
                return page;
            })
            {
                DisplayTextAction = _ =>
                    "宠物".Translate() + $" ({ViewModel.ModInfo.Pets.FilteredList.Count})"
            },
            [typeof(MovePage)] = new(w =>
            {
                var vm = new MoveEditVM() { ModInfo = ViewModel.ModInfo };
                var page = new MovePage() { DataContext = vm };

                ViewModel
                    .ModInfo.WhenAnyValue(x => x.CurrentPet!.Moves.Count)
                    .Subscribe(x => w.RefreshDisplayText())
                    .Record(this);
                return page;
            })
            {
                DisplayTextAction = _ =>
                    "移动".Translate()
                    + $" ({(ViewModel.ModInfo.CurrentPet is null ? "null" : ViewModel.ModInfo.CurrentPet.Moves.Count)})"
            },
            [typeof(WorkPage)] = new(w =>
            {
                var vm = new WorkEditVM() { ModInfo = ViewModel.ModInfo };
                var page = new WorkPage() { DataContext = vm };

                ViewModel
                    .ModInfo.WhenAnyValue(x => x.CurrentPet!.Works.Count)
                    .Subscribe(x => w.RefreshDisplayText())
                    .Record(this);
                return page;
            })
            {
                DisplayTextAction = _ =>
                    "工作".Translate()
                    + $" ({(ViewModel.ModInfo.CurrentPet is null ? "null" : ViewModel.ModInfo.CurrentPet.Works.Count)})"
            },
            [typeof(AnimePage)] = new(w =>
            {
                var vm = new AnimeVM() { ModInfo = ViewModel.ModInfo };
                var page = new AnimePage() { DataContext = vm };

                ViewModel
                    .ModInfo.WhenAnyValue(
                        x => x.CurrentPet!.Animes.Count,
                        x => x.CurrentPet!.FoodAnimes.Count
                    )
                    .Subscribe(x => w.RefreshDisplayText())
                    .Record(this);
                return page;
            })
            {
                DisplayTextAction = _ =>
                    "动画".Translate()
                    + $" ({(ViewModel.ModInfo.CurrentPet is null ? "null" : ViewModel.ModInfo.CurrentPet.Animes.Count + ViewModel.ModInfo.CurrentPet.FoodAnimes.Count)})"
            },
        };

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
        ViewModel.Reset();
        foreach (var page in PageByType.Values)
            page.Reset();
        this.DisposeAll();
        MessageBus.Current.SendMessage<ModInfoModel?>(null);
    }

    /// <summary>
    /// 显示页面
    /// </summary>
    /// <param name="index">索引值</param>
    public void ShowTab(int index)
    {
        ListBox_Pages.SelectedIndex = index;
        if (index == 0)
        {
            foreach (var page in PageByType.Values)
                page.RefreshDisplayText();
        }
    }

    private void ListBox_Pages_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        // 横向滚动
        if (e.Delta == 0)
            return;

        var s = ListBox_Pages.FindVisualChild<ScrollViewer>();
        s.ScrollToHorizontalOffset(s.HorizontalOffset - e.Delta);
        e.Handled = true;
    }
}
