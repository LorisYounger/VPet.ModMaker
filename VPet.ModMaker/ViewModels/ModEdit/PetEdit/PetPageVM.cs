using HKW.HKWUtils.Observable;

using LinePutScript.Localization.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VPet.ModMaker.Models;
using VPet.ModMaker.ViewModels.ModEdit.I18nEdit;
using VPet.ModMaker.Views.ModEdit.I18nEdit;
using VPet.ModMaker.Views.ModEdit.PetEdit;

namespace VPet.ModMaker.ViewModels.ModEdit.PetEdit;

public class PetPageVM
{
    public static ModInfoModel ModInfo => ModInfoModel.Current;
    #region Value
    public ObservableValue<ObservableCollection<PetModel>> ShowPets { get; } = new();
    public ObservableCollection<PetModel> Pets => ModInfoModel.Current.Pets;
    public ObservableValue<string> Search { get; } = new();
    #endregion
    #region Command
    public ObservableCommand AddCommand { get; } = new();
    public ObservableCommand<PetModel> EditCommand { get; } = new();
    public ObservableCommand<PetModel> RemoveCommand { get; } = new();
    #endregion
    public PetPageVM()
    {
        ShowPets.Value = Pets;
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
            ShowPets.Value = Pets;
        }
        else
        {
            ShowPets.Value = new(
                Pets.Where(m => m.Id.Value.Contains(e.NewValue, StringComparison.OrdinalIgnoreCase))
            );
        }
    }

    public void Close() { }

    private void Add()
    {
        var window = new PetEditWindow();
        var vm = window.ViewModel;
        window.ShowDialog();
        if (window.IsCancel)
            return;
        Pets.Add(vm.Pet.Value);
    }

    public void Edit(PetModel model)
    {
        if (model.FromMain.Value)
        {
            if (
                MessageBox.Show("这是本体自带的宠物, 确定要编辑吗".Translate(), "", MessageBoxButton.YesNo)
                is not MessageBoxResult.Yes
            )
                return;
        }
        var window = new PetEditWindow();
        var vm = window.ViewModel;
        vm.OldPet = model;
        var newPet = vm.Pet.Value = new(model);
        window.ShowDialog();
        if (window.IsCancel)
            return;
        if (model.FromMain.Value)
        {
            var index = Pets.IndexOf(model);
            Pets.Remove(model);
            Pets.Insert(index, newPet);
        }
        else
        {
            Pets[Pets.IndexOf(model)] = newPet;
        }
        if (ShowPets.Value.Count != Pets.Count)
            ShowPets.Value[ShowPets.Value.IndexOf(model)] = newPet;
        model.Close();
    }

    private void Remove(PetModel model)
    {
        if (model.FromMain.Value)
        {
            MessageBox.Show("这是本体自带的宠物, 无法删除".Translate());
            return;
        }
        if (MessageBox.Show("确定删除吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.No)
            return;
        if (ShowPets.Value.Count == Pets.Count)
        {
            Pets.Remove(model);
        }
        else
        {
            ShowPets.Value.Remove(model);
            Pets.Remove(model);
        }
    }
}
