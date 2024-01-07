using HKW.HKWUtils.Observable;

using LinePutScript.Localization.WPF;
using Panuon.WPF.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
    public static ModInfoModel ModInfo => ModInfoModel.Current;

    #region Value
    /// <summary>
    /// 显示的动画
    /// </summary>
    public ObservableValue<ObservableCollection<object>> ShowAnimes { get; } = new();

    /// <summary>
    /// 所有动画
    /// </summary>
    public ObservableCollection<object> AllAnimes { get; } = new();

    /// <summary>
    /// 动画
    /// </summary>
    public ObservableCollection<AnimeTypeModel> Animes => CurrentPet.Value.Animes;

    /// <summary>
    /// 食物动画
    /// </summary>
    public ObservableCollection<FoodAnimeTypeModel> FoodAnimes => CurrentPet.Value.FoodAnimes;

    /// <summary>
    /// 宠物列表
    /// </summary>
    public ObservableCollection<PetModel> Pets => ModInfoModel.Current.Pets;

    /// <summary>
    /// 当前宠物
    /// </summary>
    public ObservableValue<PetModel> CurrentPet { get; } = new(new());

    /// <summary>
    /// 搜索
    /// </summary>
    public ObservableValue<string> Search { get; } = new();
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
    public AnimePageVM()
    {
        ShowAnimes.Value = AllAnimes;
        CurrentPet.ValueChanged += CurrentPet_ValueChanged;
        Search.ValueChanged += Search_ValueChanged;

        AddCommand.ExecuteCommand += Add;
        EditCommand.ExecuteCommand += Edit;
        RemoveCommand.ExecuteCommand += Remove;
    }

    private void InitializeAllAnimes()
    {
        AllAnimes.Clear();
        foreach (var item in Animes)
            AllAnimes.Add(item);
        foreach (var item in FoodAnimes)
            AllAnimes.Add(item);
        Animes.CollectionChanged += Animes_CollectionChanged;
        FoodAnimes.CollectionChanged += Animes_CollectionChanged;
    }

    private void Animes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action is NotifyCollectionChangedAction.Add)
            AllAnimes.Add(e.NewItems[0]);
        else if (e.Action is NotifyCollectionChangedAction.Remove)
            AllAnimes.Remove(e.OldItems[0]);
        else if (e.Action is NotifyCollectionChangedAction.Replace)
            AllAnimes[AllAnimes.IndexOf(e.OldItems[0])] = e.NewItems[0];
    }

    private void CurrentPet_ValueChanged(
        ObservableValue<PetModel> sender,
        ValueChangedEventArgs<PetModel> e
    )
    {
        InitializeAllAnimes();
        ShowAnimes.Value = AllAnimes;
    }

    private void Search_ValueChanged(
        ObservableValue<string> sender,
        ValueChangedEventArgs<string> e
    )
    {
        if (string.IsNullOrWhiteSpace(e.NewValue))
        {
            ShowAnimes.Value = AllAnimes;
        }
        else
        {
            ShowAnimes.Value = new(
                AllAnimes.Where(m =>
                {
                    if (m is AnimeTypeModel animeTypeModel)
                        return animeTypeModel.Id.Value.Contains(
                            e.NewValue,
                            StringComparison.OrdinalIgnoreCase
                        );
                    else if (m is FoodAnimeTypeModel foodAnimeTypeModel)
                        return foodAnimeTypeModel.Id.Value.Contains(
                            e.NewValue,
                            StringComparison.OrdinalIgnoreCase
                        );
                    else
                        throw new Exception("???");
                })
            );
        }
    }

    /// <summary>
    /// 添加动画
    /// </summary>
    private void Add()
    {
        var selectGraphTypeWindow = new SelectGraphTypeWindow();
        selectGraphTypeWindow.CurrentPet.Value = CurrentPet.Value;
        selectGraphTypeWindow.ShowDialog();
        var graphType = selectGraphTypeWindow.GraphType.Value;
        var animeName = selectGraphTypeWindow.AnimeName.Value;
        if (selectGraphTypeWindow.IsCancel)
            return;
        if (
            graphType is VPet_Simulator.Core.GraphInfo.GraphType.Common
            && FoodAnimeTypeModel.FoodAnimeNames.Contains(animeName)
        )
        {
            var window = new FoodAnimeEditWindow();
            var vm = window.ViewModel;
            vm.CurrentPet = CurrentPet.Value;
            vm.Anime.Value.Name.Value = animeName;
            window.ShowDialog();
            if (window.IsCancel)
                return;
            FoodAnimes.Add(vm.Anime.Value);
        }
        else
        {
            var window = new AnimeEditWindow();
            var vm = window.ViewModel;
            vm.CurrentPet = CurrentPet.Value;
            vm.Anime.Value.GraphType.Value = graphType;
            vm.Anime.Value.Name.Value = animeName;
            window.ShowDialog();
            if (window.IsCancel)
                return;
            Animes.Add(vm.Anime.Value);
        }
    }

    /// <summary>
    /// 编辑动画
    /// </summary>
    /// <param name="model">动画类型模型</param>
    public void Edit(object model)
    {
        var pendingHandler = PendingBox.Show("载入中".Translate());
        if (model is AnimeTypeModel animeTypeModel)
        {
            var window = new AnimeEditWindow();
            var vm = window.ViewModel;
            vm.CurrentPet = CurrentPet.Value;
            vm.OldAnime = animeTypeModel;
            var newAnime = vm.Anime.Value = new(animeTypeModel);
            pendingHandler.Close();
            window.ShowDialog();
            if (window.IsCancel)
                return;
            Animes[Animes.IndexOf(animeTypeModel)] = newAnime;
            if (ShowAnimes.Value.Count != Animes.Count)
                ShowAnimes.Value[ShowAnimes.Value.IndexOf(animeTypeModel)] = newAnime;
        }
        else if (model is FoodAnimeTypeModel foodAnimeTypeModel)
        {
            var window = new FoodAnimeEditWindow();
            var vm = window.ViewModel;
            vm.CurrentPet = CurrentPet.Value;
            vm.OldAnime = foodAnimeTypeModel;
            var newAnime = vm.Anime.Value = new(foodAnimeTypeModel);
            pendingHandler.Close();
            window.ShowDialog();
            if (window.IsCancel)
                return;
            FoodAnimes[FoodAnimes.IndexOf(foodAnimeTypeModel)] = newAnime;
            if (ShowAnimes.Value.Count != FoodAnimes.Count)
                ShowAnimes.Value[ShowAnimes.Value.IndexOf(foodAnimeTypeModel)] = newAnime;
        }
    }

    /// <summary>
    /// 删除动画
    /// </summary>
    /// <param name="model">动画类型模型</param>
    private void Remove(object model)
    {
        if (MessageBox.Show("确定删除吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.No)
            return;
        ShowAnimes.Value.Remove(model);
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
