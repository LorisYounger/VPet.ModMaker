using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using Mapster;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels.ModEdit.I18nEdit;

public class I18nEditWindowVM : ObservableObjectX
{
    public I18nEditWindowVM()
    {
        SearchTarget = SearchTargets.First();
        PropertyChanged += I18nEditWindowVM_PropertyChanged;

        ModInfoModel.Current.I18nResource.Cultures.SetChanged -= Cultures_SetChanged;
        ModInfoModel.Current.I18nResource.Cultures.SetChanged += Cultures_SetChanged;

        I18nDatas = new() { Filter = DataFilter, FilteredList = [] };
        foreach (var data in ModInfoModel.Current.I18nResource.CultureDatas.Values)
            I18nDatas.Add(data);
        foreach (var culture in ModInfoModel.Current.I18nResource.Cultures)
            SearchTargets.Add(culture.Name);
    }

    #region Search
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _search = string.Empty;

    /// <summary>
    /// 搜索
    /// </summary>
    public string Search
    {
        get => _search;
        set => SetProperty(ref _search, value);
    }
    #endregion

    /// <summary>
    /// 全部I18n资源
    /// <para>
    /// (ID, (CultureName, Value))
    /// </para>
    /// </summary>
    public ObservableFilterList<
        ObservableCultureDataDictionary<string, string>,
        ObservableList<ObservableCultureDataDictionary<string, string>>
    > I18nDatas { get; } = null!;

    /// <summary>
    /// 搜索目标列表
    /// </summary>
    public ObservableSet<string> SearchTargets { get; } = [nameof(ModInfoModel.ID)];

    /// <summary>
    /// 搜索目标
    /// </summary>
    #region SearchTarget
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _searchTarget = string.Empty;

    public string SearchTarget
    {
        get => _searchTarget;
        set => SetProperty(ref _searchTarget, value);
    }
    #endregion

    private bool DataFilter(ObservableCultureDataDictionary<string, string> item)
    {
        if (SearchTarget == nameof(ModInfoModel.ID))
        {
            // 如果是ID则搜索ID
            return item.Key.Contains(Search, StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            // 如果是I18n数据则搜索对应文化
            if (item.TryGetValue(CultureInfo.GetCultureInfo(SearchTarget), out var data))
            {
                return data.Contains(Search, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }
    }

    private void Cultures_SetChanged(
        IObservableSet<CultureInfo> sender,
        NotifySetChangedEventArgs<CultureInfo> e
    )
    {
        if (e.Action is SetChangeAction.Add)
        {
            if (e.NewItems is null)
                return;
            foreach (var item in e.NewItems)
            {
                AddCulture(item.Name);
                SearchTargets.Add(item.Name);
            }
        }
        else if (e.Action is SetChangeAction.Remove)
        {
            if (e.OldItems is null)
                return;
            foreach (var item in e.OldItems)
            {
                RemoveCulture(item.Name);
                SearchTargets.Remove(item.Name);
            }
        }
    }

    private void I18nEditWindowVM_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Search))
        {
            I18nDatas.Refresh();
        }
        else if (e.PropertyName == nameof(SearchTarget))
        {
            I18nDatas.Refresh();
        }
    }

    #region Event
    private void AddCulture(string culture)
    {
        CultureChanged?.Invoke(string.Empty, culture);
    }

    private void RemoveCulture(string culture)
    {
        CultureChanged?.Invoke(culture, string.Empty);
    }

    private void ReplaceCulture(string oldCulture, string newCulture)
    {
        CultureChanged?.Invoke(oldCulture, newCulture);
    }

    public event CultureChangedEventHandler? CultureChanged;
    #endregion
}

public delegate void CultureChangedEventHandler(string oldCulture, string newCulture);
