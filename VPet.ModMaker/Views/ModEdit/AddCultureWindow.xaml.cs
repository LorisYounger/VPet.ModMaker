﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
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
using LinePutScript.Localization.WPF;
using Panuon.WPF.UI;
using VPet.ModMaker.Models;
using VPet.ModMaker.ViewModels;
using VPet.ModMaker.ViewModels.ModEdit;

namespace VPet.ModMaker.Views.ModEdit;

/// <summary>
/// Window_AddLang.xaml 的交互逻辑
/// </summary>
public partial class AddCultureWindow : WindowX
{
    public bool IsCancel { get; private set; } = true;

    public AddCultureWindowVM ViewModel => (AddCultureWindowVM)DataContext;

    public AddCultureWindow()
    {
        InitializeComponent();
        DataContext = new AddCultureWindowVM();
        TextBox_Lang.Focus();
        TextBox_Lang.Dispatcher.InvokeAsync(TextBox_Lang.SelectAll);
    }

    private void Button_Cancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Button_Yes_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(ViewModel.Culture))
        {
            MessageBox.Show("文化不可为空".Translate(), "", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        if (I18nHelper.Current.CultureNames.Contains(ViewModel.Culture))
        {
            MessageBox.Show("此文化已存在".Translate(), "", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        IsCancel = false;
        Close();
    }

    private void Hyperlink_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(
            new ProcessStartInfo(
                "https://learn.microsoft.com/en-us/openspecs/windows_protocols/ms-lcid/a9eac961-e77d-41a6-90a5-ce1a8b0cdb9c"
            )
        );
    }
}
