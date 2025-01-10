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
/// 选择文本编辑视图模型
/// </summary>
public partial class SelectTextEditVM : DialogViewModel, IEnableLogger<ViewModelBase>, IDisposable
{
    /// <inheritdoc/>
    public SelectTextEditVM(ModInfoModel modInfo)
    {
        ModInfo = modInfo;
        SelectTexts = new(
            modInfo.SelectTexts,
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

        this.WhenAnyValue(
                x => x.Search,
                x => x.SearchTargets.SelectedItem,
                x => x.ModInfo.I18nResource.CurrentCulture
            )
            .Throttle(TimeSpan.FromSeconds(0.5), RxApp.TaskpoolScheduler)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => SelectTexts.Refresh())
            .Record(this);

        Closing += SelectTextEditVM_Closing;
    }

    private void SelectTextEditVM_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (DialogResult is not true)
            return;
        if (string.IsNullOrWhiteSpace(SelectText.ID))
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
        else if (SelectText.Text is null)
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
        else if (
            OldSelectText?.ID != SelectText.ID
            && ModInfo.SelectTexts.Any(i => i.ID == SelectText.ID)
        )
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
    public ModInfoModel ModInfo { get; set; } = null!;

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
    public ObservableSelectableSetWrapper<
        SelectTextSearchTarget,
        FrozenSet<SelectTextSearchTarget>
    > SearchTargets { get; } = new(EnumInfo<SelectTextSearchTarget>.Values);

    /// <summary>
    /// 旧选择文本
    /// </summary>
    public SelectTextModel? OldSelectText { get; set; } = null!;

    /// <summary>
    /// 选择文本
    /// </summary>
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
        await ModMakerVM.DialogService.ShowDialogAsyncX(this, this);
        if (DialogResult is not true)
        {
            SelectText.Close();
        }
        else
        {
            SelectText.I18nResource.CopyDataTo(ModInfo.I18nResource, true);
            SelectText.I18nResource = ModInfo.I18nResource;
            SelectTexts.Add(SelectText);
            if (this.LogX().Level is LogLevel.Info)
                this.LogX().Info("添加新选择文本 {selectText}", SelectText.ID);
            else
                this.LogX()
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
        model.I18nResource.CopyDataTo(newModel.I18nResource, [model.ID, model.ChooseID], true);
        SelectText = newModel;
        await ModMakerVM.DialogService.ShowDialogAsync(this, this);
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
            if (this.LogX().Level is LogLevel.Info)
                this.LogX()
                    .Info(
                        "编辑选择文本 {oldSelectText} => {newSelectText}",
                        OldSelectText.ID,
                        SelectText.ID
                    );
            else
                this.LogX()
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
    /// <param name="list">列表</param>
    [ReactiveCommand]
    private void Remove(IList list)
    {
        var models = list.Cast<SelectTextModel>().ToArray();
        if (
            ModMakerVM.DialogService.ShowMessageBoxX(
                this,
                "确定删除已选中的 {0} 个选择文本吗".Translate(models.Length),
                "删除选择文本".Translate(),
                MessageBoxButton.YesNo
            )
            is not true
        )
            return;
        foreach (var model in models)
        {
            SelectTexts.Remove(model);
            model.Close();
            this.LogX().Info("删除选择文本 {selectText}", model.ID);
        }
    }

    /// <summary>
    /// 重置
    /// </summary>
    public void Reset()
    {
        SelectText = null!;
        OldSelectText = null!;
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
