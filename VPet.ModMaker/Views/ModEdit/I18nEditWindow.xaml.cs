using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Panuon.WPF.UI;
using VPet.ModMaker.ViewModels.ModEdit;

namespace VPet.ModMaker.Views.ModEdit;

/// <summary>
/// I18nEditWindow.xaml 的交互逻辑
/// </summary>
public partial class I18nEditWindow : WindowX
{
    /// <summary>
    /// 视图模型
    /// </summary>
    public I18nEditVM ViewModel => (I18nEditVM)DataContext;

    /// <inheritdoc/>
    public I18nEditWindow()
    {
        InitializeComponent();
        DataContextChanged += I18nEditWindow_DataContextChanged;
    }

    private void I18nEditWindow_DataContextChanged(
        object sender,
        DependencyPropertyChangedEventArgs e
    )
    {
        foreach (var culture in ViewModel.ModInfo.I18nResource.Cultures)
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
    /// <param name="cultureName">文化名称</param>
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
    /// <param name="cultureName">文化名称</param>
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
    /// <param name="oldCultureName">旧文化</param>
    /// <param name="newCultureName">新文化</param>
    public void ReplaceCulture(string oldCultureName, string newCultureName)
    {
        var column = _dataGridI18nColumns[oldCultureName];
        column.Header = newCultureName;
        _dataGridI18nColumns.Remove(oldCultureName);
        _dataGridI18nColumns.Add(newCultureName, column);
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
            ElementStyle = (Style)NativeData.NativeStyles["TextBlock_Wrap"],
            SortMemberPath = dataPath,
            CanUserSort = true
        };
    }

    #endregion

    private void DataGrid_Datas_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
    {
        ViewModel.CellEdit = true;
    }
}
