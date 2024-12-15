using System.Collections;
using System.Collections.Frozen;
using System.Reactive.Linq;
using DynamicData.Binding;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using HKW.HKWMapper;
using HKW.HKWReactiveUI;
using HKW.HKWUtils;
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

/// <summary>
/// 低状态文本编辑视图模型
/// </summary>
public partial class LowTextEditVM : DialogViewModel, IEnableLogger<ViewModelBase>, IDisposable
{
    /// <inheritdoc/>
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
            .Throttle(TimeSpan.FromSeconds(0.5), RxApp.TaskpoolScheduler)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => LowTexts.Refresh())
            .Record(this);

        SearchTargets
            .WhenValueChanged(x => x.SelectedItem)
            .Throttle(TimeSpan.FromSeconds(0.5), RxApp.TaskpoolScheduler)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => LowTexts.Refresh())
            .Record(this);

        Closing += LowTextEditVM_Closing;
    }

    private void LowTextEditVM_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (DialogResult is not true)
            return;
        if (string.IsNullOrWhiteSpace(LowText.ID))
        {
            ModMakerVM.DialogService.ShowMessageBoxX(
                this,
                "ID不可为空".Translate(),
                "数据错误".Translate(),
                MessageBoxButton.Ok,
                MessageBoxImage.Warning
            );
            e.Cancel = true;
        }
        else if (LowText.Text is null)
        {
            ModMakerVM.DialogService.ShowMessageBoxX(
                this,
                "文本不可为空".Translate(),
                "数据错误".Translate(),
                MessageBoxButton.Ok,
                MessageBoxImage.Warning
            );
            e.Cancel = true;
        }
        else if (OldLowText?.ID != LowText.ID && ModInfo.LowTexts.Any(i => i.ID == LowText.ID))
        {
            ModMakerVM.DialogService.ShowMessageBoxX(
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
            LowTexts.BaseList.BindingList(oldValue.LowTexts, true);
        }
        LowTexts.Clear();
        LowTexts.AutoFilter = false;
        if (newValue is not null)
        {
            newValue
                .I18nResource.WhenValueChanged(x => x.CurrentCulture)
                .Throttle(TimeSpan.FromSeconds(0.5), RxApp.TaskpoolScheduler)
                .DistinctUntilChanged()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => LowTexts.Refresh())
                .Record(this);
            LowTexts.AddRange(newValue.LowTexts);
            LowTexts.BaseList.BindingList(newValue.LowTexts);
            Search = string.Empty;
            SearchTargets.SelectedItem = LowTextSearchTarget.ID;
        }
        LowTexts.Refresh();
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
    public ObservableSelectableSetWrapper<
        LowTextSearchTarget,
        FrozenSet<LowTextSearchTarget>
    > SearchTargets { get; } = new(EnumInfo<LowTextSearchTarget>.Values);

    /// <summary>
    /// 旧低状态文本
    /// </summary>
    public LowTextModel? OldLowText { get; set; }

    /// <summary>
    /// 低状态文本
    /// </summary>
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
        await ModMakerVM.DialogService.ShowDialogAsyncX(this, this);
        if (DialogResult is not true)
        {
            LowText.Close();
        }
        else
        {
            LowText.I18nResource.CopyDataTo(ModInfo.I18nResource, true);
            LowText.I18nResource = ModInfo.I18nResource;
            LowTexts.Add(LowText);
            if (this.LogX().Level is LogLevel.Info)
                this.LogX().Info("添加新低状态文本 {lowText}", LowText.ID);
            else
                this.LogX()
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
        await ModMakerVM.DialogService.ShowDialogAsync(this, this);
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
            if (this.LogX().Level is LogLevel.Info)
                this.LogX().Info("编辑低状态文本 {oldLowText} => {newLowText}", OldLowText.ID, LowText.ID);
            else
                this.LogX()
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
    /// <param name="list">列表</param>
    [ReactiveCommand]
    private void Remove(IList list)
    {
        var models = list.Cast<LowTextModel>().ToArray();
        if (
            ModMakerVM.DialogService.ShowMessageBoxX(
                this,
                "确定删除已选中的 {0} 个低状态文本吗".Translate(models.Length),
                "删除低状态文本".Translate(),
                MessageBoxButton.YesNo
            )
            is not true
        )
            return;
        foreach (var model in models)
        {
            LowTexts.Remove(model);
            model.Close();
            this.LogX().Info("删除低状态文本 {lowText}", model.ID);
        }
    }

    /// <summary>
    /// 重置
    /// </summary>
    public void Reset()
    {
        LowText = null!;
        OldLowText = null!;
        DialogResult = false;
        ModInfo.TempI18nResource.ClearCultureData();
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (_disposed)
            return;
        base.Dispose(disposing);
        if (disposing) { }
        Reset();
        ModInfo = null!;
        _disposed = false;
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
