using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
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
using HKW.MVVMDialogs;
using ReactiveUI;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels.ModEdit;

/// <summary>
/// I18n编辑
/// </summary>
public partial class I18nEditVM : DialogViewModel
{
    /// <inheritdoc/>
    public I18nEditVM(ModInfoModel modInfo)
    {
        ModInfo = modInfo;
        ModInfo.I18nResource.ClearUnreferencedData();
        this.WhenValueChanged(x => x.Search)
            .Throttle(TimeSpan.FromSeconds(0.5), RxApp.TaskpoolScheduler)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => I18nDatas.Refresh());
        SearchTargets
            .WhenValueChanged(x => x.SelectedItem)
            .Throttle(TimeSpan.FromSeconds(0.5), RxApp.TaskpoolScheduler)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => I18nDatas.Refresh());

        ModInfo.I18nResource.Cultures.SetChanged -= Cultures_SetChanged;
        ModInfo.I18nResource.Cultures.SetChanged += Cultures_SetChanged;

        ModInfo.I18nResource.CultureDatas.DictionaryChanged -= CultureDatas_DictionaryChanged;
        ModInfo.I18nResource.CultureDatas.DictionaryChanged += CultureDatas_DictionaryChanged;

        I18nDatas = new([], [], DataFilter);
        foreach (var pair in ModInfo.I18nResource.CultureDatas)
        {
            I18nDatas.Add(pair.Value);
            pair.Value.DictionaryChanged += Data_DictionaryChanged;
        }
        foreach (var culture in ModInfo.I18nResource.Cultures)
        {
            SearchTargets.Add(culture.Name);
        }
    }

    /// <summary>
    /// 模组信息
    /// </summary>
    public ModInfoModel ModInfo { get; } = null!;

    /// <summary>
    /// 单元格编辑
    /// </summary>
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
    public ObservableSelectableList<string, List<string>> SearchTargets { get; } = new(["ID"], 0);

    private bool DataFilter(ObservableCultureDataDictionary<string, string> item)
    {
        if (SearchTargets.SelectedItem == nameof(ModInfoModel.ID))
        {
            // 如果是ID则搜索ID
            return item.Key.Contains(Search, StringComparison.OrdinalIgnoreCase);
        }
        else
        {
            // 如果是I18n数据则搜索对应文化
            if (
                item.TryGetValue(
                    CultureInfo.GetCultureInfo(SearchTargets.SelectedItem!),
                    out var data
                )
            )
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

    /// <summary>
    /// 文化改变事件
    /// </summary>
    public event CultureChangedEventHandler? CultureChanged;
    #endregion
}

/// <summary>
/// 文化改变
/// </summary>
/// <param name="oldCulture">旧文化</param>
/// <param name="newCulture">新文化</param>
public delegate void CultureChangedEventHandler(string oldCulture, string newCulture);
