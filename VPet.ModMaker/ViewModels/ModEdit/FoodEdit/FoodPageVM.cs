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
using ReactiveUI;
using VPet.ModMaker.Models;
using VPet.ModMaker.Views.ModEdit.FoodEdit;

namespace VPet.ModMaker.ViewModels.ModEdit.FoodEdit;

public partial class FoodPageVM : ViewModelBase
{
    public FoodPageVM()
    {
        Foods = new(
            new(ModInfoModel.Current.Foods),
            [],
            f => f.ID.Contains(Search, StringComparison.OrdinalIgnoreCase)
        );
        Foods
            .BaseList.WhenValueChanged(x => x.Count)
            .Throttle(TimeSpan.FromSeconds(1), RxApp.TaskpoolScheduler)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => Foods.Refresh());

        this.WhenValueChanged(x => x.Search)
            .Throttle(TimeSpan.FromSeconds(1), RxApp.TaskpoolScheduler)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => Foods.Refresh());
    }

    #region Property

    public FilterListWrapper<
        FoodModel,
        ObservableList<FoodModel>,
        ObservableList<FoodModel>
    > Foods { get; set; }

    [ReactiveProperty]
    public string Search { get; set; } = string.Empty;

    #endregion

    /// <summary>
    /// 添加
    /// </summary>
    [ReactiveCommand]
    private void Add()
    {
        var window = new FoodEditWindow();
        var vm = window.ViewModel;
        window.ShowDialog();
        if (window.IsCancel)
            return;
        Foods.Add(vm.Food);
    }

    /// <summary>
    /// 编辑
    /// </summary>
    /// <param name="model">模型</param>
    [ReactiveCommand]
    public void Edit(FoodModel model)
    {
        var window = new FoodEditWindow();
        var vm = window.ViewModel;
        vm.OldFood = model;
        var newModel = vm.Food = new(model)
        {
            I18nResource = ModInfoModel.Current.TempI18nResource
        };
        model.I18nResource.CopyDataTo(newModel.I18nResource, [model.ID, model.DescriptionID], true);
        window.ShowDialog();
        if (window.IsCancel)
        {
            newModel.I18nResource.ClearCultureData();
            newModel.Close();
            return;
        }
        newModel.I18nResource.CopyDataTo(ModInfoModel.Current.I18nResource, true);
        newModel.I18nResource = ModInfoModel.Current.I18nResource;
        Foods[Foods.IndexOf(model)] = newModel;
        model.Close();
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="model">模型</param>
    [ReactiveCommand]
    private void Remove(FoodModel model)
    {
        if (MessageBox.Show("确定删除吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.No)
            return;
        Foods.Remove(model);
        model.Close();
    }
}
