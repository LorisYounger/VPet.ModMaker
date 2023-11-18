using HKW.HKWViewModels.SimpleObservable;
using LinePutScript.Localization.WPF;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels.ModEdit;

public class SaveTranslationModWindowVM
{
    #region Value
    public ObservableValue<bool?> CheckAll { get; } = new(true);

    public ObservableCollection<CheckCultureModel> CheckCultures { get; } = [];
    #endregion

    #region Command
    public ObservableCommand SaveCommand { get; } = new();
    #endregion

    public SaveTranslationModWindowVM()
    {
        foreach (var culture in I18nHelper.Current.CultureNames)
        {
            var model = new CheckCultureModel();
            model.CultureName.Value = culture;
            CheckCultures.Add(model);
            CheckAll.AddNotifySender(model.IsChecked);
        }
        CheckAll.SenderPropertyChanged += CheckAll_SenderPropertyChanged;
        SaveCommand.ExecuteEvent += Save;
    }

    private void CheckAll_SenderPropertyChanged(ObservableValue<bool?> source, INotifyPropertyChanged sender)
    {
        var count = 0;
        foreach (var model in CheckCultures)
            if (model.IsChecked.Value)
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
        SaveFileDialog saveFileDialog = new()
        {
            Title = "保存模组信息文件,并在文件夹内保存模组数据".Translate(),
            Filter = $"LPS文件|*.lps;".Translate(),
            FileName = "info.lps".Translate()
        };
        if (saveFileDialog.ShowDialog() is not true)
            return;
        try
        {
            ModInfoModel.Current.SaveTranslationMod(Path.GetDirectoryName(saveFileDialog.FileName), CheckCultures.Where(m => m.IsChecked.Value).Select(m => m.CultureName.Value));
            MessageBox.Show("保存成功".Translate());
        }
        catch (Exception ex)
        {
            MessageBox.Show("保存失败 错误信息:\n{0}".Translate(ex));
        }
    }
}

public class CheckCultureModel
{
    public ObservableValue<bool> IsChecked { get; } = new(true);
    public ObservableValue<string> CultureName { get; } = new();
}
