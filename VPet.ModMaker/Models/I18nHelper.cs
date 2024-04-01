using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HKW.HKWUtils.Observable;

namespace VPet.ModMaker.Models;

// TODO: 更新事件
/// <summary>
/// I18n助手
/// </summary>
public class I18nHelper : ObservableObjectX
{
    /// <summary>
    /// 当前数据
    /// </summary>
    public static I18nHelper Current { get; set; } = new();

    /// <summary>
    /// 当前文化名称
    /// </summary>
    #region CultureName
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _cultureName;

    public string CultureName
    {
        get => _cultureName;
        set => SetProperty(ref _cultureName, value);
    }
    #endregion

    /// <summary>
    /// 文化列表
    /// </summary>
    public ObservableList<string> CultureNames { get; } = new();
}
