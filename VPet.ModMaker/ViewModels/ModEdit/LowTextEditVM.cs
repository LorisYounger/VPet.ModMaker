using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicData.Binding;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using HanumanInstitute.MvvmDialogs.Wpf;
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
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.ViewModels.ModEdit;

public partial class LowTextEditVM : DialogViewModel
{
    private static IDialogService DialogService => Locator.Current.GetService<IDialogService>()!;

    public LowTextEditVM()
    {
        LowTexts = new(
            [],
            [],
            f =>
            {
                return SearchTargets.SelectedItem switch
                {
                    LowTextSearchTarget.ID
                        => f.ID.Contains(Search, StringComparison.OrdinalIgnoreCase),
                    LowTextSearchTarget.Text
                        => f.Text.Contains(Search, StringComparison.OrdinalIgnoreCase),
                    _ => false
                };
                ;
            }
        );

        this.WhenValueChanged(x => x.Search)
            .Throttle(TimeSpan.FromSeconds(1), RxApp.TaskpoolScheduler)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => LowTexts.Refresh());

        SearchTargets
            .WhenValueChanged(x => x.SelectedItem)
            .Throttle(TimeSpan.FromSeconds(1), RxApp.TaskpoolScheduler)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => LowTexts.Refresh());

        Closing += LowTextEditVM_Closing;
    }

    private void LowTextEditVM_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(LowText.ID))
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
        if (LowText.Text is null)
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
        if (OldLowText?.ID != LowText.ID && ModInfo.LowTexts.Any(i => i.ID == LowText.ID))
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
        LowTexts.AutoFilter = false;
        LowTexts.Clear();
        if (newValue is null)
            return;
        LowTexts.AddRange(newValue.LowTexts);
        Search = string.Empty;
        SearchTargets.SelectedItem = LowTextSearchTarget.ID;
        LowTexts.AutoFilter = true;
    }

    /// <summary>
    /// 低状态文本
    /// </summary>
    public FilterListWrapper<
        LowTextModel,
        ObservableList<LowTextModel>,
        ObservableList<LowTextModel>
    > LowTexts { get; set; } = null!;

    /// <summary>
    /// 搜索
    /// </summary>
    [ReactiveProperty]
    public string Search { get; set; } = string.Empty;

    /// <summary>
    /// 搜索目标
    /// </summary>
    public ObservableSelectableSet<
        LowTextSearchTarget,
        FrozenSet<LowTextSearchTarget>
    > SearchTargets { get; } = new(EnumInfo<LowTextSearchTarget>.Values);

    public LowTextModel? OldLowText { get; set; }

    [ReactiveProperty]
    public LowTextModel LowText { get; set; } = null!;

    /// <summary>
    /// 添加
    /// </summary>
    [ReactiveCommand]
    private async void Add()
    {
        ModInfo.TempI18nResource.ClearCultureData();
        LowText = new() { I18nResource = ModInfo.I18nResource };
        await DialogService.ShowSingletonDialogAsync(this, this);
        if (DialogResult is not true)
        {
            LowText.Close();
        }
        else
        {
            LowText.I18nResource.CopyDataTo(ModInfo.I18nResource, true);
            LowText.I18nResource = ModInfo.I18nResource;
            LowTexts.Add(LowText);
            if (this.Log().Level is LogLevel.Info)
                this.Log().Info("添加新低状态文本 {lowText}", LowText.ID);
            else
                this.Log()
                    .Debug(
                        "添加新低状态文本 {$lowText}",
                        LPSConvert.SerializeObjectToLine<Line>(
                            LowText.MapToLowText(new()),
                            "LowText"
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
    public async void Edit(LowTextModel model)
    {
        OldLowText = model;
        var newModel = new LowTextModel(model) { I18nResource = ModInfo.TempI18nResource };
        model.I18nResource.CopyDataTo(newModel.I18nResource, [model.ID], true);
        LowText = newModel;
        await DialogService.ShowDialogAsync(this, this);
        if (DialogResult is not true)
        {
            newModel.I18nResource.ClearCultureData();
            newModel.Close();
        }
        else
        {
            OldLowText.Close();
            newModel.I18nResource.CopyDataTo(ModInfo.I18nResource, true);
            newModel.I18nResource = ModInfo.I18nResource;
            LowTexts[LowTexts.IndexOf(model)] = newModel;
            if (this.Log().Level is LogLevel.Info)
                this.Log().Info("编辑低状态文本 {oldLowText} => {newLowText}", OldLowText.ID, LowText.ID);
            else
                this.Log()
                    .Debug(
                        "编辑低状态文本\n {$oldLowText} => {$newLowText}",
                        LPSConvert.SerializeObjectToLine<Line>(
                            OldLowText.MapToLowText(new()),
                            "LowText"
                        ),
                        LPSConvert.SerializeObjectToLine<Line>(
                            LowText.MapToLowText(new()),
                            "LowText"
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
    private void Remove(LowTextModel model)
    {
        if (
            DialogService.ShowMessageBoxX(
                this,
                "确定删除 {0} 吗".Translate(model.ID),
                "删除低状态文本".Translate(),
                MessageBoxButton.YesNo
            )
            is not true
        )
            return;
        LowTexts.Remove(model);
        model.Close();
        this.Log().Info("删除低状态文本 {lowText}", model.ID);
        Reset();
    }

    public void Reset()
    {
        LowText = null!;
        OldLowText = null!;
        DialogResult = false;
    }
}

/// <summary>
/// 低状态文本搜索目标
/// </summary>
public enum LowTextSearchTarget
{
    /// <summary>
    /// ID
    /// </summary>
    ID,

    /// <summary>
    /// 文本
    /// </summary>
    Text,
}
