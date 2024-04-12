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

    public static I18nEditWindow Current { get; private set; } = null!;

    public I18nEditWindowVM ViewModel => (I18nEditWindowVM)DataContext;

    public I18nEditWindow()
    {
        // 只隐藏, 不关闭
        Closing += (s, e) =>
        {
            Hide();
            if (_close is false)
                e.Cancel = true;
        };
        Closed += (s, e) =>
        {
            //TODO
            //ViewModel.Close();
            try
            {
                DataContext = null;
                Current = null;
            }
            catch { }
        };
        InitializeComponent();
        DataContext = new I18nEditWindowVM();
        //TODO
        //ViewModel.CultureChanged += ViewModel_CultureChanged;
        //ViewModel.InitializeI18nData(ModInfoModel.Current);
    }

    public static void Initialize()
    {
        Current = new();
    }

    private bool _close = false;

    public void Close(bool close)
    {
        _close = close;
        Close();
    }

    private void ViewModel_CultureChanged(object? sender, string newCulture)
    {
        var oldCulture = sender as string;
        if (string.IsNullOrEmpty(oldCulture))
            AddCulture(newCulture);
        else if (string.IsNullOrEmpty(newCulture))
            RemoveCulture(oldCulture);
        else
            ReplaceCulture(oldCulture, newCulture);
    }

    #region CultureEdit
    private const string ValueBindingFormat = "Datas[{0}].Value";

    /// <summary>
    /// (culture, Column)
    /// </summary>
    private readonly Dictionary<string, DataGridTextColumn> _dataGridI18nColumns = [];

    /// <summary>
    /// 添加文化列
    /// </summary>
    /// <param name="culture"></param>
    public void AddCulture(string culture)
    {
        var dataPath = string.Format(ValueBindingFormat, culture);
        // 文化数据列
        var column = new DataGridTextColumn()
        {
            Width = 300,
            MinWidth = 100,
            MaxWidth = 500,
            Header = culture,
            Binding = new Binding(dataPath) { Mode = BindingMode.TwoWay },
            ElementStyle = (Style)ModMakerInfo.NativeStyles["TextBlock_Wrap"],
            SortMemberPath = dataPath
        };
        DataGrid_Datas.Columns.Add(column);
        _dataGridI18nColumns.Add(culture, column);
    }

    /// <summary>
    /// 删除文化列
    /// </summary>
    /// <param name="culture"></param>
    public void RemoveCulture(string culture)
    {
        DataGrid_Datas.Columns.Remove(_dataGridI18nColumns[culture]);
        _dataGridI18nColumns.Remove(culture);
        foreach (var columnData in _dataGridI18nColumns)
        {
            var dataPath = string.Format(ValueBindingFormat, culture);
            columnData.Value.Binding = new Binding(dataPath) { Mode = BindingMode.TwoWay };
            columnData.Value.SortMemberPath = dataPath;
        }
    }

    /// <summary>
    /// 替换文化列
    /// </summary>
    /// <param name="oldCulture"></param>
    /// <param name="newCulture"></param>
    public void ReplaceCulture(string oldCulture, string newCulture)
    {
        //if (_dataGridI18nColumns.ContainsKey(newCultureName))
        //    throw new();
        var column = _dataGridI18nColumns[oldCulture];
        column.Header = newCulture;
        _dataGridI18nColumns.Remove(oldCulture);
        _dataGridI18nColumns.Add(newCulture, column);
    }

    #endregion
}
