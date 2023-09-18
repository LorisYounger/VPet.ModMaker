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
using System.Windows.Navigation;
using System.Windows.Shapes;
using VPet.ModMaker.Models.ModModel;
using VPet.ModMaker.ViewModels.ModEdit.AnimeEdit;

namespace VPet.ModMaker.Views.ModEdit.AnimeEdit;

/// <summary>
/// AnimePage.xaml 的交互逻辑
/// </summary>
public partial class AnimePage : Page
{
    public AnimePage()
    {
        InitializeComponent();
        DataContext = new AnimePageVM();
    }

    public AnimePageVM ViewModel => (AnimePageVM)DataContext;

    private void DataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
    {
        if (sender is not DataGrid dataGrid || dataGrid.SelectedItem is not AnimeTypeModel model)
            return;
        ViewModel.Edit(model);
    }
}
