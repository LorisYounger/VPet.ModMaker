using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPet.ModMaker.Models;

public class I18nModel<T>
    where T : class, new()
{
    public ObservableValue<T> CurrentI18nData { get; } = new();
    public Dictionary<string, T> I18nDatas { get; } = new();

    public I18nModel()
    {
        I18nHelper.Current.CultureName.ValueChanged += LangChanged;
        I18nHelper.Current.AddLang += AddLang;
        I18nHelper.Current.RemoveLang += RemoveLang;
        I18nHelper.Current.ReplaceLang += ReplaceLang;
        if (I18nHelper.Current.CultureNames.Count == 0)
            return;
        foreach (var item in I18nHelper.Current.CultureNames)
        {
            I18nDatas.Add(item, new());
        }
        CurrentI18nData.Value = I18nDatas[I18nHelper.Current.CultureName.Value];
    }

    private void LangChanged(string oldValue, string newValue)
    {
        if (I18nDatas.TryGetValue(newValue, out var result))
            CurrentI18nData.Value = result;
    }

    private void AddLang(string lang)
    {
        if (I18nDatas.ContainsKey(lang) is false)
            I18nDatas.Add(lang, new());
    }

    private void RemoveLang(string lang)
    {
        I18nDatas.Remove(lang);
    }

    private void ReplaceLang(string oldLang, string newLang)
    {
        var item = I18nDatas[oldLang];
        I18nDatas.Remove(oldLang);
        I18nDatas.Add(newLang, item);
    }
}
