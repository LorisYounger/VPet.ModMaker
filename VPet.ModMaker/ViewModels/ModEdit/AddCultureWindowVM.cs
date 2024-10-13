using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HKW.HKWReactiveUI;
using HKW.HKWUtils.Collections;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using LinePutScript.Localization.WPF;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels.ModEdit;

public partial class AddCultureWindowVM : ViewModelBase
{
    public AddCultureWindowVM()
    {
        AllCultures = new(
            LocalizeCore.AvailableCultures,
            [],
            c => c.Contains(Search, StringComparison.OrdinalIgnoreCase)
        );
    }

    /// <summary>
    /// 全部文化
    /// </summary>
    public FilterListWrapper<
        string,
        IList<string>,
        ObservableList<string>
    > AllCultures { get; set; }

    /// <summary>
    /// 文化名称
    /// </summary>

    [ReactiveProperty]
    public string CultureName { get; set; } = string.Empty;

    /// <summary>
    /// 文化全名
    /// </summary>
    [NotifyPropertyChangeFrom(nameof(CultureName))]
    public string CultureFullName =>
        this.To(static x =>
        {
            if (string.IsNullOrWhiteSpace(x.CultureName))
            {
                return UnknownCulture;
            }
            CultureInfo info = null!;
            try
            {
                info = CultureInfo.GetCultureInfo(x.CultureName);
            }
            catch
            {
                return UnknownCulture;
            }
            if (info is not null)
            {
                return info.GetFullInfo();
            }
            return UnknownCulture;
        });

    /// <summary>
    /// 搜索文化
    /// </summary>
    [ReactiveProperty]
    public string Search { get; set; } = string.Empty;

    partial void OnSearchChanged(string oldValue, string newValue)
    {
        AllCultures.Refresh();
    }

    public static string UnknownCulture => "未知文化".Translate();
}
