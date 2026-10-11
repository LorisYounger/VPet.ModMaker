using System.Windows.Controls;
using System.Windows.Input;
using VPet.ModMaker.Models;
using VPet.ModMaker.ViewModels;

namespace VPet.ModMaker.Views;

/// <summary>
/// LowTextView.xaml 的交互逻辑
/// </summary>
public partial class LowTextView : UserControl
{
    /// <summary>
    /// 视图模型
    /// </summary>
    public LowTextEditViewModel ViewModel => (LowTextEditViewModel)DataContext;

    /// <inheritdoc/>
    public LowTextView()
    {
        InitializeComponent();
    }

    private void DataGrid_LowText_MouseDoubleClick(object? sender, MouseButtonEventArgs e)
    {
        if (sender is not DataGrid dataGrid || dataGrid.SelectedItem is not LowTextModel lowText)
            return;
        ViewModel.Edit(lowText);
    }
}
