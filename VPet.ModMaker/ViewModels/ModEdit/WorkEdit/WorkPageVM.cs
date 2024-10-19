using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using VPet.ModMaker.Views.ModEdit;

namespace VPet.ModMaker.ViewModels.ModEdit;

public partial class WorkPageVM : ViewModelBase
{
    public WorkPageVM()
    {
        Works = new([], [], f => f.ID.Contains(Search, StringComparison.OrdinalIgnoreCase));

        if (Pets.HasValue())
            CurrentPet = Pets.FirstOrDefault(
                m => m.FromMain is false && m.Works.HasValue(),
                Pets.First()
            );

        ModInfo
            .WhenValueChanged(x => x.ShowMainPet)
            .Throttle(TimeSpan.FromSeconds(1), RxApp.TaskpoolScheduler)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ =>
            {
                if (CurrentPet?.FromMain is false)
                    CurrentPet = null!;
            });

        this.WhenValueChanged(x => x.Search)
            .Throttle(TimeSpan.FromSeconds(1), RxApp.TaskpoolScheduler)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => Works.Refresh());
    }

    public static ModInfoModel ModInfo => ModInfoModel.Current;

    #region Property

    public FilterListWrapper<
        WorkModel,
        ObservableList<WorkModel>,
        ObservableList<WorkModel>
    > Works { get; set; }

    public static ObservableList<PetModel> Pets => ModInfoModel.Current.Pets;

    [ReactiveProperty]
    public PetModel CurrentPet { get; set; } = null!;

    partial void OnCurrentPetChanged(PetModel oldValue, PetModel newValue)
    {
        Works.Clear();
        if (oldValue is not null)
            Works.BaseList.BindingList(oldValue.Works, true);
        if (newValue is null)
            return;
        Works.AddRange(CurrentPet.Works);
        Works.BaseList.BindingList(CurrentPet.Works);
    }

    [ReactiveProperty]
    public string Search { get; set; } = string.Empty;

    #endregion
    /// <summary>
    /// 添加
    /// </summary>
    [ReactiveCommand]
    private void Add()
    {
        var window = new WorkEditWindow();
        var vm = window.ViewModel;
        vm.CurrentPet = CurrentPet;
        window.ShowDialog();
        if (window.IsCancel)
            return;
        Works.Add(vm.Work);
    }

    /// <summary>
    /// 编辑
    /// </summary>
    /// <param name="model">模型</param>
    [ReactiveCommand]
    public void Edit(WorkModel model)
    {
        var window = new WorkEditWindow();
        var vm = window.ViewModel;
        vm.CurrentPet = CurrentPet;
        vm.OldWork = model;
        var newModel = vm.Work = new(model)
        {
            I18nResource = ModInfoModel.Current.TempI18nResource
        };
        model.I18nResource.CopyDataTo(newModel.I18nResource, model.ID, true);
        window.ShowDialog();
        if (window.IsCancel)
        {
            newModel.I18nResource.ClearCultureData();
            newModel.Close();
            return;
        }
        newModel.I18nResource.CopyDataTo(ModInfoModel.Current.I18nResource, true);
        newModel.I18nResource = ModInfoModel.Current.I18nResource;
        Works[Works.IndexOf(model)] = newModel;
        model.Close();
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="model">模型</param>
    [ReactiveCommand]
    private void Remove(WorkModel model)
    {
        if (MessageBox.Show("确定删除吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.No)
            return;
        Works.Remove(model);
        model.Close();
    }
}
