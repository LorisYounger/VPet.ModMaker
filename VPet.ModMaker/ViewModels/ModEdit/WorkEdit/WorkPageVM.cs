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
using VPet.ModMaker.Views.ModEdit.WorkEdit;

namespace VPet.ModMaker.ViewModels.ModEdit.WorkEdit;

public partial class WorkPageVM : ViewModelBase
{
    public WorkPageVM()
    {
        Works = new([], [], f => f.ID.Contains(Search, StringComparison.OrdinalIgnoreCase));
        //PropertyChangedX += WorkPageVM_PropertyChangedX;

        if (Pets.HasValue())
            CurrentPet = Pets.FirstOrDefault(
                m => m.FromMain is false && m.Works.HasValue(),
                Pets.First()
            );
        //AddCommand.ExecuteCommand += Add;
        //EditCommand.ExecuteCommand += Edit;
        //RemoveCommand.ExecuteCommand += Remove;
        //ModInfo.PropertyChangedX += ModInfo_PropertyChangedX;
    }

    //private void ModInfo_PropertyChangedX(object? sender, PropertyChangedXEventArgs e)
    //{
    //    if (e.PropertyName == nameof(ModInfoModel.ShowMainPet))
    //    {
    //        if (e.NewValue is false)
    //        {
    //            if (CurrentPet?.FromMain is false)
    //            {
    //                CurrentPet = null!;
    //            }
    //        }
    //    }
    //}

    public static ModInfoModel ModInfo => ModInfoModel.Current;

    #region Property

    public FilterListWrapper<
        WorkModel,
        List<WorkModel>,
        ObservableList<WorkModel>
    > Works { get; set; }

    public static ObservableList<PetModel> Pets => ModInfoModel.Current.Pets;

    [ReactiveProperty]
    public PetModel CurrentPet { get; set; } = null!;

    partial void OnCurrentPetChanged(PetModel oldValue, PetModel newValue)
    {
        //TODO:
        Works.Clear();
        //if (e.OldValue is PetModel pet)
        //    Works.BindingList(pet.Works, true);
        //if (e.NewValue is null)
        //    return;
        //Works.AddRange(CurrentPet.Works);
        //Works.BindingList(CurrentPet.Works);
    }

    [ReactiveProperty]
    public string Search { get; set; } = string.Empty;

    partial void OnSearchChanged(string oldValue, string newValue)
    {
        Works.Refresh();
    }
    #endregion
    //#region Command
    //public ObservableCommand AddCommand { get; } = new();
    //public ObservableCommand<WorkModel> EditCommand { get; } = new();
    //public ObservableCommand<WorkModel> RemoveCommand { get; } = new();
    //#endregion
    [ReactiveCommand]
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

    [ReactiveCommand]
    public void Edit(WorkModel model)
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

    [ReactiveCommand]
    private void Remove(WorkModel model)
    {
        if (MessageBox.Show("确定删除吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.No)
            return;
        Works.Remove(model);
        model.Close();
    }
}
