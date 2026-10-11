using System.Windows.Controls;
using System.Windows.Input;
using VPet.ModMaker.Models;
using VPet.ModMaker.ViewModels;

namespace VPet.ModMaker.Views;

/// <summary>
/// AnimePage.xaml 的交互逻辑
/// </summary>
public partial class AnimeView : UserControl
{
    /// <inheritdoc/>
    public AnimeView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 视图模型
    /// </summary>
    public AnimeViewModel ViewModel => (AnimeViewModel)DataContext;

    private void DataGrid_MouseDoubleClick(object? sender, MouseButtonEventArgs e)
    {
        if (sender is not DataGrid dataGrid || dataGrid.SelectedItem is null)
            return;
        ViewModel.Edit((IAnimeModel)dataGrid.SelectedItem);
    }
}
