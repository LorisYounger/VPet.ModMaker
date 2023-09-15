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
using VPet.ModMaker.Views.ModEdit.MoveEdit;
using VPet.ModMaker.Views.ModEdit.PetEdit;
using VPet.ModMaker.Views.ModEdit.SelectTextEdit;
using VPet.ModMaker.Views.ModEdit.WorkEdit;
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
    public SelectTextPage SelectTextPage { get; } = new();
    public PetPage PetPage { get; } = new();
    public WorkPage WorkPage { get; } = new();

    public MovePage MovePage { get; } = new();

    public ModEditWindow()
    {
        InitializeComponent();
        DataContext = new ModEditWindowVM(this);
        Closed += Window_ModEdit_Closed;
    }

    private void Window_ModEdit_Closed(object sender, EventArgs e)
    {
        ViewModel.Close();
        try
        {
            DataContext = null;
            FoodPage.DataContext = null;
            LowTextPage.DataContext = null;
            ClickTextPage.DataContext = null;
            SelectTextPage.DataContext = null;
            PetPage.DataContext = null;
            WorkPage.DataContext = null;
            MovePage.DataContext = null;
        }
        catch { }
    }
}
