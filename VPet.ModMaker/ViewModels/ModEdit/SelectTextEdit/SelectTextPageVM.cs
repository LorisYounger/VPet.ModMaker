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
    public ObservableValue<string> Search { get; } = new();
    #endregion
    #region Command
    public ObservableCommand AddCommand { get; } = new();
    public ObservableCommand<SelectTextModel> EditCommand { get; } = new();
    public ObservableCommand<SelectTextModel> RemoveCommand { get; } = new();
    #endregion

    public SelectTextPageVM()
    {
        ShowSelectTexts.Value = SelectTexts;
        Search.ValueChanged += Search_ValueChanged;
        AddCommand.ExecuteEvent += Add;
        EditCommand.ExecuteEvent += Edit;
        RemoveCommand.ExecuteEvent += Remove;
    }

    private void Search_ValueChanged(string oldValue, string newValue)
    {
        if (string.IsNullOrWhiteSpace(newValue))
        {
            ShowSelectTexts.Value = SelectTexts;
        }
        else
        {
            ShowSelectTexts.Value = new(
                SelectTexts.Where(
                    m => m.Id.Value.Contains(newValue, StringComparison.OrdinalIgnoreCase)
                )
            );
        }
    }

    private void Add()
    {
        var window = new SelectTextEditWindow();
        var vm = window.ViewModel;
        window.ShowDialog();
        if (window.IsCancel)
            return;
        SelectTexts.Add(vm.SelectText.Value);
    }

    public void Edit(SelectTextModel model)
    {
        var window = new SelectTextEditWindow();
        var vm = window.ViewModel;
        vm.OldSelectText = model;
        var newLowTest = vm.SelectText.Value = new(model);
        window.ShowDialog();
        if (window.IsCancel)
            return;
        SelectTexts[SelectTexts.IndexOf(model)] = newLowTest;
        if (ShowSelectTexts.Value.Count != SelectTexts.Count)
            ShowSelectTexts.Value[ShowSelectTexts.Value.IndexOf(model)] = newLowTest;
    }

    private void Remove(SelectTextModel model)
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
