using Microsoft.Win32;
using Panuon.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
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
using VPet.ModMaker.ViewModels.ModEdit;
using VPet.ModMaker.Views.ModEdit.ClickTextEdit;
using VPet.ModMaker.Views.ModEdit.FoodEdit;
using VPet.ModMaker.Views.ModEdit.LowTextEdit;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.Views.ModEdit;

/// <summary>
/// winModInfo.xaml 的交互逻辑
/// </summary>
public partial class ModEditWindow : Window
{
    public ModEditWindowVM ViewModel => (ModEditWindowVM)DataContext;
    public FoodPage FoodPage { get; } = new();
    public LowTextPage LowTextPage { get; } = new();
    public ClickTextPage ClickTextPage { get; } = new();

    public ModEditWindow()
    {
        InitializeComponent();
        Closed += Window_ModEdit_Closed;
        DataContext = new ModEditWindowVM(this);

        FoodPage.ViewModel.Foods.CollectionChanged += Foods_CollectionChanged;
        LowTextPage.ViewModel.LowTexts.CollectionChanged += LowTexts_CollectionChanged;
        ClickTextPage.ViewModel.ClickTexts.CollectionChanged += ClickTexts_CollectionChanged;
        TabItem_ClickText.Header =
            $"{TabItem_ClickText.Tag} ({ClickTextPage.ViewModel.ClickTexts.Count})";
        TabItem_LowText.Header = $"{TabItem_LowText.Tag} ({LowTextPage.ViewModel.LowTexts.Count})";
        TabItem_Food.Header = $"{TabItem_Food.Tag} ({FoodPage.ViewModel.Foods.Count})";
    }

    private void ClickTexts_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        ClickTextPage.ViewModel.ClickTexts.CollectionChanged += (s, e) =>
        {
            TabItem_ClickText.Header =
                $"{TabItem_ClickText.Tag} ({ClickTextPage.ViewModel.ClickTexts.Count})";
        };
    }

    private void LowTexts_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        LowTextPage.ViewModel.LowTexts.CollectionChanged += (s, e) =>
        {
            TabItem_LowText.Header =
                $"{TabItem_LowText.Tag} ({LowTextPage.ViewModel.LowTexts.Count})";
        };
    }

    private void Foods_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        FoodPage.ViewModel.Foods.CollectionChanged += (s, e) =>
        {
            TabItem_Food.Header = $"{TabItem_Food.Tag} ({FoodPage.ViewModel.Foods.Count})";
        };
    }

    private void Window_ModEdit_Closed(object sender, EventArgs e)
    {
        ViewModel.Close();
    }

    private void Button_AddItem_Click(object sender, RoutedEventArgs e) { }

    private void Button_AddAnime_Click(object sender, RoutedEventArgs e) { }

    private void Button_AddAudio_Click(object sender, RoutedEventArgs e) { }

    private void Button_AddClickText_Click(object sender, RoutedEventArgs e) { }

    private void Button_AddLang_Click(object sender, RoutedEventArgs e) { }
}
