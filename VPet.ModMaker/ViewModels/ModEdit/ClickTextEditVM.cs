using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicData.Binding;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using HKW.HKWReactiveUI;
using HKW.HKWUtils.Collections;
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

public partial class ClickTextEditVM : DialogViewModel
{
    private static IDialogService DialogService => Locator.Current.GetService<IDialogService>()!;

    public ClickTextEditVM()
    {
        ClickTexts = new(
            [],
            [],
            f =>
            {
                return SearchTargets.SelectedItem switch
                {
                    ClickTextSearchTarget.ID
                        => f.ID.Contains(Search, StringComparison.OrdinalIgnoreCase),
                    ClickTextSearchTarget.Working
                        => f.Working.Contains(Search, StringComparison.OrdinalIgnoreCase),
                    _ => f.ID.Contains(Search, StringComparison.OrdinalIgnoreCase)
                };
            }
        );

        this.WhenValueChanged(x => x.Search)
            .Throttle(TimeSpan.FromSeconds(1), RxApp.TaskpoolScheduler)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => ClickTexts.Refresh());
        SearchTargets
            .WhenValueChanged(x => x.SelectedItem)
            .Throttle(TimeSpan.FromSeconds(1), RxApp.TaskpoolScheduler)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => ClickTexts.Refresh());

        Closing += ClickTextEditVM_Closing;
    }

    private void ClickTextEditVM_Closing(object? sender, CancelEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ClickText.ID))
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
        if (ClickText.Text is null)
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
        if (OldClickText?.ID != ClickText.ID && ModInfo.ClickTexts.Any(i => i.ID == ClickText.ID))
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

    /// <summary>
    /// 显示的点击文本
    /// </summary>
    public FilterListWrapper<
        ClickTextModel,
        ObservableList<ClickTextModel>,
        ObservableList<ClickTextModel>
    > ClickTexts { get; }

    /// <summary>
    /// 搜索
    /// </summary>
    [ReactiveProperty]
    public string Search { get; set; } = string.Empty;

    public ObservableSelectableSet<
        ClickTextSearchTarget,
        FrozenSet<ClickTextSearchTarget>
    > SearchTargets { get; } = new(EnumInfo<ClickTextSearchTarget>.Values);

    /// <summary>
    /// I18n资源
    /// </summary>
    public I18nResource<string, string> I18nResource => ModInfo.I18nResource;

    /// <summary>
    /// 旧点击文本
    /// </summary>
    public ClickTextModel? OldClickText { get; set; }

    /// <summary>
    /// 点击文本
    /// </summary>
    [ReactiveProperty]
    public ClickTextModel ClickText { get; set; } = null!;

    /// <summary>
    /// 添加
    /// </summary>
    [ReactiveCommand]
    private async void Add()
    {
        ModInfo.TempI18nResource.ClearCultureData();
        ClickText = new() { I18nResource = ModInfo.TempI18nResource };
        await DialogService.ShowSingletonDialogAsync(this, this);
        if (DialogResult is not true)
        {
            ClickText.Close();
        }
        else
        {
            ClickText.I18nResource.CopyDataTo(ModInfo.I18nResource, true);
            ClickText.I18nResource = ModInfo.I18nResource;
            ClickTexts.Add(ClickText);
            if (this.Log().Level is LogLevel.Info)
                this.Log().Info("添加新点击文本 {clickText}", ClickText.ID);
            else
                this.Log()
                    .Debug(
                        "添加新点击文本 {$clickText}",
                        LPSConvert.SerializeObjectToLine<Line>(ClickText.ToClickText(), "ClickText")
                    );
        }
        Reset();
    }

    /// <summary>
    /// 编辑
    /// </summary>
    /// <param name="model">模型</param>
    [ReactiveCommand]
    public async void Edit(ClickTextModel model)
    {
        OldClickText = model;
        var newModel = new ClickTextModel(model) { I18nResource = ModInfo.TempI18nResource };
        model.I18nResource.CopyDataTo(newModel.I18nResource, [model.ID], true);
        ClickText = newModel;
        await DialogService.ShowDialogAsync(this, this);
        if (DialogResult is not true)
        {
            newModel.I18nResource.ClearCultureData();
            newModel.Close();
        }
        else
        {
            OldClickText.Close();
            newModel.I18nResource.CopyDataTo(ModInfo.I18nResource, true);
            newModel.I18nResource = ModInfo.I18nResource;
            ClickTexts[ClickTexts.IndexOf(model)] = newModel;
            if (this.Log().Level is LogLevel.Info)
                this.Log()
                    .Info("编辑点击文本 {oldClickText} => {newClickText}", OldClickText.ID, ClickText.ID);
            else
                this.Log()
                    .Debug(
                        "编辑点击文本\n {$oldClickText} => {$newClickText}",
                        LPSConvert.SerializeObjectToLine<Line>(
                            OldClickText.ToClickText(),
                            "ClickText"
                        ),
                        LPSConvert.SerializeObjectToLine<Line>(ClickText.ToClickText(), "ClickText")
                    );
        }
        Reset();
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="model">模型</param>
    [ReactiveCommand]
    private void Remove(ClickTextModel model)
    {
        if (
            DialogService.ShowMessageBoxX(
                this,
                "确定删除 {0} 吗".Translate(model.ID),
                "删除点击文本".Translate(),
                MessageBoxButton.YesNo
            )
            is not true
        )
            return;
        ClickTexts.Remove(model);
        model.Close();
        this.Log().Info("删除点击文本 {clickText}", model.ID);
        Reset();
    }

    public void Reset()
    {
        ClickText = null!;
        OldClickText = null!;
        DialogResult = false;
    }
}

/// <summary>
/// 点击文本搜索目标
/// </summary>
public enum ClickTextSearchTarget
{
    /// <summary>
    /// ID
    /// </summary>
    ID,

    /// <summary>
    /// 指定工作
    /// </summary>
    Working,
}
