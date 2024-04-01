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
using VPet.ModMaker.Views.ModEdit.WorkEdit;

namespace VPet.ModMaker.ViewModels.ModEdit.WorkEdit;

public class WorkPageVM : ObservableObjectX
{
    public WorkPageVM()
    {
        Works = new()
        {
            Filter = f => f.ID.Contains(Search, StringComparison.OrdinalIgnoreCase),
            FilteredList = new()
        };
        PropertyChanged += WorkPageVM_PropertyChanged;
        if (Pets.HasValue())
            CurrentPet = Pets.FirstOrDefault(
                m => m.FromMain is false && m.Works.HasValue(),
                Pets.First()
            );
        AddCommand.ExecuteCommand += AddCommand_ExecuteCommand;
        EditCommand.ExecuteCommand += EditCommand_ExecuteCommand;
        RemoveCommand.ExecuteCommand += RemoveCommand_ExecuteCommand;
    }

    public static ModInfoModel ModInfo => ModInfoModel.Current;

    #region Property
    #region Works
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ObservableFilterList<WorkModel, ObservableList<WorkModel>> _works;

    public ObservableFilterList<WorkModel, ObservableList<WorkModel>> Works
    {
        get => _works;
        set => SetProperty(ref _works, value);
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
    public ObservableCommand<WorkModel> EditCommand { get; } = new();
    public ObservableCommand<WorkModel> RemoveCommand { get; } = new();
    #endregion
    private void WorkPageVM_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CurrentPet))
        {
            Works.Clear();
            Works.AddRange(CurrentPet.Works);
        }
        else if (e.PropertyName == nameof(Search))
        {
            Works.Refresh();
        }
    }

    private void AddCommand_ExecuteCommand()
    {
        var window = new WorkEditWindow();
        var vm = window.ViewModel;
        vm.CurrentPet = CurrentPet;
        window.ShowDialog();
        if (window.IsCancel)
            return;
        Works.Add(vm.Work);
    }

    public void EditCommand_ExecuteCommand(WorkModel model)
    {
        var window = new WorkEditWindow();
        var vm = window.ViewModel;
        vm.CurrentPet = CurrentPet;
        vm.OldWork = model;
        var newWork = vm.Work = new(model);
        window.ShowDialog();
        if (window.IsCancel)
            return;
        Works[Works.IndexOf(model)] = newWork;
        model.Close();
    }

    private void RemoveCommand_ExecuteCommand(WorkModel model)
    {
        if (MessageBox.Show("确定删除吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.No)
            return;
        Works.Remove(model);
        model.Close();
    }
}
