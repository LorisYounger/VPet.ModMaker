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
using Expression = System.Linq.Expressions.Expression;

namespace VPet.ModMaker.ViewModels.ModEdit.LowTextEdit;

public class LowTextPageVM : ObservableObjectX<LowTextPageVM>
{
    #region Value
    #region ShowLowTexts
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ObservableCollection<LowTextModel> _showLowTexts;

    public ObservableCollection<LowTextModel> ShowLowTexts
    {
        get => _showLowTexts;
        set => SetProperty(ref _showLowTexts, value);
    }
    #endregion
    public ObservableCollection<LowTextModel> LowTexts => ModInfoModel.Current.LowTexts;

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
    public ObservableCommand<LowTextModel> EditCommand { get; } = new();
    public ObservableCommand<LowTextModel> RemoveCommand { get; } = new();
    #endregion

    public LowTextPageVM()
    {
        ShowLowTexts = LowTexts;
        //Search.ValueChanged += Search_ValueChanged;//TODO
        AddCommand.ExecuteCommand += Add;
        EditCommand.ExecuteCommand += Edit;
        RemoveCommand.ExecuteCommand += Remove;
    }

    private void Search_ValueChanged(
        ObservableValue<string> sender,
        ValueChangedEventArgs<string> e
    )
    {
        if (string.IsNullOrWhiteSpace(e.NewValue))
        {
            ShowLowTexts = LowTexts;
        }
        else
        {
            ShowLowTexts = new(
                LowTexts.Where(m => m.Id.Contains(e.NewValue, StringComparison.OrdinalIgnoreCase))
            );
        }
    }

    private void Add()
    {
        var window = new LowTextEditWindow();
        var vm = window.ViewModel;
        window.ShowDialog();
        if (window.IsCancel)
            return;
        LowTexts.Add(vm.LowText);
    }

    public void Edit(LowTextModel model)
    {
        var window = new LowTextEditWindow();
        var vm = window.ViewModel;
        vm.OldLowText = model;
        var newLowTest = vm.LowText = new(model);
        window.ShowDialog();
        if (window.IsCancel)
            return;
        LowTexts[LowTexts.IndexOf(model)] = newLowTest;
        if (ShowLowTexts.Count != LowTexts.Count)
            ShowLowTexts[ShowLowTexts.IndexOf(model)] = newLowTest;
    }

    private void Remove(LowTextModel model)
    {
        if (MessageBox.Show("确定删除吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.No)
            return;
        if (ShowLowTexts.Count == LowTexts.Count)
        {
            LowTexts.Remove(model);
        }
        else
        {
            ShowLowTexts.Remove(model);
            LowTexts.Remove(model);
        }
    }
}
