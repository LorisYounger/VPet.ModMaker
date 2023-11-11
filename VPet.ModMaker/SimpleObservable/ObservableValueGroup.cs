using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HKW.HKWViewModels.SimpleObservable;

/// <summary>
/// 可观察值组合
/// <para>示例:<code><![CDATA[
/// var value1 = new ObservableValue<string>();
/// var value2 = new ObservableValue<string>();
/// var group = new ObservableValueGroup<string>() { value1, value2 };
/// value1.Value = "A";
/// // value1 == "A", value2 == "A"
/// group.Remove(value1);
/// value1.Value = "C";
/// // value1 == "C", value2 == "A"]]></code></para>
/// </summary>
/// <typeparam name="T">值类型</typeparam>
[DebuggerDisplay("\\{ObservableValueGroup, Count = {Count}\\}")]
public class ObservableValueGroup<T> : IEnumerable<ObservableValue<T>?>
{
    /// <summary>
    /// 数量
    /// </summary>
    public int Count => _bindingValues.Count;

    /// <summary>
    /// 在添加的时候改变值 (如果分组中存在值)
    /// </summary>
    [DefaultValue(false)]
    public bool ChangeOnAdd { get; set; } = false;

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private readonly Dictionary<Guid, WeakReference<ObservableValue<T>>> _bindingValues = new();

    /// <summary>
    /// 添加项
    /// </summary>
    /// <param name="items">项</param>
    public void Add(params ObservableValue<T>[] items)
    {
        foreach (var item in items)
            AddToGroup(item);
    }

    private void AddToGroup(ObservableValue<T> item)
    {
        if (item.Group is not null)
            throw new ArgumentException("item.Group must be null", nameof(item));
        _bindingValues.Add(item.Guid, new(item));
        item.ValueChanged -= Item_ValueChanged;
        if (ChangeOnAdd)
        {
            foreach (var bindingValue in _bindingValues)
            {
                if (bindingValue.Value.TryGetTarget(out var target))
                {
                    item.Value = target.Value;
                    break;
                }
            }
        }
        item.ValueChanged += Item_ValueChanged;
        item.Group = this;
    }

    /// <summary>
    /// 删除项
    /// </summary>
    /// <param name="items">项</param>
    public void Remove(params ObservableValue<T>[] items)
    {
        foreach (var item in items)
            RemoveFromGroup(item);
    }

    private void RemoveFromGroup(ObservableValue<T> item)
    {
        var result = _bindingValues.Remove(item.Guid);
        if (result)
        {
            item.ValueChanged -= Item_ValueChanged;
            item.Group = null;
        }
    }

    /// <summary>
    /// 清空分组
    /// </summary>
    public void Clear()
    {
        foreach (var bindingValue in _bindingValues)
        {
            if (bindingValue.Value.TryGetTarget(out var target))
            {
                target.ValueChanged -= Item_ValueChanged;
                target.Group = null;
            }
        }
        _bindingValues.Clear();
    }

    /// <summary>
    /// 查找项
    /// </summary>
    /// <param name="item">项</param>
    /// <returns>包含为 <see langword="true"/> 不包含为 <see langword="false"/></returns>
    public bool Contains(ObservableValue<T> item)
    {
        return _bindingValues.ContainsKey(item.Guid);
    }

    /// <inheritdoc/>
    public IEnumerator<ObservableValue<T>?> GetEnumerator()
    {
        return _bindingValues.Values
            .Select(v => v.TryGetTarget(out var t) ? t : null)
            .GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private bool _onChange = false;

    private void Item_ValueChanged(T oldValue, T newValue)
    {
        if (_onChange)
            return;
        _onChange = true;
        foreach (var bindingValue in _bindingValues.AsEnumerable())
        {
            if (bindingValue.Value.TryGetTarget(out var target))
                target.Value = newValue;
            else
                _bindingValues.Remove(bindingValue.Key);
        }
        _onChange = false;
    }
}
