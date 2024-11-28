using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Windows.Automation;
using HKW.AutoGUI;
using HKW.AutoGUI.Windows;
using HKW.WPF.Extensions;
using VPet.ModMaker.Views;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace VPet.ModMaker.Tests;

[TestClass]
public class T0_ModMakerWindowTests
{
    public static AutomationElement? ModMakerWindowElement { get; set; } = null;

    public static HWND WindowPtr { get; set; }

    public static Process? Process { get; set; }

    public static void IsNotNull([NotNull] object? obj)
    {
        if (obj is null)
            ModMakerWindowElement = null;
        Assert.IsNotNull(obj);
    }

    public static void IsTrue(bool? @bool)
    {
        if (@bool is not true)
            ModMakerWindowElement = null;
        Assert.IsTrue(@bool);
    }

    public static System.Windows.Point GetClickablePoint(AutomationElement element)
    {
        System.Windows.Point p;
        try
        {
            p = element.GetClickablePoint();
        }
        catch
        {
            ModMakerWindowElement = null;
        }
        return p;
    }

    [TestMethod]
    public void T0_Launch()
    {
        // 启动程序
        Process = Process.Start(new ProcessStartInfo("VPet.ModMaker.exe"));
        IsNotNull(Process);
        // 获取窗口句柄
        var handle = Process.MainWindowHandle;
        for (var i = 0; i < 10 && handle == 0; i++)
        {
            // 等待窗口启动
            Thread.Sleep(1000);
            handle = Process.MainWindowHandle;
        }
        Assert.IsTrue(handle != 0);
        WindowPtr = new(handle);
        // 使用句柄获取自动化树
        ModMakerWindowElement = AutomationElement.FromHandle(handle);
        IsNotNull(ModMakerWindowElement);
    }

    [TestMethod]
    public void T1_ClearHistory()
    {
        IsNotNull(ModMakerWindowElement);
        PInvoke.SetForegroundWindow(WindowPtr);
        // 获取清空历史按钮
        var clearHistoryButton = ModMakerWindowElement.FindFirst(
            TreeScope.Children,
            new PropertyCondition(AutomationElement.AutomationIdProperty, "Button_ClearHistory")
        );
        IsNotNull(clearHistoryButton);

        WindowsAutoGUI
            .Default.Mouse.MoveTo(GetClickablePoint(clearHistoryButton))
            .ButtonClickAndSleep(MouseButton.Left);
        // 获取确认清空按钮
        var yesButton = ModMakerWindowElement.FindFirst(AutomationIDs.MessageBoxYesButton);
        // 获取已清空提示按钮
        yesButton ??= ModMakerWindowElement.FindFirst(AutomationIDs.MessageBoxOKButton);
        IsNotNull(yesButton);
        // 点击确认
        WindowsAutoGUI
            .Default.Mouse.MoveTo(GetClickablePoint(yesButton))
            .ButtonClickAndSleep(MouseButton.Left);
    }

    [TestMethod]
    public void T2_CreateNewModAndCancel()
    {
        IsNotNull(ModMakerWindowElement);
        PInvoke.SetForegroundWindow(WindowPtr);

        // 获取创建新模组按钮
        var createNewModButton = ModMakerWindowElement.FindFirst("Button_CreateNewMod");
        IsNotNull(createNewModButton);
        WindowsAutoGUI
            .Default.Mouse.MoveTo(GetClickablePoint(createNewModButton))
            .ButtonClickAndSleep(MouseButton.Left);

        // 检测窗口隐藏
        IsTrue(
            ModMakerWindowElement.GetCurrentPropertyValue(AutomationElement.IsOffscreenProperty)
                is true
        );

        // 获取新窗口
        PInvoke.GetCursorPos(out var p);
        var hwnd = PInvoke.WindowFromPoint(p);
        var modEditWindowElement = AutomationElement.FromHandle(hwnd);
        IsNotNull(modEditWindowElement);

        // 获取添加文化提示按钮
        var okButton = modEditWindowElement.FindFirst(AutomationIDs.MessageBoxOKButton);
        IsNotNull(okButton);
        WindowsAutoGUI
            .Default.Mouse.MoveTo(GetClickablePoint(okButton))
            .ButtonClickAndSleep(MouseButton.Left);

        // 获取添加文化确定按钮
        var yesButton = modEditWindowElement.FindFirst(AutomationIDs.MessageBoxYesButton);
        IsNotNull(yesButton);
        WindowsAutoGUI
            .Default.Mouse.MoveTo(GetClickablePoint(yesButton))
            .ButtonClickAndSleep(MouseButton.Left);
        // 未输入文化所以提示文化为空
        okButton = modEditWindowElement.FindFirst(AutomationIDs.MessageBoxOKButton);
        IsNotNull(okButton);
        WindowsAutoGUI
            .Default.Mouse.MoveTo(GetClickablePoint(okButton))
            .ButtonClickAndSleep(MouseButton.Left);

        // 获取添加文化取消按钮
        var cancelButton = modEditWindowElement.FindFirst(AutomationIDs.MessageBoxCancelButton);
        IsNotNull(cancelButton);
        WindowsAutoGUI
            .Default.Mouse.MoveTo(GetClickablePoint(cancelButton))
            .ButtonClickAndSleep(MouseButton.Left);

        // 确定不添加文化按钮
        okButton = modEditWindowElement.FindFirst(AutomationIDs.MessageBoxOKButton);
        IsNotNull(okButton);
        WindowsAutoGUI
            .Default.Mouse.MoveTo(GetClickablePoint(okButton))
            .ButtonClickAndSleep(MouseButton.Left);

        // 检测窗口关闭
        IsTrue(
            modEditWindowElement.GetCurrentPropertyValue(AutomationElement.IsOffscreenProperty)
                is true
        );
        // 检测窗口显示
        IsTrue(
            ModMakerWindowElement.GetCurrentPropertyValue(AutomationElement.IsOffscreenProperty)
                is false
        );
    }

    [TestMethod]
    public void T99_CloseProcess()
    {
        Process?.Kill(true);
    }
}
