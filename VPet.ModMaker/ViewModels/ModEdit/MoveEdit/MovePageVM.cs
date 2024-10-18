using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DynamicData.Binding;
using HKW.HKWReactiveUI;
using HKW.HKWUtils.Collections;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using LinePutScript.Localization.WPF;
using ReactiveUI;
using VPet.ModMaker.Models;
using VPet.ModMaker.Views.ModEdit.MoveEdit;

namespace VPet.ModMaker.ViewModels.ModEdit.MoveEdit;

public partial class MovePageVM : ViewModelBase
{
    public MovePageVM()
    {
        Moves = new([], [], f => f.Graph.Contains(Search, StringComparison.OrdinalIgnoreCase));

        if (Pets.HasValue())
            CurrentPet = Pets.FirstOrDefault(
                m => m.FromMain is false && m.Moves.HasValue(),
                Pets.First()
            );
        this.WhenValueChanged(x => x.Search)
            .Throttle(TimeSpan.FromSeconds(1), RxApp.TaskpoolScheduler)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => Moves.Refresh());
    }

    public static ModInfoModel ModInfo => ModInfoModel.Current;

    public FilterListWrapper<
        MoveModel,
        ObservableList<MoveModel>,
        ObservableList<MoveModel>
    > Moves { get; set; }

    public static ObservableList<PetModel> Pets => ModInfoModel.Current.Pets;

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

    public void Close() { }

    /// <summary>
    /// 添加
    /// </summary>
    [ReactiveCommand]
    private void Add()
    {
        var window = new MoveEditWindow();
        var vm = window.ViewModel;
        vm.CurrentPet = CurrentPet;
        window.ShowDialog();
        if (window.IsCancel)
            return;
        Moves.Add(vm.Move);
    }

    /// <summary>
    /// 编辑
    /// </summary>
    /// <param name="model">模型</param>
    [ReactiveCommand]
    public void Edit(MoveModel model)
    {
        var window = new MoveEditWindow();
        var vm = window.ViewModel;
        vm.CurrentPet = CurrentPet;
        vm.OldMove = model;
        var newMove = vm.Move = new(model);
        window.ShowDialog();
        if (window.IsCancel)
            return;
        Moves[Moves.IndexOf(model)] = newMove;
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="model">模型</param>
    [ReactiveCommand]
    private void Remove(MoveModel model)
    {
        if (MessageBox.Show("确定删除吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.No)
            return;
        Moves.Remove(model);
    }
}
