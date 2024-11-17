using System;
using System.Collections;
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

/// <summary>
/// 点击文本视图模型
/// </summary>
public partial class ClickTextEditVM : DialogViewModel
{
    private static IDialogService DialogService => Locator.Current.GetService<IDialogService>()!;

    /// <inheritdoc/>
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
                    ClickTextSearchTarget.Text
                        => f.Text.Contains(Search, StringComparison.OrdinalIgnoreCase),
                    ClickTextSearchTarget.Working
                        => f.Working.Contains(Search, StringComparison.OrdinalIgnoreCase),
                    _ => false
                };
            }
        );

        this.WhenValueChanged(x => x.Search)
            .Throttle(TimeSpan.FromSeconds(0.5), RxApp.TaskpoolScheduler)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => ClickTexts.Refresh());
        SearchTargets
            .WhenValueChanged(x => x.SelectedItem)
            .Throttle(TimeSpan.FromSeconds(0.5), RxApp.TaskpoolScheduler)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => ClickTexts.Refresh());

        Closing += ClickTextEditVM_Closing;
    }

    private void ClickTextEditVM_Closing(object? sender, CancelEventArgs e)
    {
        if (DialogResult is not true)
            return;
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
        }
        else if (ClickText.Text is null)
        {
            DialogService.ShowMessageBoxX(
                this,
                "文本不可为空".Translate(),
                "数据错误".Translate(),
                MessageBoxButton.Ok,
                MessageBoxImage.Warning
            );
            e.Cancel = true;
        }
        else if (
            OldClickText?.ID != ClickText.ID
            && ModInfo.ClickTexts.Any(i => i.ID == ClickText.ID)
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
        }
        DialogResult = e.Cancel is not true;
    }

    /// <summary>
    /// 模组信息
    /// </summary>
    [ReactiveProperty]
    public ModInfoModel ModInfo { get; set; } = null!;

    partial void OnModInfoChanged(ModInfoModel oldValue, ModInfoModel newValue)
    {
        if (oldValue is not null)
        {
            ClickTexts.BaseList.BindingList(oldValue.ClickTexts, true);
        }
        ClickTexts.AutoFilter = false;
        ClickTexts.Clear();
        if (newValue is not null)
        {
            newValue
                .I18nResource.WhenValueChanged(x => x.CurrentCulture)
                .Throttle(TimeSpan.FromSeconds(0.5), RxApp.TaskpoolScheduler)
                .DistinctUntilChanged()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => ClickTexts.Refresh());
            ClickTexts.AddRange(newValue.ClickTexts);
            ClickTexts.BaseList.BindingList(newValue.ClickTexts);
            Search = string.Empty;
            SearchTargets.SelectedItem = ClickTextSearchTarget.ID;
        }
        ClickTexts.Refresh();
        ClickTexts.AutoFilter = true;
    }

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

    /// <summary>
    /// 搜索目标
    /// </summary>
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
        await DialogService.ShowDialogAsyncX(this, this);
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
    /// <param name="list">列表</param>
    [ReactiveCommand]
    private void Remove(IList list)
    {
        var models = list.Cast<ClickTextModel>().ToArray();
        if (
            DialogService.ShowMessageBoxX(
                this,
                "确定删除已选中的 {0} 个点击文本吗".Translate(models.Length),
                "删除点击文本".Translate(),
                MessageBoxButton.YesNo
            )
            is not true
        )
            return;
        foreach (var model in models)
        {
            ClickTexts.Remove(model);
            model.Close();
            this.Log().Info("删除点击文本 {clickText}", model.ID);
        }
        Reset();
    }

    /// <summary>
    /// 重置
    /// </summary>
    public void Reset()
    {
        ClickText = null!;
        OldClickText = null!;
        DialogResult = false;
        ModInfo.TempI18nResource.ClearCultureData();
    }

    /// <summary>
    /// 关闭
    /// </summary>
    public void Close()
    {
        AddCommand.Dispose();
        RemoveCommand.Dispose();
        EditCommand.Dispose();
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
    /// 文本
    /// </summary>
    Text,

    /// <summary>
    /// 指定工作
    /// </summary>
    Working,
}
