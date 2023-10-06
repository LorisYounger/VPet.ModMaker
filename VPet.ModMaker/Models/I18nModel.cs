using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPet.ModMaker.Models;

/// <summary>
/// I18n模型
/// </summary>
/// <typeparam name="T">类型</typeparam>
public class I18nModel<T>
    where T : class, new()
{
    /// <summary>
    /// 当前I18n数据
    /// </summary>
    public ObservableValue<T> CurrentI18nData { get; } = new();

    /// <summary>
    /// 所有I18n数据
    /// </summary>
    public Dictionary<string, T> I18nDatas { get; } = new();

    public I18nModel()
    {
        I18nHelper.Current.CultureName.ValueChanged += CultureChanged;
        I18nHelper.Current.AddCulture += AddCulture;
        I18nHelper.Current.RemoveCulture += RemoveCulture;
        I18nHelper.Current.ReplaceCulture += ReplaceCulture;
        if (I18nHelper.Current.CultureNames.Count == 0)
            return;
        foreach (var item in I18nHelper.Current.CultureNames)
        {
            I18nDatas.Add(item, new());
        }
        CurrentI18nData.Value = I18nDatas[I18nHelper.Current.CultureName.Value];
    }

    /// <summary>
    /// 文化改变
    /// </summary>
    /// <param name="oldValue"></param>
    /// <param name="newValue"></param>
    private void CultureChanged(string oldValue, string newValue)
    {
        if (I18nDatas.TryGetValue(newValue, out var result))
            CurrentI18nData.Value = result;
    }

    /// <summary>
    /// 添加文化
    /// </summary>
    /// <param name="culture">文化名称</param>
    private void AddCulture(string culture)
    {
        if (I18nDatas.ContainsKey(culture) is false)
            I18nDatas.Add(culture, new());
    }

    /// <summary>
    /// 删除文化
    /// </summary>
    /// <param name="culture">文化名称</param>
    private void RemoveCulture(string culture)
    {
        I18nDatas.Remove(culture);
    }

    /// <summary>
    /// 替换文化
    /// </summary>
    /// <param name="oldCulture">旧文化名称</param>
    /// <param name="newCulture">新文化名称</param>
    private void ReplaceCulture(string oldCulture, string newCulture)
    {
        var item = I18nDatas[oldCulture];
        I18nDatas.Remove(oldCulture);
        I18nDatas.Add(newCulture, item);
    }
}
