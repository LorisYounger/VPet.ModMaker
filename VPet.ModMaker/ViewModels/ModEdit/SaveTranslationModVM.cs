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
using HanumanInstitute.MvvmDialogs;
using HKW.HKWReactiveUI;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using HKW.MVVMDialogs;
using HKW.WPF.MVVMDialogs;
using LinePutScript.Localization.WPF;
using Microsoft.Win32;
using Splat;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels.ModEdit;

public partial class SaveTranslationModVM : DialogViewModel
{
    private static IDialogService DialogService => Locator.Current.GetService<IDialogService>()!;

    public SaveTranslationModVM(ModInfoModel modInfo)
    {
        ModInfo = modInfo;
        CheckCultures = new(modInfo.I18nResource.Cultures);
    }

    #region Property

    public ObservableSelectionGroup<CultureInfo> CheckCultures { get; }
    #endregion

    /// <summary>
    /// 模组信息
    /// </summary>
    public ModInfoModel ModInfo { get; }

    /// <summary>
    /// 保存
    /// </summary>
    [ReactiveCommand]
    private void Save()
    {
        var saveFileDialog = DialogService.ShowSaveFileDialog(
            this,
            new()
            {
                Title = "保存模组信息文件,并在文件夹内保存模组数据".Translate(),
                Filters = [new("LPS文件".Translate(), "lps")],
                SuggestedFileName = "info.lps".Translate()
            }
        );
        if (saveFileDialog is null)
            return;
        try
        {
            ModInfo.SaveToTranslationMod(
                Path.GetDirectoryName(saveFileDialog.Name)!,
                CheckCultures.Where(m => m.IsSelected).Select(m => m.Source)
            );
            this.Log().Info("保存成功");
            DialogService.ShowMessageBoxX(this, "保存成功".Translate());
        }
        catch (Exception ex)
        {
            this.Log().Warn("保存失败", ex);
            DialogService.ShowMessageBoxX(this, "保存失败, 详情请查看日志".Translate());
        }
    }
}
