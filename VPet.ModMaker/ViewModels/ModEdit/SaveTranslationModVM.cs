﻿using System.Globalization;
using HanumanInstitute.MvvmDialogs;
using HKW.HKWReactiveUI;
using HKW.HKWUtils.Observable;
using HKW.MVVMDialogs;
using HKW.WPF.MVVMDialogs;
using LinePutScript.Localization.WPF;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels.ModEdit;

/// <summary>
/// 保持为翻译模组视图模型
/// </summary>
public partial class SaveTranslationModVM : DialogViewModel, IEnableLogger<ViewModelBase>
{
    /// <inheritdoc/>
    public SaveTranslationModVM(ModInfoModel modInfo)
    {
        ModInfo = modInfo;
        CheckCultures = new(modInfo.I18nResource.Cultures);
        CheckCultures.Leader.Value = true;
    }

    #region Property

    /// <summary>
    /// 文化选择组
    /// </summary>
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
        var saveFileDialog = NativeUtils.DialogService.ShowOpenFolderDialog(
            this,
            new() { Title = "保存模组至文件夹".Translate() }
        );
        if (saveFileDialog is null)
            return;
        try
        {
            ModInfo.SaveToTranslationMod(
                saveFileDialog.LocalPath,
                CheckCultures.Where(m => m.IsSelected).Select(m => m.Source)
            );
            this.LogX().Info("翻译模组保存成功");
            NativeUtils.DialogService.ShowMessageBoxX(this, "翻译模组保存成功".Translate());
        }
        catch (Exception ex)
        {
            this.LogX().Warn(ex, "翻译模组保存失败");
            NativeUtils.DialogService.ShowMessageBoxX(this, "翻译模组保存失败, 详情请查看日志".Translate());
        }
    }
}
