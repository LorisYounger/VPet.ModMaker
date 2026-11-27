using System.Collections;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using HKW.HKWUtils.Collections;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using HKW.MVVM;
using HKW.MVVM.SourceGenerator;
using LinePutScript.Localization.WPF;
using Microsoft.Extensions.Logging;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels;

/// <summary>
/// 动画视图模型
/// </summary>
public sealed partial class AnimeViewModel : ViewModelBase, IEditViewModel, IDisposable
{
    /// <inheritdoc/>
    public AnimeViewModel()
    {
        Animes = new([], [], f => f.ID.Contains(Search, StringComparison.CurrentCultureIgnoreCase));
        this.WhenAnyValue(x => x.Search)
            .Throttle(TimeSpan.FromSeconds(0.5), ObservableSchedulers.ThreadPool)
            .DistinctUntilChanged()
            .ObserveOn(ObservableSchedulers.Current)
            .Subscribe(_ => Animes?.Refresh());
    }

    #region Property
    [NotifyPropertyChangeFrom(nameof(CurrentPet))]
    public string DisplayText => "Anime ({0})".Translate(CurrentPet?.Animes.Count!);

    /// <summary>
    /// 模组信息
    /// </summary>
    [ObservableProperty]
    public ModInfoModel ModInfo { get; set; } = null!;

    partial void OnModInfoChanged(ModInfoModel oldValue, ModInfoModel newValue)
    {
        if (newValue is not null)
        {
            CurrentPet = newValue.CurrentPet;
            newValue
                .WhenAnyValue(x => x.CurrentPet)
                .Subscribe(x => CurrentPet = x)
                .DisposeWith(newValue.Disposables);
        }
        else
            CurrentPet = null!;
    }

    /// <summary>
    /// 所有动画
    /// </summary>
    public ObservableFilteredListWrapper<
        IAnimeModel,
        ObservableList<IAnimeModel>,
        ObservableList<IAnimeModel>
    > Animes { get; }

    /// <summary>
    /// 当前宠物
    /// </summary>
    [ObservableProperty]
    public PetModel? CurrentPet { get; set; } = null!;

    private IDisposable? _disposable;

    partial void OnCurrentPetChanged(PetModel? oldValue, PetModel? newValue)
    {
        Animes.SourceList = null!;
        _disposable?.Dispose();
        if (newValue is not null)
        {
            Animes.SourceList = newValue.Animes;

            _disposable = newValue
                .Animes.WhenAnyValue(x => x.Count)
                .Subscribe(x => NotifyPropertyChanged(nameof(DisplayText)));
        }
    }

    /// <summary>
    /// 搜索
    /// </summary>
    [ObservableProperty]
    public string Search { get; set; } = string.Empty;
    #endregion

    private SelectGraphTypeViewModel SelectGraphType =>
        field ??= DialogService?.CreateViewModel<SelectGraphTypeViewModel>()!;

    private AnimeEditViewModel AnimeEdit =>
        field ??= DialogService?.CreateViewModel<AnimeEditViewModel>()!;
    private FoodAnimeEditViewModel FoodAnimeEdit =>
        field ??= DialogService?.CreateViewModel<FoodAnimeEditViewModel>()!;

    private AnimeTypeModel CommitAnime(AnimeTypeModel source, AnimeTypeModel? oldModel)
    {
        var savedModel = new AnimeTypeModel(source);
        if (oldModel is null)
        {
            Animes.Add(savedModel);
        }
        else
        {
            oldModel.Dispose();
            var index = Animes.IndexOf(oldModel);
            if (index >= 0)
                Animes[index] = savedModel;
            else
                Animes.Add(savedModel);
        }
        return savedModel;
    }

    private FoodAnimeTypeModel CommitFoodAnime(
        FoodAnimeTypeModel source,
        FoodAnimeTypeModel? oldModel
    )
    {
        var savedModel = new FoodAnimeTypeModel(source);
        if (oldModel is null)
        {
            Animes.Add(savedModel);
        }
        else
        {
            oldModel.Dispose();
            var index = Animes.IndexOf(oldModel);
            if (index >= 0)
                Animes[index] = savedModel;
            else
                Animes.Add(savedModel);
        }
        return savedModel;
    }

    /// <summary>
    /// 添加
    /// </summary>
    [RelayCommand]
    private void Add()
    {
        SelectGraphType.CurrentPet = CurrentPet!;
        var result = DialogService.ShowDialog(this, SelectGraphType);
        if (result is not true)
            return;
        var graphType = SelectGraphType.GraphType;
        var animeName = SelectGraphType.AnimeName;
        if (
            graphType is VPet_Simulator.Core.GraphInfo.GraphType.Common
            && FoodAnimeTypeModel.FoodAnimeNames.Contains(animeName)
        )
        {
            var newModel = new FoodAnimeTypeModel() { Name = animeName };
            FoodAnimeEdit.OldAnime = null;
            FoodAnimeEdit.Anime = newModel;
            FoodAnimeEdit.CurrentPet = CurrentPet!;
            FoodAnimeEdit.CommitAction = anime =>
                FoodAnimeEdit.OldAnime = CommitFoodAnime(anime, FoodAnimeEdit.OldAnime);
            using var dis = FoodAnimeEdit.BeginEditSession();
            var editResult = DialogService.ShowDialog(this, FoodAnimeEdit);
            if (editResult is true)
            {
                FoodAnimeEdit.OldAnime = CommitFoodAnime(newModel, FoodAnimeEdit.OldAnime);
            }
            Reset();
            newModel.Dispose();
        }
        else
        {
            var newModel = new AnimeTypeModel()
            {
                GraphType = graphType,
                Name = string.IsNullOrWhiteSpace(animeName) ? graphType.ToString() : animeName,
            };
            AnimeEdit.OldAnime = null;
            AnimeEdit.Anime = newModel;
            AnimeEdit.CurrentPet = CurrentPet!;
            AnimeEdit.CommitAction = anime =>
                AnimeEdit.OldAnime = CommitAnime(anime, AnimeEdit.OldAnime);
            using var dis = AnimeEdit.BeginEditSession();
            var editResult = DialogService.ShowDialog(this, AnimeEdit);
            if (editResult is true)
            {
                AnimeEdit.OldAnime = CommitAnime(newModel, AnimeEdit.OldAnime);
            }
            Reset();
            newModel.Dispose();
        }
    }

    /// <summary>
    /// 编辑
    /// </summary>
    /// <param name="model">模型</param>
    [RelayCommand]
    public void Edit(IAnimeModel model)
    {
        if (model is AnimeTypeModel animeTypeModel)
        {
            var newModel = new AnimeTypeModel(animeTypeModel);
            newModel.LoadTypeAnime();
            AnimeEdit.OldAnime = animeTypeModel;
            AnimeEdit.Anime = newModel;
            AnimeEdit.CurrentPet = CurrentPet!;
            AnimeEdit.CommitAction = anime =>
                AnimeEdit.OldAnime = CommitAnime(anime, AnimeEdit.OldAnime);
            using var dis = AnimeEdit.BeginEditSession();
            var editResult = DialogService.ShowDialog(this, AnimeEdit);
            if (editResult is true)
            {
                AnimeEdit.OldAnime = CommitAnime(newModel, AnimeEdit.OldAnime);
            }
            Reset();
            newModel.Dispose();
        }
        else if (model is FoodAnimeTypeModel foodAnimeTypeModel)
        {
            var newModel = new FoodAnimeTypeModel(foodAnimeTypeModel);
            newModel.LoadTypeAnime();
            FoodAnimeEdit.OldAnime = foodAnimeTypeModel;
            FoodAnimeEdit.Anime = newModel;
            FoodAnimeEdit.CurrentPet = CurrentPet!;
            FoodAnimeEdit.CommitAction = anime =>
                FoodAnimeEdit.OldAnime = CommitFoodAnime(anime, FoodAnimeEdit.OldAnime);
            using var dis = FoodAnimeEdit.BeginEditSession();
            var editResult = DialogService.ShowDialog(this, FoodAnimeEdit);
            if (editResult is true)
            {
                FoodAnimeEdit.OldAnime = CommitFoodAnime(newModel, FoodAnimeEdit.OldAnime);
            }
            Reset();
            newModel.Dispose();
        }
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="list">模型</param>
    [RelayCommand]
    private void Remove(IList list)
    {
        var models = list.Cast<IAnimeModel>().ToArray();
        if (
            DialogService.ShowMessageBox(
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
            Animes.Remove(model);
            this.LogIfEnabled(LogLevel.Information)?.Log("删除动画 {food}", model.ID);
        }
    }

    public void Reset()
    {
        AnimeEdit.Anime = null!;
        AnimeEdit.OldAnime = null;
        AnimeEdit.CurrentAnimeModel = null!;
        AnimeEdit.CurrentPet = null!;
        AnimeEdit.CommitAction = null;
        AnimeEdit.DialogResult = false;
        FoodAnimeEdit.Anime = null!;
        FoodAnimeEdit.OldAnime = null;
        FoodAnimeEdit.CurrentAnimeModel = null!;
        FoodAnimeEdit.CurrentPet = null!;
        FoodAnimeEdit.CommitAction = null;
        FoodAnimeEdit.DialogResult = false;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        ModInfo = null!;
        CurrentPet = null;
    }
}
