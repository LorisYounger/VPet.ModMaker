using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicData.Binding;
using HKW.HKWReactiveUI;
using HKW.HKWUtils.Collections;
using HKW.HKWUtils.Observable;
using ReactiveUI;
using VPet.ModMaker.Models;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.ViewModels.ModEdit;

public partial class ClickTextEditVM : ViewModelBase
{
    public ClickTextEditVM()
    {
        ClickTexts = new([], [], f => f.ID.Contains(Search, StringComparison.OrdinalIgnoreCase));

        ClickTexts
            .BaseList.WhenValueChanged(x => x.Count)
            .Throttle(TimeSpan.FromSeconds(1), RxApp.TaskpoolScheduler)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => ClickTexts.Refresh());

        this.WhenValueChanged(x => x.Search)
            .Throttle(TimeSpan.FromSeconds(1), RxApp.TaskpoolScheduler)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => ClickTexts.Refresh());
    }

    /// <summary>
    /// 模组信息
    /// </summary>
    [ReactiveProperty]
    public ModInfoModel ModInfo { get; set; } = null!;

    /// <summary>
    /// 显示的点击文本
    /// </summary>
    public FilterListWrapper<
        ClickTextModel,
        ObservableList<ClickTextModel>,
        ObservableList<ClickTextModel>
    > ClickTexts { get; }

    /// <summary>
    /// 搜索
    /// </summary>
    [ReactiveProperty]
    public string Search { get; set; } = string.Empty;

    /// <summary>
    /// I18n资源
    /// </summary>
    public I18nResource<string, string> I18nResource => ModInfo.I18nResource;

    /// <summary>
    /// 旧点击文本
    /// </summary>
    public ClickTextModel? OldClickText { get; set; }

    /// <summary>
    /// 点击文本
    /// </summary>
    [ReactiveProperty]
    public ClickTextModel ClickText { get; set; } = null!;

    /// <summary>
    /// 添加
    /// </summary>
    [ReactiveCommand]
    private void Add()
    {
        //var window = new ClickTextEditWindow();
        //var vm = window.ViewModel;
        //window.ShowDialog();
        //if (window.IsCancel)
        //    return;
        //ClickTexts.Add(vm.ClickText);
    }

    /// <summary>
    /// 编辑
    /// </summary>
    /// <param name="model">模型</param>
    [ReactiveCommand]
    public void Edit(ClickTextModel model)
    {
        //var window = new ClickTextEditWindow();
        //var vm = window.ViewModel;
        //vm.OldClickText = model;
        //var newModel = vm.ClickText = new(model) { I18nResource = ModInfo.TempI18nResource };
        //model.I18nResource.CopyDataTo(newModel.I18nResource, model.ID, true);
        //window.ShowDialog();
        //if (window.IsCancel)
        //{
        //    newModel.I18nResource.ClearCultureData();
        //    newModel.Close();
        //    return;
        //}
        //newModel.I18nResource.CopyDataTo(ModInfo.I18nResource, true);
        //newModel.I18nResource = ModInfo.I18nResource;
        //ClickTexts[ClickTexts.IndexOf(model)] = newModel;
        //model.Close();
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="model">模型</param>
    [ReactiveCommand]
    private void Remove(ClickTextModel model)
    {
        //if (MessageBox.Show("确定删除吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.No)
        //    return;
        //ClickTexts.Remove(model);
        //model.Close();
    }
}
