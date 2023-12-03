using HKW.HKWUtils.Observable;
using HKW.Models;
using LinePutScript.Localization.WPF;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels.ModEdit;

public class AddCultureWindowVM
{
    /// <summary>
    /// 显示的文化
    /// </summary>
    public ObservableValue<ObservableCollection<string>> ShowCultures { get; } = new();

    /// <summary>
    /// 全部文化
    /// </summary>
    public static ObservableCollection<string> AllCultures { get; set; } =
        new(LinePutScript.Localization.WPF.LocalizeCore.AvailableCultures);

    /// <summary>
    /// 当前文化
    /// </summary>
    public ObservableValue<string> Culture { get; } = new();

    /// <summary>
    /// 当前文化全名
    /// </summary>
    public ObservableValue<string> CultureFullName { get; } = new(UnknownCulture);

    /// <summary>
    /// 搜索文化
    /// </summary>
    public ObservableValue<string> Search { get; } = new();

    public static string UnknownCulture = "未知文化".Translate();

    public AddCultureWindowVM()
    {
        ShowCultures.Value = AllCultures;
        Search.ValueChanged += Search_ValueChanged;
        Culture.ValueChanged += Culture_ValueChanged;
    }

    private void Culture_ValueChanged(
        ObservableValue<string> sender,
        ValueChangedEventArgs<string> e
    )
    {
        if (string.IsNullOrWhiteSpace(e.NewValue))
        {
            CultureFullName.Value = UnknownCulture;
            return;
        }
        CultureInfo info = null!;
        try
        {
            info = CultureInfo.GetCultureInfo(e.NewValue);
        }
        catch
        {
            CultureFullName.Value = UnknownCulture;
        }
        if (info is not null)
        {
            CultureFullName.Value = info.GetFullInfo();
        }
    }

    private void Search_ValueChanged(
        ObservableValue<string> sender,
        ValueChangedEventArgs<string> e
    )
    {
        if (string.IsNullOrWhiteSpace(e.NewValue))
        {
            ShowCultures.Value = AllCultures;
        }
        else
        {
            ShowCultures.Value = new(
                AllCultures.Where(s => s.Contains(e.NewValue, StringComparison.OrdinalIgnoreCase))
            );
        }
    }
}
