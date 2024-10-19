using VPet.ModMaker.Views;

namespace VPet.ModMaker.Tests;

[TestClass]
public class ModMakerWindowTests
{
    public ModMakerWindow Window => (ModMakerWindow)Program.App.MainWindow;

    [TestMethod]
    public void LoadHistory()
    {
        Program.RunSTATask(() => { });
        //Assert.IsTrue(Program.RunSTATask<bool>(() => Program.App.Windows.Count > 0));
    }

    [TestMethod]
    public void AddHistory()
    {
        //Program.RunSTATask(() => Window.ViewModel.);
        //Assert.IsTrue(Program.RunSTATask<bool>(() => Program.App.Windows.Count > 0));
    }

    [TestMethod]
    public void TestMethod2()
    {
        Program.RunSTATask(() => Program.App.MainWindow.Close());
    }
}
