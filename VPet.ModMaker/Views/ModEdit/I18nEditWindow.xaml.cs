using System.Collections.Specialized;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using HKW.HKWUtils.Extensions;
using Panuon.WPF.UI;
using VPet.ModMaker.ViewModels;

namespace VPet.ModMaker.Views;

/// <summary>
/// I18nEditWindow.xaml 的交互逻辑
/// </summary>
public partial class I18nEditWindow : WindowX
{
    /// <summary>
    /// 视图模型
    /// </summary>
    public I18nEditViewModel ViewModel => (I18nEditViewModel)DataContext;

    /// <inheritdoc/>
    public I18nEditWindow()
    {
        InitializeComponent();
        DataContextChanged += I18nEditWindow_DataContextChanged;
    }

    protected override void OnClosed(EventArgs e)
    {
        ViewModel.ModInfo.I18nResource.ObservableCultures.CollectionChanged -=
            ObservableCultures_CulturesChanged;
        base.OnClosed(e);
    }

    private void I18nEditWindow_DataContextChanged(
        object sender,
        DependencyPropertyChangedEventArgs e
    )
    {
        foreach (var culture in ViewModel.ModInfo.I18nResource.IndexByCulture.Keys)
            AddCulture(culture);
        ViewModel.ModInfo.I18nResource.ObservableCultures.CollectionChanged +=
            ObservableCultures_CulturesChanged;
    }

    private void ObservableCultures_CulturesChanged(
        object? sender,
        NotifyCollectionChangedEventArgs e
    )
    {
        if (e.Action is NotifyCollectionChangedAction.Add)
        {
            foreach (var c in e.NewItems!.Cast<CultureInfo>())
                AddCulture(c);
        }
        else if (e.Action is NotifyCollectionChangedAction.Remove)
        {
            foreach (var c in e.OldItems!.Cast<CultureInfo>())
                RemoveCulture(c);
        }
        else if (e.Action is NotifyCollectionChangedAction.Replace)
        {
            var oldItem = e.OldItems!.Cast<CultureInfo>().First();
            var newItem = e.NewItems!.Cast<CultureInfo>().First();
            ReplaceCulture(oldItem, newItem);
        }
    }

    #region CultureEdit
    private const string ValueBindingFormat = "[{0}]";

    /// <summary>
    /// (culture, Column)
    /// </summary>
    private readonly Dictionary<CultureInfo, DataGridTextColumn> _dataGridI18nColumns = [];

    /// <summary>
    /// 添加文化列
    /// </summary>
    /// <param name="culture">文化</param>
    public void AddCulture(CultureInfo culture)
    {
        var index = ViewModel.ModInfo.I18nResource.GetIndexByCulture(culture);
        var dataPath = string.Format(ValueBindingFormat, index);
        // 文化数据列
        var column = CreateColumn(culture.Name, dataPath);
        DataGrid_Datas.Columns.Add(column);
        _dataGridI18nColumns.Add(culture, column);
    }

    /// <summary>
    /// 删除文化列
    /// </summary>
    /// <param name="culture">文化</param>
    public void RemoveCulture(CultureInfo culture)
    {
        DataGrid_Datas.Columns.Remove(_dataGridI18nColumns[culture]);
        _dataGridI18nColumns.Remove(culture);
        foreach (
            var (column, index) in _dataGridI18nColumns.Values.Zip(
                ViewModel.ModInfo.I18nResource.IndexByCulture.Values
            )
        )
        {
            var dataPath = string.Format(ValueBindingFormat, index);
            column.Binding = new Binding(dataPath) { Mode = BindingMode.TwoWay };
            column.SortMemberPath = dataPath;
        }
    }

    /// <summary>
    /// 替换文化列
    /// </summary>
    /// <param name="oldCultureName">旧文化</param>
    /// <param name="newCultureName">新文化</param>
    public void ReplaceCulture(CultureInfo oldCulture, CultureInfo newCulture)
    {
        var column = _dataGridI18nColumns[oldCulture];
        column.Header = newCulture;
    }

    private static DataGridTextColumn CreateColumn(string header, string dataPath)
    {
        return new DataGridTextColumn()
        {
            Width = 300,
            MinWidth = 100,
            MaxWidth = 500,
            Header = header,
            Binding = new Binding(dataPath) { Mode = BindingMode.TwoWay },
            ElementStyle = (Style)ModMaker.NativeStyles["TextBlock_Wrap"],
            SortMemberPath = dataPath,
            CanUserSort = true,
        };
    }

    #endregion

    private void DataGrid_Datas_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        ViewModel.CellEdit = true;
    }
}
