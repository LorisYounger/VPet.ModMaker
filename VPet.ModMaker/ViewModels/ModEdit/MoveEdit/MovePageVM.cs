using HKW.HKWViewModels.SimpleObservable;
using LinePutScript.Localization.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VPet.ModMaker.Models;
using VPet.ModMaker.Views.ModEdit.MoveEdit;

namespace VPet.ModMaker.ViewModels.ModEdit.MoveEdit;

public class MovePageVM
{
    #region Value
    public ObservableValue<ObservableCollection<MoveModel>> ShowMoves { get; } = new();
    public ObservableCollection<MoveModel> Moves => CurrentPet.Value.Moves;

    public ObservableCollection<PetModel> Pets => ModInfoModel.Current.Pets;
    public ObservableValue<PetModel> CurrentPet { get; } = new(new());
    public ObservableValue<string> Search { get; } = new();
    #endregion
    #region Command
    public ObservableCommand AddCommand { get; } = new();
    public ObservableCommand<MoveModel> EditCommand { get; } = new();
    public ObservableCommand<MoveModel> RemoveCommand { get; } = new();
    #endregion
    public MovePageVM()
    {
        ShowMoves.Value = Moves;
        CurrentPet.ValueChanged += CurrentPet_ValueChanged;
        Search.ValueChanged += Search_ValueChanged;

        AddCommand.ExecuteEvent += Add;
        EditCommand.ExecuteEvent += Edit;
        RemoveCommand.ExecuteEvent += Remove;
    }

    private void CurrentPet_ValueChanged(PetModel oldValue, PetModel newValue)
    {
        ShowMoves.Value = newValue.Moves;
    }

    private void Search_ValueChanged(string oldValue, string newValue)
    {
        if (string.IsNullOrWhiteSpace(newValue))
        {
            ShowMoves.Value = Moves;
        }
        else
        {
            ShowMoves.Value = new(
                Moves.Where(
                    m => m.Graph.Value.Contains(newValue, StringComparison.OrdinalIgnoreCase)
                )
            );
        }
    }

    public void Close() { }

    private void Add()
    {
        var window = new MoveEditWindow();
        var vm = window.ViewModel;
        vm.CurrentPet = CurrentPet.Value;
        window.ShowDialog();
        if (window.IsCancel)
            return;
        Moves.Add(vm.Move.Value);
    }

    public void Edit(MoveModel model)
    {
        var window = new MoveEditWindow();
        var vm = window.ViewModel;
        vm.CurrentPet = CurrentPet.Value;
        vm.OldMove = model;
        var newMove = vm.Move.Value = new(model);
        window.ShowDialog();
        if (window.IsCancel)
            return;
        Moves[Moves.IndexOf(model)] = newMove;
        if (ShowMoves.Value.Count != Moves.Count)
            ShowMoves.Value[ShowMoves.Value.IndexOf(model)] = newMove;
    }

    private void Remove(MoveModel model)
    {
        if (MessageBox.Show("确定删除吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.No)
            return;
        if (ShowMoves.Value.Count == Moves.Count)
        {
            Moves.Remove(model);
        }
        else
        {
            ShowMoves.Value.Remove(model);
            Moves.Remove(model);
        }
    }
}
