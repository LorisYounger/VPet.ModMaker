using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HKW.HKWReactiveUI;
using HKW.HKWUtils.Collections;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using LinePutScript.Localization.WPF;
using VPet.ModMaker.Models;
using VPet.ModMaker.Views.ModEdit.LowTextEdit;

namespace VPet.ModMaker.ViewModels.ModEdit.LowTextEdit;

public partial class LowTextPageVM : ViewModelBase
{
    public LowTextPageVM()
    {
        LowTexts = new(
            ModInfoModel.Current.LowTexts,
            [],
            f => f.ID.Contains(Search, StringComparison.OrdinalIgnoreCase)
        );
        //TODO:
        //LowTexts.BindingList(ModInfoModel.Current.LowTexts);

        //AddCommand.ExecuteCommand += Add;
        //EditCommand.ExecuteCommand += Edit;
        //RemoveCommand.ExecuteCommand += Remove;
    }

    public FilterListWrapper<
        LowTextModel,
        ObservableList<LowTextModel>,
        ObservableList<LowTextModel>
    > LowTexts { get; set; } = null!;

    [ReactiveProperty]
    public string Search { get; set; } = string.Empty;

    partial void OnSearchChanged(string oldValue, string newValue)
    {
        LowTexts.Refresh();
    }

    //#region Command
    //public ObservableCommand AddCommand { get; } = new();
    //public ObservableCommand<LowTextModel> EditCommand { get; } = new();
    //public ObservableCommand<LowTextModel> RemoveCommand { get; } = new();
    //#endregion
    [ReactiveCommand]
    private void Add()
    {
        var window = new LowTextEditWindow();
        var vm = window.ViewModel;
        window.ShowDialog();
        if (window.IsCancel)
            return;
        LowTexts.Add(vm.LowText);
    }

    [ReactiveCommand]
    public void Edit(LowTextModel model)
    {
        var window = new LowTextEditWindow();
        var vm = window.ViewModel;
        vm.OldLowText = model;
        var newModel = vm.LowText = new(model)
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
        LowTexts[LowTexts.IndexOf(model)] = newModel;
        model.Close();
    }

    [ReactiveCommand]
    private void Remove(LowTextModel model)
    {
        if (MessageBox.Show("确定删除吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.No)
            return;
        LowTexts.Remove(model);
        model.Close();
    }
}
