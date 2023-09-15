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
using VPet.ModMaker.Models.ModModel;
using VPet.ModMaker.Views.ModEdit.AnimeEdit;

namespace VPet.ModMaker.ViewModels.ModEdit.AnimeEdit;

public class AnimePageVM
{
    //#region Value
    //public ObservableValue<ObservableCollection<AnimeModel>> ShowAnimes { get; } = new();
    //public ObservableCollection<AnimeModel> Works => CurrentPet.Value.Works;

    //public ObservableCollection<PetModel> Pets => ModInfoModel.Current.Pets;
    //public ObservableValue<PetModel> CurrentPet { get; } = new(new());
    //public ObservableValue<string> Filter { get; } = new();
    //#endregion
    //#region Command
    //public ObservableCommand AddCommand { get; } = new();
    //public ObservableCommand<AnimeModel> EditCommand { get; } = new();
    //public ObservableCommand<AnimeModel> RemoveCommand { get; } = new();
    //#endregion
    //public AnimePageVM()
    //{
    //    ShowAnimes.Value = Works;
    //    CurrentPet.ValueChanged += CurrentPet_ValueChanged;
    //    Filter.ValueChanged += Filter_ValueChanged;

    //    AddCommand.ExecuteEvent += Add;
    //    EditCommand.ExecuteEvent += Edit;
    //    RemoveCommand.ExecuteEvent += Remove;
    //}

    //private void CurrentPet_ValueChanged(PetModel oldValue, PetModel newValue)
    //{
    //    //ShowAnimes.Value = newValue.Animes;
    //}

    //private void Filter_ValueChanged(string oldValue, string newValue)
    //{
    //    if (string.IsNullOrWhiteSpace(newValue))
    //    {
    //        ShowAnimes.Value = Works;
    //    }
    //    else
    //    {
    //        ShowAnimes.Value = new(
    //            Works.Where(m => m.Id.Value.Contains(newValue, StringComparison.OrdinalIgnoreCase))
    //        );
    //    }
    //}

    //public void Close() { }

    //private void Add()
    //{
    //    var window = new AnimeEditWindow();
    //    var vm = window.ViewModel;
    //    vm.CurrentPet = CurrentPet.Value;
    //    window.ShowDialog();
    //    if (window.IsCancel)
    //        return;
    //    Works.Add(vm.Work.Value);
    //}

    //public void Edit(AnimeModel model)
    //{
    //    var window = new AnimeEditWindow();
    //    var vm = window.ViewModel;
    //    vm.CurrentPet = CurrentPet.Value;
    //    vm.OldWork = model;
    //    var newWork = vm.Work.Value = new(model);
    //    window.ShowDialog();
    //    if (window.IsCancel)
    //        return;
    //    if (ShowAnimes.Value.Count == Works.Count)
    //    {
    //        Works[Works.IndexOf(model)] = newWork;
    //    }
    //    else
    //    {
    //        Works[Works.IndexOf(model)] = newWork;
    //        ShowAnimes.Value[ShowAnimes.Value.IndexOf(model)] = newWork;
    //    }
    //}

    //private void Remove(AnimeModel food)
    //{
    //    if (MessageBox.Show("确定删除吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.No)
    //        return;
    //    if (ShowAnimes.Value.Count == Works.Count)
    //    {
    //        Works.Remove(food);
    //    }
    //    else
    //    {
    //        ShowAnimes.Value.Remove(food);
    //        Works.Remove(food);
    //    }
    //}
}
