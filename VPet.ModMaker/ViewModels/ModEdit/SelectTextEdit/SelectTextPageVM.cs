using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HKW.HKWUtils.Observable;
using LinePutScript.Localization.WPF;
using VPet.ModMaker.Models;
using VPet.ModMaker.Views.ModEdit.SelectTextEdit;

namespace VPet.ModMaker.ViewModels.ModEdit.SelectTextEdit;

public class SelectTextPageVM : ObservableObjectX<SelectTextPageVM>
{
    #region Value
    #region ShowSelectTexts
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ObservableCollection<SelectTextModel> _showSelectTexts;

    public ObservableCollection<SelectTextModel> ShowSelectTexts
    {
        get => _showSelectTexts;
        set => SetProperty(ref _showSelectTexts, value);
    }
    #endregion
    public ObservableCollection<SelectTextModel> SelectTexts => ModInfoModel.Current.SelectTexts;

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
    public ObservableCommand<SelectTextModel> EditCommand { get; } = new();
    public ObservableCommand<SelectTextModel> RemoveCommand { get; } = new();
    #endregion

    public SelectTextPageVM()
    {
        ShowSelectTexts = SelectTexts;
        //TODO
        //Search.ValueChanged += Search_ValueChanged;
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
            ShowSelectTexts = SelectTexts;
        }
        else
        {
            ShowSelectTexts = new(
                SelectTexts.Where(m =>
                    m.Id.Contains(e.NewValue, StringComparison.OrdinalIgnoreCase)
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
        SelectTexts.Add(vm.SelectText);
    }

    public void Edit(SelectTextModel model)
    {
        var window = new SelectTextEditWindow();
        var vm = window.ViewModel;
        vm.OldSelectText = model;
        var newLowTest = vm.SelectText = new(model);
        window.ShowDialog();
        if (window.IsCancel)
            return;
        SelectTexts[SelectTexts.IndexOf(model)] = newLowTest;
        if (ShowSelectTexts.Count != SelectTexts.Count)
            ShowSelectTexts[ShowSelectTexts.IndexOf(model)] = newLowTest;
    }

    private void Remove(SelectTextModel model)
    {
        if (MessageBox.Show("确定删除吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.No)
            return;
        if (ShowSelectTexts.Count == SelectTexts.Count)
        {
            SelectTexts.Remove(model);
        }
        else
        {
            ShowSelectTexts.Remove(model);
            SelectTexts.Remove(model);
        }
    }
}
