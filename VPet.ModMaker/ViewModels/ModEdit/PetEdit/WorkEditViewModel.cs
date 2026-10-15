using System.Collections;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Windows.Media.Imaging;
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
using HKW.WPF;
using HKW.WPF.Extensions;
using LinePutScript.Localization.WPF;
using Microsoft.Extensions.Logging;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels;

/// <summary>
/// 工作编辑视图模型
/// </summary>
public sealed partial class WorkEditViewModel
    : ViewModelBase,
        IEditViewModel,
        IModalDialogViewModel,
        IViewClosing,
        IDisposable
{
    /// <inheritdoc/>
    public WorkEditViewModel()
    {
        this.WhenAnyValue(x => x.Search, x => x.CurrentSearchTarget)
            .Throttle(TimeSpan.FromSeconds(0.5), ObservableSchedulers.ThreadPool)
            .DistinctUntilChanged()
            .ObserveOn(ObservableSchedulers.Current)
            .Subscribe(_ => Works?.Refresh());
    }

    #region Property

    [NotifyPropertyChangeFrom(nameof(CurrentPet))]
    public string DisplayText => "Work ({0})".Translate(CurrentPet?.Works.Count!);

    /// <summary>
    /// 模组信息
    /// </summary>
    [ObservableProperty]
    public ModInfoModel ModInfo { get; set; } = null!;

    partial void OnModInfoChanged(ModInfoModel oldValue, ModInfoModel newValue)
    {
        if (newValue is not null)
        {
            CurrentPet = newValue.CurrentPet;
            newValue
                .WhenAnyValue(x => x.CurrentPet)
                .Subscribe(x => CurrentPet = x)
                .DisposeWith(newValue.Disposables);
        }
        else
            CurrentPet = null;
    }

    /// <summary>
    /// 当前宠物
    /// </summary>
    [ObservableProperty]
    public PetModel? CurrentPet { get; set; }

    partial void OnCurrentPetChanged(PetModel? oldValue, PetModel? newValue)
    {
        Works = null!;
        ModInfo?.CurrentPet = newValue;
        if (newValue is not null)
        {
            Works = new(
                newValue.Works,
                [],
                f =>
                {
                    return CurrentSearchTarget.Value switch
                    {
                        WorkSearchTarget.ID => f.ID.Contains(
                            Search,
                            StringComparison.CurrentCultureIgnoreCase
                        ),
                        WorkSearchTarget.Graph => f.Graph.Contains(
                            Search,
                            StringComparison.CurrentCultureIgnoreCase
                        ),
                        _ => false,
                    };
                }
            );
        }
        CurrentSearchTarget = WorkSearchTarget.ID;
    }

    /// <summary>
    /// 全部工作
    /// </summary>
    [ObservableProperty]
    public FilteredListWrapper<
        WorkModel,
        ObservableList<WorkModel>,
        ObservableList<WorkModel>
    > Works { get; set; } = null!;

    /// <summary>
    /// 搜索
    /// </summary>
    [ObservableProperty]
    public string Search { get; set; } = string.Empty;

    /// <summary>
    /// 搜索目标
    /// </summary>
    public static ImmutableArray<EnumInfo<WorkSearchTarget>> SearchTargets =>
        EnumInfo<WorkSearchTarget>.StaticInfos;

    [ObservableProperty]
    public EnumInfo<WorkSearchTarget> CurrentSearchTarget { get; set; } = WorkSearchTarget.ID;

    /// <summary>
    /// 旧工作
    /// </summary>
    public WorkModel? OldWork { get; set; }

    /// <summary>
    /// 工作
    /// </summary>
    [ObservableProperty]
    public WorkModel Work { get; set; } = null!;

    partial void OnWorkChanged(WorkModel oldValue, WorkModel newValue)
    {
        if (oldValue is not null)
        {
            oldValue.PropertyChanged -= Work_PropertyChanged;
        }
        if (newValue is not null)
        {
            newValue.PropertyChanged += Work_PropertyChanged;
            SetGraphImage(newValue);
        }
    }

    /// <summary>
    /// 比例
    /// </summary>
    public static double LengthRatio { get; } = 0.5;

    private void Work_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not WorkModel workModel)
            return;
        if (e.PropertyName == nameof(WorkModel.Graph))
        {
            SetGraphImage(workModel);
        }
    }

    /// <summary>
    /// 图片
    /// </summary>
    [ObservableProperty]
    public BitmapImage? Image { get; set; }

    public bool? DialogResult { get; set; }
    #endregion

    /// <summary>
    /// 修复超模
    /// </summary>
    [RelayCommand]
    private void FixOverLoad()
    {
        Work.FixOverLoad();
    }

    /// <summary>
    /// 为所有工作修复超模
    /// </summary>
    [RelayCommand]
    private void FixOverLoadForAllWork()
    {
        var result = DialogService.ShowMessageBox(
            this,
            "将为所有工作进行超模修复,你确定吗?".Translate(),
            "修复所有工作超模".Translate(),
            MessageBoxButton.YesNo,
            MessageBoxImage.Information
        );
        if (result is not true)
            return;
        foreach (var work in Works)
        {
            work.FixOverLoad();
        }
        this.LogIfEnabled(LogLevel.Information)?.Log("已为 {count} 个工作修复超模", Works.Count);
    }

    /// <summary>
    /// 改变图像
    /// </summary>
    [RelayCommand]
    private void ChangeImage()
    {
        var openFileDialog = DialogService.ShowOpenFileDialog(
            this,
            new()
            {
                Title = "选择图片".Translate(),
                Filters = [new("图片".Translate(), ModMaker.ImageExtensions)],
            }
        );
        if (openFileDialog is null)
            return;
        var newImage = HKWImageUtils.LoadImageToMemory(openFileDialog.LocalPath);
        if (newImage is null)
        {
            DialogService.ShowMessageBox(
                this,
                "图片载入失败, 详情请查看日志".Translate(),
                "图片载入失败".Translate(),
                icon: MessageBoxImage.Warning
            );
            return;
        }
        Image?.CloseStream();
        Image = newImage;
    }

    /// <summary>
    /// 设置图片
    /// </summary>
    /// <param name="workModel">工作模型</param>
    private void SetGraphImage(WorkModel workModel)
    {
        if (CurrentPet is null)
            return;
        var graph = workModel.Graph;
        Image?.CloseStream();
        // 随机挑一张图片
        var anime = CurrentPet.Animes.FirstOrDefault(
            a => a.Name.Equals(graph, StringComparison.CurrentCultureIgnoreCase),
            null!
        );
        if (anime is not AnimeTypeModel animeType)
            return;
        if (animeType.HappyAnimes.Count > 0)
        {
            Image = animeType.HappyAnimes.Random().Images.Random().LoadImage().Clone();
        }
        else if (animeType.NomalAnimes.Count > 0)
        {
            Image = animeType.NomalAnimes.Random().Images.Random().LoadImage().Clone();
        }
        else if (animeType.PoorConditionAnimes.Count > 0)
        {
            Image = animeType.PoorConditionAnimes.Random().Images.Random().LoadImage().Clone();
        }
        else if (animeType.IllAnimes.Count > 0)
        {
            Image = animeType.IllAnimes.Random().Images.Random().LoadImage().Clone();
        }
    }

    /// <summary>
    /// 添加
    /// </summary>
    [RelayCommand]
    private void Add()
    {
        Work = new() { I18nResource = ModInfo.TempI18nResource };
        var result = DialogService.ShowDialog(this, this);
        if (result is not true)
        {
            Work.Dispose();
        }
        else
        {
            Work.I18nResource.CopyDataTo(ModInfo.I18nResource);
            Work.I18nResource = ModInfo.I18nResource;
            Works.Add(Work);
            this.LogIfEnabled(LogLevel.Information)?.Log("添加新工作 '{id}'", Work.ID);
            //if (this.LogX().Level is LogLevel.Info)
            //else
            //    this.LogX()
            //        .Debug(
            //            "添加新工作 {$work}",
            //            LPSConvert.SerializeObjectToLine<Line>(Work.MapToWork(new()), "Work")
            //        );
        }
        Reset();
    }

    /// <summary>
    /// 编辑
    /// </summary>
    /// <param name="model">模型</param>
    [RelayCommand]
    public void Edit(WorkModel model)
    {
        OldWork = model;
        var newModel = new WorkModel(model) { I18nResource = ModInfo.TempI18nResource };
        model.I18nResource.CopyDataTo(newModel.I18nResource, model.ID);
        Work = newModel;
        var result = DialogService.ShowDialog(this, this);
        if (result is not true)
        {
            newModel.Dispose();
        }
        else
        {
            OldWork.Dispose();
            newModel.I18nResource.CopyDataTo(ModInfo.I18nResource);
            newModel.I18nResource = ModInfo.I18nResource;
            Works[Works.IndexOf(model)] = newModel;
            this.LogIfEnabled(LogLevel.Information)
                ?.Log("编辑工作 {oldWork} => {newWork}", OldWork.ID, Work.ID);
            //if (this.LogX().Level is LogLevel.Info)
            //else
            //    this.LogX()
            //        .Debug(
            //            "编辑工作\n {$oldWork} => {$newWork}",
            //            LPSConvert.SerializeObjectToLine<Line>(OldWork.MapToWork(new()), "Work"),
            //            LPSConvert.SerializeObjectToLine<Line>(Work.MapToWork(new()), "Work")
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
        var models = list.Cast<WorkModel>().ToArray();
        var result = DialogService.ShowMessageBox(
            this,
            "确定删除已选中的 {0} 个工作吗".Translate(models.Length),
            "删除工作".Translate(),
            MessageBoxButton.YesNo
        );
        if (result is not true)
            return;
        foreach (var model in models)
        {
            Works.Remove(model);
            model.Dispose();
            this.LogIfEnabled(LogLevel.Information)?.Log("删除工作 '{id}'", model.ID);
        }
    }

    /// <summary>
    /// 重置
    /// </summary>
    public void Reset()
    {
        Work = null!;
        OldWork = null!;
        DialogResult = false;
        Image?.CloseStream();
        Image = null;
        ModInfo.TempI18nResource.ClearData();
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Image?.CloseStream();
        Image = null!;
        ModInfo = null!;
        CurrentPet = null;
    }

    public void OnClosing(CancelEventArgs e)
    {
        if (DialogResult is not true)
            return;
        if (string.IsNullOrWhiteSpace(Work.ID))
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
        else if (Work.Graph is null)
        {
            DialogService.ShowMessageBox(
                this,
                "指定动画不可为空".Translate(),
                "数据错误".Translate(),
                MessageBoxButton.Ok,
                MessageBoxImage.Warning
            );
            e.Cancel = true;
        }
        else if (OldWork?.ID != Work.ID && CurrentPet?.Works.Any(i => i.ID == Work.ID) is true)
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
/// 工作搜索目标
/// </summary>
public enum WorkSearchTarget
{
    /// <summary>
    /// ID
    /// </summary>
    ID,

    /// <summary>
    /// 指定图像
    /// </summary>
    Graph,
}
