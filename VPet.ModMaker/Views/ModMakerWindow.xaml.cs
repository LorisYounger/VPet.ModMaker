using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using DynamicData.Binding;
using HKW.HKWUtils.Extensions;
using HKW.WPF.Extensions;
using HKW.WPF.MVVMDialogs;
using LinePutScript;
using LinePutScript.Localization.WPF;
using Panuon.WPF.UI;
using ReactiveUI;
using Splat;
using VPet.ModMaker.Models;
using VPet.ModMaker.ViewModels;
using VPet.ModMaker.Views.ModEdit;

namespace VPet.ModMaker.Views;

/// <summary>
/// winModMaker.xaml 的交互逻辑
/// </summary>
public partial class ModMakerWindow : WindowX, IPageLocator
{
    public ModMakerWindow()
    {
        InitializeComponent();
        this.SetViewModel<ModMakerVM>((s, e) => ViewModel.Close());
        MessageBus
            .Current.Listen<ModInfoModel?>()
            .Subscribe(x =>
            {
                if (x is null)
                {
                    GC.Collect();
                    this.ShowOrActivate();
                    ModEditWindow.Hide();
                    if (string.IsNullOrEmpty(ViewModel.ModInfo?.SourcePath) is false)
                        ViewModel.AddHistory(ViewModel.ModInfo);
                    ModEditWindow.ViewModel.ModInfo = ViewModel.ModInfo = null!;
                    ModEditWindow.ShowTab(1);
                }
                else
                {
                    GC.Collect();
                    this.Hide();
                    ModEditWindow.ShowOrActivate();
                    ModEditWindow.ViewModel.ModInfo = x;
                    ModEditWindow.ShowTab(0);
                    ModEditWindow.SetLocationToCenter();

#if !RELEASE
                    //ModEditWindow.InitializePages();
#endif
                }
            });
    }

    public ModMakerVM ViewModel => (ModMakerVM)DataContext;

    /// <inheritdoc/>
    public Dictionary<Type, Func<Window, UserControl?>>? PageLocatorByType { get; } = null;

    #region ModEditWindow
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ModEditWindow? _modEditWindow;

    /// <summary>
    /// 单例窗口
    /// </summary>
    public ModEditWindow ModEditWindow => _modEditWindow ??= new ModEditWindow().MaskClose(this);
    #endregion

    private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is not ListBoxItem item)
            return;
        if (item.DataContext is not ModMakeHistory history)
            return;
        ViewModel.LoadHistory(history);
    }
}
