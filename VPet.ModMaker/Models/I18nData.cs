using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
public class I18nData : ObservableObjectX
{
    #region ID
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _id = string.Empty;

    /// <summary>
    /// ID
    /// </summary>
    public string ID
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }
    #endregion

    /// <summary>
    /// 基于 <see cref="I18nHelper.Current.CultureNames"/> 的索引的数据列表
    /// </summary>
    public ObservableList<Func<INotifyPropertyChanged, string>> Datas { get; } = new();
}
