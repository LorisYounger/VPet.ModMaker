using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
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
using Microsoft.Win32;
using ReactiveUI;
using Splat;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels.ModEdit;

public partial class MoveEditVM : DialogViewModel
{
    private static IDialogService DialogService => Locator.Current.GetService<IDialogService>()!;

    public MoveEditVM()
    {
        Moves = new([], [], f => f.Graph.Contains(Search, StringComparison.OrdinalIgnoreCase));

        this.WhenValueChanged(x => x.Search)
            .Throttle(TimeSpan.FromSeconds(0.5), RxApp.TaskpoolScheduler)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => Moves.Refresh());

        Closing += MoveEditVM_Closing;
    }

    private void MoveEditVM_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (DialogResult is not true)
            return;
        if (string.IsNullOrWhiteSpace(Move.Graph))
        {
            DialogService.ShowMessageBoxX(this, "指定动画不可为空".Translate(), "数据错误".Translate());
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
                .Subscribe(x => CurrentPet = x);
        }
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
        Moves.AutoFilter = false;
        Moves.Clear();
        if (newValue is not null)
        {
            newValue
                .I18nResource.WhenValueChanged(x => x.CurrentCulture)
                .Throttle(TimeSpan.FromSeconds(0.5), RxApp.TaskpoolScheduler)
                .DistinctUntilChanged()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(_ => Moves.Refresh());

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
    public MoveModel? OldMove { get; set; }

    [ReactiveProperty]
    public MoveModel Move { get; set; } = new();

    [ReactiveProperty]
    public double BorderLength { get; set; } = 250;

    [ReactiveProperty]
    public double LengthRatio { get; set; } = 250 / 500;

    [ReactiveProperty]
    public BitmapImage? Image { get; set; }

    public void Close()
    {
        Image?.CloseStream();
    }

    /// <summary>
    /// 添加图片
    /// </summary>
    [ReactiveCommand]
    private void AddImage()
    {
        var openFileDialog = DialogService.ShowOpenFileDialog(
            this,
            new()
            {
                Title = "选择图片".Translate(),
                Filters = [new("图片".Translate(), ["jpg", "jpeg", "png", "bmp"])]
            }
        );
        if (openFileDialog is null)
            return;
        var newImage = HKWImageUtils.LoadImageToMemory(openFileDialog.LocalPath, this);
        if (newImage is null)
        {
            DialogService.ShowMessageBoxX(
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
        var openFileDialog = DialogService.ShowOpenFileDialog(
            this,
            new()
            {
                Title = "选择图片".Translate(),
                Filters = [new("图片".Translate(), ["jpg", "jpeg", "png", "bmp"])]
            }
        );
        if (openFileDialog is null)
            return;
        var newImage = HKWImageUtils.LoadImageToMemory(openFileDialog.LocalPath, this);
        if (newImage is null)
        {
            DialogService.ShowMessageBoxX(
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
    /// 添加
    /// </summary>
    [ReactiveCommand]
    private async void Add()
    {
        await DialogService.ShowSingletonDialogAsync(this, this);
        if (DialogResult is not true)
            return;

        Moves.Add(Move);
        if (this.Log().Level is LogLevel.Info)
            this.Log().Info("添加新移动 {move}", Move.Graph);
        else
            this.Log()
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
        await DialogService.ShowDialogAsync(this, this);
        if (DialogResult is not true)
            return;
        Moves[Moves.IndexOf(model)] = newModel;
        this.Log().Info("编辑移动 {oldMove} => {newMove}", OldMove.Graph, Move.Graph);
        Reset();
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="model">模型</param>
    [ReactiveCommand]
    private void Remove(MoveModel model)
    {
        if (
            DialogService.ShowMessageBoxX(
                this,
                "确定删除 {0} 吗".Translate(model.Graph),
                "删除移动".Translate(),
                MessageBoxButton.YesNo
            )
            is not true
        )
            return;
        Moves.Remove(model);
        this.Log().Info("删除移动 {move}", model.Graph);
        Reset();
    }

    public void Reset()
    {
        Move = null!;
        OldMove = null!;
        DialogResult = false;
        ModInfo.TempI18nResource.ClearCultureData();
    }
}
