﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using LinePutScript.Localization.WPF;
using VPet.ModMaker.Models;
using VPet.ModMaker.Views.ModEdit.SelectTextEdit;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.ViewModels.ModEdit.SelectTextEdit;

public class SelectTextPageVM : ObservableObjectX
{
    public SelectTextPageVM()
    {
        SelectTexts = new(ModInfoModel.Current.SelectTexts)
        {
            Filter = f => f.ID.Contains(Search, StringComparison.OrdinalIgnoreCase),
            FilteredList = new()
        };
        SelectTexts.BindingList(ModInfoModel.Current.SelectTexts);

        AddCommand.ExecuteCommand += AddCommand_ExecuteCommand;
        EditCommand.ExecuteCommand += EditCommand_ExecuteCommand;
        RemoveCommand.ExecuteCommand += RemoveCommand_ExecuteCommand;
    }

    #region Property
    #region SelectTexts
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ObservableFilterList<SelectTextModel, ObservableList<SelectTextModel>> _selectTexts =
        null!;

    public ObservableFilterList<SelectTextModel, ObservableList<SelectTextModel>> SelectTexts
    {
        get => _selectTexts;
        set => SetProperty(ref _selectTexts, value);
    }
    #endregion

    #region Search
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _search = string.Empty;

    public string Search
    {
        get => _search;
        set
        {
            SetProperty(ref _search, value);
            SelectTexts.Refresh();
        }
    }
    #endregion
    #endregion
    #region Command
    public ObservableCommand AddCommand { get; } = new();
    public ObservableCommand<SelectTextModel> EditCommand { get; } = new();
    public ObservableCommand<SelectTextModel> RemoveCommand { get; } = new();
    #endregion

    private void AddCommand_ExecuteCommand()
    {
        var window = new SelectTextEditWindow();
        var vm = window.ViewModel;
        window.ShowDialog();
        if (window.IsCancel)
            return;
        SelectTexts.Add(vm.SelectText);
    }

    public void EditCommand_ExecuteCommand(SelectTextModel model)
    {
        var window = new SelectTextEditWindow();
        var vm = window.ViewModel;
        vm.OldSelectText = model;
        var newModel = vm.SelectText = new(model)
        {
            I18nResource = ModInfoModel.Current.I18nResource
        };
        model.I18nResource.CopyDataTo(newModel.I18nResource, [model.ID, model.ChooseID], true);
        window.ShowDialog();
        if (window.IsCancel)
        {
            newModel.I18nResource.ClearCultureData();
            newModel.Close();
            return;
        }
        newModel.I18nResource.CopyDataTo(ModInfoModel.Current.I18nResource, true);
        newModel.I18nResource = ModInfoModel.Current.I18nResource;
        SelectTexts[SelectTexts.IndexOf(model)] = newModel;
        model.Close();
    }

    private void RemoveCommand_ExecuteCommand(SelectTextModel model)
    {
        if (MessageBox.Show("确定删除吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.No)
            return;
        SelectTexts.Remove(model);
        model.Close();
    }
}
