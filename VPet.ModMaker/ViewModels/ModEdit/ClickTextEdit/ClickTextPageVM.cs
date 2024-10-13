using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HKW.HKWReactiveUI;
using HKW.HKWUtils.Collections;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using LinePutScript.Localization.WPF;
using VPet.ModMaker.Models;
using VPet.ModMaker.Views.ModEdit.ClickTextEdit;

namespace VPet.ModMaker.ViewModels.ModEdit.ClickTextEdit;

public partial class ClickTextPageVM : ViewModelBase
{
    public ClickTextPageVM()
    {
        //ClickTexts = new(ModInfoModel.Current.ClickTexts)
        //{
        //    Filter = f => f.ID.Contains(Search, StringComparison.OrdinalIgnoreCase),
        //    FilteredList = new()
        //};
        ClickTexts = new(
            ModInfoModel.Current.ClickTexts,
            [],
            f => f.ID.Contains(Search, StringComparison.OrdinalIgnoreCase)
        );
        //TODO:
        //ClickTexts.BindingList(ModInfoModel.Current.ClickTexts);

        //AddCommand.ExecuteCommand += Add;
        //EditCommand.ExecuteCommand += Edit;
        //RemoveCommand.ExecuteCommand += Remove;
    }

    /// <summary>
    /// 显示的点击文本
    /// </summary>
    public FilterListWrapper<
        ClickTextModel,
        ObservableList<ClickTextModel>,
        ObservableList<ClickTextModel>
    > ClickTexts { get; set; } = null!;

    /// <summary>
    /// 搜索
    /// </summary>
    [ReactiveProperty]
    public string Search { get; set; } = string.Empty;

    partial void OnSearchChanged(string oldValue, string newValue)
    {
        ClickTexts.Refresh();
    }

    //#region Command
    ///// <summary>
    ///// 添加命令
    ///// </summary>
    //public ObservableCommand AddCommand { get; } = new();

    ///// <summary>
    ///// 编辑命令
    ///// </summary>
    //public ObservableCommand<ClickTextModel> EditCommand { get; } = new();

    ///// <summary>
    ///// 删除命令
    ///// </summary>
    //public ObservableCommand<ClickTextModel> RemoveCommand { get; } = new();
    //#endregion

    /// <summary>
    /// 添加点击文本
    /// </summary>
    [ReactiveCommand]
    private void Add()
    {
        var window = new ClickTextEditWindow();
        var vm = window.ViewModel;
        window.ShowDialog();
        if (window.IsCancel)
            return;
        ClickTexts.Add(vm.ClickText);
    }

    /// <summary>
    /// 编辑点击文本
    /// </summary>
    /// <param name="model">模型</param>
    [ReactiveCommand]
    public void Edit(ClickTextModel model)
    {
        var window = new ClickTextEditWindow();
        var vm = window.ViewModel;
        vm.OldClickText = model;
        var newModel = vm.ClickText = new(model)
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
        ClickTexts[ClickTexts.IndexOf(model)] = newModel;
        model.Close();
    }

    /// <summary>
    /// 删除点击文本
    /// </summary>
    /// <param name="model">模型</param>
    private void Remove(ClickTextModel model)
    {
        if (MessageBox.Show("确定删除吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.No)
            return;
        ClickTexts.Remove(model);
        model.Close();
    }
}
