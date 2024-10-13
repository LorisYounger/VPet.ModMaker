using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HKW.HKWReactiveUI;
using HKW.HKWUtils.Observable;
using LinePutScript.Localization.WPF;
using Microsoft.Win32;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels.ModEdit;

public partial class SaveTranslationModWindowVM : ViewModelBase
{
    #region Property
    [ReactiveProperty]
    public bool? CheckAll { get; set; }

    public ObservableList<CheckCultureModel> CheckCultures { get; } = [];
    #endregion

    //#region Command
    //public ObservableCommand SaveCommand { get; } = new();
    //#endregion

    public SaveTranslationModWindowVM()
    {
        foreach (var culture in ModInfoModel.Current.I18nResource.Cultures)
        {
            var model = new CheckCultureModel(culture);
            model.Culture = culture;
            // TODO:使用SelectionGroup
            //model.PropertyChangedX += Model_PropertyChangedX;
            CheckCultures.Add(model);
        }
        //PropertyChangedX += SaveTranslationModWindowVM_PropertyChangedX;
        //SaveCommand.ExecuteCommand += Save;
    }

    //private void SaveTranslationModWindowVM_PropertyChangedX(
    //    object? sender,
    //    PropertyChangedXEventArgs e
    //)
    //{
    //    if (e.NewValue is null)
    //    {
    //        if (CheckCultures.All(m => m.IsChecked))
    //            CheckAll = false;
    //        else if (CheckCultures.All(m => m.IsChecked is false))
    //            CheckAll = true;
    //        return;
    //    }
    //    foreach (var model in CheckCultures)
    //        model.IsChecked = e.NewValue is true;
    //}

    //private void Model_PropertyChangedX(object? sender, PropertyChangedXEventArgs e)
    //{
    //    var count = 0;
    //    foreach (var model in CheckCultures)
    //        if (model.IsChecked)
    //            count += 1;
    //    if (count == CheckCultures.Count)
    //        CheckAll = true;
    //    else if (count == 0)
    //        CheckAll = false;
    //    else
    //        CheckAll = null;
    //}

    [ReactiveCommand]
    private void Save()
    {
        SaveFileDialog saveFileDialog =
            new()
            {
                Title = "保存模组信息文件,并在文件夹内保存模组数据".Translate(),
                Filter = $"LPS文件|*.lps;".Translate(),
                FileName = "info.lps".Translate()
            };
        if (saveFileDialog.ShowDialog() is not true)
            return;
        try
        {
            ModInfoModel.Current.SaveTranslationMod(
                Path.GetDirectoryName(saveFileDialog.FileName)!,
                CheckCultures.Where(m => m.IsChecked).Select(m => m.Culture)
            );
            MessageBox.Show("保存成功".Translate());
        }
        catch (Exception ex)
        {
            MessageBox.Show("保存失败 错误信息:\n{0}".Translate(ex));
        }
    }
}

public partial class CheckCultureModel : ViewModelBase
{
    public CheckCultureModel(CultureInfo culture)
    {
        Culture = culture;
    }

    [ReactiveProperty]
    public bool IsChecked { get; set; }

    [ReactiveProperty]
    public CultureInfo Culture { get; set; }
}
