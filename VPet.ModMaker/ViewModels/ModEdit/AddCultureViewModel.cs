using System.ComponentModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using HKW.HKWUtils;
using HKW.HKWUtils.Collections;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using HKW.MVVM;
using HKW.MVVM.SourceGenerator;
using HKW.WPF.MVVMDialogs;
using LinePutScript.Localization.WPF;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels;

/// <summary>
/// 添加文化视图模型
/// </summary>
public partial class AddCultureViewModel : ViewModelBase, IModalDialogViewModel, IViewClosing
{
    /// <inheritdoc/>
    public AddCultureViewModel()
    {
        AllCultures = new(
            new(LocalizeCore.AvailableCultures),
            [],
            c => c.Contains(Search, StringComparison.CurrentCultureIgnoreCase)
        );
        this.WhenAnyValue(x => x.Search)
            .Throttle(TimeSpan.FromSeconds(0.5), ObservableSchedulers.ThreadPool)
            .DistinctUntilChanged()
            .ObserveOn(ObservableSchedulers.CurrentThread)
            .Subscribe(_ => AllCultures.Refresh());
    }

    /// <summary>
    /// 模组信息
    /// </summary>
    [ObservableProperty]
    public ModInfoModel ModInfo { get; set; } = null!;

    /// <summary>
    /// 全部文化
    /// </summary>
    public FilteredListWrapper<
        string,
        List<string>,
        ObservableList<string>
    > AllCultures { get; set; }

    /// <summary>
    /// 文化名称
    /// </summary>
    [ObservableProperty]
    public string CultureName { get; set; } = string.Empty;

    /// <summary>
    /// 文化全名
    /// </summary>
    [NotifyPropertyChangeFrom(nameof(CultureName))]
    public string CultureFullName
    {
        get
        {
            if (string.IsNullOrWhiteSpace(CultureName))
            {
                return UnknownCulture;
            }
            CultureInfo info;
            try
            {
                info = CultureInfo.GetCultureInfo(CultureName);
            }
            catch
            {
                return UnknownCulture;
            }
            if (info is not null)
            {
                return info.FullName;
            }
            return UnknownCulture;
        }
    }

    /// <summary>
    /// 搜索文化
    /// </summary>
    [ObservableProperty]
    public string Search { get; set; } = string.Empty;

    /// <summary>
    /// 未知文化
    /// </summary>
    public static string UnknownCulture => "未知文化".Translate();
    public bool? DialogResult { get; set; }

    /// <summary>
    /// 文化链接
    /// </summary>
    public string CultureLink { get; } =
        "https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-lcid/a9eac961-e77d-41a6-90a5-ce1a8b0cdb9c";

    [RelayCommand]
    private void OpenCultureLink()
    {
        NativeUtils.OpenLink(CultureLink);
        //try
        //{
        //}
        //catch
        //{
        //    if (
        //        NativeUtils.DialogService.ShowMessageBoxX(
        //            this,
        //            "无法打开链接,需要复制自行访问吗".Translate(),
        //            "打开链接失败".Translate(),
        //            MessageBoxButton.YesNo,
        //            icon: MessageBoxImage.Warning
        //        )
        //        is not true
        //    )
        //        return;
        //    NativeUtils.ClipboardSetText(CultureLink);
        //    NativeUtils.DialogService.ShowMessageBoxX(this, "已复制到剪贴板".Translate());
        //}
    }

    public void OnClosing(CancelEventArgs e)
    {
        if (DialogResult is not true)
            return;
        if (string.IsNullOrWhiteSpace(CultureName))
        {
            DialogService.ShowMessageBox(
                this,
                "文化不可为空".Translate(),
                "数据错误".Translate(),
                icon: MessageBoxImage.Warning
            );
            e.Cancel = true;
        }
        else if (CultureInfo.TryGetCultureInfo(CultureName, out var culture) is false)
        {
            DialogService.ShowMessageBox(
                this,
                "不支持的文化".Translate(),
                "数据错误".Translate(),
                icon: MessageBoxImage.Warning
            );
            e.Cancel = true;
        }
        else if (ModInfo.I18nResource.Cultures.Contains(culture))
        {
            DialogService.ShowMessageBox(
                this,
                "此文化已存在".Translate(),
                "数据错误".Translate(),
                icon: MessageBoxImage.Warning
            );
            e.Cancel = true;
        }
        DialogResult = e.Cancel is not true;
    }

    public Task OnClosingAsync(CancelEventArgs e)
    {
        return Task.CompletedTask;
    }
}
