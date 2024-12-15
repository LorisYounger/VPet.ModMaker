using System.Windows.Controls;
using System.Windows.Input;
using VPet.ModMaker.Models.ModModel;
using VPet.ModMaker.ViewModels.ModEdit;

namespace VPet.ModMaker.Views.ModEdit;

/// <summary>
/// AnimePage.xaml 的交互逻辑
/// </summary>
public partial class AnimePage : UserControl
{
    /// <inheritdoc/>
    public AnimePage()
    {
        InitializeComponent();
    }

    /// <summary>
    /// 视图模型
    /// </summary>
    public AnimeVM ViewModel => (AnimeVM)DataContext;

    private void DataGrid_MouseDoubleClick(object? sender, MouseButtonEventArgs e)
    {
        if (sender is not DataGrid dataGrid || dataGrid.SelectedItem is null)
            return;
        ViewModel.Edit((IAnimeModel)dataGrid.SelectedItem);
    }
}
