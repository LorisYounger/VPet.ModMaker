using Panuon.WPF.UI;
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
using VPet.ModMaker.ViewModels.ModEdit;

namespace VPet.ModMaker.Views.ModEdit;

/// <summary>
/// SaveTranslationModWindow.xaml 的交互逻辑
/// </summary>
public partial class SaveTranslationModWindow : WindowX
{
    public SaveTranslationModWindowVM ViewModel => (SaveTranslationModWindowVM)DataContext;

    public SaveTranslationModWindow()
    {
        InitializeComponent();
        DataContext = new SaveTranslationModWindowVM();
        Closed += (s, e) =>
        {
            //ViewModel.Close();
            try
            {
                DataContext = null;
            }
            catch { }
        };
    }
}
