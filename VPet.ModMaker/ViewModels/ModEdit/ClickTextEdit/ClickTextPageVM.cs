using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using LinePutScript.Localization.WPF;
using VPet.ModMaker.Models;
using VPet.ModMaker.Views.ModEdit.ClickTextEdit;

namespace VPet.ModMaker.ViewModels.ModEdit.ClickTextEdit;

public class ClickTextPageVM : ObservableObjectX
{
    public ClickTextPageVM()
    {
        ClickTexts = new(ModInfoModel.Current.ClickTexts)
        {
            Filter = f => f.ID.Contains(Search, StringComparison.OrdinalIgnoreCase),
            FilteredList = new()
        };
        ClickTexts.BindingList(ModInfoModel.Current.ClickTexts);
        AddCommand.ExecuteCommand += AddCommand_ExecuteCommand;
        EditCommand.ExecuteCommand += EditCommand_ExecuteCommand;
        RemoveCommand.ExecuteCommand += RemoveCommand_ExecuteCommand;
    }

    #region Value
    #region ShowClickTexts
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ObservableFilterList<ClickTextModel, ObservableList<ClickTextModel>> _clickTexts =
        null!;

    /// <summary>
    /// 显示的点击文本
    /// </summary>
    public ObservableFilterList<ClickTextModel, ObservableList<ClickTextModel>> ClickTexts
    {
        get => _clickTexts;
        set => SetProperty(ref _clickTexts, value);
    }
    #endregion

    #region Search
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _search = string.Empty;

    /// <summary>
    /// 搜索
    /// </summary>
    public string Search
    {
        get => _search;
        set
        {
            SetProperty(ref _search, value);
            ClickTexts.Refresh();
        }
    }
    #endregion
    #endregion
    #region Command
    /// <summary>
    /// 添加命令
    /// </summary>
    public ObservableCommand AddCommand { get; } = new();

    /// <summary>
    /// 编辑命令
    /// </summary>
    public ObservableCommand<ClickTextModel> EditCommand { get; } = new();

    /// <summary>
    /// 删除命令
    /// </summary>
    public ObservableCommand<ClickTextModel> RemoveCommand { get; } = new();
    #endregion

    /// <summary>
    /// 添加点击文本
    /// </summary>
    private void AddCommand_ExecuteCommand()
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
    public void EditCommand_ExecuteCommand(ClickTextModel model)
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
    private void RemoveCommand_ExecuteCommand(ClickTextModel model)
    {
        if (MessageBox.Show("确定删除吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.No)
            return;
        ClickTexts.Remove(model);
        model.Close();
    }
}
