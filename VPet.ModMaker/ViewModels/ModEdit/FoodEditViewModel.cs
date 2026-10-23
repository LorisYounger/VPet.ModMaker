using System.Collections;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using HKW.HKWUtils;
using HKW.HKWUtils.Collections;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using HKW.Mapper;
using HKW.MVVM;
using HKW.MVVM.SourceGenerator;
using HKW.WPF;
using HKW.WPF.Extensions;
using HKW.WPF.MVVMDialogs;
using LinePutScript;
using LinePutScript.Converter;
using LinePutScript.Localization.WPF;
using Microsoft.Extensions.Logging;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels;

/// <summary>
/// 食物编辑视图模型
/// </summary>
public partial class FoodEditViewModel
    : ViewModelBase,
        IEditViewModel,
        IViewClosing,
        IModalDialogViewModel
{
    /// <inheritdoc/>
    public FoodEditViewModel()
    {
        Foods = new(
            [],
            [],
            x =>
            {
                return CurrentSearchTarget?.Value switch
                {
                    FoodSearchTarget.ID => x.ID.Contains(
                        Search,
                        StringComparison.CurrentCultureIgnoreCase
                    ),
                    FoodSearchTarget.Name => x.Name.Contains(
                        Search,
                        StringComparison.CurrentCultureIgnoreCase
                    ),
                    FoodSearchTarget.Graph => x.Graph.Contains(
                        Search,
                        StringComparison.CurrentCultureIgnoreCase
                    ),
                    _ => false,
                };
            }
        );

        this.WhenAnyValue(x => x.Search, x => x.CurrentSearchTarget)
            .Throttle(TimeSpan.FromSeconds(0.5), ObservableSchedulers.ThreadPool)
            .DistinctUntilChanged()
            .ObserveOn(ObservableSchedulers.Current)
            .Subscribe(_ => Foods?.Refresh());
    }

    #region Property

    [NotifyPropertyChangeFrom(nameof(ModInfo))]
    public string DisplayText => "Food ({0})".Translate(ModInfo?.Foods.Count!);

    /// <summary>
    /// 模组信息
    /// </summary>
    [ObservableProperty]
    public ModInfoModel ModInfo { get; set; } = null!;

    partial void OnModInfoChanged(ModInfoModel oldValue, ModInfoModel newValue)
    {
        Foods.SourceList = null!;
        if (newValue is not null)
        {
            Foods.SourceList = newValue.Foods;
            newValue
                .Foods.WhenAnyValue(x => x.Count)
                .Subscribe(_ => OnPropertyChanged(nameof(DisplayText)))
                .DisposeWith(newValue.Disposables);
        }
    }

    /// <summary>
    /// 食物
    /// </summary>
    public ObservableFilteredListWrapper<
        FoodModel,
        ObservableList<FoodModel>,
        ObservableList<FoodModel>
    > Foods { get; }

    /// <summary>
    /// 搜索
    /// </summary>
    [ObservableProperty]
    public string Search { get; set; } = string.Empty;

    /// <summary>
    /// 搜索目标
    /// </summary>
    public static ImmutableArray<EnumInfo<FoodSearchTarget>> SearchTargets =>
        EnumInfo<FoodSearchTarget>.StaticInfos;

    [ObservableProperty]
    public EnumInfo<FoodSearchTarget> CurrentSearchTarget { get; set; } = FoodSearchTarget.ID;

    public FoodModel? OldFood { get; private set; } = null;

    /// <summary>
    /// 食物
    /// </summary>
    [ObservableProperty]
    public FoodModel Food { get; private set; } = null!;

    partial void OnFoodChanged(FoodModel oldValue, FoodModel newValue)
    {
        newValue?.AutoSetFoodPrice = ModInfo.AutoSetFoodPrice;
    }

    public bool? DialogResult { get; set; }
    public bool IsChanged { get; private set; }

    #endregion

    /// <summary>
    /// 设置参考价格
    /// </summary>
    /// <param name="value">价格</param>
    [RelayCommand]
    private void SetReferencePrice(FoodModel food)
    {
        food.Price = food.ReferencePrice;
        this.LogIfEnabled(LogLevel.Debug)
            ?.Log(
                "食物 {food} 设置参考价格 {oldPrice} -> {price}",
                food.ID,
                food.Price,
                food.ReferencePrice
            );
    }

    [RelayCommand]
    private void SetReferencePriceForAllFood()
    {
        var result = DialogService.ShowMessageBox(
            this,
            "将所有食物设置为参考价格,你确定吗?".Translate(),
            "设置全部食物为参考价格".Translate(),
            MessageBoxButton.YesNo,
            MessageBoxImage.Information
        );
        if (result is not true)
            return;
        foreach (var food in Foods)
        {
            food.Price = food.ReferencePrice;
            this.LogIfEnabled(LogLevel.Debug)
                ?.Log(
                    "食物 {food} 设置参考价格 {oldPrice} -> {price}",
                    food.ID,
                    food.Price,
                    food.ReferencePrice
                );
        }
        this.LogIfEnabled(LogLevel.Information)
            ?.Log("已为 {count} 个食物设置参考价格", Foods.Count);
    }

    /// <summary>
    /// 添加图像
    /// </summary>
    [RelayCommand]
    private void AddImage()
    {
        var openFileDialog = DialogService.ShowOpenFileDialog(
            this,
            new()
            {
                Title = "选择图片".Translate(),
                Filters = [new("图片".Translate(), ModMaker.ImageExtensions)],
            }
        );
        if (openFileDialog is null)
            return;
        var newImage = HKWImageUtils.LoadImageToMemory(openFileDialog.LocalPath);
        if (newImage is null)
        {
            DialogService.ShowMessageBox(
                this,
                "图片载入失败, 详情请查看日志".Translate(),
                "图片载入失败".Translate(),
                icon: MessageBoxImage.Warning
            );
            return;
        }
        Food.Image = newImage;
    }

    /// <summary>
    /// 改变图像
    /// </summary>
    [RelayCommand]
    private void ChangeImage()
    {
        var openFileDialog = DialogService.ShowOpenFileDialog(
            this,
            new()
            {
                Title = "选择图片".Translate(),
                Filters = [new("图片".Translate(), ModMaker.ImageExtensions)],
            }
        );
        if (openFileDialog is null)
            return;
        var newImage = HKWImageUtils.LoadImageToMemory(openFileDialog.LocalPath);
        if (newImage is null)
        {
            DialogService.ShowMessageBox(
                this,
                "图片载入失败, 详情请查看日志".Translate(),
                "图片载入失败".Translate(),
                icon: MessageBoxImage.Warning
            );
            return;
        }
        Food.Image?.CloseStream();
        Food.Image = newImage;
    }

    /// <summary>
    /// 添加
    /// </summary>
    [RelayCommand]
    private void Add()
    {
        IsChanged = false;
        Food = new(ModInfo.TempI18nResource);
        using var dis = Food.Changed.Subscribe(_ => IsChanged = true);
        var result = DialogService.ShowDialog(this, this);
        if (result is not true)
        {
            Food.Image?.CloseStream();
            Food.I18nObserver.Dispose();
            Food = null!;
        }
        else
        {
            Food.I18nResource.CopyDataTo(ModInfo.I18nResource);
            Food.I18nResource = ModInfo.I18nResource;
            Foods.Add(Food);
            this.LogIfEnabled(LogLevel.Information)?.Log("添加新食物 '{id}'", Food.ID);
        }
        Reset();
    }

    /// <summary>
    /// 编辑
    /// </summary>
    /// <param name="model">模型</param>
    [RelayCommand]
    public void Edit(FoodModel model)
    {
        IsChanged = false;
        OldFood = model;
        var newModel = new FoodModel(model, ModInfo.TempI18nResource);
        model.I18nResource.CopyDataTo(newModel.I18nResource, model.ID, model.DescriptionID);
        Food = newModel;
        using var dis = newModel.Changed.Subscribe(_ => IsChanged = true);
        var result = DialogService.ShowDialog(this, this);
        if (result is not true)
        {
            newModel.I18nResource.ClearData();
            if (ReferenceEquals(model.Image, newModel.Image) is false)
                newModel.Image?.CloseStream();
            newModel.I18nObserver.Dispose();
        }
        else
        {
            if (ReferenceEquals(model.Image, newModel.Image) is false)
                model.Image?.CloseStream();
            model.I18nObserver.Dispose();

            newModel.I18nResource.CopyDataTo(ModInfo.I18nResource);
            newModel.I18nResource = ModInfo.I18nResource;
            Foods[Foods.IndexOf(model)] = newModel;
            this.LogIfEnabled(LogLevel.Information)
                ?.Log("编辑食物 {oldFood} => {newFood}", model.ID, newModel.ID);
        }
        Reset();
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="list">列表</param>
    [RelayCommand]
    private void Remove(IList list)
    {
        var models = list.Cast<FoodModel>().ToArray();
        var result = DialogService.ShowMessageBox(
            this,
            "确定删除已选中的 {0} 个食物吗".Translate(models.Length),
            "删除食物".Translate(),
            MessageBoxButton.YesNo
        );
        if (result is not true)
            return;
        foreach (var model in models)
        {
            Foods.Remove(model);
            model.Image?.CloseStream();
            model.I18nObserver.Dispose();
            this.LogIfEnabled(LogLevel.Information)?.Log("删除食物 '{id}'", model.ID);
        }
    }

    /// <summary>
    /// 重置
    /// </summary>
    public void Reset()
    {
        Food = null!;
        OldFood = null!;
        DialogResult = false;
        ModInfo.TempI18nResource.ClearData();
    }

    public void OnClosing(CancelEventArgs e)
    {
        if (IsChanged)
        {
            var result = DialogService.ShowMessageBox(
                this,
                "编辑完成, 是否保存?".Translate(),
                "编辑完成".Translate(),
                MessageBoxButton.YesNoCancel,
                MessageBoxImage.Information
            );
            if (result is null)
            {
                e.Cancel = true;
                return;
            }
            DialogResult = result;
        }

        if (DialogResult is not true)
            return;
        if (string.IsNullOrWhiteSpace(Food.ID))
        {
            DialogService.ShowMessageBox(
                this,
                "ID不可为空".Translate(),
                "数据错误".Translate(),
                MessageBoxButton.Ok,
                MessageBoxImage.Warning
            );
            e.Cancel = true;
        }
        else if (Food.Image is null)
        {
            DialogService.ShowMessageBox(
                this,
                "图像不可为空".Translate(),
                "数据错误".Translate(),
                MessageBoxButton.Ok,
                MessageBoxImage.Warning
            );
            e.Cancel = true;
        }
        else if (OldFood?.ID != Food.ID && ModInfo.Foods.Any(i => i.ID == Food.ID))
        {
            DialogService.ShowMessageBox(
                this,
                "此ID已存在".Translate(),
                "数据错误".Translate(),
                MessageBoxButton.Ok,
                MessageBoxImage.Warning
            );
            e.Cancel = true;
        }
        DialogResult = e.Cancel is not true;
    }

    Task IViewClosing.OnClosingAsync(CancelEventArgs e)
    {
        return Task.CompletedTask;
    }
}

/// <summary>
/// 食物搜索目标
/// </summary>
public enum FoodSearchTarget
{
    /// <summary>
    /// ID
    /// </summary>
    ID,

    /// <summary>
    /// 名称
    /// </summary>
    Name,

    /// <summary>
    /// 指定动画
    /// </summary>
    Graph,
}
