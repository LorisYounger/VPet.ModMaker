using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using HKW.HKWUtils.Observable;
using HKW.MVVMDialogs;
using HKW.WPF.Extensions;
using LinePutScript.Localization.WPF;
using Panuon.WPF.UI;
using VPet.ModMaker.Models;
using VPet.ModMaker.ViewModels;
using VPet.ModMaker.ViewModels.ModEdit;

namespace VPet.ModMaker.Views.ModEdit;

/// <summary>
/// Window_AddLang.xaml 的交互逻辑
/// </summary>
public partial class AddCulturePage : UserControl, IDialogPage<Window>
{
    public bool IsCancel { get; private set; } = true;

    public AddCultureVM ViewModel => (AddCultureVM)DataContext;

    public Window DialogWindow { get; set; } = null!;

    public AddCulturePage()
    {
        InitializeComponent();
        TextBox_Lang.Focus();
        TextBox_Lang.Dispatcher.InvokeAsync(TextBox_Lang.SelectAll);
        DataContextChanged += AddCulturePage_DataContextChanged;
    }

    private void AddCulturePage_DataContextChanged(
        object sender,
        DependencyPropertyChangedEventArgs e
    )
    {
        if (ViewModel is null)
            return;
        DialogWindow.SetLocationToCenter();
    }

    private void Button_Cancel_Click(object? sender, RoutedEventArgs e)
    {
        DialogWindow.Close();
    }

    private void Button_Yes_Click(object? sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ViewModel.CultureName))
        {
            MessageBox.Show(
                "文化不可为空".Translate(),
                "数据错误".Translate(),
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
            return;
        }
        if (CultureUtils.TryGetCultureInfo(ViewModel.CultureName, out var culture) is false)
        {
            MessageBox.Show(
                "不支持的文化".Translate(),
                "数据错误".Translate(),
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
            return;
        }
        if (ViewModel.ModInfo.I18nResource.Cultures.Contains(culture))
        {
            MessageBox.Show(
                "此文化已存在".Translate(),
                "数据错误".Translate(),
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
            return;
        }
        IsCancel = false;
    }

    public const string CultureLink =
        "https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-lcid/a9eac961-e77d-41a6-90a5-ce1a8b0cdb9c";

    private void Hyperlink_Click(object? sender, RoutedEventArgs e)
    {
        try
        {
            NativeUtils.OpenLink(CultureLink);
        }
        catch
        {
            if (
                MessageBoxX.Show(
                    DialogWindow,
                    "无法打开链接,需要复制自行访问吗",
                    "",
                    MessageBoxButton.YesNo,
                    MessageBoxIcon.Warning
                )
                is not MessageBoxResult.Yes
            )
                return;
            Clipboard.SetText(CultureLink);
            MessageBoxX.Show(DialogWindow, "已复制到剪贴板".Translate());
        }
    }
}
