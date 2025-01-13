using System.Collections;
using System.Collections.Specialized;
using System.Reactive.Linq;
using DynamicData.Binding;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using HKW.HKWReactiveUI;
using HKW.HKWUtils.Collections;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using HKW.MVVMDialogs;
using HKW.WPF.MVVMDialogs;
using LinePutScript.Localization.WPF;
using ReactiveUI;
using VPet.ModMaker.Models;
using VPet.ModMaker.Models.ModModel;

namespace VPet.ModMaker.ViewModels.ModEdit;

/// <summary>
/// 动画视图模型
/// </summary>
public partial class AnimeVM : ViewModelBase
{
    /// <inheritdoc/>
    public AnimeVM(ModInfoModel modInfo)
    {
        ModInfo = modInfo;
        Animes = new([], [], (f) => f.ID.Contains(Search, StringComparison.OrdinalIgnoreCase));

        this.WhenAnyValue(x => x.Search)
            .Throttle(TimeSpan.FromSeconds(0.5), RxApp.TaskpoolScheduler)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => Animes.Refresh())
            .Record(this);

        modInfo
            .WhenAnyValue(x => x.CurrentPet)
            .Throttle(TimeSpan.FromSeconds(0.5), RxApp.TaskpoolScheduler)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(x => CurrentPet = x!)
            .Record(this);
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
    public FilterListWrapper<
        IAnimeModel,
        ObservableList<IAnimeModel>,
        ObservableList<IAnimeModel>
    > Animes { get; }

    /// <summary>
    /// 当前宠物
    /// </summary>
    [ReactiveProperty]
    public PetModel CurrentPet { get; set; } = null!;

    partial void OnCurrentPetChanged(PetModel oldValue, PetModel newValue)
    {
        if (oldValue is not null)
        {
            Animes.BaseList.BindingList(newValue.Animes, true);
        }
        Animes.Clear();
        Animes.AutoFilter = false;
        if (newValue is not null)
        {
            Animes.AddRange(newValue.Animes);
            Animes.BaseList.BindingList(newValue.Animes);
            Search = string.Empty;
        }
        Animes.Refresh();
        Animes.AutoFilter = true;
    }

    /// <summary>
    /// 搜索
    /// </summary>
    [ReactiveProperty]
    public string Search { get; set; } = string.Empty;
    #endregion

    /// <summary>
    /// 添加
    /// </summary>
    [ReactiveCommand]
    private async void Add()
    {
        var vm = await NativeUtils.DialogService.ShowDialogAsyncX(
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
            var animeVM = await NativeUtils.DialogService.ShowDialogAsyncX(
                this,
                new FoodAnimeEditVM(new() { Name = animeName }) { CurrentPet = CurrentPet }
            );
            if (animeVM.DialogResult is not true)
                return;
            CurrentPet.Animes.Add(animeVM.Anime);
        }
        else
        {
            var animeVM = await NativeUtils.DialogService.ShowDialogAsyncX(
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
            var animeVM = await NativeUtils.DialogService.ShowDialogAsyncX(
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
            var animeVM = await NativeUtils.DialogService.ShowDialogAsyncX(
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
                CurrentPet.Animes[CurrentPet.Animes.IndexOf(foodAnimeTypeModel)] = animeVM.Anime;
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
            NativeUtils.DialogService.ShowMessageBoxX(
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
            CurrentPet.Animes.Remove(model);
            this.LogX().Info("删除动画 {food}", model.ID);
        }
    }
}
