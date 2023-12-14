using HKW.HKWUtils.Observable;

using LinePutScript.Localization.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VPet.ModMaker.Models;
using VPet.ModMaker.Views.ModEdit.WorkEdit;

namespace VPet.ModMaker.ViewModels.ModEdit.WorkEdit;

public class WorkPageVM
{
    public static ModInfoModel ModInfo => ModInfoModel.Current;
    #region Value
    public ObservableValue<ObservableCollection<WorkModel>> ShowWorks { get; } = new();
    public ObservableCollection<WorkModel> Works => CurrentPet.Value.Works;

    public ObservableCollection<PetModel> Pets => ModInfoModel.Current.Pets;
    public ObservableValue<PetModel> CurrentPet { get; } = new(new());
    public ObservableValue<string> Search { get; } = new();
    #endregion
    #region Command
    public ObservableCommand AddCommand { get; } = new();
    public ObservableCommand<WorkModel> EditCommand { get; } = new();
    public ObservableCommand<WorkModel> RemoveCommand { get; } = new();
    #endregion
    public WorkPageVM()
    {
        ShowWorks.Value = Works;
        CurrentPet.ValueChanged += CurrentPet_ValueChanged;
        Search.ValueChanged += Search_ValueChanged;

        AddCommand.ExecuteCommand += Add;
        EditCommand.ExecuteCommand += Edit;
        RemoveCommand.ExecuteCommand += Remove;
    }

    private void CurrentPet_ValueChanged(
        ObservableValue<PetModel> sender,
        ValueChangedEventArgs<PetModel> e
    )
    {
        ShowWorks.Value = e.NewValue.Works;
    }

    private void Search_ValueChanged(
        ObservableValue<string> sender,
        ValueChangedEventArgs<string> e
    )
    {
        if (string.IsNullOrWhiteSpace(e.NewValue))
        {
            ShowWorks.Value = Works;
        }
        else
        {
            ShowWorks.Value = new(
                Works.Where(
                    m => m.Id.Value.Contains(e.NewValue, StringComparison.OrdinalIgnoreCase)
                )
            );
        }
    }

    public void Close() { }

    private void Add()
    {
        var window = new WorkEditWindow();
        var vm = window.ViewModel;
        vm.CurrentPet = CurrentPet.Value;
        window.ShowDialog();
        if (window.IsCancel)
            return;
        Works.Add(vm.Work.Value);
    }

    public void Edit(WorkModel model)
    {
        var window = new WorkEditWindow();
        var vm = window.ViewModel;
        vm.CurrentPet = CurrentPet.Value;
        vm.OldWork = model;
        var newWork = vm.Work.Value = new(model);
        window.ShowDialog();
        if (window.IsCancel)
            return;
        Works[Works.IndexOf(model)] = newWork;
        if (ShowWorks.Value.Count != Works.Count)
            ShowWorks.Value[ShowWorks.Value.IndexOf(model)] = newWork;
    }

    private void Remove(WorkModel model)
    {
        if (MessageBox.Show("确定删除吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.No)
            return;
        if (ShowWorks.Value.Count == Works.Count)
        {
            Works.Remove(model);
        }
        else
        {
            ShowWorks.Value.Remove(model);
            Works.Remove(model);
        }
    }
}
