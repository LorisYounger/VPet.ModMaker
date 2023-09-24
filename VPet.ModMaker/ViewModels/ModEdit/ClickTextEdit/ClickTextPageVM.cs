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
using VPet.ModMaker.Views.ModEdit.ClickTextEdit;

namespace VPet.ModMaker.ViewModels.ModEdit.ClickTextEdit;

public class ClickTextPageVM
{
    #region Value
    public ObservableValue<ObservableCollection<ClickTextModel>> ShowClickTexts { get; } = new();
    public ObservableCollection<ClickTextModel> ClickTexts => ModInfoModel.Current.ClickTexts;
    public ObservableValue<string> Search { get; } = new();
    #endregion
    #region Command
    public ObservableCommand AddCommand { get; } = new();
    public ObservableCommand<ClickTextModel> EditCommand { get; } = new();
    public ObservableCommand<ClickTextModel> RemoveCommand { get; } = new();
    #endregion

    public ClickTextPageVM()
    {
        ShowClickTexts.Value = ClickTexts;
        Search.ValueChanged += Search_ValueChanged;
        AddCommand.ExecuteEvent += Add;
        EditCommand.ExecuteEvent += Edit;
        RemoveCommand.ExecuteEvent += Remove;
    }

    private void Search_ValueChanged(string oldValue, string newValue)
    {
        if (string.IsNullOrWhiteSpace(newValue))
        {
            ShowClickTexts.Value = ClickTexts;
        }
        else
        {
            ShowClickTexts.Value = new(
                ClickTexts.Where(
                    m => m.Id.Value.Contains(newValue, StringComparison.OrdinalIgnoreCase)
                )
            );
        }
    }

    private void Add()
    {
        var window = new ClickTextEditWindow();
        var vm = window.ViewModel;
        window.ShowDialog();
        if (window.IsCancel)
            return;
        ClickTexts.Add(vm.ClickText.Value);
    }

    public void Edit(ClickTextModel model)
    {
        var window = new ClickTextEditWindow();
        var vm = window.ViewModel;
        vm.OldClickText = model;
        var newLowTest = vm.ClickText.Value = new(model);
        window.ShowDialog();
        if (window.IsCancel)
            return;
        if (ShowClickTexts.Value.Count == ClickTexts.Count)
        {
            ClickTexts[ClickTexts.IndexOf(model)] = newLowTest;
        }
        else
        {
            ClickTexts[ClickTexts.IndexOf(model)] = newLowTest;
            ShowClickTexts.Value[ShowClickTexts.Value.IndexOf(model)] = newLowTest;
        }
    }

    private void Remove(ClickTextModel model)
    {
        if (MessageBox.Show("确定删除吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.No)
            return;
        if (ShowClickTexts.Value.Count == ClickTexts.Count)
        {
            ClickTexts.Remove(model);
        }
        else
        {
            ShowClickTexts.Value.Remove(model);
            ClickTexts.Remove(model);
        }
    }
}
