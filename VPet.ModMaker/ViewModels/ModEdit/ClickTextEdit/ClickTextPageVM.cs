using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HKW.HKWUtils.Observable;
using LinePutScript.Localization.WPF;
using VPet.ModMaker.Models;
using VPet.ModMaker.Views.ModEdit.ClickTextEdit;

namespace VPet.ModMaker.ViewModels.ModEdit.ClickTextEdit;

public class ClickTextPageVM : ObservableObjectX<ClickTextPageVM>
{
    #region Value
    #region ShowClickTexts
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ObservableCollection<ClickTextModel> _showClickTexts;

    /// <summary>
    /// 显示的点击文本
    /// </summary>
    public ObservableCollection<ClickTextModel> ShowClickTexts
    {
        get => _showClickTexts;
        set => SetProperty(ref _showClickTexts, value);
    }
    #endregion

    /// <summary>
    /// 点击文本
    /// </summary>
    public ObservableCollection<ClickTextModel> ClickTexts => ModInfoModel.Current.ClickTexts;

    #region Search
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _search;

    /// <summary>
    /// 搜索
    /// </summary>
    public string Search
    {
        get => _search;
        set => SetProperty(ref _search, value);
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

    public ClickTextPageVM()
    {
        ShowClickTexts = ClickTexts;
        //TODO
        //Search.ValueChanged += Search_ValueChanged;
        AddCommand.ExecuteCommand += Add;
        EditCommand.ExecuteCommand += Edit;
        RemoveCommand.ExecuteCommand += Remove;
    }

    private void Search_ValueChanged(
        ObservableValue<string> sender,
        ValueChangedEventArgs<string> e
    )
    {
        if (string.IsNullOrWhiteSpace(e.NewValue))
        {
            ShowClickTexts = ClickTexts;
        }
        else
        {
            ShowClickTexts = new(
                ClickTexts.Where(m => m.Id.Contains(e.NewValue, StringComparison.OrdinalIgnoreCase))
            );
        }
    }

    /// <summary>
    /// 添加点击文本
    /// </summary>
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
    public void Edit(ClickTextModel model)
    {
        var window = new ClickTextEditWindow();
        var vm = window.ViewModel;
        vm.OldClickText = model;
        var newLowTest = vm.ClickText = new(model);
        window.ShowDialog();
        if (window.IsCancel)
            return;
        ClickTexts[ClickTexts.IndexOf(model)] = newLowTest;
        if (ShowClickTexts.Count != ClickTexts.Count)
            ShowClickTexts[ShowClickTexts.IndexOf(model)] = newLowTest;
    }

    /// <summary>
    /// 删除点击文本
    /// </summary>
    /// <param name="model">模型</param>
    private void Remove(ClickTextModel model)
    {
        if (MessageBox.Show("确定删除吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.No)
            return;
        if (ShowClickTexts.Count == ClickTexts.Count)
        {
            ClickTexts.Remove(model);
        }
        else
        {
            ShowClickTexts.Remove(model);
            ClickTexts.Remove(model);
        }
    }
}
