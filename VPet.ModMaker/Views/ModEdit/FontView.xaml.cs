using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using VPet.ModMaker.Models;
using VPet.ModMaker.ViewModels;

namespace VPet.ModMaker.Views;

/// <summary>
/// FontView.xaml 的交互逻辑
/// </summary>
public partial class FontView : UserControl
{
    public FontView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 视图模型
    /// </summary>
    public FontEditViewModel ViewModel => (FontEditViewModel)DataContext;

    private void DataGrid_MouseDoubleClick(object? sender, MouseButtonEventArgs e)
    {
        if (sender is not DataGrid dataGrid || dataGrid.SelectedItem is not FontModel model)
            return;
        NativeUtils.OpenLink(model.FullName);
    }
}
