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

public class FoodPageVM : ObservableObjectX<FoodPageVM>
{
    #region Value
    #region ShowFoods
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ObservableCollection<FoodModel> _showFoods;

    public ObservableCollection<FoodModel> ShowFoods
    {
        get => _showFoods;
        set => SetProperty(ref _showFoods, value);
    }
    #endregion
    public ObservableCollection<FoodModel> Foods => ModInfoModel.Current.Foods;

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
    public ObservableCommand<FoodModel> EditCommand { get; } = new();
    public ObservableCommand<FoodModel> RemoveCommand { get; } = new();
    #endregion
    public FoodPageVM()
    {
        ShowFoods = Foods;
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
            ShowFoods = Foods;
        }
        else
        {
            ShowFoods = new(
                Foods.Where(m => m.Id.Contains(e.NewValue, StringComparison.OrdinalIgnoreCase))
            );
        }
    }

    public void Close() { }

    private void Add()
    {
        var window = new FoodEditWindow();
        var vm = window.ViewModel;
        window.ShowDialog();
        if (window.IsCancel)
            return;
        Foods.Add(vm.Food);
    }

    public void Edit(FoodModel food)
    {
        var window = new FoodEditWindow();
        var vm = window.ViewModel;
        vm.OldFood = food;
        var newFood = vm.Food = new(food);
        window.ShowDialog();
        if (window.IsCancel)
            return;
        Foods[Foods.IndexOf(food)] = newFood;
        if (ShowFoods.Count != Foods.Count)
            ShowFoods[ShowFoods.IndexOf(food)] = newFood;
        food.Close();
    }

    private void Remove(FoodModel food)
    {
        if (MessageBox.Show("确定删除吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.No)
            return;
        if (ShowFoods.Count == Foods.Count)
        {
            Foods.Remove(food);
        }
        else
        {
            ShowFoods.Remove(food);
            Foods.Remove(food);
        }
    }
}
