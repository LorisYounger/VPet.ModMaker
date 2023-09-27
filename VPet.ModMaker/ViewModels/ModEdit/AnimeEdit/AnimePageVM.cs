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
    #region Value
    public ObservableValue<ObservableCollection<AnimeTypeModel>> ShowAnimes { get; } = new();
    public ObservableCollection<AnimeTypeModel> Animes => CurrentPet.Value.Animes;

    public ObservableCollection<PetModel> Pets => ModInfoModel.Current.Pets;
    public ObservableValue<PetModel> CurrentPet { get; } = new(new());
    public ObservableValue<string> Search { get; } = new();
    #endregion
    #region Command
    public ObservableCommand AddCommand { get; } = new();
    public ObservableCommand<AnimeTypeModel> EditCommand { get; } = new();
    public ObservableCommand<AnimeTypeModel> RemoveCommand { get; } = new();
    #endregion
    public AnimePageVM()
    {
        ShowAnimes.Value = Animes;
        CurrentPet.ValueChanged += CurrentPet_ValueChanged;
        Search.ValueChanged += Search_ValueChanged;

        AddCommand.ExecuteEvent += Add;
        EditCommand.ExecuteEvent += Edit;
        RemoveCommand.ExecuteEvent += Remove;
    }

    private void CurrentPet_ValueChanged(PetModel oldValue, PetModel newValue)
    {
        ShowAnimes.Value = newValue.Animes;
    }

    private void Search_ValueChanged(string oldValue, string newValue)
    {
        if (string.IsNullOrWhiteSpace(newValue))
        {
            ShowAnimes.Value = Animes;
        }
        else
        {
            ShowAnimes.Value = new(
                Animes.Where(m => m.Id.Value.Contains(newValue, StringComparison.OrdinalIgnoreCase))
            );
        }
    }

    public void Close() { }

    private void Add()
    {
        var selectGraphTypeWindow = new SelectGraphTypeWindow();
        selectGraphTypeWindow.CurrentPet.Value = CurrentPet.Value;
        selectGraphTypeWindow.ShowDialog();
        var graphType = selectGraphTypeWindow.GraphType.Value;
        if (selectGraphTypeWindow.IsCancel)
            return;
        var window = new AnimeEditWindow();
        var vm = window.ViewModel;
        vm.CurrentPet = CurrentPet.Value;
        vm.Anime.Value.GraphType.Value = graphType;
        vm.Anime.Value.Name.Value = selectGraphTypeWindow.AnimeName.Value;
        window.ShowDialog();
        if (window.IsCancel)
            return;
        Animes.Add(vm.Anime.Value);
    }

    public void Edit(AnimeTypeModel model)
    {
        var window = new AnimeEditWindow();
        var vm = window.ViewModel;
        vm.CurrentPet = CurrentPet.Value;
        vm.OldAnime = model;
        var newAnime = vm.Anime.Value = new(model);
        window.ShowDialog();
        if (window.IsCancel)
            return;
        if (ShowAnimes.Value.Count == Animes.Count)
        {
            Animes[Animes.IndexOf(model)] = newAnime;
        }
        else
        {
            Animes[Animes.IndexOf(model)] = newAnime;
            ShowAnimes.Value[ShowAnimes.Value.IndexOf(model)] = newAnime;
        }
    }

    private void Remove(AnimeTypeModel food)
    {
        if (MessageBox.Show("确定删除吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.No)
            return;
        if (ShowAnimes.Value.Count == Animes.Count)
        {
            Animes.Remove(food);
        }
        else
        {
            ShowAnimes.Value.Remove(food);
            Animes.Remove(food);
        }
    }
}
