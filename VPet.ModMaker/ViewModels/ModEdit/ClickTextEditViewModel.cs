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
/// 点击文本视图模型
/// </summary>
public sealed partial class ClickTextEditViewModel
    : ViewModelBase,
        IEditViewModel,
        IModalDialogViewModel,
        IViewClosing
{
    /// <inheritdoc/>
    public ClickTextEditViewModel()
    {
        ClickTexts = new(
            [],
            [],
            x =>
            {
                return CurrentSearchTarget.Value switch
                {
                    ClickTextSearchTarget.ID => x.ID.Contains(
                        Search,
                        StringComparison.CurrentCultureIgnoreCase
                    ),
                    ClickTextSearchTarget.Text => x.Text.Contains(
                        Search,
                        StringComparison.CurrentCultureIgnoreCase
                    ),
                    ClickTextSearchTarget.Working => x.Working.Contains(
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
            .Subscribe(_ => ClickTexts?.Refresh());
    }

    [NotifyPropertyChangeFrom(nameof(ModInfo))]
    public string DisplayText => "ClickText ({0})".Translate(ModInfo?.ClickTexts.Count!);

    /// <summary>
    /// 模组信息
    /// </summary>
    [ObservableProperty]
    public ModInfoModel ModInfo { get; set; } = null!;

    partial void OnModInfoChanged(ModInfoModel oldValue, ModInfoModel newValue)
    {
        ClickTexts.SourceList = null!;
        if (newValue is not null)
        {
            ClickTexts.SourceList = newValue.ClickTexts;
            newValue
                .ClickTexts.WhenAnyValue(x => x.Count)
                .Subscribe(_ => OnPropertyChanged(nameof(DisplayText)))
                .DisposeWith(newValue.Disposables);
        }
    }

    /// <summary>
    /// 显示的点击文本
    /// </summary>
    public ObservableFilteredListWrapper<
        ClickTextModel,
        ObservableList<ClickTextModel>,
        ObservableList<ClickTextModel>
    > ClickTexts { get; }

    /// <summary>
    /// 搜索
    /// </summary>
    [ObservableProperty]
    public string Search { get; set; } = string.Empty;

    /// <summary>
    /// 搜索目标
    /// </summary>
    public static ImmutableArray<EnumInfo<ClickTextSearchTarget>> SearchTargets =>
        EnumInfo<ClickTextSearchTarget>.StaticInfos;

    [ObservableProperty]
    public EnumInfo<ClickTextSearchTarget> CurrentSearchTarget { get; set; } =
        ClickTextSearchTarget.ID;

    /// <summary>
    /// 旧点击文本
    /// </summary>
    public ClickTextModel? OldClickText { get; set; }

    /// <summary>
    /// 点击文本
    /// </summary>
    [ObservableProperty]
    public ClickTextModel ClickText { get; set; } = null!;
    public bool? DialogResult { get; set; }
    public bool IsChanged { get; private set; }

    /// <summary>
    /// 添加
    /// </summary>
    [RelayCommand]
    private void Add()
    {
        IsChanged = false;
        ClickText = new(ModInfo.TempI18nResource);
        using var dis = ClickText.Changed.Subscribe(_ => IsChanged = true);
        var result = DialogService.ShowDialog(this, this);
        if (result is not true)
        {
            ClickText.Dispose();
        }
        else
        {
            ClickText.I18nResource.CopyDataTo(ModInfo.I18nResource);
            ClickText.I18nResource = ModInfo.I18nResource;
            ClickTexts.Add(ClickText);
            this.LogIfEnabled(LogLevel.Information)?.Log("添加新点击文本 '{id}'", ClickText.ID);
        }
        Reset();
    }

    /// <summary>
    /// 编辑
    /// </summary>
    /// <param name="model">模型</param>
    [RelayCommand]
    public void Edit(ClickTextModel model)
    {
        IsChanged = false;
        OldClickText = model;
        var newModel = new ClickTextModel(model, ModInfo.TempI18nResource);
        model.I18nResource.CopyDataTo(newModel.I18nResource, model.ID);
        ClickText = newModel;
        using var dis = newModel.Changed.Subscribe(_ => IsChanged = true);
        var result = DialogService.ShowDialog(this, this);
        if (result is not true)
        {
            newModel.Dispose();
        }
        else
        {
            OldClickText.Dispose();
            newModel.I18nResource.CopyDataTo(ModInfo.I18nResource);
            newModel.I18nResource = ModInfo.I18nResource;
            ClickTexts[ClickTexts.IndexOf(model)] = newModel;
            this.LogIfEnabled(LogLevel.Information)
                ?.Log(
                    "编辑点击文本 {oldClickText} => {newClickText}",
                    OldClickText.ID,
                    ClickText.ID
                );
            //if (this.LogX().Level is LogLevel.Info)
            //else
            //    this.LogX()
            //        .Debug(
            //            "编辑点击文本\n {$oldClickText} => {$newClickText}",
            //            LPSConvert.SerializeObjectToLine<Line>(
            //                OldClickText.ToClickText(),
            //                "ClickText"
            //            ),
            //            LPSConvert.SerializeObjectToLine<Line>(ClickText.ToClickText(), "ClickText")
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
        var models = list.Cast<ClickTextModel>().ToArray();
        var result = DialogService.ShowMessageBox(
            this,
            "确定删除已选中的 {0} 个点击文本吗".Translate(models.Length),
            "删除点击文本".Translate(),
            MessageBoxButton.YesNo
        );
        if (result is not true)
            return;
        foreach (var model in models)
        {
            ClickTexts.Remove(model);
            model.Dispose();
            this.LogIfEnabled(LogLevel.Information)?.Log("删除点击文本 '{id}'", model.ID);
        }
    }

    /// <summary>
    /// 重置
    /// </summary>
    public void Reset()
    {
        ClickText = null!;
        OldClickText = null!;
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
        if (string.IsNullOrWhiteSpace(ClickText.ID))
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
        else if (ClickText.Text is null)
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
            OldClickText?.ID != ClickText.ID
            && ModInfo.ClickTexts.Any(i => i.ID == ClickText.ID)
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
