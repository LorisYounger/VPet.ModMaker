using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicData.Binding;
using HKW.HKWReactiveUI;
using HKW.HKWUtils.Collections;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using LinePutScript.Localization.WPF;
using ReactiveUI;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels.ModEdit;

public partial class AddCultureWindowVM : ViewModelBase
{
    public AddCultureWindowVM()
    {
        AllCultures = new(
            new(LocalizeCore.AvailableCultures),
            [],
            c => c.Contains(Search, StringComparison.OrdinalIgnoreCase)
        );
        this.WhenValueChanged(x => x.Search)
            .Throttle(TimeSpan.FromSeconds(1), RxApp.TaskpoolScheduler)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => AllCultures.Refresh());
    }

    /// <summary>
    /// 全部文化
    /// </summary>
    public FilterListWrapper<string, List<string>, ObservableList<string>> AllCultures { get; set; }

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

    public static string UnknownCulture => "未知文化".Translate();
}
