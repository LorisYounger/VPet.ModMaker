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
        PropertyChangedX += WorkPageVM_PropertyChangedX;

        if (Pets.HasValue())
            CurrentPet = Pets.FirstOrDefault(
                m => m.FromMain is false && m.Works.HasValue(),
                Pets.First()
            );
        AddCommand.ExecuteCommand += AddCommand_ExecuteCommand;
        EditCommand.ExecuteCommand += EditCommand_ExecuteCommand;
        RemoveCommand.ExecuteCommand += RemoveCommand_ExecuteCommand;
        ModInfo.PropertyChangedX += ModInfo_PropertyChangedX;
    }

    private void ModInfo_PropertyChangedX(object? sender, PropertyChangedXEventArgs e)
    {
        if (e.PropertyName == nameof(ModInfoModel.ShowMainPet))
        {
            if (e.NewValue is false)
            {
                if (CurrentPet?.FromMain is false)
                {
                    CurrentPet = null!;
                }
            }
        }
    }

    public static ModInfoModel ModInfo => ModInfoModel.Current;

    #region Property
    #region Works
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ObservableFilterList<WorkModel, ObservableList<WorkModel>> _works = null!;

    public ObservableFilterList<WorkModel, ObservableList<WorkModel>> Works
    {
        get => _works;
        set => SetProperty(ref _works, value);
    }
    #endregion

    public static ObservableList<PetModel> Pets => ModInfoModel.Current.Pets;

    #region CurrentPet
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private PetModel _currentPet = null!;

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

    private void WorkPageVM_PropertyChangedX(object? sender, PropertyChangedXEventArgs e)
    {
        if (e.PropertyName == nameof(CurrentPet))
        {
            Works.Clear();
            if (e.OldValue is PetModel pet)
                Works.BindingList(pet.Works, true);
            if (e.NewValue is null)
                return;
            Works.AddRange(CurrentPet.Works);
            Works.BindingList(CurrentPet.Works);
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
        var newModel = vm.Work = new(model)
        {
            I18nResource = ModInfoModel.Current.TempI18nResource
        };
        model.I18nResource.CopyDataTo(newModel.I18nResource, model.ID, true);
        window.ShowDialog();
        if (window.IsCancel)
        {
            newModel.I18nResource.ClearCultureData();
            newModel.Close();
            return;
        }
        newModel.I18nResource.CopyDataTo(ModInfoModel.Current.I18nResource, true);
        newModel.I18nResource = ModInfoModel.Current.I18nResource;
        Works[Works.IndexOf(model)] = newModel;
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
