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
                }
                else
                {
                    this.Hide();
                    ModEditWindow.ShowOrActivate();
                    ModEditWindow.ViewModel.ModInfo = x;
                    ModEditWindow.ContentControl_Food_Loaded(null!, null!);
                    ModEditWindow.SetLocationToCenter();

#if !RELEASE
                    ModEditWindow.InitializePage();
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

    public const string WikiLink = "https://github.com/LorisYounger/VPet.ModMaker/wiki";

    private void Hyperlink_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            NativeUtils.OpenLink(WikiLink);
        }
        catch
        {
            if (
                MessageBoxX.Show(
                    this,
                    "无法打开链接,需要复制自行访问吗".Translate(),
                    "",
                    MessageBoxButton.YesNo,
                    MessageBoxIcon.Warning
                )
                is not MessageBoxResult.Yes
            )
                return;
            Clipboard.SetText(WikiLink);
            MessageBoxX.Show(this, "已复制链接到剪贴板".Translate());
        }
    }

    private void ListBox_ItemClick(object sender, RoutedEventArgs e)
    {
        if (e.OriginalSource is not ListBoxItem item)
            return;
        if (item.DataContext is not ModMakeHistory history)
            return;
        ViewModel.LoadHistory(history);
    }
}
