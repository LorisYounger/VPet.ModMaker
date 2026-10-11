using System.Windows;
using HanumanInstitute.MvvmDialogs;
using HKW.MVVM;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VPet.ModMaker.ViewModels;

namespace VPet.ModMaker;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    public ModMaker ModMaker { get; private set; } = null!;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ModMaker = new(null!);
        ModMaker.LoadPlugin();
        ModMaker.Setting();
    }
}
