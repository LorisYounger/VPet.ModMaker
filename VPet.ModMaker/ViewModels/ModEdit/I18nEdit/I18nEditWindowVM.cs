using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels.ModEdit.I18nEdit;

public class I18nEditWindowVM
{
    /// <summary>
    /// 搜索
    /// </summary>
    public ObservableValue<string> Search { get; } = new();

    /// <summary>
    /// 全部I18n数据 (Id, I18nData)
    /// </summary>
    public Dictionary<string, I18nData> AllI18nDatas { get; } = new();

    /// <summary>
    /// 全部I18n数据
    /// </summary>
    public ObservableCollection<I18nData> I18nDatas { get; } = new();

    /// <summary>
    /// 显示的I18n数据
    /// </summary>
    public ObservableValue<ObservableCollection<I18nData>> ShowI18nDatas { get; } = new();

    /// <summary>
    /// 搜索目标列表
    /// </summary>
    public ObservableCollection<string> SearchTargets { get; } = new() { nameof(ModInfoModel.Id) };

    /// <summary>
    /// 搜索目标
    /// </summary>
    public ObservableValue<string> SearchTarget { get; } = new();

    public I18nEditWindowVM()
    {
        Search.ValueChanged += Search_ValueChanged;
        ShowI18nDatas.Value = I18nDatas;
        SearchTarget.Value = nameof(ModInfoModel.Id);
    }

    /// <summary>
    /// 搜索改变事件
    /// </summary>
    /// <param name="oldValue"></param>
    /// <param name="newValue"></param>
    private void Search_ValueChanged(
        ObservableValue<string> sender,
        ValueChangedEventArgs<string> e
    )
    {
        if (string.IsNullOrWhiteSpace(e.NewValue))
        {
            ShowI18nDatas.Value = I18nDatas;
        }
        else if (SearchTarget.Value == nameof(ModInfoModel.Id))
        {
            ShowI18nDatas.Value = new(
                I18nDatas.Where(
                    m =>
                        m.Id.Value?.Contains(e.NewValue, StringComparison.OrdinalIgnoreCase) is true
                )
            );
        }
        else
        {
            var cultureIndex = I18nHelper.Current.CultureNames.IndexOf(SearchTarget.Value);
            ShowI18nDatas.Value = new(
                I18nDatas.Where(
                    m =>
                        m.Datas[cultureIndex].Value?.Contains(
                            e.NewValue,
                            StringComparison.OrdinalIgnoreCase
                        )
                            is true
                )
            );
        }
    }

    /// <summary>
    /// 文化列表改变事件
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void CultureNames_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action is NotifyCollectionChangedAction.Add)
        {
            var newCulture = (string)e.NewItems[0];
            AddCulture(newCulture);
            SearchTargets.Add(newCulture);
            foreach (var data in AllI18nDatas)
                data.Value.Datas.Add(new());
        }
        else if (e.Action is NotifyCollectionChangedAction.Remove)
        {
            var oldCulture = (string)e.OldItems[0];
            RemoveCulture(oldCulture);
            SearchTargets.Remove(oldCulture);
            foreach (var data in AllI18nDatas)
            {
                var value = data.Value.Datas[e.OldStartingIndex];
                value.Group?.Remove(value);
                data.Value.Datas.RemoveAt(e.OldStartingIndex);
            }
            if (SearchTarget.Value is null)
                SearchTarget.Value = nameof(ModInfoModel.Id);
        }
        else if (e.Action is NotifyCollectionChangedAction.Replace)
        {
            var oldCulture = (string)e.OldItems[0];
            var newCulture = (string)e.NewItems[0];
            ReplaceCulture(oldCulture, newCulture);
            SearchTargets[SearchTargets.IndexOf(oldCulture)] = newCulture;
        }
    }

    #region LoadData
    /// <summary>
    /// 初始化I18n数据
    /// </summary>
    /// <param name="model"></param>
    public void InitializeI18nData(ModInfoModel model)
    {
        foreach (var culture in I18nHelper.Current.CultureNames)
        {
            AddCulture(culture);
            SearchTargets.Add(culture);
        }
        I18nHelper.Current.CultureNames.CollectionChanged -= CultureNames_CollectionChanged;
        I18nHelper.Current.CultureNames.CollectionChanged += CultureNames_CollectionChanged;
        LoadFood(model);
        LoadClickText(model);
        LoadLowText(model);
        LoadSelectText(model);
        LoadPets(model);
    }

    /// <summary>
    /// 载入食物
    /// </summary>
    /// <param name="model"></param>
    private void LoadFood(ModInfoModel model)
    {
        foreach (var food in model.Foods)
        {
            AddData(food.Id, food, (m) => m.Name);
            AddData(food.DescriptionId, food, (m) => m.Description);
        }
        model.Foods.CollectionChanged += (s, e) =>
        {
            if (e.Action is NotifyCollectionChangedAction.Add)
            {
                var newModel = (FoodModel)e.NewItems[0];
                AddData(newModel.Id, newModel, (m) => m.Name);
                AddData(newModel.DescriptionId, newModel, (m) => m.Description);
            }
            else if (e.Action is NotifyCollectionChangedAction.Remove)
            {
                var oldModel = (FoodModel)e.OldItems[0];
                RemoveData(oldModel.Id, oldModel, (m) => m.Name);
                RemoveData(oldModel.DescriptionId, oldModel, (m) => m.Description);
            }
            else if (e.Action is NotifyCollectionChangedAction.Replace)
            {
                var newModel = (FoodModel)e.NewItems[0];
                var oldModel = (FoodModel)e.OldItems[0];
                ReplaceData(newModel.Id, newModel, (m) => m.Name);
                ReplaceData(newModel.DescriptionId, newModel, (m) => m.Description);
            }
        };
    }

    /// <summary>
    /// 载入点击文本
    /// </summary>
    /// <param name="model"></param>
    private void LoadClickText(ModInfoModel model)
    {
        foreach (var text in model.ClickTexts)
        {
            AddData(text.Id, text, (m) => m.Text);
        }
        model.ClickTexts.CollectionChanged += (s, e) =>
        {
            if (e.Action is NotifyCollectionChangedAction.Add)
            {
                var newModel = (ClickTextModel)e.NewItems[0];
                AddData(newModel.Id, newModel, (m) => m.Text);
            }
            else if (e.Action is NotifyCollectionChangedAction.Remove)
            {
                var oldModel = (ClickTextModel)e.OldItems[0];
                RemoveData(oldModel.Id, oldModel, (m) => m.Text);
            }
            else if (e.Action is NotifyCollectionChangedAction.Replace)
            {
                var newModel = (ClickTextModel)e.NewItems[0];
                var oldModel = (ClickTextModel)e.OldItems[0];
                ReplaceData(newModel.Id, newModel, (m) => m.Text);
            }
        };
    }

    /// <summary>
    /// 载入低状态为文本
    /// </summary>
    /// <param name="model"></param>
    private void LoadLowText(ModInfoModel model)
    {
        foreach (var text in model.LowTexts)
        {
            AddData(text.Id, text, (m) => m.Text);
        }
        model.LowTexts.CollectionChanged += (s, e) =>
        {
            if (e.Action is NotifyCollectionChangedAction.Add)
            {
                var newModel = (LowTextModel)e.NewItems[0];
                AddData(newModel.Id, newModel, (m) => m.Text);
            }
            else if (e.Action is NotifyCollectionChangedAction.Remove)
            {
                var oldModel = (LowTextModel)e.OldItems[0];
                RemoveData(oldModel.Id, oldModel, (m) => m.Text);
            }
            else if (e.Action is NotifyCollectionChangedAction.Replace)
            {
                var newModel = (LowTextModel)e.NewItems[0];
                var oldModel = (LowTextModel)e.OldItems[0];
                ReplaceData(newModel.Id, newModel, (m) => m.Text);
            }
        };
    }

    /// <summary>
    /// 载入选择文本
    /// </summary>
    /// <param name="model"></param>
    private void LoadSelectText(ModInfoModel model)
    {
        foreach (var text in model.SelectTexts)
        {
            AddData(text.Id, text, (m) => m.Text);
            AddData(text.ChooseId, text, (m) => m.Choose);
        }
        model.SelectTexts.CollectionChanged += (s, e) =>
        {
            if (e.Action is NotifyCollectionChangedAction.Add)
            {
                var newModel = (SelectTextModel)e.NewItems[0];
                AddData(newModel.Id, newModel, (m) => m.Text);
                AddData(newModel.ChooseId, newModel, (m) => m.Choose);
            }
            else if (e.Action is NotifyCollectionChangedAction.Remove)
            {
                var oldModel = (SelectTextModel)e.OldItems[0];
                RemoveData(oldModel.Id, oldModel, (m) => m.Text);
                RemoveData(oldModel.ChooseId, oldModel, (m) => m.Choose);
            }
            else if (e.Action is NotifyCollectionChangedAction.Replace)
            {
                var newModel = (SelectTextModel)e.NewItems[0];
                var oldModel = (SelectTextModel)e.OldItems[0];
                ReplaceData(newModel.Id, newModel, (m) => m.Text);
                ReplaceData(newModel.ChooseId, newModel, (m) => m.Choose);
            }
        };
    }

    /// <summary>
    /// 载入宠物
    /// </summary>
    /// <param name="model"></param>
    private void LoadPets(ModInfoModel model)
    {
        foreach (var pet in model.Pets)
        {
            if (pet.IsSimplePetModel)
                continue;
            AddData(pet.Id, pet, (m) => m.Name);
            AddData(pet.PetNameId, pet, (m) => m.PetName);
            AddData(pet.DescriptionId, pet, (m) => m.Description);
            foreach (var work in pet.Works)
                AddData(work.Id, work, (m) => m.Name);
        }
        model.Pets.CollectionChanged += (s, e) =>
        {
            if (e.Action is NotifyCollectionChangedAction.Add)
            {
                var newModel = (PetModel)e.NewItems[0];
                AddData(newModel.Id, newModel, (m) => m.Name);
                AddData(newModel.DescriptionId, newModel, (m) => m.Description);
                foreach (var work in newModel.Works)
                    AddData(work.Id, work, (m) => m.Name);
            }
            else if (e.Action is NotifyCollectionChangedAction.Remove)
            {
                var oldModel = (PetModel)e.OldItems[0];
                RemoveData(oldModel.Id, oldModel, (m) => m.Name);
                RemoveData(oldModel.DescriptionId, oldModel, (m) => m.Description);
                foreach (var work in oldModel.Works)
                    RemoveData(work.Id, work, (m) => m.Name);
            }
            else if (e.Action is NotifyCollectionChangedAction.Replace)
            {
                var newModel = (PetModel)e.NewItems[0];
                var oldModel = (PetModel)e.OldItems[0];
                ReplaceData(newModel.Id, newModel, (m) => m.Name);
                ReplaceData(newModel.DescriptionId, newModel, (m) => m.Description);
                foreach (var work in newModel.Works)
                    ReplaceData(work.Id, work, (m) => m.Name);
            }
        };
    }
    #endregion

    #region DatEdit
    /// <summary>
    /// 添加数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="id"></param>
    /// <param name="i18nModel"></param>
    /// <param name="i18nValue"></param>
    private void AddData<T>(
        ObservableValue<string> id,
        I18nModel<T> i18nModel,
        Func<T, ObservableValue<string>> i18nValue
    )
        where T : class, new()
    {
        if (AllI18nDatas.TryGetValue(id.Value, out var outData))
        {
            foreach (var culture in I18nHelper.Current.CultureNames.Enumerate())
            {
                if (outData.Datas[culture.Index].Group is null)
                {
                    var group = new ObservableValueGroup<string>() { outData.Datas[culture.Index] };
                }
                outData.Datas[culture.Index].Group!.Add(
                    i18nValue(i18nModel.I18nDatas[culture.Value])
                );
            }
        }
        else
        {
            var data = new I18nData();
            data.Id.Value = id.Value;
            foreach (var culture in I18nHelper.Current.CultureNames)
                data.Datas.Add(i18nValue(i18nModel.I18nDatas[culture]));
            I18nDatas.Add(data);
            AllI18nDatas.Add(id.Value, data);
            //id.ValueChanged += IdChange;
        }
    }

    /// <summary>
    /// Id改变
    /// </summary>
    /// <param name="oldValue"></param>
    /// <param name="newValue"></param>
    private void IdChange(ObservableValue<string> sender, ValueChangedEventArgs<string> e)
    {
        var sourceData = AllI18nDatas[e.OldValue];
        sourceData.Id.Group?.Remove(sourceData.Id);
        if (AllI18nDatas.TryGetValue(e.OldValue, out var outData))
        {
            foreach (var culture in I18nHelper.Current.CultureNames.Enumerate())
            {
                if (outData.Datas[culture.Index].Group is null)
                {
                    var group = new ObservableValueGroup<string>() { outData.Datas[culture.Index] };
                }
                outData.Datas[culture.Index].Group!.Add(sourceData.Datas[culture.Index]);
            }
        }
        else
        {
            sourceData.Id.Value = e.NewValue;
            AllI18nDatas.Remove(e.OldValue);
            AllI18nDatas.Add(e.NewValue, sourceData);
        }
    }

    /// <summary>
    /// 删除I18n数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="id"></param>
    /// <param name="i18nModel"></param>
    /// <param name="i18nValue"></param>
    private void RemoveData<T>(
        ObservableValue<string> id,
        I18nModel<T> i18nModel,
        Func<T, ObservableValue<string>> i18nValue
    )
        where T : class, new()
    {
        var data = AllI18nDatas[id.Value];
        foreach (var culture in I18nHelper.Current.CultureNames.Enumerate())
        {
            if (data.Datas[culture.Index].Group is ObservableValueGroup<string> group)
            {
                group.Remove(i18nValue(i18nModel.I18nDatas[culture.Value]));
                if (group.Count == 1)
                {
                    group.Clear();
                    return;
                }
            }
            else
            {
                I18nDatas.Remove(data);
                AllI18nDatas.Remove(id.Value);
                return;
            }
        }
    }

    /// <summary>
    /// 替换I18n数据
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="id"></param>
    /// <param name="i18nModel"></param>
    /// <param name="i18nValue"></param>
    private void ReplaceData<T>(
        ObservableValue<string> id,
        I18nModel<T> i18nModel,
        Func<T, ObservableValue<string>> i18nValue
    )
        where T : class, new()
    {
        var data = AllI18nDatas[id.Value];
        foreach (var culture in I18nHelper.Current.CultureNames.Enumerate())
        {
            var oldValue = data.Datas[culture.Index];
            var newValue = i18nValue(i18nModel.I18nDatas[culture.Value]);
            if (oldValue.Group is ObservableValueGroup<string> group)
            {
                group.Add(newValue);
                group.Remove(oldValue);
            }
            data.Datas[culture.Index] = newValue;
        }
    }
    #endregion

    public void Close()
    {
        I18nHelper.Current.CultureNames.CollectionChanged -= CultureNames_CollectionChanged;
        foreach (var i18nData in AllI18nDatas)
        {
            foreach (var data in i18nData.Value.Datas)
                data.Group?.Clear();
        }
    }

    #region Event
    private void AddCulture(string culture)
    {
        CultureChanged?.Invoke(null, culture);
    }

    private void RemoveCulture(string culture)
    {
        CultureChanged?.Invoke(culture, string.Empty);
    }

    private void ReplaceCulture(string oldCulture, string newCulture)
    {
        CultureChanged?.Invoke(oldCulture, newCulture);
    }

    public event EventHandler<string> CultureChanged;
    #endregion
}
