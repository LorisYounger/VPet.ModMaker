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
    public FoodAnimeEditViewModel ViewModel => (FoodAnimeEditViewModel)DataContext;

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
        {
            if (listBox.Name == FrontImages.Name)
            {
                ViewModel.AddImages(
                    ((FoodAnimeModel)listBox.DataContext).FrontImages,
                    array.Cast<string>()
                );
            }
            else if (listBox.Name == BackImages.Name)
            {
                ViewModel.AddImages(
                    ((FoodAnimeModel)listBox.DataContext).BackImages,
                    array.Cast<string>()
                );
            }
        }
        if (_dropSender is not null && sender.Equals(_dropSender) is false)
        {
            ViewModel.DialogService.ShowMessageBox(
                ViewModel,
                "无法移动不同动画的图片".Translate(),
                icon: HanumanInstitute.MvvmDialogs.FrameworkDialogs.MessageBoxImage.Warning
            );
            return;
        }
        var pos = e.GetPosition(listBox);
        var result = VisualTreeHelper.HitTest(listBox, pos);
        if (result == null)
            return;
        //查找元数据
        if (e.Data.GetData(typeof(ImageModel)) is ImageModel sourceImage)
        {
            //查找目标数据
            var listBoxItem = result.VisualHit.FindVisuaParent<ListBoxItem>();
            if (listBoxItem == null)
                return;
            var targetImage = (ImageModel)listBoxItem.Content;
            if (ReferenceEquals(targetImage, sourceImage))
                return;
            if (listBox.ItemsSource is not IList<ImageModel> list)
                return;
            var sourceIndex = list.IndexOf(sourceImage);
            var targetIndex = list.IndexOf(targetImage);
            (list[targetIndex], list[sourceIndex]) = (list[sourceIndex], list[targetIndex]);
        }
        else if (
            e.Data.GetData(typeof(FoodAnimeLocationModel)) is FoodAnimeLocationModel sourceLocation
        )
        {
            //查找目标数据
            var listBoxItem = result.VisualHit.FindVisuaParent<ListBoxItem>();
            if (listBoxItem == null)
                return;
            var targetLocation = (FoodAnimeLocationModel)listBoxItem.Content;
            if (ReferenceEquals(targetLocation, sourceLocation))
                return;
            if (listBox.ItemsSource is not IList<FoodAnimeLocationModel> list)
                return;
            var sourceIndex = list.IndexOf(sourceLocation);
            var targetIndex = list.IndexOf(targetLocation);
            (list[targetIndex], list[sourceIndex]) = (list[sourceIndex], list[targetIndex]);
        }
    }

    private void ButtonShowContextMenu_Click(object sender, RoutedEventArgs e)
    {
        var button = (Button)sender;
        button.ContextMenu.PlacementTarget = button;
        button.ContextMenu.IsOpen = true;
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

    private void ModeTypes_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var listBox = (ListBox)sender;
        var tag = ((ListBoxItem)listBox.SelectedItem).Tag;
        if (tag is null)
            return;
        if (ReferenceEquals(tag, ViewModel?.Anime?.HappyAnimes))
            ViewModel.CurrentMode = ModeType.Happy;
        else if (ReferenceEquals(tag, ViewModel?.Anime?.NomalAnimes))
            ViewModel.CurrentMode = ModeType.Nomal;
        else if (ReferenceEquals(tag, ViewModel?.Anime?.PoorConditionAnimes))
            ViewModel.CurrentMode = ModeType.PoorCondition;
        else if (ReferenceEquals(tag, ViewModel?.Anime?.IllAnimes))
            ViewModel.CurrentMode = ModeType.Ill;
    }
}
