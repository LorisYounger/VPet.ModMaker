using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HKW.AutoGUI;
using HKW.WPF.Extensions;
using HKW.WPF.MVVMDialogs;
using HKW.WPF.MVVMDialogs.Windows;
using VPet.ModMaker.Models.ModModel;
using VPet.ModMaker.ViewModels.ModEdit;
using VPet.ModMaker.Views.ModEdit;
using VPet_Simulator.Core;

namespace VPet.ModMaker.Tests;

/// <summary>
/// 拓展
/// </summary>
public static class NativeExtensions
{
    public static IMouseSimulator<TMouseSimulator> MoveTo<TMouseSimulator>(
        this IMouseSimulator<TMouseSimulator> mouseSimulator,
        System.Windows.Point point
    )
        where TMouseSimulator : IMouseSimulator<TMouseSimulator>
    {
        mouseSimulator.MoveTo(point.ToDrawingPoint());
        return mouseSimulator;
    }

    public static IMouseSimulator<TMouseSimulator> ButtonClickAndSleep<TMouseSimulator>(
        this IMouseSimulator<TMouseSimulator> mouseSimulator,
        MouseButton button
    )
        where TMouseSimulator : IMouseSimulator<TMouseSimulator>
    {
        mouseSimulator.ButtonClick(button).Sleep(NativeData.DefaultOperationInterval);
        return mouseSimulator;
    }

    public static AutomationElement FindFirst(
        this AutomationElement element,
        string automationID,
        TreeScope treeScope = TreeScope.Descendants
    )
    {
        return element.FindFirst(
            treeScope,
            new PropertyCondition(AutomationElement.AutomationIdProperty, automationID)
        );
    }
}
