using HKW.HKWViewModels.SimpleObservable;
using LinePutScript.Localization.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VPet.ModMaker.Models;
using VPet.ModMaker.Views.ModEdit.FoodEdit;

namespace VPet.ModMaker.ViewModels.ModEdit.FoodEdit;

public class FoodPageVM
{
    #region Value
    public ObservableValue<ObservableCollection<FoodModel>> ShowFoods { get; } = new();
    public ObservableCollection<FoodModel> Foods => ModInfoModel.Current.Foods;
    public ObservableValue<string> Search { get; } = new();
    #endregion
    #region Command
    public ObservableCommand AddCommand { get; } = new();
    public ObservableCommand<FoodModel> EditCommand { get; } = new();
    public ObservableCommand<FoodModel> RemoveCommand { get; } = new();
    #endregion
    public FoodPageVM()
    {
        ShowFoods.Value = Foods;
        Search.ValueChanged += Search_ValueChanged;

        AddCommand.ExecuteEvent += Add;
        EditCommand.ExecuteEvent += Edit;
        RemoveCommand.ExecuteEvent += Remove;
    }

    private void Search_ValueChanged(string oldValue, string newValue)
    {
        if (string.IsNullOrWhiteSpace(newValue))
        {
            ShowFoods.Value = Foods;
        }
        else
        {
            ShowFoods.Value = new(
                Foods.Where(m => m.Id.Value.Contains(newValue, StringComparison.OrdinalIgnoreCase))
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
        Foods.Add(vm.Food.Value);
    }

    public void Edit(FoodModel food)
    {
        var window = new FoodEditWindow();
        var vm = window.ViewModel;
        vm.OldFood = food;
        var newFood = vm.Food.Value = new(food);
        window.ShowDialog();
        if (window.IsCancel)
            return;
        Foods[Foods.IndexOf(food)] = newFood;
        if (ShowFoods.Value.Count != Foods.Count)
            ShowFoods.Value[ShowFoods.Value.IndexOf(food)] = newFood;
        food.Close();
    }

    private void Remove(FoodModel food)
    {
        if (MessageBox.Show("确定删除吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.No)
            return;
        if (ShowFoods.Value.Count == Foods.Count)
        {
            Foods.Remove(food);
        }
        else
        {
            ShowFoods.Value.Remove(food);
            Foods.Remove(food);
        }
    }
}
