using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HKW.HKWUtils.Observable;
using LinePutScript.Localization.WPF;
using VPet.ModMaker.Models;
using VPet.ModMaker.Views.ModEdit.MoveEdit;

namespace VPet.ModMaker.ViewModels.ModEdit.MoveEdit;

public class MovePageVM : ObservableObjectX<MovePageVM>
{
    public static ModInfoModel ModInfo => ModInfoModel.Current;

    #region Value
    #region ShowMoves
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ObservableCollection<MoveModel> _showMoves;

    public ObservableCollection<MoveModel> ShowMoves
    {
        get => _showMoves;
        set => SetProperty(ref _showMoves, value);
    }
    #endregion
    public ObservableCollection<MoveModel> Moves => CurrentPet.Value.Moves;

    public ObservableCollection<PetModel> Pets => ModInfoModel.Current.Pets;
    public ObservableValue<PetModel> CurrentPet { get; } = new(new());

    #region Search
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _search;

    public string Search
    {
        get => _search;
        set => SetProperty(ref _search, value);
    }
    #endregion
    #endregion
    #region Command
    public ObservableCommand AddCommand { get; } = new();
    public ObservableCommand<MoveModel> EditCommand { get; } = new();
    public ObservableCommand<MoveModel> RemoveCommand { get; } = new();
    #endregion
    public MovePageVM()
    {
        ShowMoves = Moves;
        CurrentPet.ValueChanged += CurrentPet_ValueChanged;
        //TODO
        //Search.ValueChanged += Search_ValueChanged;

        AddCommand.ExecuteCommand += Add;
        EditCommand.ExecuteCommand += Edit;
        RemoveCommand.ExecuteCommand += Remove;
    }

    private void CurrentPet_ValueChanged(
        ObservableValue<PetModel> sender,
        ValueChangedEventArgs<PetModel> e
    )
    {
        //ShowMoves.Value = e.NewValue.Moves;
    }

    private void Search_ValueChanged(
        ObservableValue<string> sender,
        ValueChangedEventArgs<string> e
    )
    {
        if (string.IsNullOrWhiteSpace(e.NewValue))
        {
            ShowMoves = Moves;
        }
        else
        {
            ShowMoves = new(
                Moves.Where(m => m.Graph.Contains(e.NewValue, StringComparison.OrdinalIgnoreCase))
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
        Moves.Add(vm.Move);
    }

    public void Edit(MoveModel model)
    {
        var window = new MoveEditWindow();
        var vm = window.ViewModel;
        vm.CurrentPet = CurrentPet.Value;
        vm.OldMove = model;
        var newMove = vm.Move = new(model);
        window.ShowDialog();
        if (window.IsCancel)
            return;
        Moves[Moves.IndexOf(model)] = newMove;
        if (ShowMoves.Count != Moves.Count)
            ShowMoves[ShowMoves.IndexOf(model)] = newMove;
    }

    private void Remove(MoveModel model)
    {
        if (MessageBox.Show("确定删除吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.No)
            return;
        if (ShowMoves.Count == Moves.Count)
        {
            Moves.Remove(model);
        }
        else
        {
            ShowMoves.Remove(model);
            Moves.Remove(model);
        }
    }
}
