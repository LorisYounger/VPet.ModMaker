using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
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
using VPet.ModMaker.Views.ModEdit;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.ViewModels.ModEdit;

public partial class SelectTextPageVM : ViewModelBase
{
    public SelectTextPageVM()
    {
        SelectTexts = new(
            ModInfoModel.Current.SelectTexts,
            [],
            f => f.ID.Contains(Search, StringComparison.OrdinalIgnoreCase)
        );
        SelectTexts
            .BaseList.WhenValueChanged(x => x.Count)
            .Throttle(TimeSpan.FromSeconds(1), RxApp.TaskpoolScheduler)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => SelectTexts.Refresh());

        this.WhenValueChanged(x => x.Search)
            .Throttle(TimeSpan.FromSeconds(1), RxApp.TaskpoolScheduler)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => SelectTexts.Refresh());
    }

    #region Property

    public FilterListWrapper<
        SelectTextModel,
        ObservableList<SelectTextModel>,
        ObservableList<SelectTextModel>
    > SelectTexts { get; set; }

    [ReactiveProperty]
    public string Search { get; set; } = string.Empty;

    #endregion
    /// <summary>
    /// 添加
    /// </summary>
    [ReactiveCommand]
    private void Add()
    {
        var window = new SelectTextEditWindow();
        var vm = window.ViewModel;
        window.ShowDialog();
        if (window.IsCancel)
            return;
        SelectTexts.Add(vm.SelectText);
    }

    /// <summary>
    /// 编辑
    /// </summary>
    /// <param name="model">模型</param>
    [ReactiveCommand]
    public void Edit(SelectTextModel model)
    {
        var window = new SelectTextEditWindow();
        var vm = window.ViewModel;
        vm.OldSelectText = model;
        var newModel = vm.SelectText = new(model)
        {
            I18nResource = ModInfoModel.Current.I18nResource
        };
        model.I18nResource.CopyDataTo(newModel.I18nResource, [model.ID, model.ChooseID], true);
        window.ShowDialog();
        if (window.IsCancel)
        {
            newModel.I18nResource.ClearCultureData();
            newModel.Close();
            return;
        }
        newModel.I18nResource.CopyDataTo(ModInfoModel.Current.I18nResource, true);
        newModel.I18nResource = ModInfoModel.Current.I18nResource;
        SelectTexts[SelectTexts.IndexOf(model)] = newModel;
        model.Close();
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="model">模型</param>
    [ReactiveCommand]
    private void Remove(SelectTextModel model)
    {
        if (MessageBox.Show("确定删除吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.No)
            return;
        SelectTexts.Remove(model);
        model.Close();
    }
}
