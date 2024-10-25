using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using DynamicData.Binding;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using HKW.HKWReactiveUI;
using HKW.HKWUtils.Collections;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using HKW.MVVMDialogs;
using HKW.WPF.MVVMDialogs;
using LinePutScript;
using LinePutScript.Converter;
using LinePutScript.Localization.WPF;
using ReactiveUI;
using Splat;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels.ModEdit;

public partial class FoodEditVM : DialogViewModel
{
    private static IDialogService DialogService => Locator.Current.GetService<IDialogService>()!;

    public FoodEditVM()
    {
        Foods = new(
            [],
            [],
            f =>
            {
                if (SearchTargets.SelectedItem is FoodSearchTarget.ID)
                    return f.ID.Contains(Search, StringComparison.OrdinalIgnoreCase);
                else if (SearchTargets.SelectedItem is FoodSearchTarget.Name)
                    return f.Name.Contains(Search, StringComparison.OrdinalIgnoreCase);
                else if (SearchTargets.SelectedItem is FoodSearchTarget.Type)
                    return f.Type.ToString().Contains(Search, StringComparison.OrdinalIgnoreCase);
                else if (SearchTargets.SelectedItem is FoodSearchTarget.Graph)
                    return f.Graph.Contains(Search, StringComparison.OrdinalIgnoreCase);
                return false;
            }
        );

        this.WhenValueChanged(x => x.Search)
            .Throttle(TimeSpan.FromSeconds(1), RxApp.TaskpoolScheduler)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => Foods.Refresh());
        SearchTargets
            .WhenValueChanged(x => x.SelectedItem)
            .Throttle(TimeSpan.FromSeconds(1), RxApp.TaskpoolScheduler)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => Foods.Refresh());

        Closing += FoodEditVM_Closing;
    }

    private void FoodEditVM_Closing(object? sender, CancelEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(Food.ID))
        {
            DialogService.ShowMessageBoxX(
                this,
                "ID不可为空".Translate(),
                "数据错误".Translate(),
                MessageBoxButton.Ok,
                MessageBoxImage.Warning
            );
            e.Cancel = true;
            return;
        }
        if (Food.Image is null)
        {
            DialogService.ShowMessageBoxX(
                this,
                "图像不可为空".Translate(),
                "数据错误".Translate(),
                MessageBoxButton.Ok,
                MessageBoxImage.Warning
            );
            e.Cancel = true;
            return;
        }
        if (OldFood?.ID != Food.ID && ModInfo.Foods.Any(i => i.ID == Food.ID))
        {
            DialogService.ShowMessageBoxX(
                this,
                "此ID已存在",
                "数据错误".Translate(),
                MessageBoxButton.Ok,
                MessageBoxImage.Warning
            );
            e.Cancel = true;
            return;
        }
        DialogResult = true;
    }

    #region Property

    /// <summary>
    /// 模组信息
    /// </summary>
    [ReactiveProperty]
    public ModInfoModel ModInfo { get; set; } = null!;

    partial void OnModInfoChanged(ModInfoModel oldValue, ModInfoModel newValue)
    {
        Foods.AutoFilter = false;
        Foods.Clear();
        if (newValue is null)
            return;
        Foods.AddRange(newValue.Foods);
        Search = string.Empty;
        SearchTargets.SelectedItem = FoodSearchTarget.ID;
        Foods.AutoFilter = true;
    }

    public FilterListWrapper<
        FoodModel,
        ObservableList<FoodModel>,
        ObservableList<FoodModel>
    > Foods { get; }

    [ReactiveProperty]
    public string Search { get; set; } = string.Empty;

    public ObservableSelectableSet<
        FoodSearchTarget,
        FrozenSet<FoodSearchTarget>
    > SearchTargets { get; } = new(EnumInfo<FoodSearchTarget>.Values, FoodSearchTarget.ID);
    public FoodModel OldFood { get; private set; }

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
    /// 设置推荐价格
    /// </summary>
    /// <param name="value">价格</param>
    [ReactiveCommand]
    private void SetReferencePrice(double value)
    {
        Food.Price = value;
        this.Log().Info("设置推荐价格 {price}", value);
    }

    /// <summary>
    /// 添加图像
    /// </summary>
    [ReactiveCommand]
    private void AddImage()
    {
        var openFileDialog = DialogService.ShowOpenFileDialog(
            this,
            new()
            {
                Title = "选择图片".Translate(),
                Filters = [new("图片".Translate(), ["jpg", "jpeg", "png", "bmp"])]
            }
        );
        if (openFileDialog is null)
            return;
        Food.Image = NativeUtils.LoadImageToMemoryStream(openFileDialog.LocalPath);
    }

    /// <summary>
    /// 改变图像
    /// </summary>
    [ReactiveCommand]
    private void ChangeImage()
    {
        var openFileDialog = DialogService.ShowOpenFileDialog(
            this,
            new()
            {
                Title = "选择图片".Translate(),
                Filters = [new("图片".Translate(), ["jpg", "jpeg", "png", "bmp"])]
            }
        );
        if (openFileDialog is null)
            return;
        Food.Image?.StreamSource?.Close();
        Food.Image = NativeUtils.LoadImageToMemoryStream(openFileDialog.LocalPath);
    }

    public void Close()
    {
        Food.Close();
    }

    /// <summary>
    /// 添加
    /// </summary>
    [ReactiveCommand]
    private async void Add()
    {
        Food = new() { I18nResource = ModInfo.I18nResource };
        await DialogService.ShowSingletonDialogAsync(this, this);
        if (DialogResult is not true)
        {
            Food.Close();
        }
        else
        {
            Foods.Add(Food);
            if (this.Log().Level is LogLevel.Info)
                this.Log().Info("添加新食物 {food}", Food.ID);
            else
                this.Log()
                    .Debug(
                        "添加新食物 {$food}",
                        LPSConvert.SerializeObjectToLine<Line>(Food.ToFood(), "Food")
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
        var newModel = new FoodModel(model) { I18nResource = ModInfo.TempI18nResource };
        model.I18nResource.CopyDataTo(newModel.I18nResource, [model.ID, model.DescriptionID], true);
        Food = newModel;
        await DialogService.ShowDialogAsync(this, this);
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
            if (this.Log().Level is LogLevel.Info)
                this.Log().Info("编辑食物 {oldFood} => {newFood}", OldFood.ID, Food.ID);
            else
                this.Log()
                    .Debug(
                        "编辑食物\n {$oldFood} => {$newFood}",
                        LPSConvert.SerializeObjectToLine<Line>(OldFood.ToFood(), "Food"),
                        LPSConvert.SerializeObjectToLine<Line>(Food.ToFood(), "Food")
                    );
        }
        Reset();
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="model">模型</param>
    [ReactiveCommand]
    private void Remove(FoodModel model)
    {
        if (
            DialogService.ShowMessageBoxX(
                this,
                "确定删除 {0} 吗".Translate(model.ID),
                "删除食物".Translate(),
                MessageBoxButton.YesNo
            )
            is not true
        )
            return;
        Foods.Remove(model);
        model.Close();
        this.Log().Info("删除食物 {food}", model.ID);
        Reset();
    }

    public void Reset()
    {
        Food = null!;
        OldFood = null!;
        DialogResult = false;
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
    /// 类型
    /// </summary>
    Type,

    /// <summary>
    /// 指定动画
    /// </summary>
    Graph,
}
