﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HKW.HKWUtils.Observable;

namespace VPet.ModMaker.Models;

/// <summary>
/// I18n数据
/// </summary>
[DebuggerDisplay("{ID}, Count = {Datas.Count}")]
public class I18nData : ObservableObjectX<I18nData>
{
    /// <summary>
    /// Id
    /// </summary>
    #region Id
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _id = string.Empty;

    public string Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }
    #endregion

    /// <summary>
    /// 基于 <see cref="I18nHelper.Current.CultureNames"/> 的索引的数据列表
    /// </summary>
    public ObservableCollection<ObservableValue<string>> Datas { get; } = new();
}
