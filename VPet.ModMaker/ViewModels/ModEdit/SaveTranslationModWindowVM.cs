using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HKW.HKWUtils.Observable;
using LinePutScript.Localization.WPF;
using Microsoft.Win32;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels.ModEdit;

public class SaveTranslationModWindowVM : ObservableObjectX<SaveTranslationModWindowVM>
{
    #region Value
    #region CheckAll
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private bool? _checkAll;

    public bool? CheckAll
    {
        get => _checkAll;
        set => SetProperty(ref _checkAll, value);
    }
    #endregion

    public ObservableList<CheckCultureModel> CheckCultures { get; } = [];
    #endregion

    #region Command
    public ObservableCommand SaveCommand { get; } = new();
    #endregion

    public SaveTranslationModWindowVM()
    {
        //TODO
        //foreach (var culture in I18nHelper.Current.CultureNames)
        //{
        //    var model = new CheckCultureModel();
        //    model.CultureName.Value = culture;
        //    CheckCultures.Add(model);
        //    CheckAll.AddNotifySender(model.IsChecked);
        //}
        //CheckAll.ValueChanged += CheckAll_ValueChanged;
        //CheckAll.SenderPropertyChanged += CheckAll_SenderPropertyChanged;
        //SaveCommand.ExecuteCommand += Save;
    }

    private void CheckAll_ValueChanged(
        ObservableValue<bool?> sender,
        ValueChangedEventArgs<bool?> e
    )
    {
        if (e.NewValue is null)
        {
            if (CheckCultures.All(m => m.IsChecked))
                CheckAll = false;
            else if (CheckCultures.All(m => m.IsChecked is false))
                CheckAll = true;
            return;
        }
        foreach (var model in CheckCultures)
            model.IsChecked = e.NewValue.Value;
    }

    private void CheckAll_SenderPropertyChanged(
        ObservableValue<bool?> source,
        INotifyPropertyChanged sender
    )
    {
        var count = 0;
        foreach (var model in CheckCultures)
            if (model.IsChecked)
                count += 1;

        if (count == CheckCultures.Count)
            source.Value = true;
        else if (count == 0)
            source.Value = false;
        else
            source.Value = null;
    }

    public void Save()
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
                Path.GetDirectoryName(saveFileDialog.FileName),
                CheckCultures.Where(m => m.IsChecked).Select(m => m.CultureName)
            );
            MessageBox.Show("保存成功".Translate());
        }
        catch (Exception ex)
        {
            MessageBox.Show("保存失败 错误信息:\n{0}".Translate(ex));
        }
    }
}

public class CheckCultureModel : ObservableObjectX<CheckCultureModel>
{
    #region IsChecked
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private bool _isChecked;

    public bool IsChecked
    {
        get => _isChecked;
        set => SetProperty(ref _isChecked, value);
    }
    #endregion
    #region CultureName
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _cultureName = string.Empty;

    public string CultureName
    {
        get => _cultureName;
        set => SetProperty(ref _cultureName, value);
    }
    #endregion
}
