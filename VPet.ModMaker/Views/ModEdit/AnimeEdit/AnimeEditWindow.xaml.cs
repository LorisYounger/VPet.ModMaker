using LinePutScript.Localization.WPF;
using System;
using System.Collections.Generic;
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
using VPet.ModMaker.ViewModels.ModEdit.AnimeEdit;
using VPet_Simulator.Core;

namespace VPet.ModMaker.Views.ModEdit.AnimeEdit;

/// <summary>
/// AnimeEditWindow.xaml 的交互逻辑
/// </summary>
public partial class AnimeEditWindow : Window
{
    public AnimeEditWindow()
    {
        DataContext = new AnimeEditWindowVM();
        InitializeComponent();
        Closed += (s, e) =>
        {
            try
            {
                DataContext = null;
            }
            catch { }
        };
    }

    public AnimeEditWindowVM ViewModel => (AnimeEditWindowVM)DataContext;

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

    private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (
            sender is not TabControl tabControl
            || tabControl.SelectedItem is not TabItem item
            || item.Tag is not string str
        )
            return;
        if (Enum.TryParse<GameSave.ModeType>(str, true, out var mode))
        {
            ViewModel.CurrentMode = mode;
            ViewModel.CurrentImageModel.Value = null;
            ViewModel.CurrentAnimeModel.Value = null;
        }
    }

    private void ListBox_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
        {
            RoutedEvent = MouseWheelEvent,
            Source = sender
        };
        var parent = ((Control)sender).Parent as UIElement;
        parent.RaiseEvent(eventArg);
        e.Handled = true;
    }

    private object _dropSender;

    private void ListBox_PreviewMouseMove(object sender, MouseEventArgs e)
    {
        if (sender is not ListBox listBox)
            return;
        if (e.LeftButton != MouseButtonState.Pressed)
            return;
        var pos = e.GetPosition(listBox);
        HitTestResult result = VisualTreeHelper.HitTest(listBox, pos);
        if (result is null)
            return;
        var listBoxItem = result.VisualHit.FindParent<ListBoxItem>();
        if (listBoxItem == null || listBoxItem.Content != listBox.SelectedItem)
            return;
        var dataObj = new DataObject(listBoxItem.Content);
        _dropSender = sender;
        DragDrop.DoDragDrop(listBox, dataObj, DragDropEffects.Move);
    }

    private void ListBox_Drop(object sender, DragEventArgs e)
    {
        if (sender is not ListBox listBox)
            return;
        if (e.Data.GetData(DataFormats.FileDrop) is Array array)
            ViewModel.AddImages(((AnimeModel)listBox.DataContext).Images, array.Cast<string>());
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
        var listBoxItem = result.VisualHit.FindParent<ListBoxItem>();
        if (listBoxItem == null)
            return;
        var targetPerson = listBoxItem.Content as ImageModel;
        if (ReferenceEquals(targetPerson, sourcePerson))
            return;
        if (listBox.ItemsSource is not IList<ImageModel> list)
            return;
        var sourceIndex = list.IndexOf(sourcePerson);
        var targetIndex = list.IndexOf(targetPerson);
        var temp = list[sourceIndex];
        list[sourceIndex] = list[targetIndex];
        list[targetIndex] = temp;
    }

    private void ListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is not ListBox listBox)
            return;
        if (listBox.DataContext is AnimeModel model)
            ViewModel.CurrentAnimeModel.Value = model;
        listBox.ScrollIntoView(listBox.SelectedItem);
        e.Handled = true;
    }

    private void ListBox_Animes_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        e.Handled = true;
    }
}
