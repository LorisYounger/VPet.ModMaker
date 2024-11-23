using System.Diagnostics;
using System.Windows.Automation;
using System.Windows.Input;
using VPet.ModMaker.Views;

namespace VPet.ModMaker.Tests;

[TestClass]
public class T0_ModMakerWindowTests
{
    public AutomationElement ModMakerWindowElement = null!;

    [TestMethod]
    public void T1_Launch()
    {
        var process = Process.Start(new ProcessStartInfo("VPet.ModMaker.exe") { });
        Assert.IsNotNull(process);
        var handle = process.MainWindowHandle;
        for (var i = 0; i < 10 && handle == 0; i++)
        {
            Thread.Sleep(1000);
            handle = process.MainWindowHandle;
        }
        Assert.IsTrue(handle != 0);
        ModMakerWindowElement = AutomationElement.FromHandle(handle);
        Assert.IsNotNull(ModMakerWindowElement);
        // 点击历史按钮
        var buttonPattern =
            ModMakerWindowElement
                .FindFirst(
                    TreeScope.Children,
                    new PropertyCondition(
                        AutomationElement.AutomationIdProperty,
                        "Button_ClearHistory"
                    )
                )
                ?.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
        Assert.IsNotNull(buttonPattern);
        buttonPattern?.Invoke();
        Thread.Sleep(1000);
        // 确认清空
        var yesButtonPattern =
            ModMakerWindowElement
                .FindFirst(
                    TreeScope.Descendants,
                    new PropertyCondition(AutomationElement.AutomationIdProperty, "PART_YesButton")
                )
                ?.GetCurrentPattern(InvokePattern.Pattern) as InvokePattern;
        Assert.IsNotNull(yesButtonPattern);
        yesButtonPattern?.Invoke();
    }
}
