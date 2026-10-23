using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Text;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using HKW.HKWUtils;
using HKW.HKWUtils.Collections;
using HKW.HKWUtils.Observable;
using HKW.MVVM;
using HKW.MVVM.SourceGenerator;
using LinePutScript.Localization.WPF;
using Microsoft.Extensions.Logging;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels;

public sealed partial class ThemeEditViewModel
    : ViewModelBase,
        IEditViewModel,
        IModalDialogViewModel,
        IViewClosing
{
    /// <inheritdoc/>
    public ThemeEditViewModel()
    {
        Themes = new(
            [],
            [],
            x =>
            {
                return CurrentSearchTarget.Value switch
                {
                    ThemeSearchTarget.ID => x.ID.Contains(
                        Search,
                        StringComparison.CurrentCultureIgnoreCase
                    ),
                    ThemeSearchTarget.Name => x.Name.Contains(
                        Search,
                        StringComparison.CurrentCultureIgnoreCase
                    ),
                    _ => false,
                };
            }
        );
        this.WhenAnyValue(x => x.Search, x => x.CurrentSearchTarget)
            .Throttle(TimeSpan.FromSeconds(0.5), ObservableSchedulers.ThreadPool)
            .DistinctUntilChanged()
            .ObserveOn(ObservableSchedulers.Current)
            .Subscribe(_ => Themes?.Refresh());
    }

    [NotifyPropertyChangeFrom(nameof(ModInfo))]
    public string DisplayText => "Themes ({0})".Translate(ModInfo?.Themes.Count!);

    /// <summary>
    /// 模组信息
    /// </summary>
    [ObservableProperty]
    public ModInfoModel ModInfo { get; set; } = null!;

    partial void OnModInfoChanged(ModInfoModel oldValue, ModInfoModel newValue)
    {
        Themes.SourceList = null!;
        if (newValue is not null)
        {
            Themes.SourceList = newValue.Themes;
            newValue
                .Themes.WhenAnyValue(x => x.Count)
                .Subscribe(_ => OnPropertyChanged(nameof(DisplayText)))
                .DisposeWith(newValue.Disposables);
        }
    }

    public ObservableFilteredListWrapper<
        ThemeModel,
        ObservableList<ThemeModel>,
        ObservableList<ThemeModel>
    > Themes { get; }

    /// <summary>
    /// 搜索
    /// </summary>
    [ObservableProperty]
    public string Search { get; set; } = string.Empty;

    /// <summary>
    /// 搜索目标
    /// </summary>
    public static ImmutableArray<EnumInfo<ThemeSearchTarget>> SearchTargets =>
        EnumInfo<ThemeSearchTarget>.StaticInfos;

    [ObservableProperty]
    public EnumInfo<ThemeSearchTarget> CurrentSearchTarget { get; set; } = ThemeSearchTarget.ID;

    /// <summary>
    /// 旧主题
    /// </summary>
    public ThemeModel? OldTheme { get; set; }

    /// <summary>
    /// 主题
    /// </summary>
    [ObservableProperty]
    public ThemeModel Theme { get; set; } = null!;
    public bool? DialogResult { get; set; }

    public bool IsChanged { get; private set; }

    /// <summary>
    /// 添加
    /// </summary>
    [RelayCommand]
    private void Add()
    {
        IsChanged = false;
        Theme = new(ModInfo.TempI18nResource);
        using var dis = Theme.Changed.Subscribe(_ => IsChanged = true);
        var result = DialogService.ShowDialog(this, this);
        if (result is not true)
        {
            Theme.Dispose();
        }
        else
        {
            Theme.I18nResource.CopyDataTo(ModInfo.I18nResource);
            Theme.I18nResource = ModInfo.I18nResource;
            Themes.Add(Theme);
            this.LogIfEnabled(LogLevel.Information)?.Log("添加新主题 '{id}'", Theme.ID);
        }
        Reset();
    }

    /// <summary>
    /// 编辑
    /// </summary>
    /// <param name="model">模型</param>
    [RelayCommand]
    public void Edit(ThemeModel model)
    {
        IsChanged = false;
        OldTheme = model;
        var newModel = new ThemeModel(model, ModInfo.TempI18nResource);
        model.I18nResource.CopyDataTo(newModel.I18nResource, model.ID);
        Theme = newModel;
        using var dis = newModel.Changed.Subscribe(_ => IsChanged = true);
        var result = DialogService.ShowDialog(this, this);
        if (result is not true)
        {
            newModel.Dispose();
        }
        else
        {
            OldTheme.Dispose();
            newModel.I18nResource.CopyDataTo(ModInfo.I18nResource);
            newModel.I18nResource = ModInfo.I18nResource;
            Themes[Themes.IndexOf(model)] = newModel;
            this.LogIfEnabled(LogLevel.Information)
                ?.Log("编辑主题 {oldModel} => {newModel}", OldTheme.ID, Theme.ID);
            //if (this.LogX().Level is LogLevel.Info)
            //else
            //    this.LogX()
            //        .Debug(
            //            "编辑主题\n {$oldTheme} => {$newTheme}",
            //            LPSConvert.SerializeObjectToLine<Line>(
            //                OldTheme.ToTheme(),
            //                "Theme"
            //            ),
            //            LPSConvert.SerializeObjectToLine<Line>(Theme.ToTheme(), "Theme")
            //        );
        }
        Reset();
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="list">列表</param>
    [RelayCommand]
    private void Remove(IList list)
    {
        var models = list.Cast<ThemeModel>().ToArray();
        var result = DialogService.ShowMessageBox(
            this,
            "确定删除已选中的 {0} 个主题吗".Translate(models.Length),
            "删除主题".Translate(),
            MessageBoxButton.YesNo
        );
        if (result is not true)
            return;
        foreach (var model in models)
        {
            Themes.Remove(model);
            model.Dispose();
            this.LogIfEnabled(LogLevel.Information)?.Log("删除主题 '{id}'", model.ID);
        }
    }

    /// <summary>
    /// 重置
    /// </summary>
    public void Reset()
    {
        Theme = null!;
        OldTheme = null!;
        DialogResult = false;
        ModInfo.TempI18nResource.ClearData();
    }

    public void OnClosing(CancelEventArgs e)
    {
        if (IsChanged)
        {
            var result = DialogService.ShowMessageBox(
                this,
                "编辑完成, 是否保存?".Translate(),
                "编辑完成".Translate(),
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Information
            );
            if (result is null)
            {
                e.Cancel = true;
                return;
            }
            DialogResult = result;
        }

        if (DialogResult is not true)
            return;
        if (string.IsNullOrWhiteSpace(Theme.ID))
        {
            DialogService.ShowMessageBox(
                this,
                "ID不可为空".Translate(),
                "数据错误".Translate(),
                MessageBoxButton.Ok,
                MessageBoxImage.Warning
            );
            e.Cancel = true;
        }
        else if (OldTheme?.ID != Theme.ID && ModInfo.Themes.Any(i => i.ID == Theme.ID))
        {
            DialogService.ShowMessageBox(
                this,
                "此ID已存在".Translate(),
                "数据错误".Translate(),
                MessageBoxButton.Ok,
                MessageBoxImage.Warning
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

/// <summary>
/// 主题搜索目标
/// </summary>
public enum ThemeSearchTarget
{
    /// <summary>
    /// ID
    /// </summary>
    ID,

    /// <summary>
    /// 名称
    /// </summary>
    Name,
}
