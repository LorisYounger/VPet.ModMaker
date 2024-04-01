using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HKW.HKWUtils.Observable;
using LinePutScript.Localization.WPF;
using VPet.ModMaker.Models;
using VPet.ModMaker.Views.ModEdit.FoodEdit;

namespace VPet.ModMaker.ViewModels.ModEdit.FoodEdit;

public class FoodPageVM : ObservableObjectX
{
    public FoodPageVM()
    {
        Foods = new(ModInfoModel.Current.Foods)
        {
            Filter = f => f.ID.Contains(Search, StringComparison.OrdinalIgnoreCase),
            FilteredList = new()
        };

        AddCommand.ExecuteCommand += AddCommand_ExecuteCommand;
        EditCommand.ExecuteCommand += EditCommand_ExecuteCommand;
        RemoveCommand.ExecuteCommand += RemoveCommand_ExecuteCommand;
    }

    #region Property

    #region Foods
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ObservableFilterList<FoodModel, ObservableList<FoodModel>> _foods = null!;

    public ObservableFilterList<FoodModel, ObservableList<FoodModel>> Foods
    {
        get => _foods;
        set => SetProperty(ref _foods, value);
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
            if (SetProperty(ref _search, value))
            {
                Foods.Refresh();
            }
        }
    }
    #endregion
    #endregion
    #region Command
    public ObservableCommand AddCommand { get; } = new();
    public ObservableCommand<FoodModel> EditCommand { get; } = new();
    public ObservableCommand<FoodModel> RemoveCommand { get; } = new();
    #endregion

    private void AddCommand_ExecuteCommand()
    {
        var window = new FoodEditWindow();
        var vm = window.ViewModel;
        window.ShowDialog();
        if (window.IsCancel)
            return;
        Foods.Add(vm.Food);
    }

    public void EditCommand_ExecuteCommand(FoodModel food)
    {
        var window = new FoodEditWindow();
        var vm = window.ViewModel;
        vm.OldFood = food;
        var newFood = vm.Food = new(food);
        window.ShowDialog();
        if (window.IsCancel)
            return;
        Foods[Foods.IndexOf(food)] = newFood;
        food.Close();
    }

    private void RemoveCommand_ExecuteCommand(FoodModel food)
    {
        if (MessageBox.Show("确定删除吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.No)
            return;
        Foods.Remove(food);
    }
}
