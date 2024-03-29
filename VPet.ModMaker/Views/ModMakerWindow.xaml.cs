﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
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
using LinePutScript;
using LinePutScript.Localization.WPF;
using Panuon.WPF.UI;
using VPet.ModMaker.Models;
using VPet.ModMaker.ViewModels;
using VPet.ModMaker.Views.ModEdit;
using VPet.ModMaker.Views.ModEdit.AnimeEdit;
using VPet.ModMaker.Views.ModEdit.PetEdit;

namespace VPet.ModMaker.Views;

/// <summary>
/// winModMaker.xaml 的交互逻辑
/// </summary>
public partial class ModMakerWindow : WindowX
{
    public ModMakerWindowVM ViewModel => (ModMakerWindowVM)DataContext;
    public Models.ModMaker ModMaker { get; internal set; }

    public ModMakerWindow()
    {
        InitializeComponent();
        DataContext = new ModMakerWindowVM(this);
        Closed += ModMakerWindow_Closed;
        //new AnimeEditWindow().Show();
    }

    private void ModMakerWindow_Closed(object sender, EventArgs e)
    {
        // 当模组载入错误时, 会产生野窗口, 需要手动关闭
        foreach (var item in Application.Current.Windows)
        {
            if (item is ModEditWindow window)
                window.Close();
        }
    }

    private void ListBoxItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is not ListBoxItem item)
            return;
        if (item.DataContext is not ModMakeHistory history)
            return;
        if (Directory.Exists(history.SourcePath) is false)
        {
            if (
                MessageBox.Show($"路径不存在, 是否删除?".Translate(), "", MessageBoxButton.YesNo)
                is MessageBoxResult.Yes
            )
            {
                ViewModel.Histories.Remove(history);
                ViewModel.ShowHistories.Remove(history);
            }
        }
        ViewModel.LoadMod(history.SourcePath);
    }

    private void Hyperlink_Click(object sender, RoutedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://github.com/LorisYounger/VPet.ModMaker/wiki"));
    }
}
