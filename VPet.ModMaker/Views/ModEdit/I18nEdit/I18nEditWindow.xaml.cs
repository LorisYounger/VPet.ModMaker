using HKW.HKWViewModels.SimpleObservable;
using Panuon.WPF.UI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        else
        {
            ShowI18nDatas.Value = new(
                I18nDatas.Where(
                    m => m.Id.Value.Contains(newValue, StringComparison.OrdinalIgnoreCase)
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
            try
            {
                DataContext = null;
                Instance = null;
            }
            catch { }
        };
        InitializeI18nData(model);
        ShowI18nDatas.Value = I18nDatas;
    }

    public void InitializeI18nData(ModInfoModel model)
    {
        foreach (var culture in I18nHelper.Current.CultureNames)
        {
            AddCulture(culture);
        }
        LoadFood(model);
        LoadClickText(model);
        LoadLowText(model);
        LoadSelectText(model);
        LoadPets(model);
    }

    private void LoadFood(ModInfoModel model)
    {
        foreach (var food in model.Foods)
        {
            var nameData = new I18nData();
            var descriptionData = new I18nData();
            nameData.Id.Value = food.Id.Value;
            descriptionData.Id.Value = food.DescriptionId.Value;
            foreach (var culture in I18nHelper.Current.CultureNames)
            {
                nameData.Cultures.Add(culture);
                nameData.Datas.Add(food.I18nDatas[culture].Name);
                descriptionData.Cultures.Add(culture);
                descriptionData.Datas.Add(food.I18nDatas[culture].Description);
            }
            I18nDatas.Add(nameData);
            I18nDatas.Add(descriptionData);
        }
    }

    private void LoadClickText(ModInfoModel model)
    {
        foreach (var text in model.ClickTexts)
        {
            var data = new I18nData();
            data.Id.Value = text.Id.Value;
            foreach (var culture in I18nHelper.Current.CultureNames)
            {
                data.Cultures.Add(culture);
                data.Datas.Add(text.I18nDatas[culture].Text);
            }
            I18nDatas.Add(data);
        }
    }

    private void LoadLowText(ModInfoModel model)
    {
        foreach (var text in model.LowTexts)
        {
            var data = new I18nData();
            data.Id.Value = text.Id.Value;
            foreach (var culture in I18nHelper.Current.CultureNames)
            {
                data.Cultures.Add(culture);
                data.Datas.Add(text.I18nDatas[culture].Text);
            }
            I18nDatas.Add(data);
        }
    }

    private void LoadSelectText(ModInfoModel model)
    {
        foreach (var text in model.SelectTexts)
        {
            var data = new I18nData();
            var chooseData = new I18nData();
            data.Id.Value = text.Id.Value;
            chooseData.Id.Value = text.ChooseId.Value;
            foreach (var culture in I18nHelper.Current.CultureNames)
            {
                data.Cultures.Add(culture);
                data.Datas.Add(text.I18nDatas[culture].Text);
                chooseData.Cultures.Add(culture);
                chooseData.Datas.Add(text.I18nDatas[culture].Choose);
            }
            I18nDatas.Add(data);
            I18nDatas.Add(chooseData);
        }
    }

    private void LoadPets(ModInfoModel model)
    {
        foreach (var pet in model.Pets)
        {
            var data = new I18nData();
            var petNameData = new I18nData();
            var descriptionData = new I18nData();
            data.Id.Value = pet.Id.Value;
            petNameData.Id.Value = pet.PetNameId.Value;
            descriptionData.Id.Value = pet.DescriptionId.Value;
            foreach (var culture in I18nHelper.Current.CultureNames)
            {
                data.Cultures.Add(culture);
                data.Datas.Add(pet.I18nDatas[culture].Name);
                petNameData.Cultures.Add(culture);
                petNameData.Datas.Add(pet.I18nDatas[culture].PetName);
                descriptionData.Cultures.Add(culture);
                descriptionData.Datas.Add(pet.I18nDatas[culture].Description);
            }
            I18nDatas.Add(data);
            I18nDatas.Add(petNameData);
            I18nDatas.Add(descriptionData);
        }
    }

    private readonly Dictionary<string, DataGridTextColumn> _dataGridI18nColumns = new();
    public HashSet<string> Ids { get; } = new();
    public ObservableCollection<I18nData> I18nDatas { get; } = new();
    public ObservableValue<ObservableCollection<I18nData>> ShowI18nDatas { get; } = new();

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
        var dataPath = string.Format(
            ValueBindingFormat,
            I18nHelper.Current.CultureNames.IndexOf(newCulture)
        );
        var column = _dataGridI18nColumns[oldCulture];
        column.Header = newCulture;
        column.Binding = new Binding(dataPath) { Mode = BindingMode.TwoWay };
        column.SortMemberPath = dataPath;
        _dataGridI18nColumns.Remove(oldCulture);
        _dataGridI18nColumns.Add(newCulture, column);
    }
    #endregion
}
