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

public class AddCultureWindowVM : ObservableObjectX
{
    /// <summary>
    /// 显示的文化
    /// </summary>
    #region ShowCultures
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ObservableFilterList<string, ObservableList<string>> _allCultures = null!;

    /// <summary>
    /// 全部文化
    /// </summary>
    public ObservableFilterList<string, ObservableList<string>> AllCultures
    {
        get => _allCultures;
        set => SetProperty(ref _allCultures, value);
    }
    #endregion

    /// <summary>
    /// 当前文化
    /// </summary>
    #region Culture
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _culture = string.Empty;

    public string CultureName
    {
        get => _culture;
        set
        {
            SetProperty(ref _culture, value);
            if (string.IsNullOrWhiteSpace(CultureName))
            {
                CultureFullName = UnknownCulture;
                return;
            }
            CultureInfo info = null!;
            try
            {
                info = CultureInfo.GetCultureInfo(CultureName);
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
    }
    #endregion

    /// <summary>
    /// 当前文化全名
    /// </summary>
    #region CultureFullName
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _cultureFullName = string.Empty;

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
    private string _search = string.Empty;

    public string Search
    {
        get => _search;
        set
        {
            SetProperty(ref _search, value);
            AllCultures.Refresh();
        }
    }
    #endregion

    public static string UnknownCulture => "未知文化".Translate();

    public AddCultureWindowVM()
    {
        AllCultures = new(LocalizeCore.AvailableCultures)
        {
            Filter = c => c.Contains(Search, StringComparison.OrdinalIgnoreCase),
            FilteredList = new()
        };
    }
}
