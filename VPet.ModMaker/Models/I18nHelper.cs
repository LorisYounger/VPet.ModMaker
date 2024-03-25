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
public class I18nHelper : ObservableObjectX<I18nHelper>
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
    public ObservableCollection<string> CultureNames { get; } = new();

    public I18nHelper()
    {
        CultureNames.CollectionChanged += Cultures_CollectionChanged;
    }

    private void Cultures_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        // 替换
        if (e.NewStartingIndex == e.OldStartingIndex)
        {
            ReplaceCulture?.Invoke((string)e.OldItems[0], (string)e.NewItems[0]);
            return;
        }
        // 删除
        if (e.OldItems is not null)
        {
            RemoveCulture?.Invoke((string)e.OldItems[0]);
        }
        // 新增
        if (e.NewItems is not null)
        {
            AddCulture?.Invoke((string)e.NewItems[0]);
        }
    }

    /// <summary>
    /// 添加文化事件
    /// </summary>
    public event CultureEventHandler AddCulture;

    /// <summary>
    /// 删除文化事件
    /// </summary>
    public event CultureEventHandler RemoveCulture;

    /// <summary>
    /// 修改文化事件
    /// </summary>
    public event ReplaceCultureEventHandler ReplaceCulture;

    public delegate void CultureEventHandler(string culture);
    public delegate void ReplaceCultureEventHandler(string oldCulture, string newCulture);
}
