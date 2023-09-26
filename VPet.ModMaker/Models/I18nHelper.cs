using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPet.ModMaker.Models;

public class I18nHelper
{
    public static I18nHelper Current { get; set; } = new();
    public ObservableValue<string> CultureName { get; } = new();
    public ObservableCollection<string> CultureNames { get; } = new();

    public I18nHelper()
    {
        CultureNames.CollectionChanged += Cultures_CollectionChanged;
    }

    private void Cultures_CollectionChanged(
        object sender,
        System.Collections.Specialized.NotifyCollectionChangedEventArgs e
    )
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

    public event CultureEventHandler AddCulture;
    public event CultureEventHandler RemoveCulture;
    public event ReplaceCultureEventHandler ReplaceCulture;

    public delegate void CultureEventHandler(string culture);
    public delegate void ReplaceCultureEventHandler(string oldCulture, string newCulture);
}
