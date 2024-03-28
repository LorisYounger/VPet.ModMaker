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
using HKW.HKWUtils.Observable;
using LinePutScript.Localization.WPF;
using VPet.ModMaker.Models;
using VPet.ModMaker.Views.ModEdit.LowTextEdit;

namespace VPet.ModMaker.ViewModels.ModEdit.LowTextEdit;

public class LowTextPageVM : ObservableObjectX<LowTextPageVM>
{
    public LowTextPageVM()
    {
        LowTexts = new(ModInfoModel.Current.LowTexts)
        {
            Filter = f => f.ID.Contains(Search, StringComparison.OrdinalIgnoreCase),
            FilteredList = new()
        };

        AddCommand.ExecuteCommand += AddCommand_ExecuteCommand;
        EditCommand.ExecuteCommand += EditCommand_ExecuteCommand;
        RemoveCommand.ExecuteCommand += RemoveCommand_ExecuteCommand;
    }

    #region Property
    #region LowTexts
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ObservableFilterList<LowTextModel, ObservableList<LowTextModel>> _lowTexts = null!;

    public ObservableFilterList<LowTextModel, ObservableList<LowTextModel>> LowTexts
    {
        get => _lowTexts;
        set => SetProperty(ref _lowTexts, value);
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
            LowTexts.Refresh();
        }
    }
    #endregion
    #endregion
    #region Command
    public ObservableCommand AddCommand { get; } = new();
    public ObservableCommand<LowTextModel> EditCommand { get; } = new();
    public ObservableCommand<LowTextModel> RemoveCommand { get; } = new();
    #endregion

    private void AddCommand_ExecuteCommand()
    {
        var window = new LowTextEditWindow();
        var vm = window.ViewModel;
        window.ShowDialog();
        if (window.IsCancel)
            return;
        LowTexts.Add(vm.LowText);
    }

    public void EditCommand_ExecuteCommand(LowTextModel model)
    {
        var window = new LowTextEditWindow();
        var vm = window.ViewModel;
        vm.OldLowText = model;
        var newLowTest = vm.LowText = new(model);
        window.ShowDialog();
        if (window.IsCancel)
            return;
        LowTexts[LowTexts.IndexOf(model)] = newLowTest;
    }

    private void RemoveCommand_ExecuteCommand(LowTextModel model)
    {
        if (MessageBox.Show("确定删除吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.No)
            return;
        LowTexts.Remove(model);
    }
}
