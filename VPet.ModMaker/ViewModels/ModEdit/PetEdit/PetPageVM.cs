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
using VPet.ModMaker.Views.ModEdit.I18nEdit;
using VPet.ModMaker.Views.ModEdit.PetEdit;

namespace VPet.ModMaker.ViewModels.ModEdit.PetEdit;

public class PetPageVM : ObservableObjectX<PetPageVM>
{
    public static ModInfoModel ModInfo => ModInfoModel.Current;

    #region Value
    #region ShowPets
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ObservableCollection<PetModel> _showPets;

    public ObservableCollection<PetModel> ShowPets
    {
        get => _showPets;
        set => SetProperty(ref _showPets, value);
    }
    #endregion
    public ObservableCollection<PetModel> Pets => ModInfoModel.Current.Pets;

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
    public ObservableCommand<PetModel> EditCommand { get; } = new();
    public ObservableCommand<PetModel> RemoveCommand { get; } = new();
    #endregion
    public PetPageVM()
    {
        //TODO
        //ShowPets = Pets;
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
            ShowPets = Pets;
        }
        else
        {
            ShowPets = new(
                Pets.Where(m => m.ID.Contains(e.NewValue, StringComparison.OrdinalIgnoreCase))
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
        Pets.Add(vm.Pet);
    }

    public void Edit(PetModel model)
    {
        if (model.FromMain)
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
        var newPet = vm.Pet = new(model);
        window.ShowDialog();
        if (window.IsCancel)
            return;
        if (model.FromMain)
        {
            var index = Pets.IndexOf(model);
            Pets.Remove(model);
            Pets.Insert(index, newPet);
        }
        else
        {
            Pets[Pets.IndexOf(model)] = newPet;
        }
        if (ShowPets.Count != Pets.Count)
            ShowPets[ShowPets.IndexOf(model)] = newPet;
        model.Close();
    }

    private void Remove(PetModel model)
    {
        if (model.FromMain)
        {
            MessageBox.Show("这是本体自带的宠物, 无法删除".Translate());
            return;
        }
        if (MessageBox.Show("确定删除吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.No)
            return;
        if (ShowPets.Count == Pets.Count)
        {
            Pets.Remove(model);
        }
        else
        {
            ShowPets.Remove(model);
            Pets.Remove(model);
        }
    }
}
