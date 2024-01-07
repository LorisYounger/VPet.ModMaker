using HKW.HKWUtils.Observable;
using LinePutScript.Localization.WPF;
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
        GraphType.ValueChanged += GraphType_ValueChanged;
        Closed += (s, e) =>
        {
            try
            {
                DataContext = null;
            }
            catch { }
        };
    }

    private void GraphType_ValueChanged(
        ObservableValue<GraphInfo.GraphType> sender,
        ValueChangedEventArgs<GraphInfo.GraphType> e
    )
    {
        if (e.NewValue.IsHasNameAnime())
            HasNameAnime.Value = true;
        else
            HasNameAnime.Value = false;
    }

    private void CurrentPet_ValueChanged(
        ObservableValue<PetModel> sender,
        ValueChangedEventArgs<PetModel> e
    )
    {
        GraphTypes.Value = new(
            AnimeTypeModel.GraphTypes.Except(CurrentPet.Value.Animes.Select(m => m.GraphType.Value))
        );
        // 可添加多个项的类型
        foreach (var graphType in AnimeTypeModel.HasNameAnimes)
            if (GraphTypes.Value.Contains(graphType) is false)
                GraphTypes.Value.Add(graphType);
    }

    public ObservableValue<PetModel> CurrentPet { get; } = new();
    public ObservableValue<GraphInfo.GraphType> GraphType { get; } = new();
    public ObservableValue<ObservableCollection<GraphInfo.GraphType>> GraphTypes { get; } =
        new(new());
    public ObservableValue<string> AnimeName { get; } = new();

    public ObservableValue<bool> HasNameAnime { get; } = new(true);

    public bool IsCancel { get; private set; } = true;

    private void Button_Cancel_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Button_Yes_Click(object sender, RoutedEventArgs e)
    {
        //if (string.IsNullOrWhiteSpace(AnimeName.Value))
        //{
        //    MessageBox.Show(
        //        "动画名称不能为空".Translate(),
        //        "",
        //        MessageBoxButton.OK,
        //        MessageBoxImage.Warning
        //    );
        //    return;
        //}
        IsCancel = false;
        Close();
    }
}
