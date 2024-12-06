using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicData.Binding;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using HKW.HKWReactiveUI;
using HKW.HKWUtils.Collections;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using HKW.MVVMDialogs;
using HKW.WPF.MVVMDialogs;
using LinePutScript.Localization.WPF;
using Panuon.WPF.UI;
using ReactiveUI;
using Splat;
using VPet.ModMaker.Models;
using VPet.ModMaker.Models.ModModel;
using VPet.ModMaker.Views.ModEdit;

namespace VPet.ModMaker.ViewModels.ModEdit;

/// <summary>
/// 动画视图模型
/// </summary>
public partial class AnimeVM : ViewModelBase
{
    private static IDialogService DialogService => Locator.Current.GetService<IDialogService>()!;

    /// <inheritdoc/>
    public AnimeVM()
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
                    return false;
            }
        );

        this.WhenValueChanged(x => x.Search)
            .Throttle(TimeSpan.FromSeconds(0.5), RxApp.TaskpoolScheduler)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => AllAnimes.Refresh());
    }

    #region Property
    /// <summary>
    /// 模组信息
    /// </summary>
    [ReactiveProperty]
    public ModInfoModel ModInfo { get; set; } = null!;

    partial void OnModInfoChanged(ModInfoModel oldValue, ModInfoModel newValue)
    {
        if (newValue is not null)
        {
            newValue
                .WhenValueChanged(x => x.CurrentPet)
                .Throttle(TimeSpan.FromSeconds(0.5), RxApp.TaskpoolScheduler)
                .DistinctUntilChanged()
                .ObserveOn(RxApp.MainThreadScheduler)
                .Subscribe(x => CurrentPet = x!);
        }
        else if (newValue is null)
            CurrentPet = null!;
    }

    /// <summary>
    /// 所有动画
    /// </summary>
    public FilterListWrapper<object, List<object>, ObservableList<object>> AllAnimes { get; }

    /// <summary>
    /// 当前宠物
    /// </summary>
    [ReactiveProperty]
    public PetModel CurrentPet { get; set; } = null!;

    partial void OnCurrentPetChanged(PetModel oldValue, PetModel newValue)
    {
        if (oldValue is not null)
        {
            oldValue.Animes.CollectionChanged -= Animes_CollectionChanged;
            oldValue.FoodAnimes.CollectionChanged -= Animes_CollectionChanged;
        }
        AllAnimes.Clear();
        AllAnimes.AutoFilter = false;
        if (newValue is not null)
        {
            AllAnimes.AddRange(newValue.Animes);
            AllAnimes.AddRange(newValue.FoodAnimes);
            Search = string.Empty;
            newValue.Animes.CollectionChanged -= Animes_CollectionChanged;
            newValue.Animes.CollectionChanged += Animes_CollectionChanged;
            newValue.FoodAnimes.CollectionChanged -= Animes_CollectionChanged;
            newValue.FoodAnimes.CollectionChanged += Animes_CollectionChanged;
        }
        AllAnimes.Refresh();
        AllAnimes.AutoFilter = true;
    }

    /// <summary>
    /// 搜索
    /// </summary>
    [ReactiveProperty]
    public string Search { get; set; } = string.Empty;
    #endregion

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
    private async void Add()
    {
        var vm = await DialogService.ShowDialogAsyncX(
            this,
            new SelectGraphTypeVM() { CurrentPet = CurrentPet, }
        );
        if (vm.DialogResult is not true)
            return;
        var graphType = vm.GraphType;
        var animeName = vm.AnimeName;
        if (
            graphType is VPet_Simulator.Core.GraphInfo.GraphType.Common
            && FoodAnimeTypeModel.FoodAnimeNames.Contains(animeName)
        )
        {
            var animeVM = await DialogService.ShowDialogAsyncX(
                this,
                new FoodAnimeEditVM(new() { Name = animeName }) { CurrentPet = CurrentPet }
            );
            if (animeVM.DialogResult is not true)
                return;
            CurrentPet.FoodAnimes.Add(animeVM.Anime);
        }
        else
        {
            var animeVM = await DialogService.ShowDialogAsyncX(
                this,
                new AnimeEditVM(
                    new()
                    {
                        GraphType = graphType,
                        Name = string.IsNullOrWhiteSpace(animeName)
                            ? graphType.ToString()
                            : animeName
                    }
                )
                {
                    CurrentPet = CurrentPet
                }
            );
            if (animeVM.DialogResult is not true)
                return;
            CurrentPet.Animes.Add(animeVM.Anime);
        }
    }

    /// <summary>
    /// 编辑
    /// </summary>
    /// <param name="model">模型</param>
    [ReactiveCommand]
    public async void Edit(IAnimeModel model)
    {
        //var pendingHandler = PendingBox.Show("载入中".Translate());
        if (model is AnimeTypeModel animeTypeModel)
        {
            var animeVM = await DialogService.ShowDialogAsyncX(
                this,
                new AnimeEditVM(new(animeTypeModel))
                {
                    CurrentPet = CurrentPet,
                    OldAnime = animeTypeModel
                }
            );
            if (animeVM.DialogResult is not true)
            {
                animeVM.Anime?.Close();
            }
            else
            {
                animeVM.OldAnime?.Close();
                CurrentPet.Animes[CurrentPet.Animes.IndexOf(animeTypeModel)] = animeVM.Anime;
            }
        }
        else if (model is FoodAnimeTypeModel foodAnimeTypeModel)
        {
            var animeVM = await DialogService.ShowDialogAsyncX(
                this,
                new FoodAnimeEditVM(new(foodAnimeTypeModel))
                {
                    CurrentPet = CurrentPet,
                    OldAnime = foodAnimeTypeModel
                }
            );
            if (animeVM.DialogResult is not true)
            {
                animeVM.Anime?.Close();
            }
            else
            {
                animeVM.OldAnime?.Close();
                CurrentPet.FoodAnimes[CurrentPet.FoodAnimes.IndexOf(foodAnimeTypeModel)] =
                    animeVM.Anime;
            }
        }
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="list">模型</param>
    [ReactiveCommand]
    private void Remove(IList list)
    {
        var models = list.Cast<IAnimeModel>().ToArray();
        if (
            DialogService.ShowMessageBoxX(
                this,
                "确定删除已选中的 {0} 个动画吗".Translate(models.Length),
                "删除动画".Translate(),
                MessageBoxButton.YesNo
            )
            is not true
        )
            return;
        foreach (var model in models)
        {
            if (model is AnimeTypeModel animeTypeModel)
            {
                CurrentPet.Animes.Remove(animeTypeModel);
            }
            else if (model is FoodAnimeTypeModel foodAnimeTypeModel)
            {
                CurrentPet.FoodAnimes.Remove(foodAnimeTypeModel);
            }
            else
            {
                this.Log().Warn("未知动画类型 {anime}", model);
                continue;
            }
            model.Close();
            this.Log().Info("删除动画 {food}", model.ID);
        }
    }

    /// <summary>
    /// 重置
    /// </summary>
    public void Reset() { }
}
