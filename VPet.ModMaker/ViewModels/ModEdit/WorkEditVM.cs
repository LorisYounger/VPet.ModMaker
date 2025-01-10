using System.Collections;
using System.Collections.Frozen;
using System.ComponentModel;
using System.Reactive.Linq;
using System.Windows.Media.Imaging;
using DynamicData.Binding;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using HKW.HKWMapper;
using HKW.HKWReactiveUI;
using HKW.HKWUtils;
using HKW.HKWUtils.Collections;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using HKW.MVVMDialogs;
using HKW.WPF;
using HKW.WPF.Extensions;
using HKW.WPF.MVVMDialogs;
using LinePutScript;
using LinePutScript.Converter;
using LinePutScript.Localization.WPF;
using ReactiveUI;
using Splat;
using VPet.ModMaker.Models;
using VPet.ModMaker.Models.ModModel;

namespace VPet.ModMaker.ViewModels.ModEdit;

/// <summary>
/// 工作编辑视图模型
/// </summary>
public partial class WorkEditVM : DialogViewModel, IEnableLogger<ViewModelBase>, IDisposable
{
    /// <inheritdoc/>
    public WorkEditVM(ModInfoModel modInfo)
    {
        ModInfo = modInfo;
        Works = new(
            [],
            [],
            f =>
            {
                return SearchTargets.SelectedItem switch
                {
                    WorkSearchTarget.ID
                        => f.ID.Contains(Search, StringComparison.OrdinalIgnoreCase),
                    WorkSearchTarget.Graph
                        => f.Graph.Contains(Search, StringComparison.OrdinalIgnoreCase),
                    _ => false,
                };
            }
        );

        modInfo
            .WhenValueChanged(x => x.CurrentPet)
            .Throttle(TimeSpan.FromSeconds(0.5), RxApp.TaskpoolScheduler)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(x => CurrentPet = x)
            .Record(this);

        this.WhenAnyValue(x => x.Search, x => x.SearchTargets.SelectedItem)
            .Throttle(TimeSpan.FromSeconds(0.5), RxApp.TaskpoolScheduler)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => Works.Refresh())
            .Record(this);

        Closing += WorkEditVM_Closing;
    }

    private void WorkEditVM_Closing(object? sender, CancelEventArgs e)
    {
        if (DialogResult is not true)
            return;
        if (string.IsNullOrWhiteSpace(Work.ID))
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
        else if (Work.Graph is null)
        {
            ModMakerVM.DialogService.ShowMessageBoxX(
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

    #region Property

    /// <summary>
    /// 模组信息
    /// </summary>
    public ModInfoModel ModInfo { get; set; } = null!;

    /// <summary>
    /// 当前宠物
    /// </summary>
    [ReactiveProperty]
    public PetModel? CurrentPet { get; set; }

    partial void OnCurrentPetChanged(PetModel oldValue, PetModel newValue)
    {
        if (oldValue is not null)
        {
            Works.BaseList.BindingList(oldValue.Works, true);
        }
        Works.Clear();
        Works.AutoFilter = false;
        if (newValue is not null)
        {
            Works.AddRange(newValue.Works);
            Works.BaseList.BindingList(newValue.Works);
            Search = string.Empty;
            SearchTargets.SelectedItem = WorkSearchTarget.ID;
        }
        Works.Refresh();
        Works.AutoFilter = true;
    }

    /// <summary>
    /// 全部工作
    /// </summary>
    public FilterListWrapper<
        WorkModel,
        ObservableList<WorkModel>,
        ObservableList<WorkModel>
    > Works { get; set; } = null!;

    /// <summary>
    /// 搜索
    /// </summary>
    [ReactiveProperty]
    public string Search { get; set; } = string.Empty;

    /// <summary>
    /// 搜索目标
    /// </summary>
    public ObservableSelectableSetWrapper<
        WorkSearchTarget,
        FrozenSet<WorkSearchTarget>
    > SearchTargets { get; } = new(EnumInfo<WorkSearchTarget>.Values);

    /// <summary>
    /// 旧工作
    /// </summary>
    public WorkModel? OldWork { get; set; }

    /// <summary>
    /// 工作
    /// </summary>
    [ReactiveProperty]
    public WorkModel Work { get; set; } = null!;

    /// <summary>
    /// 比例
    /// </summary>
    public static double LengthRatio { get; } = 0.5;

    partial void OnWorkChanged(WorkModel oldValue, WorkModel newValue)
    {
        if (oldValue is not null)
        {
            oldValue.PropertyChanged -= Work_PropertyChanged;
        }
        if (newValue is not null)
        {
            newValue.PropertyChanged -= Work_PropertyChanged;
            newValue.PropertyChanged += Work_PropertyChanged;
            SetGraphImage(newValue);
        }
    }

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
    [ReactiveProperty]
    public BitmapImage? Image { get; set; }
    #endregion

    /// <summary>
    /// 修复超模
    /// </summary>
    [ReactiveCommand]
    private void FixOverLoad()
    {
        Work.FixOverLoad();
    }

    /// <summary>
    /// 为所有工作修复超模
    /// </summary>
    [ReactiveCommand]
    private void FixOverLoadForAllWork()
    {
        if (
            ModMakerVM.DialogService.ShowMessageBoxX(
                this,
                "将为所有工作进行超模修复,你确定吗?".Translate(),
                "修复所有工作超模".Translate(),
                MessageBoxButton.YesNo,
                MessageBoxImage.Information
            )
            is not true
        )
            return;
        foreach (var work in Works)
        {
            work.FixOverLoad();
        }
        this.LogX().Info("已为 {count} 个工作修复超模", Works.Count);
    }

    /// <summary>
    /// 添加图像
    /// </summary>
    [ReactiveCommand]
    private void AddImage()
    {
        var openFileDialog = ModMakerVM.DialogService.ShowOpenFileDialog(
            this,
            new()
            {
                Title = "选择图片".Translate(),
                Filters = [new("图片".Translate(), ["jpg", "jpeg", "png", "bmp"])]
            }
        );
        if (openFileDialog is null)
            return;
        var newImage = HKWImageUtils.LoadImageToMemory(openFileDialog.LocalPath);
        if (newImage is null)
        {
            ModMakerVM.DialogService.ShowMessageBoxX(
                this,
                "图片载入失败, 详情请查看日志".Translate(),
                "图片载入失败".Translate(),
                icon: MessageBoxImage.Warning
            );
            return;
        }
        Image = newImage;
    }

    /// <summary>
    /// 改变图像
    /// </summary>
    [ReactiveCommand]
    private void ChangeImage()
    {
        var openFileDialog = ModMakerVM.DialogService.ShowOpenFileDialog(
            this,
            new()
            {
                Title = "选择图片".Translate(),
                Filters = [new("图片".Translate(), ["jpg", "jpeg", "png", "bmp"])]
            }
        );
        if (openFileDialog is null)
            return;
        var newImage = HKWImageUtils.LoadImageToMemory(openFileDialog.LocalPath);
        if (newImage is null)
        {
            ModMakerVM.DialogService.ShowMessageBoxX(
                this,
                "图片载入失败, 详情请查看日志".Translate(),
                "图片载入失败".Translate(),
                icon: MessageBoxImage.Warning
            );
            return;
        }
        Image?.CloseStreamWhenNoReference();
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
        Image?.CloseStreamWhenNoReference();
        Image = null;
        // 随机挑一张图片
        if (
            CurrentPet.Animes.FirstOrDefault(
                a =>
                    a.GraphType is VPet_Simulator.Core.GraphInfo.GraphType.Work
                    && a.Name.Equals(graph, StringComparison.OrdinalIgnoreCase),
                null!
            )
            is not AnimeTypeModel anime
        )
            return;
        if (anime.HappyAnimes.HasValue())
        {
            Image = anime.HappyAnimes.Random().Images.Random().Clone().Image;
        }
        else if (anime.NomalAnimes.HasValue())
        {
            Image = anime.NomalAnimes.Random().Images.Random().Clone().Image;
        }
        else if (anime.PoorConditionAnimes.HasValue())
        {
            Image = anime.PoorConditionAnimes.Random().Images.Random().Clone().Image;
        }
        else if (anime.IllAnimes.HasValue())
        {
            Image = anime.IllAnimes.Random().Images.Random().Clone().Image;
        }
    }

    /// <summary>
    /// 添加
    /// </summary>
    [ReactiveCommand]
    private async void Add()
    {
        ModInfo.TempI18nResource.ClearCultureData();
        Work = new() { I18nResource = ModInfo.I18nResource };
        await ModMakerVM.DialogService.ShowDialogAsyncX(this, this);
        if (DialogResult is not true)
        {
            Work.Close();
        }
        else
        {
            Work.I18nResource.CopyDataTo(ModInfo.I18nResource, true);
            Work.I18nResource = ModInfo.I18nResource;
            Works.Add(Work);
            if (this.LogX().Level is LogLevel.Info)
                this.LogX().Info("添加新工作 {work}", Work.ID);
            else
                this.LogX()
                    .Debug(
                        "添加新工作 {$work}",
                        LPSConvert.SerializeObjectToLine<Line>(Work.MapToWork(new()), "Work")
                    );
        }
        Reset();
    }

    /// <summary>
    /// 编辑
    /// </summary>
    /// <param name="model">模型</param>
    [ReactiveCommand]
    public async void Edit(WorkModel model)
    {
        OldWork = model;
        var newModel = new WorkModel(model) { I18nResource = ModInfo.TempI18nResource };
        model.I18nResource.CopyDataTo(newModel.I18nResource, [model.ID], true);
        Work = newModel;
        await ModMakerVM.DialogService.ShowDialogAsync(this, this);
        if (DialogResult is not true)
        {
            newModel.I18nResource.ClearCulture();
            newModel.Close();
        }
        else
        {
            OldWork.Close();
            newModel.I18nResource.CopyDataTo(ModInfo.I18nResource, true);
            newModel.I18nResource = ModInfo.I18nResource;
            Works[Works.IndexOf(model)] = newModel;
            if (this.LogX().Level is LogLevel.Info)
                this.LogX().Info("编辑工作 {oldWork} => {newWork}", OldWork.ID, Work.ID);
            else
                this.LogX()
                    .Debug(
                        "编辑工作\n {$oldWork} => {$newWork}",
                        LPSConvert.SerializeObjectToLine<Line>(OldWork.MapToWork(new()), "Work"),
                        LPSConvert.SerializeObjectToLine<Line>(Work.MapToWork(new()), "Work")
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
        var models = list.Cast<WorkModel>().ToArray();
        if (
            ModMakerVM.DialogService.ShowMessageBoxX(
                this,
                "确定删除已选中的 {0} 个工作吗".Translate(models.Length),
                "删除工作".Translate(),
                MessageBoxButton.YesNo
            )
            is not true
        )
            return;
        foreach (var model in models)
        {
            Works.Remove(model);
            model.Close();
            this.LogX().Info("删除工作 {work}", model.ID);
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
        Image?.CloseStreamWhenNoReference();
        Image = null;
        ModInfo.TempI18nResource.ClearCultureData();
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (_disposed)
            return;
        base.Dispose(disposing);
        Reset();
        ModInfo = null!;
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
