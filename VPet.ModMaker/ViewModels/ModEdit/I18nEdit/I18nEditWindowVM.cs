using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HKW.HKWUtils.Observable;
using Mapster;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels.ModEdit.I18nEdit;

public class I18nEditWindowVM : ObservableObjectX { }
//{
//    public I18nEditWindowVM()
//    {
//        I18nDatas = new()
//        {
//            Filter = (d) =>
//            {
//                if (SearchTarget == nameof(ModInfoModel.ID))
//                {
//                    return d.ID.Contains(Search, StringComparison.OrdinalIgnoreCase);
//                }
//                else
//                {
//                    var cultureIndex = I18nHelper.Current.CultureNames.IndexOf(SearchTarget);
//                    throw new();
//                    //return d.Datas[cultureIndex]()
//                    //    .Contains(Search, StringComparison.OrdinalIgnoreCase);
//                }
//            },
//            FilteredList = new()
//        };
//        SearchTarget = nameof(ModInfoModel.ID);
//        PropertyChanged += I18nEditWindowVM_PropertyChanged;
//    }

//    private void I18nEditWindowVM_PropertyChanged(
//        object? sender,
//        System.ComponentModel.PropertyChangedEventArgs e
//    )
//    {
//        if (e.PropertyName == nameof(Search))
//        {
//            I18nDatas.Refresh();
//        }
//        else if (e.PropertyName == nameof(SearchTarget))
//        {
//            I18nDatas.Refresh();
//        }
//    }

//    #region Search
//    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
//    private string _search = string.Empty;

//    /// <summary>
//    /// 搜索
//    /// </summary>
//    public string Search
//    {
//        get => _search;
//        set => SetProperty(ref _search, value);
//    }
//    #endregion

//    /// <summary>
//    /// 全部I18n资源 (ID, I18nData)
//    /// </summary>
//    public Dictionary<string, I18nData> AllI18nDatas { get; } = new();

//    /// <summary>
//    /// 全部的I18n资源
//    /// </summary>
//    #region ShowI18nDatas
//    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
//    private ObservableFilterList<I18nData, ObservableList<I18nData>> _i18nDatas;

//    public ObservableFilterList<I18nData, ObservableList<I18nData>> I18nDatas
//    {
//        get => _i18nDatas;
//        set => SetProperty(ref _i18nDatas, value);
//    }
//    #endregion

//    /// <summary>
//    /// 搜索目标列表
//    /// </summary>
//    public ObservableList<string> SearchTargets { get; } = new() { nameof(ModInfoModel.ID) };

//    /// <summary>
//    /// 搜索目标
//    /// </summary>
//    #region SearchTarget
//    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
//    private string _searchTarget;

//    public string SearchTarget
//    {
//        get => _searchTarget;
//        set => SetProperty(ref _searchTarget, value);
//    }
//    #endregion

//    /// <summary>
//    /// 文化列表改变事件
//    /// </summary>
//    /// <param name="sender"></param>
//    /// <param name="e"></param>
//    private void CultureNames_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
//    {
//        if (e.Action is NotifyCollectionChangedAction.Add)
//        {
//            var newCulture = (string)e.NewItems[0];
//            AddCulture(newCulture);
//            SearchTargets.Add(newCulture);
//            foreach (var data in AllI18nDatas)
//                data.Value.Datas.Add(new());
//        }
//        else if (e.Action is NotifyCollectionChangedAction.Remove)
//        {
//            var oldCulture = (string)e.OldItems[0];
//            RemoveCulture(oldCulture);
//            SearchTargets.Remove(oldCulture);
//            foreach (var data in AllI18nDatas)
//            {
//                var value = data.Value.Datas[e.OldStartingIndex];
//                value.Group?.Remove(value);
//                data.Value.Datas.RemoveAt(e.OldStartingIndex);
//            }
//            if (SearchTarget is null)
//                SearchTarget = nameof(ModInfoModel.ID);
//        }
//        else if (e.Action is NotifyCollectionChangedAction.Replace)
//        {
//            var oldCulture = (string)e.OldItems[0];
//            var newCulture = (string)e.NewItems[0];
//            ReplaceCulture(oldCulture, newCulture);
//            SearchTargets[SearchTargets.IndexOf(oldCulture)] = newCulture;
//        }
//    }

//    #region LoadData
//    /// <summary>
//    /// 初始化I18n资源
//    /// </summary>
//    /// <param name="model"></param>
//    public void InitializeI18nData(ModInfoModel model)
//    {
//        foreach (var culture in I18nHelper.Current.CultureNames)
//        {
//            AddCulture(culture);
//            SearchTargets.Add(culture);
//        }
//        try
//        {
//            LoadFood(model);
//            LoadClickText(model);
//            LoadLowText(model);
//            LoadSelectText(model);
//            LoadPets(model);
//        }
//        catch
//        {
//            Close();
//            throw;
//        }
//        I18nHelper.Current.CultureNames.CollectionChanged -= CultureNames_CollectionChanged;
//        I18nHelper.Current.CultureNames.CollectionChanged += CultureNames_CollectionChanged;
//    }

//    /// <summary>
//    /// 载入食物
//    /// </summary>
//    /// <param name="model"></param>
//    private void LoadFood(ModInfoModel model)
//    {
//        foreach (var food in model.Foods)
//        {
//            AddData(food.ID, food, (m) => m.Name);
//            AddData(food.DescriptionID, food, (m) => m.Description);
//        }
//        model.Foods.CollectionChanged += (s, e) =>
//        {
//            if (e.Action is NotifyCollectionChangedAction.Add)
//            {
//                var newModel = (FoodModel)e.NewItems[0];
//                AddData(newModel.ID, newModel, (m) => m.Name);
//                AddData(newModel.DescriptionID, newModel, (m) => m.Description);
//            }
//            else if (e.Action is NotifyCollectionChangedAction.Remove)
//            {
//                var oldModel = (FoodModel)e.OldItems[0];
//                RemoveData(oldModel.ID, oldModel, (m) => m.Name);
//                RemoveData(oldModel.DescriptionID, oldModel, (m) => m.Description);
//            }
//            else if (e.Action is NotifyCollectionChangedAction.Replace)
//            {
//                var newModel = (FoodModel)e.NewItems[0];
//                var oldModel = (FoodModel)e.OldItems[0];
//                ReplaceData(newModel.ID, newModel, (m) => m.Name);
//                ReplaceData(newModel.DescriptionID, newModel, (m) => m.Description);
//            }
//        };
//    }

//    /// <summary>
//    /// 载入点击文本
//    /// </summary>
//    /// <param name="model"></param>
//    private void LoadClickText(ModInfoModel model)
//    {
//        foreach (var text in model.ClickTexts)
//        {
//            AddData(text.ID, text, (m) => m.Text);
//        }
//        model.ClickTexts.CollectionChanged += (s, e) =>
//        {
//            if (e.Action is NotifyCollectionChangedAction.Add)
//            {
//                var newModel = (ClickTextModel)e.NewItems[0];
//                AddData(newModel.ID, newModel, (m) => m.Text);
//            }
//            else if (e.Action is NotifyCollectionChangedAction.Remove)
//            {
//                var oldModel = (ClickTextModel)e.OldItems[0];
//                RemoveData(oldModel.ID, oldModel, (m) => m.Text);
//            }
//            else if (e.Action is NotifyCollectionChangedAction.Replace)
//            {
//                var newModel = (ClickTextModel)e.NewItems[0];
//                var oldModel = (ClickTextModel)e.OldItems[0];
//                ReplaceData(newModel.ID, newModel, (m) => m.Text);
//            }
//        };
//    }

//    /// <summary>
//    /// 载入低状态为文本
//    /// </summary>
//    /// <param name="model"></param>
//    private void LoadLowText(ModInfoModel model)
//    {
//        foreach (var text in model.LowTexts)
//        {
//            AddData(text.ID, text, (m) => m.Text);
//        }
//        model.LowTexts.CollectionChanged += (s, e) =>
//        {
//            if (e.Action is NotifyCollectionChangedAction.Add)
//            {
//                var newModel = (LowTextModel)e.NewItems[0];
//                AddData(newModel.ID, newModel, (m) => m.Text);
//            }
//            else if (e.Action is NotifyCollectionChangedAction.Remove)
//            {
//                var oldModel = (LowTextModel)e.OldItems[0];
//                RemoveData(oldModel.ID, oldModel, (m) => m.Text);
//            }
//            else if (e.Action is NotifyCollectionChangedAction.Replace)
//            {
//                var newModel = (LowTextModel)e.NewItems[0];
//                var oldModel = (LowTextModel)e.OldItems[0];
//                ReplaceData(newModel.ID, newModel, (m) => m.Text);
//            }
//        };
//    }

//    /// <summary>
//    /// 载入选择文本
//    /// </summary>
//    /// <param name="model"></param>
//    private void LoadSelectText(ModInfoModel model)
//    {
//        foreach (var text in model.SelectTexts)
//        {
//            AddData(text.ID, text, (m) => m.Text);
//            AddData(text.ChooseId, text, (m) => m.Choose);
//        }
//        model.SelectTexts.CollectionChanged += (s, e) =>
//        {
//            if (e.Action is NotifyCollectionChangedAction.Add)
//            {
//                var newModel = (SelectTextModel)e.NewItems[0];
//                AddData(newModel.ID, newModel, (m) => m.Text);
//                AddData(newModel.ChooseId, newModel, (m) => m.Choose);
//            }
//            else if (e.Action is NotifyCollectionChangedAction.Remove)
//            {
//                var oldModel = (SelectTextModel)e.OldItems[0];
//                RemoveData(oldModel.ID, oldModel, (m) => m.Text);
//                RemoveData(oldModel.ChooseId, oldModel, (m) => m.Choose);
//            }
//            else if (e.Action is NotifyCollectionChangedAction.Replace)
//            {
//                var newModel = (SelectTextModel)e.NewItems[0];
//                var oldModel = (SelectTextModel)e.OldItems[0];
//                ReplaceData(newModel.ID, newModel, (m) => m.Text);
//                ReplaceData(newModel.ChooseId, newModel, (m) => m.Choose);
//            }
//        };
//    }

//    /// <summary>
//    /// 载入宠物
//    /// </summary>
//    /// <param name="model"></param>
//    public void LoadPets(ModInfoModel model)
//    {
//        foreach (var pet in model.Pets)
//        {
//            if (pet.FromMain.Value)
//                continue;
//            AddData(pet.ID, pet, (m) => m.Name);
//            AddData(pet.PetNameId, pet, (m) => m.PetName);
//            AddData(pet.DescriptionID, pet, (m) => m.Description);
//            foreach (var work in pet.Works)
//                AddData(work.ID, work, (m) => m.Name);
//        }
//        model.Pets.CollectionChanged += (s, e) =>
//        {
//            if (e.Action is NotifyCollectionChangedAction.Add)
//            {
//                var newModel = (PetModel)e.NewItems[0];
//                AddData(newModel.ID, newModel, (m) => m.Name);
//                AddData(newModel.DescriptionID, newModel, (m) => m.Description);
//                foreach (var work in newModel.Works)
//                    AddData(work.ID, work, (m) => m.Name);
//            }
//            else if (e.Action is NotifyCollectionChangedAction.Remove)
//            {
//                var oldModel = (PetModel)e.OldItems[0];
//                if (oldModel.FromMain.Value)
//                    return;
//                RemoveData(oldModel.ID, oldModel, (m) => m.Name);
//                RemoveData(oldModel.DescriptionID, oldModel, (m) => m.Description);
//                foreach (var work in oldModel.Works)
//                    RemoveData(work.ID, work, (m) => m.Name);
//            }
//            else if (e.Action is NotifyCollectionChangedAction.Replace)
//            {
//                var newModel = (PetModel)e.NewItems[0];
//                var oldModel = (PetModel)e.OldItems[0];
//                ReplaceData(newModel.ID, newModel, (m) => m.Name);
//                ReplaceData(newModel.DescriptionID, newModel, (m) => m.Description);
//                foreach (var work in newModel.Works)
//                    ReplaceData(work.ID, work, (m) => m.Name);
//            }
//        };
//    }
//    #endregion

//    #region DatEdit
//    /// <summary>
//    /// 添加数据
//    /// </summary>
//    /// <typeparam name="T"></typeparam>
//    /// <param name="id"></param>
//    /// <param name="i18nModel"></param>
//    /// <param name="i18nValue"></param>
//    private void AddData<TViewModel, TI18nModel>(
//        TViewModel viewModel,
//        Func<TViewModel, string> getID,
//        Action<TViewModel, string> setID,
//        Func<TI18nModel, string> getI18nData,
//        Action<TI18nModel, string> setI18nData
//    )
//        where TViewModel : I18nModel<TI18nModel>
//        where TI18nModel : ObservableObjectX, new()
//    {
//        if (AllI18nDatas.TryGetValue(getID(viewModel), out var outData))
//        {
//            AdaptMemberAttribute
//            foreach (var culture in I18nHelper.Current.CultureNames.EnumerateIndex())
//            {
//                if (outData.Datas[culture.Index].Group is null)
//                {
//                    var group = new ObservableValueGroup<string>() { outData.Datas[culture.Index] };
//                }
//                outData
//                    .Datas[culture.Index]
//                    .Group!.Add(i18nValue(i18nModel.I18nDatas[culture.Value]));
//            }
//        }
//        else
//        {
//            var data = new I18nData();
//            data.ID = getID(viewModel);
//            foreach (var culture in I18nHelper.Current.CultureNames)
//                data.Datas.Add(i18nValue(i18nModel.I18nDatas[culture]));
//            I18nDatas.Add(data);
//            AllI18nDatas.Add(id.Value, data);
//            //id.ValueChanged += IdChange;
//        }
//    }

//    /// <summary>
//    /// Id改变
//    /// </summary>
//    /// <param name="oldValue"></param>
//    /// <param name="newValue"></param>
//    private void IdChange(ObservableValue<string> sender, ValueChangedEventArgs<string> e)
//    {
//        var sourceData = AllI18nDatas[e.OldValue];
//        //sourceData.ID.Group?.Remove(sourceData.ID); //TODO
//        if (AllI18nDatas.TryGetValue(e.OldValue, out var outData))
//        {
//            foreach (var culture in I18nHelper.Current.CultureNames.EnumerateIndex())
//            {
//                if (outData.Datas[culture.Index].Group is null)
//                {
//                    var group = new ObservableValueGroup<string>() { outData.Datas[culture.Index] };
//                }
//                outData.Datas[culture.Index].Group!.Add(sourceData.Datas[culture.Index]);
//            }
//        }
//        else
//        {
//            sourceData.ID = e.NewValue;
//            AllI18nDatas.Remove(e.OldValue);
//            AllI18nDatas.Add(e.NewValue, sourceData);
//        }
//    }

//    /// <summary>
//    /// 删除I18n资源
//    /// </summary>
//    /// <typeparam name="T"></typeparam>
//    /// <param name="id"></param>
//    /// <param name="i18nModel"></param>
//    /// <param name="i18nValue"></param>
//    private void RemoveData<T>(
//        ObservableValue<string> id,
//        I18nModel<T> i18nModel,
//        Func<T, ObservableValue<string>> i18nValue
//    )
//        where T : class, new()
//    {
//        var data = AllI18nDatas[id.Value];
//        foreach (var culture in I18nHelper.Current.CultureNames.EnumerateIndex())
//        {
//            if (data.Datas[culture.Index].Group is ObservableValueGroup<string> group)
//            {
//                group.Remove(i18nValue(i18nModel.I18nDatas[culture.Value]));
//                if (group.Count == 1)
//                {
//                    group.Clear();
//                    return;
//                }
//            }
//            else
//            {
//                I18nDatas.Remove(data);
//                AllI18nDatas.Remove(id.Value);
//                return;
//            }
//        }
//    }

//    /// <summary>
//    /// 替换I18n资源
//    /// </summary>
//    /// <typeparam name="T"></typeparam>
//    /// <param name="id"></param>
//    /// <param name="i18nModel"></param>
//    /// <param name="i18nValue"></param>
//    private void ReplaceData<T>(
//        ObservableValue<string> id,
//        I18nModel<T> i18nModel,
//        Func<T, ObservableValue<string>> i18nValue
//    )
//        where T : class, new()
//    {
//        var data = AllI18nDatas[id.Value];
//        foreach (var culture in I18nHelper.Current.CultureNames.EnumerateIndex())
//        {
//            var oldValue = data.Datas[culture.Index];
//            var newValue = i18nValue(i18nModel.I18nDatas[culture.Value]);
//            if (oldValue.Group is ObservableValueGroup<string> group)
//            {
//                group.Add(newValue);
//                group.Remove(oldValue);
//            }
//            data.Datas[culture.Index] = newValue;
//        }
//    }
//    #endregion

//    public void Close()
//    {
//        I18nHelper.Current.CultureNames.CollectionChanged -= CultureNames_CollectionChanged;
//        foreach (var i18nData in AllI18nDatas)
//        {
//            foreach (var data in i18nData.Value.Datas)
//                data.Group?.Clear();
//        }
//    }

//    #region Event
//    private void AddCulture(string culture)
//    {
//        CultureChanged?.Invoke(null, culture);
//    }

//    private void RemoveCulture(string culture)
//    {
//        CultureChanged?.Invoke(culture, string.Empty);
//    }

//    private void ReplaceCulture(string oldCulture, string newCulture)
//    {
//        CultureChanged?.Invoke(oldCulture, newCulture);
//    }

//    public event EventHandler<string> CultureChanged;
//    #endregion
//}
