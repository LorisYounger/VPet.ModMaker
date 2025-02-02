﻿using System.Collections;
using System.Collections.Frozen;
using System.ComponentModel;
using System.Reactive.Linq;
using DynamicData.Binding;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using HKW.HKWMapper;
using HKW.HKWReactiveUI;
using HKW.HKWUtils;
using HKW.HKWUtils.Collections;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using HKW.MVVMDialogs;
using HKW.WPF;
using HKW.WPF.Extensions;
using HKW.WPF.MVVMDialogs;
using LinePutScript;
using LinePutScript.Converter;
using LinePutScript.Localization.WPF;
using ReactiveUI;
using Splat;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels.ModEdit;

/// <summary>
/// 食物编辑视图模型
/// </summary>
public partial class FoodEditVM : DialogViewModel, IEnableLogger<ViewModelBase>, IDisposable
{
    /// <inheritdoc/>
    public FoodEditVM(ModInfoModel modInfo)
    {
        ModInfo = modInfo;
        Foods = new(
            modInfo.Foods,
            [],
            f =>
            {
                return SearchTargets.SelectedItem switch
                {
                    FoodSearchTarget.ID
                        => f.ID.Contains(Search, StringComparison.OrdinalIgnoreCase),
                    FoodSearchTarget.Name
                        => f.Name.Contains(Search, StringComparison.OrdinalIgnoreCase),
                    FoodSearchTarget.Graph
                        => f.Graph.Contains(Search, StringComparison.OrdinalIgnoreCase),
                    _ => false,
                };
            }
        );

        this.WhenAnyValue(
                x => x.Search,
                x => x.SearchTargets.SelectedItem,
                x => x.ModInfo.I18nResource.CurrentCulture
            )
            .Throttle(TimeSpan.FromSeconds(0.5), RxApp.TaskpoolScheduler)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => Foods.Refresh())
            .Record(this);

        Closing += FoodEditVM_Closing;
    }

    private void FoodEditVM_Closing(object? sender, CancelEventArgs e)
    {
        if (DialogResult is not true)
            return;
        if (string.IsNullOrWhiteSpace(Food.ID))
        {
            NativeUtils.DialogService.ShowMessageBoxX(
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
            NativeUtils.DialogService.ShowMessageBoxX(
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
            NativeUtils.DialogService.ShowMessageBoxX(
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

    #region Property

    /// <summary>
    /// 模组信息
    /// </summary>
    public ModInfoModel ModInfo { get; set; } = null!;

    /// <summary>
    /// 食物
    /// </summary>
    public FilterListWrapper<
        FoodModel,
        ObservableList<FoodModel>,
        ObservableList<FoodModel>
    > Foods { get; }

    /// <summary>
    /// 搜索
    /// </summary>
    [ReactiveProperty]
    public string Search { get; set; } = string.Empty;

    /// <summary>
    /// 搜索目标
    /// </summary>
    public ObservableSelectableSetWrapper<
        FoodSearchTarget,
        FrozenSet<FoodSearchTarget>
    > SearchTargets { get; } = new(EnumInfo<FoodSearchTarget>.Values);

    /// <summary>
    /// 旧食物
    /// </summary>
    public FoodModel OldFood { get; private set; } = null!;

    /// <summary>
    /// 食物
    /// </summary>
    [ReactiveProperty]
    public FoodModel Food { get; private set; } = null!;

    partial void OnFoodChanged(FoodModel oldValue, FoodModel newValue)
    {
        if (oldValue is not null)
            oldValue.PropertyChanged -= Food_PropertyChanged;
        if (newValue is not null)
            newValue.PropertyChanged += Food_PropertyChanged;
    }

    private void Food_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(FoodModel.ReferencePrice))
        {
            if (ModInfo.AutoSetFoodPrice)
                SetReferencePrice(Food.ReferencePrice);
        }
    }
    #endregion

    /// <summary>
    /// 设置参考价格
    /// </summary>
    /// <param name="value">价格</param>
    [ReactiveCommand]
    private void SetReferencePrice(double value)
    {
        if (Food.Price != Food.ReferencePrice)
            this.LogX().Info("食物 {food} 设置参考价格 {oldPrice} -> {price}", Food.ID, Food.Price, value);
        Food.Price = value;
    }

    [ReactiveCommand]
    private void SetReferencePriceForAllFood()
    {
        if (
            NativeUtils.DialogService.ShowMessageBoxX(
                this,
                "将所有食物设置为参考价格,你确定吗?".Translate(),
                "设置全部食物为参考价格".Translate(),
                MessageBoxButton.YesNo,
                MessageBoxImage.Information
            )
            is not true
        )
            return;
        var count = 0;
        foreach (var food in Foods)
        {
            if (food.Price == food.ReferencePrice)
                continue;

            this.LogX()
                .Debug(
                    "食物 {food} 设置参考价格 {oldPrice} -> {price}",
                    food.ID,
                    food.Price,
                    food.ReferencePrice
                );
            food.Price = food.ReferencePrice;
            count++;
        }
        this.LogX().Info("已为 {count} 个食物设置参考价格", count);
    }

    /// <summary>
    /// 添加图像
    /// </summary>
    [ReactiveCommand]
    private void AddImage()
    {
        var openFileDialog = NativeUtils.DialogService.ShowOpenFileDialog(
            this,
            new()
            {
                Title = "选择图片".Translate(),
                Filters = [new("图片".Translate(), ["jpg", "jpeg", "png", "bmp"])]
            }
        );
        if (openFileDialog is null)
            return;
        var newImage = HKWImageUtils.LoadImageToMemory(openFileDialog.LocalPath);
        if (newImage is null)
        {
            NativeUtils.DialogService.ShowMessageBoxX(
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
    [ReactiveCommand]
    private void ChangeImage()
    {
        var openFileDialog = NativeUtils.DialogService.ShowOpenFileDialog(
            this,
            new()
            {
                Title = "选择图片".Translate(),
                Filters = [new("图片".Translate(), ["jpg", "jpeg", "png", "bmp"])]
            }
        );
        if (openFileDialog is null)
            return;
        var newImage = HKWImageUtils.LoadImageToMemory(openFileDialog.LocalPath);
        if (newImage is null)
        {
            NativeUtils.DialogService.ShowMessageBoxX(
                this,
                "图片载入失败, 详情请查看日志".Translate(),
                "图片载入失败".Translate(),
                icon: MessageBoxImage.Warning
            );
            return;
        }
        Food.Image?.CloseStreamWhenNoReference();
        Food.Image = newImage;
    }

    /// <summary>
    /// 添加
    /// </summary>
    [ReactiveCommand]
    private async void Add()
    {
        ModInfo.TempI18nResource.ClearCultureData();
        Food = new() { I18nResource = ModInfo.TempI18nResource };
        await NativeUtils.DialogService.ShowDialogAsyncX(this, this);
        if (DialogResult is not true)
        {
            Food.Close();
        }
        else
        {
            Food.I18nResource.CopyDataTo(ModInfo.I18nResource, true);
            Food.I18nResource = ModInfo.I18nResource;
            Foods.Add(Food);
            if (this.LogX().Level is LogLevel.Info)
                this.LogX().Info("添加新食物 {food}", Food.ID);
            else
                this.LogX()
                    .Debug(
                        "添加新食物 {$food}",
                        LPSConvert.SerializeObjectToLine<Line>(Food.MapToFood(new()), "Food")
                    );
        }
        Reset();
    }

    /// <summary>
    /// 编辑
    /// </summary>
    /// <param name="model">模型</param>
    [ReactiveCommand]
    public async void Edit(FoodModel model)
    {
        OldFood = model;
        ModInfo.TempI18nResource.ClearCultureData();
        var newModel = new FoodModel(model) { I18nResource = ModInfo.TempI18nResource };
        model.I18nResource.CopyDataTo(newModel.I18nResource, [model.ID, model.DescriptionID], true);
        Food = newModel;
        await NativeUtils.DialogService.ShowDialogAsync(this, this);
        if (DialogResult is not true)
        {
            newModel.I18nResource.ClearCultureData();
            newModel.Close();
        }
        else
        {
            OldFood.Close();
            newModel.I18nResource.CopyDataTo(ModInfo.I18nResource, true);
            newModel.I18nResource = ModInfo.I18nResource;
            Foods[Foods.IndexOf(model)] = newModel;
            if (this.LogX().Level is LogLevel.Info)
                this.LogX().Info("编辑食物 {oldFood} => {newFood}", OldFood.ID, Food.ID);
            else
                this.LogX()
                    .Debug(
                        "编辑食物\n {$oldFood} => {$newFood}",
                        LPSConvert.SerializeObjectToLine<Line>(OldFood.MapToFood(new()), "Food"),
                        LPSConvert.SerializeObjectToLine<Line>(Food.MapToFood(new()), "Food")
                    );
        }
        Reset();
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="list">列表</param>
    [ReactiveCommand]
    private void Remove(IList list)
    {
        var models = list.Cast<FoodModel>().ToArray();
        if (
            NativeUtils.DialogService.ShowMessageBoxX(
                this,
                "确定删除已选中的 {0} 个食物吗".Translate(models.Length),
                "删除食物".Translate(),
                MessageBoxButton.YesNo
            )
            is not true
        )
            return;
        foreach (var model in models)
        {
            Foods.Remove(model);
            model.Close();
            this.LogX().Info("删除食物 {food}", model.ID);
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
        ModInfo.TempI18nResource.ClearCultureData();
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (_disposed)
            return;
        base.Dispose(disposing);
        if (disposing) { }
        Reset();
        ModInfo = null!;
        _disposed = false;
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
