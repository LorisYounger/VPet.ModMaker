using System.Windows.Controls;
using System.Windows.Input;
using VPet.ModMaker.Models;
using VPet.ModMaker.ViewModels.ModEdit;

namespace VPet.ModMaker.Views.ModEdit;

/// <summary>
/// Page_LowText.xaml 的交互逻辑
/// </summary>
public partial class LowTextPage : UserControl
{
    /// <summary>
    /// 视图模型
    /// </summary>
    public LowTextEditVM ViewModel => (LowTextEditVM)DataContext;

    /// <inheritdoc/>
    public LowTextPage()
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
