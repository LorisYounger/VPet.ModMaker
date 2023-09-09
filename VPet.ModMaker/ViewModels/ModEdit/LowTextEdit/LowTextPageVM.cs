using HKW.HKWViewModels.SimpleObservable;
using LinePutScript.Localization.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VPet.ModMaker.Models;
using VPet.ModMaker.Views.ModEdit.LowTextEdit;
using Expression = System.Linq.Expressions.Expression;

namespace VPet.ModMaker.ViewModels.ModEdit.LowTextEdit;

public class LowTextPageVM
{
    #region Value
    public ObservableValue<ObservableCollection<LowTextModel>> ShowLowTexts { get; } = new();
    public ObservableCollection<LowTextModel> LowTexts => ModInfoModel.Current.LowTexts;
    public ObservableValue<string> Filter { get; } = new();
    #endregion
    #region Command
    public ObservableCommand AddCommand { get; } = new();
    public ObservableCommand<LowTextModel> EditCommand { get; } = new();
    public ObservableCommand<LowTextModel> RemoveCommand { get; } = new();
    #endregion

    public LowTextPageVM()
    {
        ShowLowTexts.Value = LowTexts;
        Filter.ValueChanged += Filter_ValueChanged;
        AddCommand.ExecuteEvent += Add;
        EditCommand.ExecuteEvent += Edit;
        RemoveCommand.ExecuteEvent += Remove;
    }

    private void Filter_ValueChanged(string oldValue, string newValue)
    {
        if (string.IsNullOrWhiteSpace(newValue))
        {
            ShowLowTexts.Value = LowTexts;
        }
        else
        {
            ShowLowTexts.Value = new(
                LowTexts.Where(
                    m => m.Id.Value.Contains(newValue, StringComparison.OrdinalIgnoreCase)
                )
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
        LowTexts.Add(vm.LowText.Value);
    }

    public void Edit(LowTextModel model)
    {
        var window = new LowTextEditWindow();
        var vm = window.ViewModel;
        vm.OldLowText = model;
        var newLowTest = vm.LowText.Value = new(model);
        window.ShowDialog();
        if (window.IsCancel)
            return;
        if (ShowLowTexts.Value.Count == LowTexts.Count)
        {
            LowTexts[LowTexts.IndexOf(model)] = newLowTest;
        }
        else
        {
            LowTexts[LowTexts.IndexOf(model)] = newLowTest;
            ShowLowTexts.Value[ShowLowTexts.Value.IndexOf(model)] = newLowTest;
        }
    }

    private void Remove(LowTextModel model)
    {
        if (MessageBox.Show("确定删除吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.No)
            return;
        if (ShowLowTexts.Value.Count == LowTexts.Count)
        {
            LowTexts.Remove(model);
        }
        else
        {
            ShowLowTexts.Value.Remove(model);
            LowTexts.Remove(model);
        }
    }
}
