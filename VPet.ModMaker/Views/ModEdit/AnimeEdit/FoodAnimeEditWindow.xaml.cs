using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using HKW.WPF.Extensions;
using LinePutScript.Localization.WPF;
using Panuon.WPF.UI;
using VPet.ModMaker.Models.ModModel;
using VPet.ModMaker.ViewModels.ModEdit;
using static VPet_Simulator.Core.IGameSave;

namespace VPet.ModMaker.Views.ModEdit;

/// <summary>
/// AnimeEditWindow.xaml 的交互逻辑
/// </summary>
public partial class FoodAnimeEditWindow : WindowX
{
    /// <inheritdoc/>
    public FoodAnimeEditWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 视图模型
    /// </summary>
    public FoodAnimeEditVM ViewModel => (FoodAnimeEditVM)DataContext;

    private void Button_Cancel_Click(object? sender, RoutedEventArgs e)
    {
        Close();
    }

    private void Button_Yes_Click(object? sender, RoutedEventArgs e)
    {
        ViewModel.DialogResult = true;
        Close();
    }

    private void TabControl_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (ViewModel is null)
            return;
        if (
            sender is not TabControl tabControl
            || tabControl.SelectedItem is not TabItem item
            || item.Tag is not string str
        )
            return;
        if (Enum.TryParse<ModeType>(str, true, out var mode))
        {
            ViewModel.CurrentMode = mode;
            ViewModel.CurrentFrontImageModel = null!;
            ViewModel.CurrentBackImageModel = null!;
            ViewModel.CurrentFoodLocationModel = null!;
            ViewModel.CurrentAnimeModel = null!;
        }
    }

    private void ListBox_PreviewMouseWheel(object? sender, MouseWheelEventArgs e)
    {
        var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
        {
            RoutedEvent = MouseWheelEvent,
            Source = sender
        };
        var parent = ((Control)sender!).Parent as UIElement;
        parent!.RaiseEvent(eventArg);
        e.Handled = true;
    }

    private object _dropSender = null!;

    private void ListBox_PreviewMouseMove(object? sender, MouseEventArgs e)
    {
        if (sender is not ListBox listBox)
            return;
        if (e.LeftButton != MouseButtonState.Pressed)
            return;
        var pos = e.GetPosition(listBox);
        HitTestResult result = VisualTreeHelper.HitTest(listBox, pos);
        if (result is null)
            return;
        var listBoxItem = result.VisualHit.FindVisuaParent<ListBoxItem>();
        if (listBoxItem == null || listBoxItem.Content != listBox.SelectedItem)
            return;
        var dataObj = new DataObject(listBoxItem.Content);
        _dropSender = sender;
        DragDrop.DoDragDrop(listBox, dataObj, DragDropEffects.Move);
    }

    private void ListBox_Drop(object? sender, DragEventArgs e)
    {
        if (sender is not ListBox listBox)
            return;
        if (e.Data.GetData(DataFormats.FileDrop) is Array array)
        {
            if (listBox.Name == "ListBox_FrontImages")
            {
                ViewModel.AddImages(
                    ((FoodAnimeModel)listBox.DataContext).FrontImages,
                    array.Cast<string>()
                );
            }
            else if (listBox.Name == "ListBox_BackImages")
            {
                ViewModel.AddImages(
                    ((FoodAnimeModel)listBox.DataContext).BackImages,
                    array.Cast<string>()
                );
            }
        }
        if (_dropSender is not null && sender.Equals(_dropSender) is false)
        {
            MessageBox.Show("无法移动不同动画的图片".Translate());
            return;
        }
        var pos = e.GetPosition(listBox);
        var result = VisualTreeHelper.HitTest(listBox, pos);
        if (result == null)
            return;
        //查找元数据
        if (e.Data.GetData(typeof(ImageModel)) is not ImageModel sourcePerson)
            return;
        //查找目标数据
        var listBoxItem = result.VisualHit.FindVisuaParent<ListBoxItem>();
        if (listBoxItem == null)
            return;
        var targetPerson = (ImageModel)listBoxItem.Content;
        if (ReferenceEquals(targetPerson, sourcePerson))
            return;
        if (listBox.ItemsSource is not IList<ImageModel> list)
            return;
        var sourceIndex = list.IndexOf(sourcePerson);
        var targetIndex = list.IndexOf(targetPerson);
        (list[targetIndex], list[sourceIndex]) = (list[sourceIndex], list[targetIndex]);
    }

    private void ListBox_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not ListBox listBox)
            return;
        if (listBox.DataContext is FoodAnimeModel model)
            ViewModel.CurrentAnimeModel = model;
        listBox.ScrollIntoView(listBox.SelectedItem);
        e.Handled = true;
    }

    private void ListBox_Animes_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        e.Handled = true;
    }
}
