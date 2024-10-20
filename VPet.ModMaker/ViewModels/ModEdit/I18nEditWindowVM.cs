﻿using System;
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
using HKW.HKWReactiveUI;
using HKW.HKWUtils.Collections;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using Mapster;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels.ModEdit;

public partial class I18nEditWindowVM : ViewModelBase
{
    public I18nEditWindowVM()
    {
        SearchTarget = SearchTargets.First();
        PropertyChanged += I18nEditWindowVM_PropertyChanged;

        I18nResource.Cultures.SetChanged -= Cultures_SetChanged;
        I18nResource.Cultures.SetChanged += Cultures_SetChanged;

        I18nResource.CultureDatas.DictionaryChanged -= CultureDatas_DictionaryChanged;
        I18nResource.CultureDatas.DictionaryChanged += CultureDatas_DictionaryChanged;

        I18nDatas = new([], [], DataFilter);
        foreach (var data in I18nResource.CultureDatas.Values)
        {
            I18nDatas.Add(data);
            data.DictionaryChanged += Data_DictionaryChanged;
        }
        foreach (var culture in I18nResource.Cultures)
            SearchTargets.Add(culture.Name);
    }

    public I18nResource<string, string> I18nResource { get; set; } =
        ModInfoModel.Current.I18nResource;

    public bool CellEdit { get; set; } = false;

    /// <summary>
    /// 搜索
    /// </summary>
    [ReactiveProperty]
    public string Search { get; set; } = string.Empty;

    /// <summary>
    /// 全部I18n资源
    /// <para>
    /// (ID, (CultureName, Value))
    /// </para>
    /// </summary>
    public FilterListWrapper<
        ObservableCultureDataDictionary<string, string>,
        List<ObservableCultureDataDictionary<string, string>>,
        ObservableList<ObservableCultureDataDictionary<string, string>>
    > I18nDatas { get; } = null!;

    /// <summary>
    /// 搜索目标列表
    /// </summary>
    public ObservableSet<string> SearchTargets { get; } = [nameof(ModInfoModel.ID)];

    /// <summary>
    /// 搜索目标
    /// </summary>
    [ReactiveProperty]
    public string SearchTarget { get; set; } = string.Empty;

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
        NotifySetChangeEventArgs<CultureInfo> e
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

    private void CultureDatas_DictionaryChanged(
        IObservableDictionary<string, ObservableCultureDataDictionary<string, string>> sender,
        NotifyDictionaryChangeEventArgs<string, ObservableCultureDataDictionary<string, string>> e
    )
    {
        if (e.Action is DictionaryChangeAction.Add)
        {
            if (e.TryGetNewPair(out var newPair) is false)
                return;
            I18nDatas.Add(newPair.Value);
            newPair.Value.DictionaryChanged -= Data_DictionaryChanged;
            newPair.Value.DictionaryChanged += Data_DictionaryChanged;
        }
        else if (e.Action is DictionaryChangeAction.Remove)
        {
            if (e.TryGetOldPair(out var oldPair) is false)
                return;
            I18nDatas.Remove(oldPair.Value);
            oldPair.Value.DictionaryChanged -= Data_DictionaryChanged;
        }
        else if (e.Action is DictionaryChangeAction.Replace)
        {
            if (e.TryGetNewPair(out var newPair) is false)
                return;
            if (e.TryGetOldPair(out var oldPair) is false)
                return;
            if (I18nDatas.TryFind(0, i => i.Key == newPair.Key, out var itemInfo) is false)
                return;
            I18nDatas.RemoveAt(itemInfo.Index);
            I18nDatas.Insert(itemInfo.Index, newPair.Value);
            oldPair.Value.DictionaryChanged -= Data_DictionaryChanged;
            newPair.Value.DictionaryChanged -= Data_DictionaryChanged;
            newPair.Value.DictionaryChanged += Data_DictionaryChanged;
        }
        else if (e.Action is DictionaryChangeAction.Clear)
        {
            I18nDatas.Clear();
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

    private void Data_DictionaryChanged(
        IObservableDictionary<CultureInfo, string> sender,
        NotifyDictionaryChangeEventArgs<CultureInfo, string> e
    )
    {
        if (sender is not ObservableCultureDataDictionary<string, string> cultureDatas)
            return;
        // 刷新修改后的数据
        if (e.Action is DictionaryChangeAction.Replace)
        {
            if (CellEdit)
            {
                // 防止在编辑单元格时重复响应
                CellEdit = false;
                return;
            }
            if (I18nDatas.TryFind(0, i => i.Key == cultureDatas.Key, out var itemInfo) is false)
                return;
            I18nDatas.RemoveAt(itemInfo.Index);
            I18nDatas.Insert(itemInfo.Index, cultureDatas);
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

    //private void ReplaceCulture(string oldCulture, string newCulture)
    //{
    //    CultureChanged?.Invoke(oldCulture, newCulture);
    //}

    public event CultureChangedEventHandler? CultureChanged;
    #endregion
}

public delegate void CultureChangedEventHandler(string oldCulture, string newCulture);
