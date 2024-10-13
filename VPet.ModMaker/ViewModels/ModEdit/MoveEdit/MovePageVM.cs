using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HKW.HKWReactiveUI;
using HKW.HKWUtils.Collections;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using LinePutScript.Localization.WPF;
using VPet.ModMaker.Models;
using VPet.ModMaker.Views.ModEdit.MoveEdit;

namespace VPet.ModMaker.ViewModels.ModEdit.MoveEdit;

public partial class MovePageVM : ViewModelBase
{
    public MovePageVM()
    {
        Moves = new([], [], f => f.Graph.Contains(Search, StringComparison.OrdinalIgnoreCase));

        //PropertyChangedX += MovePageVM_PropertyChangedX;
        if (Pets.HasValue())
            CurrentPet = Pets.FirstOrDefault(
                m => m.FromMain is false && m.Moves.HasValue(),
                Pets.First()
            );
        //AddCommand.ExecuteCommand += Add;
        //EditCommand.ExecuteCommand += Edit;
        //RemoveCommand.ExecuteCommand += Remove;
    }

    public static ModInfoModel ModInfo => ModInfoModel.Current;

    public FilterListWrapper<
        MoveModel,
        List<MoveModel>,
        ObservableList<MoveModel>
    > Moves { get; set; }

    public static ObservableList<PetModel> Pets => ModInfoModel.Current.Pets;

    [ReactiveProperty]
    public PetModel CurrentPet { get; set; }

    partial void OnCurrentPetChanged(PetModel oldValue, PetModel newValue)
    {
        //TODO:
        //Moves.Clear();
        //if (oldValue is not null )
        //    Moves.BindingList(pet.Moves, true);
        //if (newValue is null)
        //    return;
        //Moves.AddRange(CurrentPet.Moves);
        //Moves.BindingList(CurrentPet.Moves);
    }

    [ReactiveProperty]
    public string Search { get; set; } = string.Empty;

    partial void OnSearchChanged(string oldValue, string newValue)
    {
        Moves.Refresh();
    }

    //#region Command
    //public ObservableCommand AddCommand { get; } = new();
    //public ObservableCommand<MoveModel> EditCommand { get; } = new();
    //public ObservableCommand<MoveModel> RemoveCommand { get; } = new();
    //#endregion

    public void Close() { }

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

    [ReactiveCommand]
    private void Remove(MoveModel model)
    {
        if (MessageBox.Show("确定删除吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.No)
            return;
        Moves.Remove(model);
    }
}
