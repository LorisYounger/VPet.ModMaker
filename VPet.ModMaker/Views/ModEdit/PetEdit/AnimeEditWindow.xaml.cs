using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using HKW.WPF.Extensions;
using LinePutScript.Localization.WPF;
using Panuon.WPF.UI;
using VPet.ModMaker.Models;
using VPet.ModMaker.ViewModels;
using static VPet_Simulator.Core.IGameSave;

namespace VPet.ModMaker.Views;

/// <summary>
/// AnimeEditWindow.xaml 的交互逻辑
/// </summary>
public partial class AnimeEditWindow : WindowX
{
    /// <inheritdoc/>
    public AnimeEditWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 视图模型
    /// </summary>
    public AnimeEditViewModel ViewModel => (AnimeEditViewModel)DataContext;

    private object _dropSender = null!;

    private void ListBox_Images_PreviewMouseMove(object? sender, MouseEventArgs e)
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

    private void ListBox_Images_Drop(object? sender, DragEventArgs e)
    {
        if (sender is not ListBox listBox)
            return;
        if (e.Data.GetData(DataFormats.FileDrop) is Array array)
            ViewModel.AddImages(((AnimeModel)listBox.DataContext).Images, array.Cast<string>());
        if (_dropSender is not null && sender.Equals(_dropSender) is false)
        {
            MessageBoxX.Show(this, "无法移动不同动画的图片".Translate());
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

    private ListBox _lastListBox;

    private void ListBox_Images_SelectionChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is not ListBox listBox || listBox.SelectedItem is null)
            return;
        if (listBox != _lastListBox)
        {
            _lastListBox?.SelectedItem = null;
            _lastListBox = listBox;
            _lastListBox?.SelectedItem = e.AddedItems[0];
        }
        if (listBox.DataContext is AnimeModel model)
            ViewModel.CurrentAnimeModel = model;
        listBox.ScrollIntoView(listBox.SelectedItem);
        e.Handled = true;
    }

    private void ListBox_HorizontalScrolling_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        var listBox = (ListBox)sender;
        var scrollViewer = listBox.FindVisualChild<ScrollViewer>();

        if (scrollViewer == null || scrollViewer.ScrollableWidth <= 0)
            return;

        if (e.Delta < 0)
            scrollViewer.LineRight();
        else
            scrollViewer.LineLeft();

        e.Handled = true;
    }
}
