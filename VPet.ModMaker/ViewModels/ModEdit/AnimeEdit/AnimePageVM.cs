using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using DynamicData.Binding;
using HKW.HKWReactiveUI;
using HKW.HKWUtils.Collections;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using LinePutScript.Localization.WPF;
using Panuon.WPF.UI;
using ReactiveUI;
using VPet.ModMaker.Models;
using VPet.ModMaker.Models.ModModel;
using VPet.ModMaker.Views.ModEdit;

namespace VPet.ModMaker.ViewModels.ModEdit;

public partial class AnimePageVM : ViewModelBase
{
    public AnimePageVM()
    {
        AllAnimes = new(
            [],
            [],
            (f) =>
            {
                if (f is AnimeTypeModel animeModel)
                {
                    return animeModel.ID.Contains(Search, StringComparison.OrdinalIgnoreCase);
                }
                else if (f is FoodAnimeTypeModel foodAnimeModel)
                {
                    return foodAnimeModel.ID.Contains(Search, StringComparison.OrdinalIgnoreCase);
                }
                else
                    throw new Exception("???");
            }
        );
        if (Pets.HasValue())
            CurrentPet = Pets.FirstOrDefault(
                m => m.FromMain is false && m.AnimeCount > 0,
                Pets.First()
            );

        ModInfo.PropertyChanged += ModInfo_PropertyChanged;

        this.WhenValueChanged(x => x.Search)
            .Throttle(TimeSpan.FromSeconds(1), RxApp.TaskpoolScheduler)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => AllAnimes.Refresh());
    }

    private void ModInfo_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ModInfoModel.ShowMainPet))
        {
            if (ModInfo.ShowMainPet is false)
            {
                if (CurrentPet?.FromMain is true)
                {
                    CurrentPet = null!;
                }
            }
        }
    }

    public static ModInfoModel ModInfo => ModInfoModel.Current;

    #region Property
    /// <summary>
    /// 所有动画
    /// </summary>

    public FilterListWrapper<object, List<object>, ObservableList<object>> AllAnimes { get; }

    /// <summary>
    /// 动画
    /// </summary>
    public ObservableList<AnimeTypeModel> Animes => CurrentPet.Animes;

    /// <summary>
    /// 食物动画
    /// </summary>
    public ObservableList<FoodAnimeTypeModel> FoodAnimes => CurrentPet.FoodAnimes;

    /// <summary>
    /// 宠物列表
    /// </summary>
    public static ObservableList<PetModel> Pets => ModInfoModel.Current.Pets;

    /// <summary>
    /// 当前宠物
    /// </summary>
    [ReactiveProperty]
    public PetModel CurrentPet { get; set; } = null!;

    partial void OnCurrentPetChanged(PetModel oldValue, PetModel newValue)
    {
        InitializeAllAnimes();
    }

    /// <summary>
    /// 搜索
    /// </summary>
    [ReactiveProperty]
    public string Search { get; set; } = string.Empty;
    #endregion

    private void InitializeAllAnimes()
    {
        AllAnimes.Clear();
        if (CurrentPet is null)
            return;
        foreach (var item in Animes)
            AllAnimes.Add(item);
        foreach (var item in FoodAnimes)
            AllAnimes.Add(item);
        Animes.CollectionChanged -= Animes_CollectionChanged;
        Animes.CollectionChanged += Animes_CollectionChanged;
        FoodAnimes.CollectionChanged -= Animes_CollectionChanged;
        FoodAnimes.CollectionChanged += Animes_CollectionChanged;
    }

    private void Animes_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action is NotifyCollectionChangedAction.Add)
            AllAnimes.Add(e.NewItems![0]!);
        else if (e.Action is NotifyCollectionChangedAction.Remove)
            AllAnimes.Remove(e.OldItems![0]!);
        else if (e.Action is NotifyCollectionChangedAction.Replace)
            AllAnimes[AllAnimes.IndexOf(e.OldItems![0]!)] = e.NewItems![0]!;
    }

    /// <summary>
    /// 添加
    /// </summary>
    [ReactiveCommand]
    private void Add()
    {
        var selectGraphTypeWindow = new SelectGraphTypeWindow();
        selectGraphTypeWindow.ViewModel.CurrentPet = CurrentPet;
        selectGraphTypeWindow.ShowDialog();
        var graphType = selectGraphTypeWindow.ViewModel.GraphType;
        var animeName = selectGraphTypeWindow.ViewModel.AnimeName;
        if (selectGraphTypeWindow.IsCancel)
            return;
        if (
            graphType is VPet_Simulator.Core.GraphInfo.GraphType.Common
            && FoodAnimeTypeModel.FoodAnimeNames.Contains(animeName)
        )
        {
            var window = new FoodAnimeEditWindow();
            var vm = window.ViewModel;
            vm.CurrentPet = CurrentPet;
            vm.Anime.Name = animeName;
            window.ShowDialog();
            if (window.IsCancel)
                return;
            FoodAnimes.Add(vm.Anime);
        }
        else
        {
            var window = new AnimeEditWindow();
            var vm = window.ViewModel;
            vm.CurrentPet = CurrentPet;
            vm.Anime.GraphType = graphType;
            if (string.IsNullOrWhiteSpace(animeName))
                vm.Anime.ID = graphType.ToString();
            else
                vm.Anime.Name = animeName;
            window.ShowDialog();
            if (window.IsCancel)
                return;
            Animes.Add(vm.Anime);
        }
    }

    /// <summary>
    /// 编辑
    /// </summary>
    /// <param name="model">模型</param>
    [ReactiveCommand]
    public void Edit(object model)
    {
        var pendingHandler = PendingBox.Show("载入中".Translate());
        if (model is AnimeTypeModel animeTypeModel)
        {
            var window = new AnimeEditWindow();
            var vm = window.ViewModel;
            vm.CurrentPet = CurrentPet;
            vm.OldAnime = animeTypeModel;
            var newAnime = vm.Anime = new(animeTypeModel);
            pendingHandler.Close();
            window.ShowDialog();
            if (window.IsCancel)
                return;
            Animes[Animes.IndexOf(animeTypeModel)] = newAnime;
        }
        else if (model is FoodAnimeTypeModel foodAnimeTypeModel)
        {
            var window = new FoodAnimeEditWindow();
            var vm = window.ViewModel;
            vm.CurrentPet = CurrentPet;
            vm.OldAnime = foodAnimeTypeModel;
            var newAnime = vm.Anime = new(foodAnimeTypeModel);
            pendingHandler.Close();
            window.ShowDialog();
            if (window.IsCancel)
                return;
            FoodAnimes[FoodAnimes.IndexOf(foodAnimeTypeModel)] = newAnime;
        }
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="model">模型</param>
    [ReactiveCommand]
    private void Remove(object model)
    {
        if (MessageBox.Show("确定删除吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.No)
            return;
        AllAnimes.Remove(model);
        if (model is AnimeTypeModel animeTypeModel)
        {
            Animes.Remove(animeTypeModel);
            animeTypeModel.Close();
        }
        else if (model is FoodAnimeTypeModel foodAnimeTypeModel)
        {
            FoodAnimes.Remove(foodAnimeTypeModel);
            foodAnimeTypeModel.Close();
        }
    }
}
