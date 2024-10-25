using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using DynamicData.Binding;
using HanumanInstitute.MvvmDialogs;
using HKW.HKWReactiveUI;
using HKW.HKWUtils.Collections;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using HKW.WPF.Extensions;
using LinePutScript.Localization.WPF;
using Microsoft.Win32;
using ReactiveUI;
using Splat;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels.ModEdit;

public partial class MoveEditVM : ViewModelBase
{
    private static IDialogService DialogService => Locator.Current.GetService<IDialogService>()!;

    public MoveEditVM()
    {
        Moves = new([], [], f => f.Graph.Contains(Search, StringComparison.OrdinalIgnoreCase));

        //if (Pets.HasValue())
        //    CurrentPet = Pets.FirstOrDefault(
        //        m => m.FromMain is false && m.Moves.HasValue(),
        //        Pets.First()
        //    );
        this.WhenValueChanged(x => x.Search)
            .Throttle(TimeSpan.FromSeconds(1), RxApp.TaskpoolScheduler)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => Moves.Refresh());
    }

    /// <summary>
    /// 模组信息
    /// </summary>
    public ModInfoModel ModInfo { get; } = null!;

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
    public PetModel CurrentPet { get; set; }

    partial void OnCurrentPetChanged(PetModel oldValue, PetModel newValue)
    {
        if (oldValue is not null)
            Moves.BaseList.BindingList(oldValue.Moves, true);
        Moves.Clear();
        if (newValue is null)
            return;
        Moves.AddRange(CurrentPet.Moves);
        Moves.BaseList.BindingList(CurrentPet.Moves);
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
        Image = NativeUtils.LoadImageToMemoryStream(openFileDialog.LocalPath);
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
        Image?.CloseStream();
        Image = NativeUtils.LoadImageToMemoryStream(openFileDialog.LocalPath);
    }

    /// <summary>
    /// 添加
    /// </summary>
    [ReactiveCommand]
    private void Add()
    {
        //var window = new MoveEditWindow();
        //var vm = window.ViewModel;
        //vm.CurrentPet = CurrentPet;
        //window.ShowDialog();
        //if (window.IsCancel)
        //    return;
        //Moves.Add(vm.Move);
    }

    /// <summary>
    /// 编辑
    /// </summary>
    /// <param name="model">模型</param>
    [ReactiveCommand]
    public void Edit(MoveModel model)
    {
        //var window = new MoveEditWindow();
        //var vm = window.ViewModel;
        //vm.CurrentPet = CurrentPet;
        //vm.OldMove = model;
        //var newMove = vm.Move = new(model);
        //window.ShowDialog();
        //if (window.IsCancel)
        //    return;
        //Moves[Moves.IndexOf(model)] = newMove;
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="model">模型</param>
    [ReactiveCommand]
    private void Remove(MoveModel model)
    {
        //if (MessageBox.Show("确定删除吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.No)
        //    return;
        //Moves.Remove(model);
    }
}
