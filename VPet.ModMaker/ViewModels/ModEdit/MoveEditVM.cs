using System.Collections;
using System.Reactive.Linq;
using System.Windows.Media.Imaging;
using DynamicData.Binding;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using HKW.HKWMapper;
using HKW.HKWReactiveUI;
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

namespace VPet.ModMaker.ViewModels.ModEdit;

/// <summary>
/// 移动编辑视图模型
/// </summary>
public partial class MoveEditVM : DialogViewModel, IEnableLogger<ViewModelBase>, IDisposable
{
    /// <inheritdoc/>
    public MoveEditVM()
    {
        Moves = new([], [], f => f.Graph.Contains(Search, StringComparison.OrdinalIgnoreCase));

        this.WhenValueChanged(x => x.Search)
            .Throttle(TimeSpan.FromSeconds(0.5), RxApp.TaskpoolScheduler)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => Moves.Refresh())
            .Record(this);

        Closing += MoveEditVM_Closing;
    }

    private void MoveEditVM_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (DialogResult is not true)
            return;
        if (string.IsNullOrWhiteSpace(Move.Graph))
        {
            ModMakerVM.DialogService.ShowMessageBoxX(
                this,
                "指定动画不可为空".Translate(),
                "数据错误".Translate()
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
        if (oldValue is not null) { }
        if (newValue is not null)
        {
            newValue
                .WhenValueChanged(x => x.CurrentPet)
                .Throttle(TimeSpan.FromSeconds(0.5), RxApp.TaskpoolScheduler)
                .DistinctUntilChanged()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => CurrentPet = x)
                .Record(this);
        }
        if (newValue is null)
            CurrentPet = null;
    }

    /// <summary>
    /// 全部移动
    /// </summary>
    public FilterListWrapper<
        MoveModel,
        ObservableList<MoveModel>,
        ObservableList<MoveModel>
    > Moves { get; set; }

    /// <summary>
    /// 当前宠物
    /// </summary>
    [ReactiveProperty]
    public PetModel? CurrentPet { get; set; }

    partial void OnCurrentPetChanged(PetModel oldValue, PetModel newValue)
    {
        if (oldValue is not null)
        {
            Moves.BaseList.BindingList(oldValue.Moves, true);
        }
        Moves.Clear();
        Moves.AutoFilter = false;
        if (newValue is not null)
        {
            newValue
                .I18nResource.WhenValueChanged(x => x.CurrentCulture)
                .Throttle(TimeSpan.FromSeconds(0.5), RxApp.TaskpoolScheduler)
                .DistinctUntilChanged()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => Moves.Refresh())
                .Record(this);

            Moves.AddRange(newValue.Moves);
            Moves.BaseList.BindingList(newValue.Moves);
            Search = string.Empty;
            Moves.Refresh();
            Moves.AutoFilter = true;
        }
    }

    /// <summary>
    /// 搜索
    /// </summary>
    [ReactiveProperty]
    public string Search { get; set; } = string.Empty;

    /// <summary>
    /// 旧移动
    /// </summary>
    public MoveModel? OldMove { get; set; }

    /// <summary>
    /// 移动
    /// </summary>
    [ReactiveProperty]
    public MoveModel Move { get; set; } = new();

    /// <summary>
    /// 图像
    /// </summary>
    [ReactiveProperty]
    public BitmapImage? Image { get; set; }

    /// <summary>
    /// 添加图片
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
    /// 改变图片
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
    /// 添加
    /// </summary>
    [ReactiveCommand]
    private async void Add()
    {
        await ModMakerVM.DialogService.ShowDialogAsyncX(this, this);
        if (DialogResult is not true)
            return;

        Moves.Add(Move);
        if (this.LogX().Level is LogLevel.Info)
            this.LogX().Info("添加新移动 {move}", Move.Graph);
        else
            this.LogX()
                .Debug(
                    "添加新移动 {$move}",
                    LPSConvert.SerializeObjectToLine<Line>(Move.MapToMove(new()), "Move")
                );
        Reset();
    }

    /// <summary>
    /// 编辑
    /// </summary>
    /// <param name="model">模型</param>
    [ReactiveCommand]
    public async void Edit(MoveModel model)
    {
        OldMove = model;
        var newModel = new MoveModel(model);
        Move = newModel;
        await ModMakerVM.DialogService.ShowDialogAsync(this, this);
        if (DialogResult is not true)
            return;
        Moves[Moves.IndexOf(model)] = newModel;
        this.LogX().Info("编辑移动 {oldMove} => {newMove}", OldMove.Graph, Move.Graph);
        Reset();
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="list">列表</param>
    [ReactiveCommand]
    private void Remove(IList list)
    {
        var models = list.Cast<MoveModel>().ToArray();
        if (
            ModMakerVM.DialogService.ShowMessageBoxX(
                this,
                "确定删除已选中的 {0} 个移动吗".Translate(models.Length),
                "删除移动".Translate(),
                MessageBoxButton.YesNo
            )
            is not true
        )
            return;
        foreach (var model in models)
        {
            Moves.Remove(model);
            this.LogX().Info("删除移动 {move}", model.Graph);
        }
    }

    /// <summary>
    /// 重置
    /// </summary>
    public void Reset()
    {
        Move = null!;
        OldMove = null!;
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
        CurrentPet = null!;
        ModInfo = null!;
        _disposed = false;
    }
}
