using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using LinePutScript.Localization.WPF;
using VPet.ModMaker.Models;
using VPet.ModMaker.Views.ModEdit.MoveEdit;

namespace VPet.ModMaker.ViewModels.ModEdit.MoveEdit;

public class MovePageVM : ObservableObjectX<MovePageVM>
{
    public MovePageVM()
    {
        Moves = new()
        {
            Filter = f => f.Graph.Contains(Search, StringComparison.OrdinalIgnoreCase),
            FilteredList = new()
        };
        CurrentPet = Pets.First();
        PropertyChanged += MovePageVM_PropertyChanged;
        AddCommand.ExecuteCommand += AddCommand_ExecuteCommand;
        EditCommand.ExecuteCommand += EditCommand_ExecuteCommand;
        RemoveCommand.ExecuteCommand += RemoveCommand_ExecuteCommand;
    }

    public static ModInfoModel ModInfo => ModInfoModel.Current;

    #region Property
    #region ShowMoves
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ObservableFilterList<MoveModel, ObservableList<MoveModel>> _moves;

    public ObservableFilterList<MoveModel, ObservableList<MoveModel>> Moves
    {
        get => _moves;
        set => SetProperty(ref _moves, value);
    }
    #endregion

    public static ObservableList<PetModel> Pets => ModInfoModel.Current.Pets;

    #region CurrentPet
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private PetModel _currentPet;

    public PetModel CurrentPet
    {
        get => _currentPet;
        set => SetProperty(ref _currentPet, value);
    }
    #endregion

    #region Search
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _search = string.Empty;

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
    private void MovePageVM_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CurrentPet))
        {
            Moves.Clear();
            Moves.AddRange(CurrentPet.Moves);
        }
        else if (e.PropertyName == nameof(Search))
        {
            Moves.Refresh();
        }
    }

    public void Close() { }

    private void AddCommand_ExecuteCommand()
    {
        var window = new MoveEditWindow();
        var vm = window.ViewModel;
        vm.CurrentPet = CurrentPet;
        window.ShowDialog();
        if (window.IsCancel)
            return;
        Moves.Add(vm.Move);
    }

    public void EditCommand_ExecuteCommand(MoveModel model)
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

    private void RemoveCommand_ExecuteCommand(MoveModel model)
    {
        if (MessageBox.Show("确定删除吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.No)
            return;
        Moves.Remove(model);
    }
}
