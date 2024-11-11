using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicData.Binding;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using HKW.HKWReactiveUI;
using HKW.HKWUtils.Collections;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using HKW.MVVMDialogs;
using HKW.WPF.MVVMDialogs;
using LinePutScript.Localization.WPF;
using ReactiveUI;
using Splat;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels.ModEdit;

public partial class AddCultureVM : DialogViewModel
{
    private static IDialogService DialogService => Locator.Current.GetService<IDialogService>()!;

    public AddCultureVM(ModInfoModel modInfo)
    {
        ModInfo = modInfo;
        AllCultures = new(
            new(LocalizeCore.AvailableCultures),
            [],
            c => c.Contains(Search, StringComparison.OrdinalIgnoreCase)
        );
        this.WhenValueChanged(x => x.Search)
            .Throttle(TimeSpan.FromSeconds(1), RxApp.TaskpoolScheduler)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => AllCultures.Refresh());

        Closing += AddCultureVM_Closing;
    }

    private void AddCultureVM_Closing(object? sender, CancelEventArgs e)
    {
        if (DialogResult is not true)
            return;
        if (string.IsNullOrWhiteSpace(CultureName))
        {
            DialogService.ShowMessageBoxX(
                this,
                "文化不可为空".Translate(),
                "数据错误".Translate(),
                icon: MessageBoxImage.Warning
            );
            return;
        }
        else if (CultureUtils.TryGetCultureInfo(CultureName, out var culture) is false)
        {
            DialogService.ShowMessageBoxX(
                this,
                "不支持的文化".Translate(),
                "数据错误".Translate(),
                icon: MessageBoxImage.Warning
            );
            return;
        }
        else if (ModInfo.I18nResource.Cultures.Contains(culture))
        {
            DialogService.ShowMessageBoxX(
                this,
                "此文化已存在".Translate(),
                "数据错误".Translate(),
                icon: MessageBoxImage.Warning
            );
            return;
        }
        DialogResult = e.Cancel is not true;
    }

    /// <summary>
    /// 模组信息
    /// </summary>
    [ReactiveProperty]
    public ModInfoModel ModInfo { get; set; } = null!;

    /// <summary>
    /// 全部文化
    /// </summary>
    public FilterListWrapper<string, List<string>, ObservableList<string>> AllCultures { get; set; }

    /// <summary>
    /// 文化名称
    /// </summary>

    [ReactiveProperty]
    public string CultureName { get; set; } = string.Empty;

    /// <summary>
    /// 文化全名
    /// </summary>
    [NotifyPropertyChangeFrom(nameof(CultureName))]
    public string CultureFullName =>
        this.To(static x =>
        {
            if (string.IsNullOrWhiteSpace(x.CultureName))
            {
                return UnknownCulture;
            }
            CultureInfo info = null!;
            try
            {
                info = CultureInfo.GetCultureInfo(x.CultureName);
            }
            catch
            {
                return UnknownCulture;
            }
            if (info is not null)
            {
                return info.GetFullInfo();
            }
            return UnknownCulture;
        });

    /// <summary>
    /// 搜索文化
    /// </summary>
    [ReactiveProperty]
    public string Search { get; set; } = string.Empty;

    public static string UnknownCulture => "未知文化".Translate();

    public string CultureLink { get; } =
        "https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-lcid/a9eac961-e77d-41a6-90a5-ce1a8b0cdb9c";

    [ReactiveCommand]
    private void OpenCultureLink()
    {
        try
        {
            NativeUtils.OpenLink(CultureLink);
        }
        catch
        {
            if (
                DialogService.ShowMessageBoxX(
                    this,
                    "无法打开链接,需要复制自行访问吗".Translate(),
                    "打开链接失败".Translate(),
                    MessageBoxButton.YesNo,
                    icon: MessageBoxImage.Warning
                )
                is not true
            )
                return;
            NativeUtils.ClipboardSetText(CultureLink);
            DialogService.ShowMessageBoxX(this, "已复制到剪贴板".Translate());
        }
    }
}
