using Panuon.WPF.UI;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
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
        DataContext = new I18nEditWindowVM();
        Closed += (s, e) =>
        {
            //if (IsCancel)
            //ViewModel.Close();
            try
            {
                DataContext = null;
            }
            catch { }
        };
    }

    private readonly Dictionary<string, DataGridTextColumn> _dataGridI18nColumns = new();

    // TODO: 国际化标头
    private const string ValueBindingFormat = "Datas[{0}].Value";

    /// <summary>
    /// 添加文化列
    /// </summary>
    /// <param name="culture"></param>
    public void AddCulture(string culture)
    {
        // 文化数据列
        var column = new DataGridTextColumn()
        {
            MaxWidth = 200,
            Header = culture,
            Binding = new Binding(string.Format(ValueBindingFormat, culture))
            {
                Mode = BindingMode.TwoWay
            },
            ElementStyle = (Style)Utils.ModMakerStyles["TextBlock_LeftCenter"],
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
        column.Binding = new Binding(string.Format(ValueBindingFormat, newCulture));
        _dataGridI18nColumns.Remove(oldCulture);
        _dataGridI18nColumns.Add(newCulture, column);
    }
}
