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

    public ObservableSelectionGroup<CultureInfo> CheckCultures { get; }
    #endregion

    public SaveTranslationModWindowVM()
    {
        CheckCultures = new(ModInfoModel.Current.I18nResource.Cultures);
    }

    /// <summary>
    /// 保存
    /// </summary>
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
                CheckCultures.Where(m => m.IsSelected).Select(m => m.Source)
            );
            MessageBox.Show("保存成功".Translate());
        }
        catch (Exception ex)
        {
            MessageBox.Show("保存失败 错误信息:\n{0}".Translate(ex));
        }
    }
}
