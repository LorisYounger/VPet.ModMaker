using HKW.HKWViewModels.SimpleObservable;
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
    public ObservableValue<string> FilterLowText { get; } = new();
    public ObservableValue<ObservableCollection<LowTextModel>> ShowLowTexts { get; } = new();
    public ObservableCollection<LowTextModel> LowTexts => ModInfoModel.Current.LowTexts;
    #endregion
    #region Command
    public ObservableCommand AddLowTextCommand { get; } = new();
    public ObservableCommand<LowTextModel> EditLowTextCommand { get; } = new();
    public ObservableCommand<LowTextModel> RemoveLowTextCommand { get; } = new();
    #endregion

    public LowTextPageVM()
    {
        ShowLowTexts.Value = LowTexts;
        FilterLowText.ValueChanged += FilterLowText_ValueChanged;
        AddLowTextCommand.ExecuteAction = AddLowText;
        EditLowTextCommand.ExecuteAction = EditLowText;
        RemoveLowTextCommand.ExecuteAction = RemoveLowText;
    }

    private void FilterLowText_ValueChanged(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            ShowLowTexts.Value = LowTexts;
        }
        else
        {
            ShowLowTexts.Value = new(
                LowTexts.Where(f => f.CurrentI18nData.Value.Text.Value.Contains(value))
            );
        }
    }

    private void AddLowText()
    {
        var window = new LowTextEditWindow();
        var vm = window.ViewModel;
        window.ShowDialog();
        if (window.IsCancel)
            return;
        LowTexts.Add(vm.LowText.Value);
    }

    public void EditLowText(LowTextModel lowText)
    {
        var window = new LowTextEditWindow();
        var vm = window.ViewModel;
        vm.OldLowText = lowText;
        var newLowTest = vm.LowText.Value = new(lowText);
        window.ShowDialog();
        if (window.IsCancel)
            return;
        if (ShowLowTexts.Value.Count == LowTexts.Count)
        {
            LowTexts[LowTexts.IndexOf(lowText)] = newLowTest;
        }
        else
        {
            LowTexts[LowTexts.IndexOf(lowText)] = newLowTest;
            ShowLowTexts.Value[ShowLowTexts.Value.IndexOf(lowText)] = newLowTest;
        }
    }

    private void RemoveLowText(LowTextModel lowText)
    {
        if (MessageBox.Show("确定删除吗", "", MessageBoxButton.YesNo) is MessageBoxResult.No)
            return;
        if (ShowLowTexts.Value.Count == LowTexts.Count)
        {
            LowTexts.Remove(lowText);
        }
        else
        {
            ShowLowTexts.Value.Remove(lowText);
            LowTexts.Remove(lowText);
        }
    }
}
