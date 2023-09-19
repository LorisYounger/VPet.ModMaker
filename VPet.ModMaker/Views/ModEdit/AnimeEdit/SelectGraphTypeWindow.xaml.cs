using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using VPet.ModMaker.Models.ModModel;
using VPet_Simulator.Core;

namespace VPet.ModMaker.Views.ModEdit.AnimeEdit;

/// <summary>
/// SelectGraphTypeWindow.xaml 的交互逻辑
/// </summary>
public partial class SelectGraphTypeWindow : Window
{
    public SelectGraphTypeWindow()
    {
        InitializeComponent();
        DataContext = this;
        CurrentPet.ValueChanged += CurrentPet_ValueChanged;
        Closed += (s, e) =>
        {
            try
            {
                DataContext = null;
            }
            catch { }
        };
    }

    private void CurrentPet_ValueChanged(PetModel oldValue, PetModel newValue)
    {
        if (CurrentPet.Value.Animes.Count == 0)
        {
            GraphTypes.Value.Add(GraphInfo.GraphType.Default);
            return;
        }
        GraphTypes.Value = new(
            AnimeTypeModel.GraphTypes.Except(CurrentPet.Value.Animes.Select(m => m.GraphType.Value))
        );
    }

    public ObservableValue<PetModel> CurrentPet { get; } = new();
    public ObservableValue<GraphInfo.GraphType> GraphType { get; } = new();
    public ObservableValue<ObservableCollection<GraphInfo.GraphType>> GraphTypes { get; } =
        new(new());

    public bool IsCancel { get; private set; } = true;

    private void Button_Cancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Button_Yes_Click(object sender, RoutedEventArgs e)
    {
        IsCancel = false;
        Close();
    }
}
