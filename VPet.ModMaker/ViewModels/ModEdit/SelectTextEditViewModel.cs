using System.Collections;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using HKW.HKWUtils;
using HKW.HKWUtils.Collections;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using HKW.Mapper;
using HKW.MVVM;
using HKW.MVVM.SourceGenerator;
using HKW.WPF.MVVMDialogs;
using LinePutScript;
using LinePutScript.Converter;
using LinePutScript.Localization.WPF;
using Microsoft.Extensions.Logging;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels;

/// <summary>
/// 选择文本编辑视图模型
/// </summary>
public partial class SelectTextEditViewModel
    : ViewModelBase,
        IEditViewModel,
        IModalDialogViewModel,
        IViewClosing
{
    /// <inheritdoc/>
    public SelectTextEditViewModel()
    {
        SelectTexts = new(
            [],
            [],
            m =>
            {
                return CurrentSearchTarget.Value switch
                {
                    SelectTextSearchTarget.ID => m.ID.Contains(
                        Search,
                        StringComparison.CurrentCultureIgnoreCase
                    ),
                    SelectTextSearchTarget.Text => m.Text.Contains(
                        Search,
                        StringComparison.CurrentCultureIgnoreCase
                    ),
                    SelectTextSearchTarget.Tags => m.Tags.Contains(
                        Search,
                        StringComparison.CurrentCultureIgnoreCase
                    ),
                    SelectTextSearchTarget.ToTags => m.ToTags.Contains(
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
            .Subscribe(_ => SelectTexts?.Refresh());
    }

    [NotifyPropertyChangeFrom(nameof(ModInfo))]
    public string DisplayText => "SelectText ({0})".Translate(ModInfo?.SelectTexts.Count!);

    /// <summary>
    /// 模组信息
    /// </summary>
    [ObservableProperty]
    public ModInfoModel ModInfo { get; set; } = null!;

    partial void OnModInfoChanged(ModInfoModel oldValue, ModInfoModel newValue)
    {
        SelectTexts.SourceList = null!;
        if (newValue is not null)
        {
            SelectTexts.SourceList = newValue.SelectTexts;
            newValue
                .SelectTexts.WhenAnyValue(x => x.Count)
                .Subscribe(_ => OnPropertyChanged(nameof(DisplayText)))
                .DisposeWith(newValue.Disposables);
        }
    }

    /// <summary>
    /// 全部选择文本
    /// </summary>
    public ObservableFilteredListWrapper<
        SelectTextModel,
        ObservableList<SelectTextModel>,
        ObservableList<SelectTextModel>
    > SelectTexts { get; }

    /// <summary>
    /// 搜索
    /// </summary>
    [ObservableProperty]
    public string Search { get; set; } = string.Empty;

    /// <summary>
    /// 搜索目标
    /// </summary>
    public static ImmutableArray<EnumInfo<SelectTextSearchTarget>> SearchTargets =>
        EnumInfo<SelectTextSearchTarget>.StaticInfos;

    [ObservableProperty]
    public EnumInfo<SelectTextSearchTarget> CurrentSearchTarget { get; set; } =
        SelectTextSearchTarget.ID;

    /// <summary>
    /// 旧选择文本
    /// </summary>
    public SelectTextModel? OldSelectText { get; set; } = null!;

    /// <summary>
    /// 选择文本
    /// </summary>
    [ObservableProperty]
    public SelectTextModel SelectText { get; set; } = null!;

    public bool? DialogResult { get; set; }
    public bool IsChanged { get; private set; }

    /// <summary>
    /// 添加
    /// </summary>
    [RelayCommand]
    private void Add()
    {
        IsChanged = false;
        SelectText = new(ModInfo.TempI18nResource);
        using var dis = SelectText.Changed.Subscribe(_ => IsChanged = true);
        var result = DialogService.ShowDialog(this, this);
        if (result is not true)
        {
            SelectText.Dispose();
        }
        else
        {
            SelectText.I18nResource.CopyDataTo(ModInfo.I18nResource);
            SelectText.I18nResource = ModInfo.I18nResource;
            SelectTexts.Add(SelectText);
            this.LogIfEnabled(LogLevel.Information)?.Log("添加新选择文本 '{id}'", SelectText.ID);
            //if (this.LogX().Level is LogLevel.Info)
            //else
            //    this.LogX()
            //        .Debug(
            //            "添加新选择文本 {$selectText}",
            //            LPSConvert.SerializeObjectToLine<Line>(
            //                SelectText.MapToSelectText(new()),
            //                "SelectText"
            //            )
            //        );
        }
        Reset();
    }

    /// <summary>
    /// 编辑
    /// </summary>
    /// <param name="model">模型</param>
    [RelayCommand]
    public void Edit(SelectTextModel model)
    {
        IsChanged = false;
        OldSelectText = model;
        var newModel = new SelectTextModel(model, ModInfo.TempI18nResource);
        model.I18nResource.CopyDataTo(newModel.I18nResource, model.ID, model.ChooseID);
        SelectText = newModel;
        using var dis = newModel.Changed.Subscribe(_ => IsChanged = true);
        var result = DialogService.ShowDialog(this, this);
        if (result is not true)
        {
            newModel.Dispose();
        }
        else
        {
            OldSelectText.Dispose();
            newModel.I18nResource.CopyDataTo(ModInfo.I18nResource);
            newModel.I18nResource = ModInfo.I18nResource;
            SelectTexts[SelectTexts.IndexOf(model)] = newModel;
            this.LogIfEnabled(LogLevel.Information)
                ?.Log(
                    "编辑选择文本 {oldSelectText} => {newSelectText}",
                    OldSelectText.ID,
                    SelectText.ID
                );
            //if (this.LogX().Level is LogLevel.Info)
            //else
            //    this.LogX()
            //        .Debug(
            //            "编辑选择文本\n {$oldSelectText} => {$newSelectText}",
            //            LPSConvert.SerializeObjectToLine<Line>(
            //                OldSelectText.MapToSelectText(new()),
            //                "SelectText"
            //            ),
            //            LPSConvert.SerializeObjectToLine<Line>(
            //                SelectText.MapToSelectText(new()),
            //                "SelectText"
            //            )
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
        var models = list.Cast<SelectTextModel>().ToArray();
        var result = DialogService.ShowMessageBox(
            this,
            "确定删除已选中的 {0} 个选择文本吗".Translate(models.Length),
            "删除选择文本".Translate(),
            MessageBoxButton.YesNo
        );
        if (result is not true)
            return;
        foreach (var model in models)
        {
            SelectTexts.Remove(model);
            model.Dispose();
            this.LogIfEnabled(LogLevel.Information)?.Log("删除选择文本 '{id}'", model.ID);
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
        if (string.IsNullOrWhiteSpace(SelectText.ID))
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
        else if (SelectText.Text is null)
        {
            DialogService.ShowMessageBox(
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
