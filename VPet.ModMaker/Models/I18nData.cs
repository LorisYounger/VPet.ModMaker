using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPet.ModMaker.Models;

/// <summary>
/// I18n数据
/// </summary>
[DebuggerDisplay("{Id}, Count = {Datas.Count}")]
public class I18nData
{
    /// <summary>
    /// Id
    /// </summary>
    public ObservableValue<string> Id { get; } = new();

    /// <summary>
    /// 基于 <see cref="I18nHelper.Current.CultureNames"/> 的索引的数据列表
    /// </summary>
    public ObservableCollection<ObservableValue<string>> Datas { get; } = new();
}
