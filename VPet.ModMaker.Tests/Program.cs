using System.Diagnostics;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Interop;
using VPet.ModMaker.Views;

namespace VPet.ModMaker.Tests;

internal class Program
{
    public const string BasePath = "..\\..\\..\\..\\VPet.ModMaker\\bin\\Test\\net8.0-windows\\";
    private static App _app = null!;
    public static App App
    {
        [STAThread]
        get
        {
            if (_app == null)
            {
                _app = new App();
                _app.InitializeComponent();
                _app.Dispatcher.Invoke(() => _app.MainWindow = new ModMakerWindow());
                _app.MainWindow.Show();
            }
            return _app;
        }
    }
    private static Thread STAThread { get; set; } = null!;

    private static Thread GetSTAThread()
    {
        var thread = new Thread(() =>
        {
            while (true)
            {
                Thread.Sleep(100);
                if (STAFunc is not null)
                {
                    Result = STAFunc();
                    STAFunc = null;
                }
                if (STAAction is not null)
                {
                    STAAction();
                    STAAction = null;
                }
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        return thread;
    }

    private static Func<object>? STAFunc;

    private static object? Result;

    private static Action? STAAction;

    public static T RunSTATask<T>(Func<object> func)
    {
        STAThread ??= GetSTAThread();
        STAFunc = func;
        while (STAFunc is not null)
        {
            Thread.Sleep(100);
        }
        var result = (T)Result!;
        Result = null;
        return result;
    }

    public static void RunSTATask(Action action)
    {
        STAThread ??= GetSTAThread();
        STAAction = action;
        while (STAAction is not null)
        {
            Thread.Sleep(100);
        }
    }

    //[STAThread]
    //static void Main(string[] args) { }
}
