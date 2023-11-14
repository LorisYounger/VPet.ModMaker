using HKW.HKWViewModels.SimpleObservable;
using Panuon.WPF.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.Views.ModEdit.I18nEdit;

/// <summary>
/// I18nEditWindow.xaml 的交互逻辑
/// </summary>
public partial class I18nEditWindow : WindowX
{
    public bool IsCancel { get; private set; } = true;

    public static I18nEditWindow Instance { get; private set; }

    public ObservableValue<string> Search { get; } = new();

    private void Search_ValueChanged(string oldValue, string newValue)
    {
        if (string.IsNullOrWhiteSpace(newValue))
        {
            ShowI18nDatas.Value = I18nDatas;
        }
        else if (SearchTarget.Value == nameof(ModInfoModel.Id))
        {
            ShowI18nDatas.Value = new(
                I18nDatas.Where(
                    m => m.Id.Value?.Contains(newValue, StringComparison.OrdinalIgnoreCase) is true
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
                            newValue,
                            StringComparison.OrdinalIgnoreCase
                        )
                            is true
                )
            );
        }
    }

    //public I18nEditWindowVM ViewModel => (I18nEditWindowVM)DataContext;

    public I18nEditWindow(ModInfoModel model)
    {
        InitializeComponent();
        DataContext = this;
        Instance = this;
        Search.ValueChanged += Search_ValueChanged;
        Closed += (s, e) =>
        {
            //if (IsCancel)
            //ViewModel.Close();
            I18nHelper.Current.CultureNames.CollectionChanged -= CultureNames_CollectionChanged;
            foreach (var i18nData in AllI18nDatas)
            {
                foreach (var data in i18nData.Value.Datas)
                    data.Group?.Clear();
            }
            try
            {
                DataContext = null;
                Instance = null;
            }
            catch { }
        };
        InitializeI18nData(model);
        ShowI18nDatas.Value = I18nDatas;
        SearchTarget.Value = nameof(ModInfoModel.Id);
    }

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

    private void IdChange(string oldValue, string newValue)
    {
        var sourceData = AllI18nDatas[oldValue];
        sourceData.Id.Group?.Remove(sourceData.Id);
        if (AllI18nDatas.TryGetValue(oldValue, out var outData))
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
            sourceData.Id.Value = newValue;
            AllI18nDatas.Remove(oldValue);
            AllI18nDatas.Add(newValue, sourceData);
        }
    }

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

    private readonly Dictionary<string, DataGridTextColumn> _dataGridI18nColumns = new();

    /// <summary>
    /// (Id, I18nData)
    /// </summary>
    public Dictionary<string, I18nData> AllI18nDatas { get; } = new();
    public ObservableCollection<I18nData> I18nDatas { get; } = new();
    public ObservableValue<ObservableCollection<I18nData>> ShowI18nDatas { get; } = new();

    /// <summary>
    /// 搜索目标列表
    /// </summary>
    public ObservableCollection<string> SearchTargets { get; } = new() { nameof(ModInfoModel.Id) };

    /// <summary>
    /// 搜索目标
    /// </summary>
    public ObservableValue<string> SearchTarget { get; } = new();

    #region CultureEdit
    // TODO: 国际化标头
    private const string ValueBindingFormat = "Datas[{0}].Value";

    /// <summary>
    /// 添加文化列
    /// </summary>
    /// <param name="culture"></param>
    public void AddCulture(string culture)
    {
        var dataPath = string.Format(
            ValueBindingFormat,
            I18nHelper.Current.CultureNames.IndexOf(culture)
        );
        // 文化数据列
        var column = new DataGridTextColumn()
        {
            MaxWidth = 300,
            Header = culture,
            Binding = new Binding(dataPath) { Mode = BindingMode.TwoWay },
            ElementStyle = (Style)Utils.ModMakerStyles["TextBlock_Wrap"],
            SortMemberPath = dataPath
        };
        DataGrid_Datas.Columns.Add(column);
        _dataGridI18nColumns.Add(culture, column);
    }

    /// <summary>
    /// 删除文化列
    /// </summary>
    /// <param name="culture"></param>
    public void RemoveCulture(string culture)
    {
        DataGrid_Datas.Columns.Remove(_dataGridI18nColumns[culture]);
        _dataGridI18nColumns.Remove(culture);
        for (var i = 1; i < DataGrid_Datas.Columns.Count; i++)
        {
            var column = (DataGridTextColumn)DataGrid_Datas.Columns[i];
            var dataPath = string.Format(ValueBindingFormat, i - 1);
            column.Binding = new Binding(dataPath) { Mode = BindingMode.TwoWay };
            column.SortMemberPath = dataPath;
        }
    }

    /// <summary>
    /// 替换文化列
    /// </summary>
    /// <param name="oldCulture"></param>
    /// <param name="newCulture"></param>
    public void ReplaceCulture(string oldCulture, string newCulture)
    {
        //if (_dataGridI18nColumns.ContainsKey(newCultureName))
        //    throw new();
        var column = _dataGridI18nColumns[oldCulture];
        column.Header = newCulture;
        _dataGridI18nColumns.Remove(oldCulture);
        _dataGridI18nColumns.Add(newCulture, column);
    }

    #endregion
}
