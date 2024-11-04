using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicData.Binding;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using HKW.HKWMapper;
using HKW.HKWReactiveUI;
using HKW.HKWUtils.Collections;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using HKW.MVVMDialogs;
using HKW.WPF.MVVMDialogs;
using LinePutScript;
using LinePutScript.Converter;
using LinePutScript.Localization.WPF;
using ReactiveUI;
using Splat;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels.ModEdit;

public partial class SelectTextEditVM : DialogViewModel
{
    private static IDialogService DialogService => Locator.Current.GetService<IDialogService>()!;

    public SelectTextEditVM()
    {
        SelectTexts = new(
            [],
            [],
            f =>
            {
                return SearchTargets.SelectedItem switch
                {
                    SelectTextSearchTarget.ID
                        => f.ID.Contains(Search, StringComparison.OrdinalIgnoreCase),
                    SelectTextSearchTarget.Text
                        => f.Text.Contains(Search, StringComparison.OrdinalIgnoreCase),
                    SelectTextSearchTarget.Tags
                        => f.Tags.Contains(Search, StringComparison.OrdinalIgnoreCase),
                    SelectTextSearchTarget.ToTags
                        => f.ToTags.Contains(Search, StringComparison.OrdinalIgnoreCase),
                    _ => false
                };
            }
        );

        this.WhenValueChanged(x => x.Search)
            .Throttle(TimeSpan.FromSeconds(1), RxApp.TaskpoolScheduler)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => SelectTexts.Refresh());

        SearchTargets
            .WhenValueChanged(x => x.SelectedItem)
            .Throttle(TimeSpan.FromSeconds(1), RxApp.TaskpoolScheduler)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => SelectTexts.Refresh());

        Closing += SelectTextEditVM_Closing;
    }

    private void SelectTextEditVM_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(SelectText.ID))
        {
            DialogService.ShowMessageBoxX(
                this,
                "ID不可为空".Translate(),
                "数据错误".Translate(),
                MessageBoxButton.Ok,
                MessageBoxImage.Warning
            );
            e.Cancel = true;
            return;
        }
        if (SelectText.Text is null)
        {
            DialogService.ShowMessageBoxX(
                this,
                "文本不可为空".Translate(),
                "数据错误".Translate(),
                MessageBoxButton.Ok,
                MessageBoxImage.Warning
            );
            e.Cancel = true;
            return;
        }
        if (
            OldSelectText?.ID != SelectText.ID
            && ModInfo.SelectTexts.Any(i => i.ID == SelectText.ID)
        )
        {
            DialogService.ShowMessageBoxX(
                this,
                "此ID已存在".Translate(),
                "数据错误".Translate(),
                MessageBoxButton.Ok,
                MessageBoxImage.Warning
            );
            e.Cancel = true;
            return;
        }
        DialogResult = true;
    }

    /// <summary>
    /// 模组信息
    /// </summary>
    [ReactiveProperty]
    public ModInfoModel ModInfo { get; set; } = null!;

    partial void OnModInfoChanged(ModInfoModel oldValue, ModInfoModel newValue)
    {
        SelectTexts.AutoFilter = false;
        SelectTexts.Clear();
        if (newValue is null)
            return;
        SelectTexts.AddRange(newValue.SelectTexts);
        Search = string.Empty;
        SearchTargets.SelectedItem = SelectTextSearchTarget.ID;
        SelectTexts.Refresh();
        SelectTexts.AutoFilter = true;
    }

    /// <summary>
    /// 全部选择文本
    /// </summary>
    public FilterListWrapper<
        SelectTextModel,
        ObservableList<SelectTextModel>,
        ObservableList<SelectTextModel>
    > SelectTexts { get; set; }

    /// <summary>
    /// 搜索
    /// </summary>
    [ReactiveProperty]
    public string Search { get; set; } = string.Empty;

    /// <summary>
    /// 搜索目标
    /// </summary>
    public ObservableSelectableSet<
        SelectTextSearchTarget,
        FrozenSet<SelectTextSearchTarget>
    > SearchTargets { get; } = new(EnumInfo<SelectTextSearchTarget>.Values);

    /// <summary>
    /// I18n资源
    /// </summary>
    public I18nResource<string, string> I18nResource => ModInfo.I18nResource;
    public SelectTextModel? OldSelectText { get; set; } = null!;

    [ReactiveProperty]
    public SelectTextModel SelectText { get; set; } = null!;

    /// <summary>
    /// 添加
    /// </summary>
    [ReactiveCommand]
    private async void Add()
    {
        ModInfo.TempI18nResource.ClearCultureData();
        SelectText = new() { I18nResource = ModInfo.I18nResource };
        await DialogService.ShowSingletonDialogAsync(this, this);
        if (DialogResult is not true)
        {
            SelectText.Close();
        }
        else
        {
            SelectText.I18nResource.CopyDataTo(ModInfo.I18nResource, true);
            SelectText.I18nResource = ModInfo.I18nResource;
            SelectTexts.Add(SelectText);
            if (this.Log().Level is LogLevel.Info)
                this.Log().Info("添加新选择文本 {selectText}", SelectText.ID);
            else
                this.Log()
                    .Debug(
                        "添加新选择文本 {$selectText}",
                        LPSConvert.SerializeObjectToLine<Line>(
                            SelectText.MapToSelectText(new()),
                            "SelectText"
                        )
                    );
        }
        Reset();
    }

    /// <summary>
    /// 编辑
    /// </summary>
    /// <param name="model">模型</param>
    [ReactiveCommand]
    public async void Edit(SelectTextModel model)
    {
        OldSelectText = model;
        var newModel = new SelectTextModel(model) { I18nResource = ModInfo.TempI18nResource };
        model.I18nResource.CopyDataTo(newModel.I18nResource, [model.ID], true);
        SelectText = newModel;
        await DialogService.ShowDialogAsync(this, this);
        if (DialogResult is not true)
        {
            newModel.I18nResource.ClearCultureData();
            newModel.Close();
        }
        else
        {
            OldSelectText.Close();
            newModel.I18nResource.CopyDataTo(ModInfo.I18nResource, true);
            newModel.I18nResource = ModInfo.I18nResource;
            SelectTexts[SelectTexts.IndexOf(model)] = newModel;
            if (this.Log().Level is LogLevel.Info)
                this.Log()
                    .Info(
                        "编辑选择文本 {oldSelectText} => {newSelectText}",
                        OldSelectText.ID,
                        SelectText.ID
                    );
            else
                this.Log()
                    .Debug(
                        "编辑选择文本\n {$oldSelectText} => {$newSelectText}",
                        LPSConvert.SerializeObjectToLine<Line>(
                            OldSelectText.MapToSelectText(new()),
                            "SelectText"
                        ),
                        LPSConvert.SerializeObjectToLine<Line>(
                            SelectText.MapToSelectText(new()),
                            "SelectText"
                        )
                    );
        }
        Reset();
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="model">模型</param>
    [ReactiveCommand]
    private void Remove(SelectTextModel model)
    {
        if (
            DialogService.ShowMessageBoxX(
                this,
                "确定删除 {0} 吗".Translate(model.ID),
                "删除选择文本".Translate(),
                MessageBoxButton.YesNo
            )
            is not true
        )
            return;
        SelectTexts.Remove(model);
        model.Close();
        this.Log().Info("删除选择文本 {selectText}", model.ID);
        Reset();
    }

    public void Reset()
    {
        SelectText = null!;
        OldSelectText = null!;
        DialogResult = false;
    }
}

/// <summary>
/// 选择文本搜索目标
/// </summary>
public enum SelectTextSearchTarget
{
    /// <summary>
    /// ID
    /// </summary>
    ID,

    /// <summary>
    /// 文本
    /// </summary>
    Text,

    /// <summary>
    /// 标记
    /// </summary>
    Tags,

    /// <summary>
    /// 目标标记
    /// </summary>
    ToTags,
}
