using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HKW.HKWUtils.Observable;
using LinePutScript.Localization.WPF;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels.ModEdit;

public class AddCultureWindowVM : ObservableObjectX<AddCultureWindowVM>
{
    /// <summary>
    /// 显示的文化
    /// </summary>
    #region ShowCultures
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ObservableCollection<string> _showCultures;

    public ObservableCollection<string> ShowCultures
    {
        get => _showCultures;
        set => SetProperty(ref _showCultures, value);
    }
    #endregion

    /// <summary>
    /// 全部文化
    /// </summary>
    public static ObservableCollection<string> AllCultures { get; set; } =
        new(LinePutScript.Localization.WPF.LocalizeCore.AvailableCultures);

    /// <summary>
    /// 当前文化
    /// </summary>
    #region Culture
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _culture;

    public string Culture
    {
        get => _culture;
        set => SetProperty(ref _culture, value);
    }
    #endregion

    /// <summary>
    /// 当前文化全名
    /// </summary>
    #region CultureFullName
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _cultureFullName;

    public string CultureFullName
    {
        get => _cultureFullName;
        set => SetProperty(ref _cultureFullName, value);
    }
    #endregion

    /// <summary>
    /// 搜索文化
    /// </summary>
    #region Search
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _Search;

    public string Search
    {
        get => _Search;
        set => SetProperty(ref _Search, value);
    }
    #endregion

    public static string UnknownCulture = "未知文化".Translate();

    public AddCultureWindowVM()
    {
        //TODO
        //ShowCultures = AllCultures;
        //Search.ValueChanged += Search_ValueChanged;
        //Culture.ValueChanged += Culture_ValueChanged;
    }

    private void Culture_ValueChanged(
        ObservableValue<string> sender,
        ValueChangedEventArgs<string> e
    )
    {
        if (string.IsNullOrWhiteSpace(e.NewValue))
        {
            CultureFullName = UnknownCulture;
            return;
        }
        CultureInfo info = null!;
        try
        {
            info = CultureInfo.GetCultureInfo(e.NewValue);
        }
        catch
        {
            CultureFullName = UnknownCulture;
        }
        if (info is not null)
        {
            CultureFullName = info.GetFullInfo();
        }
    }

    private void Search_ValueChanged(
        ObservableValue<string> sender,
        ValueChangedEventArgs<string> e
    )
    {
        if (string.IsNullOrWhiteSpace(e.NewValue))
        {
            ShowCultures = AllCultures;
        }
        else
        {
            ShowCultures = new(
                AllCultures.Where(s => s.Contains(e.NewValue, StringComparison.OrdinalIgnoreCase))
            );
        }
    }
}
