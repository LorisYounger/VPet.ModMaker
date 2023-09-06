using HKW.HKWViewModels.SimpleObservable;
using LinePutScript.Localization.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VPet.ModMaker.Models;
using VPet.ModMaker.Views.ModEdit.SelectTextEdit;

namespace VPet.ModMaker.ViewModels.ModEdit.SelectTextEdit;

public class SelectTextPageVM
{
    #region Value
    public ObservableValue<ObservableCollection<SelectTextModel>> ShowSelectTexts { get; } = new();
    public ObservableCollection<SelectTextModel> SelectTexts => ModInfoModel.Current.SelectTexts;
    public ObservableValue<string> FilterSelectText { get; } = new();
    #endregion
    #region Command
    public ObservableCommand AddSelectTextCommand { get; } = new();
    public ObservableCommand<SelectTextModel> EditSelectTextCommand { get; } = new();
    public ObservableCommand<SelectTextModel> RemoveSelectTextCommand { get; } = new();
    #endregion

    public SelectTextPageVM()
    {
        ShowSelectTexts.Value = SelectTexts;
        FilterSelectText.ValueChanged += FilterSelectText_ValueChanged;
        AddSelectTextCommand.ExecuteEvent += AddSelectText;
        EditSelectTextCommand.ExecuteEvent += EditSelectText;
        RemoveSelectTextCommand.ExecuteEvent += RemoveSelectText;
    }

    private void FilterSelectText_ValueChanged(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            ShowSelectTexts.Value = SelectTexts;
        }
        else
        {
            ShowSelectTexts.Value = new(
                SelectTexts.Where(f => f.CurrentI18nData.Value.Text.Value.Contains(value))
            );
        }
    }

    private void AddSelectText()
    {
        var window = new SelectTextEditWindow();
        var vm = window.ViewModel;
        window.ShowDialog();
        if (window.IsCancel)
            return;
        SelectTexts.Add(vm.SelectText.Value);
    }

    public void EditSelectText(SelectTextModel model)
    {
        var window = new SelectTextEditWindow();
        var vm = window.ViewModel;
        vm.OldSelectText = model;
        var newLowTest = vm.SelectText.Value = new(model);
        window.ShowDialog();
        if (window.IsCancel)
            return;
        if (ShowSelectTexts.Value.Count == SelectTexts.Count)
        {
            SelectTexts[SelectTexts.IndexOf(model)] = newLowTest;
        }
        else
        {
            SelectTexts[SelectTexts.IndexOf(model)] = newLowTest;
            ShowSelectTexts.Value[ShowSelectTexts.Value.IndexOf(model)] = newLowTest;
        }
    }

    private void RemoveSelectText(SelectTextModel model)
    {
        if (MessageBox.Show("确定删除吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.No)
            return;
        if (ShowSelectTexts.Value.Count == SelectTexts.Count)
        {
            SelectTexts.Remove(model);
        }
        else
        {
            ShowSelectTexts.Value.Remove(model);
            SelectTexts.Remove(model);
        }
    }
}
