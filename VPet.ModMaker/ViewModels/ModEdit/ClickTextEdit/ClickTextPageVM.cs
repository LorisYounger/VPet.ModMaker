using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VPet.ModMaker.Models;
using VPet.ModMaker.Views.ModEdit.ClickTextEdit;

namespace VPet.ModMaker.ViewModels.ModEdit.ClickTextEdit;

public class ClickTextPageVM
{
    #region Value
    public ObservableValue<ObservableCollection<ClickTextModel>> ShowClickTexts { get; } = new();
    public ObservableCollection<ClickTextModel> ClickTexts => ModInfoModel.Current.ClickTexts;
    public ObservableValue<string> FilterClickText { get; } = new();
    #endregion
    #region Command
    public ObservableCommand AddClickTextCommand { get; } = new();
    public ObservableCommand<ClickTextModel> EditClickTextCommand { get; } = new();
    public ObservableCommand<ClickTextModel> RemoveClickTextCommand { get; } = new();
    #endregion

    public ClickTextPageVM()
    {
        ShowClickTexts.Value = ClickTexts;
        FilterClickText.ValueChanged += FilterClickText_ValueChanged;
        AddClickTextCommand.ExecuteAction = AddClickText;
        EditClickTextCommand.ExecuteAction = EditClickText;
        RemoveClickTextCommand.ExecuteAction = RemoveClickText;
    }

    private void FilterClickText_ValueChanged(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            ShowClickTexts.Value = ClickTexts;
        }
        else
        {
            ShowClickTexts.Value = new(
                ClickTexts.Where(f => f.CurrentI18nData.Value.Text.Value.Contains(value))
            );
        }
    }

    private void AddClickText()
    {
        var window = new ClickTextEditWindow();
        var vm = window.ViewModel;
        window.ShowDialog();
        if (window.IsCancel)
            return;
        ClickTexts.Add(vm.ClickText.Value);
    }

    public void EditClickText(ClickTextModel clickText)
    {
        var window = new ClickTextEditWindow();
        var vm = window.ViewModel;
        vm.OldClickText = clickText;
        var newLowTest = vm.ClickText.Value = new(clickText);
        window.ShowDialog();
        if (window.IsCancel)
            return;
        if (ShowClickTexts.Value.Count == ClickTexts.Count)
        {
            ClickTexts[ClickTexts.IndexOf(clickText)] = newLowTest;
        }
        else
        {
            ClickTexts[ClickTexts.IndexOf(clickText)] = newLowTest;
            ShowClickTexts.Value[ShowClickTexts.Value.IndexOf(clickText)] = newLowTest;
        }
    }

    private void RemoveClickText(ClickTextModel clickText)
    {
        if (MessageBox.Show("确定删除吗", "", MessageBoxButton.YesNo) is MessageBoxResult.No)
            return;
        if (ShowClickTexts.Value.Count == ClickTexts.Count)
        {
            ClickTexts.Remove(clickText);
        }
        else
        {
            ShowClickTexts.Value.Remove(clickText);
            ClickTexts.Remove(clickText);
        }
    }
}
