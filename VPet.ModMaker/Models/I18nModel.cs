using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using Mapster;

namespace VPet.ModMaker.Models;

/// <summary>
/// I18n模型
/// </summary>
/// <typeparam name="T">类型</typeparam>
public class I18nModel<T> : ObservableObjectX<I18nModel<T>>
    where T : class, new()
{
    /// <summary>
    /// 当前I18n数据
    /// </summary>
    #region CurrentI18nData
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private T _currentI18nData;

    [AdaptIgnore]
    public T CurrentI18nData
    {
        get => _currentI18nData;
        set => SetProperty(ref _currentI18nData, value);
    }
    #endregion

    /// <summary>
    /// 所有I18n数据
    /// </summary>
    [AdaptIgnore]
    public Dictionary<string, T> I18nDatas { get; } = new();

    public I18nModel()
    {
        I18nHelper.Current.PropertyChangedX += Current_PropertyChangedX;
        I18nHelper.Current.CultureNames.ListChanged += CultureNames_ListChanged;
        if (I18nHelper.Current.CultureNames.HasValue() is false)
            return;
        foreach (var item in I18nHelper.Current.CultureNames)
            I18nDatas.Add(item, new());
        CurrentI18nData = I18nDatas[I18nHelper.Current.CultureName];
    }

    private void CultureNames_ListChanged(
        IObservableList<string> sender,
        NotifyListChangedEventArgs<string> e
    )
    {
        if (e.Action is ListChangeAction.Add && e.NewItems is not null)
        {
            foreach (var item in e.NewItems)
                I18nDatas.TryAdd(item, new());
        }
        else if (e.Action is ListChangeAction.Remove && e.OldItems is not null)
        {
            foreach (var item in e.OldItems)
                I18nDatas.Remove(item);
        }
        else if (
            e.Action is ListChangeAction.Add
            && e.NewItems is not null
            && e.OldItems is not null
        )
        {
            var newItem = e.NewItems.First();
            var oldItem = e.OldItems.First();
            if (I18nDatas.ContainsKey(oldItem) is false)
                return;
            I18nDatas[newItem] = I18nDatas[oldItem];
            I18nDatas.Remove(oldItem);
        }
    }

    /// <summary>
    /// 文化改变
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void Current_PropertyChangedX(I18nHelper sender, PropertyChangedXEventArgs e)
    {
        if (e.PropertyName == nameof(I18nHelper.CultureName))
        {
            if (e.NewValue is null)
                CurrentI18nData = null!;
            else if (I18nDatas.TryGetValue((string)e.NewValue, out var result))
                CurrentI18nData = result;
        }
    }
}
