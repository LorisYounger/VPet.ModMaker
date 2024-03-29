using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HKW.HKWUtils.Observable;
using LinePutScript.Localization.WPF;
using Panuon.WPF.UI;
using VPet.ModMaker.Models;
using VPet.ModMaker.Models.ModModel;
using VPet.ModMaker.Views.ModEdit.AnimeEdit;

namespace VPet.ModMaker.ViewModels.ModEdit.AnimeEdit;

public class AnimePageVM : ObservableObjectX<AnimePageVM>
{
    public AnimePageVM()
    {
        AllAnimes = new()
        {
            Filter = (f) =>
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
            },
            FilteredList = new()
        };
        PropertyChangedX += AnimePageVM_PropertyChangedX;

        AddCommand.ExecuteCommand += AddCommand_ExecuteCommand;
        EditCommand.ExecuteCommand += EditCommand_ExecuteCommand;
        RemoveCommand.ExecuteCommand += RemoveCommand_ExecuteCommand;
    }

    private void AnimePageVM_PropertyChangedX(AnimePageVM sender, PropertyChangedXEventArgs e)
    {
        if (e.PropertyName == nameof(CurrentPet))
        {
            InitializeAllAnimes();
        }
        else if (e.PropertyName == nameof(Search))
        {
            AllAnimes.Refresh();
        }
    }

    public static ModInfoModel ModInfo => ModInfoModel.Current;

    #region Property
    /// <summary>
    /// 所有动画
    /// </summary>
    #region AllAnimes
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ObservableFilterList<object, ObservableList<object>> _allAnimes = null!;

    public ObservableFilterList<object, ObservableList<object>> AllAnimes
    {
        get => _allAnimes;
        set => SetProperty(ref _allAnimes, value);
    }
    #endregion

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

    #region CurrentPEt
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private PetModel _currentPet = null!;

    /// <summary>
    /// 当前宠物
    /// </summary>
    public PetModel CurrentPet
    {
        get => _currentPet;
        set => SetProperty(ref _currentPet, value);
    }
    #endregion


    #region Search
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _search = string.Empty;

    /// <summary>
    /// 搜索
    /// </summary>
    public string Search
    {
        get => _search;
        set => SetProperty(ref _search, value);
    }
    #endregion
    #endregion
    #region Command
    /// <summary>
    /// 添加命令
    /// </summary>
    public ObservableCommand AddCommand { get; } = new();

    /// <summary>
    /// 编辑命令
    /// </summary>
    public ObservableCommand<object> EditCommand { get; } = new();

    /// <summary>
    /// 删除命令
    /// </summary>
    public ObservableCommand<object> RemoveCommand { get; } = new();
    #endregion
    private void InitializeAllAnimes()
    {
        AllAnimes.Clear();
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
    /// 添加动画
    /// </summary>
    private void AddCommand_ExecuteCommand()
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
            vm.Anime.Name = animeName;
            window.ShowDialog();
            if (window.IsCancel)
                return;
            Animes.Add(vm.Anime);
        }
    }

    /// <summary>
    /// 编辑动画
    /// </summary>
    /// <param name="model">动画类型模型</param>
    public void EditCommand_ExecuteCommand(object model)
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
    /// 删除动画
    /// </summary>
    /// <param name="model">动画类型模型</param>
    private void RemoveCommand_ExecuteCommand(object model)
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
