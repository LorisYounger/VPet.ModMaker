using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using Panuon.WPF.UI;
using VPet.ModMaker.Models;
using VPet.ModMaker.ViewModels.ModEdit.I18nEdit;

namespace VPet.ModMaker.Views.ModEdit.I18nEdit;

/// <summary>
/// I18nEditWindow.xaml 的交互逻辑
/// </summary>
public partial class I18nEditWindow : WindowX
{
    public bool IsCancel { get; private set; } = true;

    public I18nEditWindowVM ViewModel => (I18nEditWindowVM)DataContext;

    public I18nEditWindow()
    {
        InitializeComponent();
        this.SetDataContext<I18nEditWindowVM>();
        this.SetCloseState(WindowCloseState.Collapsed);
        foreach (var culture in ModInfoModel.Current.I18nResource.Cultures)
            AddCulture(culture.Name);
        ViewModel.CultureChanged += ViewModel_CultureChanged;
    }

    private void ViewModel_CultureChanged(string oldCultureName, string newCultureName)
    {
        if (string.IsNullOrEmpty(oldCultureName))
            AddCulture(newCultureName);
        else if (string.IsNullOrEmpty(newCultureName))
            RemoveCulture(oldCultureName);
        else
            ReplaceCulture(oldCultureName, newCultureName);
    }

    #region CultureEdit
    private const string ValueBindingFormat = "[{0}]";

    /// <summary>
    /// (culture, Column)
    /// </summary>
    private readonly Dictionary<string, DataGridTextColumn> _dataGridI18nColumns = [];

    /// <summary>
    /// 添加文化列
    /// </summary>
    /// <param name="cultureName"></param>
    public void AddCulture(string cultureName)
    {
        var dataPath = string.Format(ValueBindingFormat, cultureName);
        // 文化数据列
        var column = CreateColumn(cultureName, dataPath);
        DataGrid_Datas.Columns.Add(column);
        _dataGridI18nColumns.Add(cultureName, column);
    }

    /// <summary>
    /// 删除文化列
    /// </summary>
    /// <param name="cultureName"></param>
    public void RemoveCulture(string cultureName)
    {
        DataGrid_Datas.Columns.Remove(_dataGridI18nColumns[cultureName]);
        _dataGridI18nColumns.Remove(cultureName);
        foreach (var columnData in _dataGridI18nColumns)
        {
            var dataPath = string.Format(ValueBindingFormat, cultureName);
            columnData.Value.Binding = new Binding(dataPath) { Mode = BindingMode.TwoWay };
            columnData.Value.SortMemberPath = dataPath;
        }
    }

    /// <summary>
    /// 替换文化列
    /// </summary>
    /// <param name="oldCultureName"></param>
    /// <param name="newCultureName"></param>
    public void ReplaceCulture(string oldCultureName, string newCultureName)
    {
        //if (_dataGridI18nColumns.ContainsKey(newCultureName))
        //    throw new();
        var column = _dataGridI18nColumns[oldCultureName];
        column.Header = newCultureName;
        _dataGridI18nColumns.Remove(oldCultureName);
        _dataGridI18nColumns.Add(newCultureName, column);
    }

    public static DataGridTextColumn CreateColumn(string header, string dataPath)
    {
        return new DataGridTextColumn()
        {
            Width = 300,
            MinWidth = 100,
            MaxWidth = 500,
            Header = header,
            Binding = new Binding(dataPath) { Mode = BindingMode.TwoWay },
            ElementStyle = (Style)ModMakerInfo.NativeStyles["TextBlock_Wrap"],
            SortMemberPath = dataPath,
            CanUserSort = true
        };
    }

    #endregion
}
