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
/// 低状态文本编辑视图模型
/// </summary>
public partial class LowTextEditViewModel
    : ViewModelBase,
        IEditViewModel,
        IModalDialogViewModel,
        IViewClosing
{
    /// <inheritdoc/>
    public LowTextEditViewModel()
    {
        LowTexts = new(
            [],
            [],
            f =>
            {
                return CurrentSearchTarget.Value switch
                {
                    LowTextSearchTarget.ID => f.ID.Contains(
                        Search,
                        StringComparison.CurrentCultureIgnoreCase
                    ),
                    LowTextSearchTarget.Text => f.Text.Contains(
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
            .Subscribe(_ => LowTexts?.Refresh());
    }

    [NotifyPropertyChangeFrom(nameof(ModInfo))]
    public string DisplayText => "LowText ({0})".Translate(ModInfo?.LowTexts.Count!);

    /// <summary>
    /// 模组信息
    /// </summary>
    [ObservableProperty]
    public ModInfoModel ModInfo { get; set; } = null!;

    partial void OnModInfoChanged(ModInfoModel oldValue, ModInfoModel newValue)
    {
        LowTexts.SourceList = null!;
        if (newValue is not null)
        {
            LowTexts.SourceList = newValue.LowTexts;
            newValue
                .LowTexts.WhenAnyValue(x => x.Count)
                .Subscribe(_ => OnPropertyChanged(nameof(DisplayText)))
                .DisposeWith(newValue.Disposables);
        }
    }

    /// <summary>
    /// 低状态文本
    /// </summary>
    public ObservableFilteredListWrapper<
        LowTextModel,
        ObservableList<LowTextModel>,
        ObservableList<LowTextModel>
    > LowTexts { get; }

    /// <summary>
    /// 搜索
    /// </summary>
    [ObservableProperty]
    public string Search { get; set; } = string.Empty;

    /// <summary>
    /// 搜索目标
    /// </summary>
    public static ImmutableArray<EnumInfo<LowTextSearchTarget>> SearchTargets =>
        EnumInfo<LowTextSearchTarget>.StaticInfos;

    [ObservableProperty]
    public EnumInfo<LowTextSearchTarget> CurrentSearchTarget { get; set; } = LowTextSearchTarget.ID;

    /// <summary>
    /// 旧低状态文本
    /// </summary>
    public LowTextModel? OldLowText { get; set; }

    /// <summary>
    /// 低状态文本
    /// </summary>
    [ObservableProperty]
    public LowTextModel LowText { get; set; } = null!;
    public bool? DialogResult { get; set; }
    public bool IsChanged { get; private set; }

    /// <summary>
    /// 保存
    /// </summary>
    [RelayCommand]
    private void Save()
    {
        if (Validate() is false)
            return;
        Commit();
        DialogService.ShowMessageBox(this, "保存成功".Translate());
    }

    private bool Validate()
    {
        if (string.IsNullOrWhiteSpace(LowText.ID))
        {
            DialogService.ShowMessageBox(
                this,
                "ID不可为空".Translate(),
                "数据错误".Translate(),
                MessageBoxButton.Ok,
                MessageBoxImage.Warning
            );
            return false;
        }
        if (LowText.Text is null)
        {
            DialogService.ShowMessageBox(
                this,
                "文本不可为空".Translate(),
                "数据错误".Translate(),
                MessageBoxButton.Ok,
                MessageBoxImage.Warning
            );
            return false;
        }
        if (OldLowText?.ID != LowText.ID && ModInfo.LowTexts.Any(i => i.ID == LowText.ID))
        {
            DialogService.ShowMessageBox(
                this,
                "此ID已存在".Translate(),
                "数据错误".Translate(),
                MessageBoxButton.Ok,
                MessageBoxImage.Warning
            );
            return false;
        }
        return true;
    }

    private void Commit()
    {
        LowText.I18nResource.CopyDataTo(ModInfo.I18nResource);
        var savedModel = new LowTextModel(LowText, ModInfo.I18nResource);
        if (OldLowText is null)
        {
            LowTexts.Add(savedModel);
            this.LogIfEnabled(LogLevel.Information)?.Log("添加低状态文本 '{id}'", savedModel.ID);
        }
        else
        {
            OldLowText.Dispose();
            var index = LowTexts.IndexOf(OldLowText);
            if (index >= 0)
                LowTexts[index] = savedModel;
            else
                LowTexts.Add(savedModel);
            this.LogIfEnabled(LogLevel.Information)
                ?.Log("编辑低状态文本 {oldLowText} => {newLowText}", OldLowText.ID, savedModel.ID);
        }
        OldLowText = savedModel;
        IsChanged = false;
    }

    /// <summary>
    /// 添加
    /// </summary>
    [RelayCommand]
    private void Add()
    {
        IsChanged = false;
        LowText = new(ModInfo.TempI18nResource);
        using var dis = LowText.Changed.Subscribe(_ => IsChanged = true);
        var result = DialogService.ShowDialog(this, this);
        if (result is true)
        {
            Commit();
        }
        LowText.Dispose();
        Reset();
    }

    /// <summary>
    /// 编辑
    /// </summary>
    /// <param name="model">模型</param>
    [RelayCommand]
    public void Edit(LowTextModel model)
    {
        IsChanged = false;
        OldLowText = model;
        var newModel = new LowTextModel(model, ModInfo.TempI18nResource);
        model.I18nResource.CopyDataTo(newModel.I18nResource, model.ID);
        LowText = newModel;
        using var dis = newModel.Changed.Subscribe(_ => IsChanged = true);
        var result = DialogService.ShowDialog(this, this);
        if (result is true)
        {
            Commit();
        }
        newModel.Dispose();
        Reset();
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="list">列表</param>
    [RelayCommand]
    private void Remove(IList list)
    {
        var models = list.Cast<LowTextModel>().ToArray();
        var result = DialogService.ShowMessageBox(
            this,
            "确定删除已选中的 {0} 个低状态文本吗".Translate(models.Length),
            "删除低状态文本".Translate(),
            MessageBoxButton.YesNo
        );
        if (result is not true)
            return;
        foreach (var model in models)
        {
            LowTexts.Remove(model);
            model.Dispose();
            this.LogIfEnabled(LogLevel.Information)?.Log("删除低状态文本 '{id}'", model.ID);
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
        if (Validate() is false)
            e.Cancel = true;
        DialogResult = e.Cancel is not true;
    }

    public Task OnClosingAsync(CancelEventArgs e)
    {
        return Task.CompletedTask;
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
