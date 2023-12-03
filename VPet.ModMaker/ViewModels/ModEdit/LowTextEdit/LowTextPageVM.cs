using HKW.HKWUtils.Observable;
using HKW.Models;
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
    public ObservableValue<string> Search { get; } = new();
    #endregion
    #region Command
    public ObservableCommand AddCommand { get; } = new();
    public ObservableCommand<LowTextModel> EditCommand { get; } = new();
    public ObservableCommand<LowTextModel> RemoveCommand { get; } = new();
    #endregion

    public LowTextPageVM()
    {
        ShowLowTexts.Value = LowTexts;
        Search.ValueChanged += Search_ValueChanged;
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
            ShowLowTexts.Value = LowTexts;
        }
        else
        {
            ShowLowTexts.Value = new(
                LowTexts.Where(
                    m => m.Id.Value.Contains(e.NewValue, StringComparison.OrdinalIgnoreCase)
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
        LowTexts[LowTexts.IndexOf(model)] = newLowTest;
        if (ShowLowTexts.Value.Count != LowTexts.Count)
            ShowLowTexts.Value[ShowLowTexts.Value.IndexOf(model)] = newLowTest;
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
