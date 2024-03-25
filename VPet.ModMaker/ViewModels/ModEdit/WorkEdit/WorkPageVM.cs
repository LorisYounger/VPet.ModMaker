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
using VPet.ModMaker.Views.ModEdit.WorkEdit;

namespace VPet.ModMaker.ViewModels.ModEdit.WorkEdit;

public class WorkPageVM : ObservableObjectX<WorkPageVM>
{
    public static ModInfoModel ModInfo => ModInfoModel.Current;

    #region Value
    #region ShowWorks
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ObservableCollection<WorkModel> _showWorks;

    public ObservableCollection<WorkModel> ShowWorks
    {
        get => _showWorks;
        set => SetProperty(ref _showWorks, value);
    }
    #endregion
    public ObservableCollection<WorkModel> Works => CurrentPet.Works;

    public ObservableCollection<PetModel> Pets => ModInfoModel.Current.Pets;

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
    public ObservableCommand<WorkModel> EditCommand { get; } = new();
    public ObservableCommand<WorkModel> RemoveCommand { get; } = new();
    #endregion
    public WorkPageVM()
    {
        ShowWorks = Works;
        //TODO
        //CurrentPet.ValueChanged += CurrentPet_ValueChanged;
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
        ShowWorks = e.NewValue.Works;
    }

    private void Search_ValueChanged(
        ObservableValue<string> sender,
        ValueChangedEventArgs<string> e
    )
    {
        if (string.IsNullOrWhiteSpace(e.NewValue))
        {
            ShowWorks = Works;
        }
        else
        {
            ShowWorks = new(
                Works.Where(m => m.Id.Contains(e.NewValue, StringComparison.OrdinalIgnoreCase))
            );
        }
    }

    private void Add()
    {
        var window = new WorkEditWindow();
        var vm = window.ViewModel;
        vm.CurrentPet = CurrentPet;
        window.ShowDialog();
        if (window.IsCancel)
            return;
        Works.Add(vm.Work);
    }

    public void Edit(WorkModel model)
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
        if (ShowWorks.Count != Works.Count)
            ShowWorks[ShowWorks.IndexOf(model)] = newWork;
    }

    private void Remove(WorkModel model)
    {
        if (MessageBox.Show("确定删除吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.No)
            return;
        if (ShowWorks.Count == Works.Count)
        {
            Works.Remove(model);
        }
        else
        {
            ShowWorks.Remove(model);
            Works.Remove(model);
        }
    }
}
