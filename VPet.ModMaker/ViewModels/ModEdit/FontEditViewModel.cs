using System;
using System.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using HKW.HKWUtils.Collections;
using HKW.HKWUtils.Observable;
using HKW.MVVM;
using HKW.MVVM.SourceGenerator;
using LinePutScript.Localization.WPF;
using Microsoft.Extensions.Logging;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels;

public sealed partial class FontEditViewModel : ViewModelBase, IEditViewModel, IModalDialogViewModel
{
    /// <inheritdoc/>
    public FontEditViewModel()
    {
        Fonts = new(
            [],
            [],
            x => x.Name.Contains(Search, StringComparison.CurrentCultureIgnoreCase)
        );
        this.WhenAnyValue(x => x.Search)
            .Throttle(TimeSpan.FromSeconds(0.5), ObservableSchedulers.ThreadPool)
            .DistinctUntilChanged()
            .ObserveOn(ObservableSchedulers.Current)
            .Subscribe(_ => Fonts?.Refresh());
    }

    [NotifyPropertyChangeFrom(nameof(ModInfo))]
    public string DisplayText => "Fonts ({0})".Translate(ModInfo?.Fonts.Count!);

    /// <summary>
    /// 模组信息
    /// </summary>
    [ObservableProperty]
    public ModInfoModel ModInfo { get; set; } = null!;

    partial void OnModInfoChanged(ModInfoModel oldValue, ModInfoModel newValue)
    {
        Fonts.SourceList = null!;
        if (newValue is not null)
        {
            Fonts.SourceList = newValue.Fonts;
            newValue
                .Fonts.WhenAnyValue(x => x.Count)
                .Subscribe(_ => OnPropertyChanged(nameof(DisplayText)))
                .DisposeWith(newValue.Disposables);
        }
    }

    public ObservableFilteredListWrapper<
        FontModel,
        ObservableList<FontModel>,
        ObservableList<FontModel>
    > Fonts { get; }

    /// <summary>
    /// 搜索
    /// </summary>
    [ObservableProperty]
    public string Search { get; set; } = string.Empty;

    public bool? DialogResult { get; set; }

    /// <summary>
    /// 添加
    /// </summary>
    [RelayCommand]
    private void Add()
    {
        var openFileDialog = DialogService.ShowOpenFileDialog(
            this,
            new()
            {
                Title = "选择字体".Translate(),
                Filters = [new("字体".Translate(), ModMaker.FontExtensions)],
            }
        );
        if (openFileDialog is null)
            return;
        try
        {
            var font = new FontModel(openFileDialog.LocalPath);
            Fonts.Add(font);
            this.LogIfEnabled(LogLevel.Information)?.Log("添加新字体 '{id}'", font.Name);
        }
        catch (Exception ex)
        {
            this.LogIfEnabled(LogLevel.Warning)
                ?.Log(ex, "添加新字体失败, 文件路径: '{file}'", openFileDialog.LocalPath);
            DialogService.ShowMessageBox(
                this,
                "添加新字体失败, 详情请查看日志".Translate(),
                "添加失败".Translate()
            );
        }
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="list">列表</param>
    [RelayCommand]
    private void Remove(IList list)
    {
        var models = list.Cast<FontModel>().ToArray();
        var result = DialogService.ShowMessageBox(
            this,
            "确定删除已选中的 {0} 个字体吗".Translate(models.Length),
            "删除字体".Translate(),
            MessageBoxButton.YesNo
        );
        if (result is not true)
            return;
        foreach (var model in models)
        {
            Fonts.Remove(model);
            this.LogIfEnabled(LogLevel.Information)?.Log("删除字体 '{id}'", model.Name);
        }
    }
}
