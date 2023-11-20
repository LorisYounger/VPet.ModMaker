using HKW.HKWViewModels.SimpleObservable;
using LinePutScript.Localization.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VPet.ModMaker.Models;
using VPet.ModMaker.Views.ModEdit.ClickTextEdit;

namespace VPet.ModMaker.ViewModels.ModEdit.ClickTextEdit;

public class ClickTextPageVM
{
    #region Value
    /// <summary>
    /// 显示的点击文本
    /// </summary>
    public ObservableValue<ObservableCollection<ClickTextModel>> ShowClickTexts { get; } = new();

    /// <summary>
    /// 点击文本
    /// </summary>
    public ObservableCollection<ClickTextModel> ClickTexts => ModInfoModel.Current.ClickTexts;

    /// <summary>
    /// 搜索
    /// </summary>
    public ObservableValue<string> Search { get; } = new();
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
        ShowClickTexts.Value = ClickTexts;
        Search.ValueChanged += Search_ValueChanged;
        AddCommand.ExecuteEvent += Add;
        EditCommand.ExecuteEvent += Edit;
        RemoveCommand.ExecuteEvent += Remove;
    }

    private void Search_ValueChanged(
        ObservableValue<string> sender,
        ValueChangedEventArgs<string> e
    )
    {
        if (string.IsNullOrWhiteSpace(e.NewValue))
        {
            ShowClickTexts.Value = ClickTexts;
        }
        else
        {
            ShowClickTexts.Value = new(
                ClickTexts.Where(
                    m => m.Id.Value.Contains(e.NewValue, StringComparison.OrdinalIgnoreCase)
                )
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
        ClickTexts.Add(vm.ClickText.Value);
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
        var newLowTest = vm.ClickText.Value = new(model);
        window.ShowDialog();
        if (window.IsCancel)
            return;
        ClickTexts[ClickTexts.IndexOf(model)] = newLowTest;
        if (ShowClickTexts.Value.Count != ClickTexts.Count)
            ShowClickTexts.Value[ShowClickTexts.Value.IndexOf(model)] = newLowTest;
    }

    /// <summary>
    /// 删除点击文本
    /// </summary>
    /// <param name="model">模型</param>
    private void Remove(ClickTextModel model)
    {
        if (MessageBox.Show("确定删除吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.No)
            return;
        if (ShowClickTexts.Value.Count == ClickTexts.Count)
        {
            ClickTexts.Remove(model);
        }
        else
        {
            ShowClickTexts.Value.Remove(model);
            ClickTexts.Remove(model);
        }
    }
}
