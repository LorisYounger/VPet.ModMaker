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
using VPet.ModMaker.Views.ModEdit.WorkEdit;

namespace VPet.ModMaker.ViewModels.ModEdit.WorkEdit;

public class WorkPageVM
{
    #region Value
    public ObservableValue<ObservableCollection<WorkModel>> ShowWorks { get; } = new();
    public ObservableCollection<WorkModel> Works => CurrentPet.Value.Works;

    public ObservableCollection<PetModel> Pets => ModInfoModel.Current.Pets;
    public ObservableValue<PetModel> CurrentPet { get; } = new(new());
    public ObservableValue<string> Filter { get; } = new();
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
        Filter.ValueChanged += Filter_ValueChanged;

        AddCommand.ExecuteEvent += Add;
        EditCommand.ExecuteEvent += Edit;
        RemoveCommand.ExecuteEvent += Remove;
    }

    private void CurrentPet_ValueChanged(PetModel value)
    {
        ShowWorks.Value = value.Works;
    }

    private void Filter_ValueChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            ShowWorks.Value = Works;
        }
        else
        {
            ShowWorks.Value = new(
                Works.Where(m => m.Name.Value.Contains(value, StringComparison.OrdinalIgnoreCase))
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
        if (ShowWorks.Value.Count == Works.Count)
        {
            Works[Works.IndexOf(model)] = newWork;
        }
        else
        {
            Works[Works.IndexOf(model)] = newWork;
            ShowWorks.Value[ShowWorks.Value.IndexOf(model)] = newWork;
        }
    }

    private void Remove(WorkModel food)
    {
        if (MessageBox.Show("确定删除吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.No)
            return;
        if (ShowWorks.Value.Count == Works.Count)
        {
            Works.Remove(food);
        }
        else
        {
            ShowWorks.Value.Remove(food);
            Works.Remove(food);
        }
    }
}
