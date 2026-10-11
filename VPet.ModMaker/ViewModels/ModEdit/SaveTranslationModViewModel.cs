using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using HKW.WPF.MVVMDialogs;
using LinePutScript.Localization.WPF;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels;

/// <summary>
/// 保持为翻译模组视图模型
/// </summary>
public partial class SaveTranslationModViewModel : ViewModelBase, IModalDialogViewModel
{
    /// <inheritdoc/>
    public SaveTranslationModViewModel() { }

    #region Property

    /// <summary>
    /// 文化选择组
    /// </summary>
    public ObservableList<CultureInfo> Cultures { get; } = [];

    public ObservableList<CultureInfo> SelectedCultures { get; } = [];
    #endregion

    /// <summary>
    /// 模组信息
    /// </summary>
    [ObservableProperty]
    public ModInfoModel ModInfo { get; set; }

    partial void OnModInfoChanged(ModInfoModel oldValue, ModInfoModel newValue)
    {
        Cultures.Clear();
        SelectedCultures.Clear();
        if (newValue is not null)
        {
            Cultures.AddRange(newValue.I18nResource.Cultures);
            SelectedCultures.AddRange(newValue.I18nResource.Cultures);
        }
    }

    public bool? DialogResult { get; set; }

    /// <summary>
    /// 保存
    /// </summary>
    [RelayCommand]
    private void Save()
    {
        var saveFileDialog = DialogService.ShowOpenFolderDialog(
            this,
            new() { Title = "保存模组至文件夹".Translate() }
        );
        if (saveFileDialog is null)
            return;
        try
        {
            ModInfo.SaveToTranslationMod(saveFileDialog.LocalPath, SelectedCultures);
            //this.LogX().Info("翻译模组保存成功");
            DialogService.ShowMessageBox(this, "翻译模组保存成功".Translate());
        }
        catch (Exception ex)
        {
            //this.LogX().Warn(ex, "翻译模组保存失败");
            DialogService.ShowMessageBox(this, "翻译模组保存失败, 详情请查看日志".Translate());
        }
    }
}
