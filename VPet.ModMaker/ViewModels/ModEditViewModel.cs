using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using HKW.HKWUtils;
using HKW.HKWUtils.Extensions;
using HKW.MVVM;
using HKW.MVVM.SourceGenerator;
using HKW.WPF;
using HKW.WPF.Extensions;
using HKW.WPF.MVVMDialogs;
using LinePutScript.Localization.WPF;
using Microsoft.Extensions.Logging;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels;

/// <summary>
/// 模组编辑视图模型
/// </summary>
public sealed partial class ModEditViewModel
    : ViewModelBase,
        IViewClosing,
        IViewClosed,
        IModalDialogViewModel
{
    /// <inheritdoc/>
    public ModEditViewModel() { }

    #region Property
    /// <summary>
    /// 当前模组信息
    /// </summary>
    [ObservableProperty]
    public ModInfoModel ModInfo { get; set; } = null!;

    partial void OnModInfoChanged(ModInfoModel oldValue, ModInfoModel newValue)
    {
        oldValue?.Dispose();
        if (newValue is not null)
        {
            if (ModInfo.I18nResource.Cultures.Count == 0)
            {
                DialogService.ShowMessageBox(
                    this,
                    "未添加任何文化,请添加文化".Translate(),
                    "缺少文化".Translate(),
                    icon: MessageBoxImage.Information
                );
                AddCulture();
                if (ModInfo.I18nResource.Cultures.Count == 0)
                {
                    DialogService.ShowMessageBox(
                        this,
                        "未设置文化, 将退出编辑".Translate(),
                        "数据错误".Translate(),
                        icon: MessageBoxImage.Warning
                    );
                    DialogService.DialogManager.FindViewByViewModel(this)?.Close();
                    return;
                }
            }
            // 更新模组
            if (ModUpdataHelper.CanUpdata(ModInfo))
            {
                var result = DialogService.ShowMessageBox(
                    this,
                    "是否更新模组\n当前版本: {0}\n最新版本: {1}".Translate(
                        ModInfo.ModVersion,
                        ModUpdataHelper.LastGameVersion
                    ),
                    "更新模组".Translate(),
                    MessageBoxButton.YesNo
                );
                if (result is true)
                {
                    try
                    {
                        var version = ModUpdataHelper.Updata(ModInfo);
                        DialogService.ShowMessageBox(
                            this,
                            "更新成功更新至版本 {0}, 请手动保存".Translate(version)
                        );
                        this.LogIfEnabled(LogLevel.Information)
                            ?.Log("更新成功更新至版本 {version}", version);
                    }
                    catch (Exception ex)
                    {
                        DialogService.ShowMessageBox(
                            this,
                            "模组更新失败, 详情请查看日志".Translate()
                        );
                        this.LogIfEnabled(LogLevel.Error)?.Log(ex, "模组更新失败");
                    }
                }
            }
        }
        foreach (var vm in ViewModels.Values)
            vm.ModInfo = ModInfo;
    }

    public Dictionary<EditType, IEditViewModel> ViewModels =>
        field ??= new()
        {
            [EditType.Food] = DialogService.CreateViewModel<FoodEditViewModel>(),
            [EditType.ClickText] = DialogService.CreateViewModel<ClickTextEditViewModel>(),
            [EditType.LowText] = DialogService.CreateViewModel<LowTextEditViewModel>(),
            [EditType.SelectText] = DialogService.CreateViewModel<SelectTextEditViewModel>(),
            [EditType.Theme] = DialogService.CreateViewModel<ThemeEditViewModel>(),
            [EditType.Font] = DialogService.CreateViewModel<FontEditViewModel>(),
            [EditType.Pet] = DialogService.CreateViewModel<PetEditViewModel>(),
            [EditType.Work] = DialogService.CreateViewModel<WorkEditViewModel>(),
            [EditType.Move] = DialogService.CreateViewModel<MoveEditViewModel>(),
            [EditType.Anime] = DialogService.CreateViewModel<AnimeViewModel>(),
        };

    public bool? DialogResult { get; }
    #endregion

    #region Command
    private SaveTranslationModViewModel SaveTranslationModViewModel =>
        field ??= DialogService.CreateViewModel<SaveTranslationModViewModel>();

    /// <summary>
    /// 保存为翻译模组
    /// </summary>
    [RelayCommand]
    private void SaveAsTranslationMod()
    {
        SaveTranslationModViewModel.ModInfo = ModInfo;
        DialogService.ShowDialog(this, SaveTranslationModViewModel);
    }

    private I18nEditViewModel I18nEditViewModel =>
        field ??= DialogService.CreateViewModel<I18nEditViewModel>();

    /// <summary>
    /// 编辑I18n数据
    /// </summary>
    [RelayCommand]
    private void EditI18n()
    {
        ModInfo.I18nResource.ClearUnobservedData();
        I18nEditViewModel.ModInfo = ModInfo;
        DialogService.ShowDialog(this, I18nEditViewModel);
    }

    /// <summary>
    /// 改变图片
    /// </summary>
    [RelayCommand]
    private void ChangeImage()
    {
        var openFileDialog = DialogService.ShowOpenFileDialog(
            this,
            new()
            {
                Title = "选择图片".Translate(),
                Filters = [new("图片".Translate(), ["jpg", "jpeg", "png", "bmp"])],
            }
        );
        if (openFileDialog is null)
            return;
        var newImage = HKWImageUtils.LoadImageToMemory(openFileDialog.LocalPath);
        if (newImage is null)
        {
            DialogService.ShowMessageBox(
                this,
                "图片载入失败, 详情请查看日志".Translate(),
                "图片载入失败".Translate(),
                icon: MessageBoxImage.Warning
            );
            return;
        }
        ModInfo.Image?.CloseStream();
        ModInfo.Image = newImage;
    }

    private AddCultureViewModel AddCultureViewModel =>
        field ??= DialogService.CreateViewModel<AddCultureViewModel>();

    #region Culture
    /// <summary>
    /// 添加文化
    /// </summary>
    [RelayCommand]
    public void AddCulture()
    {
        AddCultureViewModel.ModInfo = ModInfo;
        var result = DialogService.ShowDialog(this, AddCultureViewModel);
        if (result is not true)
        {
            Reset();
            return;
        }
        var currentInfo = CultureInfo.GetCultureInfo(AddCultureViewModel.CultureName);
        ModInfo.I18nResource.AddCulture(currentInfo);
        if (ModInfo.I18nResource.CurrentCulture is null)
            ModInfo.I18nResource.CurrentCulture = currentInfo;
        this.LogIfEnabled(LogLevel.Information)?.Log("添加文化 {culture}", currentInfo.FullName);
        Reset();
    }

    /// <summary>
    /// 编辑文化
    /// </summary>
    /// <param name="oldCulture">旧文化</param>
    [RelayCommand]
    private void EditCulture(CultureInfo oldCulture)
    {
        var result = DialogService.ShowDialog(this, AddCultureViewModel);
        if (result is not true || oldCulture.Name == AddCultureViewModel.CultureName)
        {
            Reset();
            return;
        }
        var currentInfo = CultureInfo.GetCultureInfo(AddCultureViewModel.CultureName);
        ModInfo.I18nResource.ReplaceCulture(oldCulture, currentInfo);
        if (ModInfo.I18nResource.CurrentCulture is null)
            ModInfo.I18nResource.SetCurrentCulture(currentInfo);
        this.LogIfEnabled(LogLevel.Information)
            ?.Log("重命名文化 {oldCulture} => {newCulture}", oldCulture.Name, currentInfo.Name);
        Reset();
    }

    /// <summary>
    /// 删除文化
    /// </summary>
    /// <param name="oldCulture">旧文化</param>
    [RelayCommand]
    private void RemoveCulture(CultureInfo oldCulture)
    {
        if (ModInfo.I18nResource.Cultures.Count == 1)
        {
            DialogService.ShowMessageBox(
                this,
                "你不能删除最后一个文化".Translate(),
                "删除文化".Translate()
            );
            return;
        }
        var result = DialogService.ShowMessageBox(
            this,
            "确定删除文化 '{0}' 吗, 这将删除文化及其所有文化数据".Translate(oldCulture.Name),
            "删除文化".Translate(),
            MessageBoxButton.YesNo
        );
        if (result is not true)
            return;
        var isOld = ModInfo.I18nResource.CurrentCulture == oldCulture;
        ModInfo.I18nResource.RemoveCulture(oldCulture);
        if (isOld)
            ModInfo.I18nResource.CurrentCulture = ModInfo.I18nResource.Cultures.FirstOrDefault()!;
        this.LogIfEnabled(LogLevel.Information)?.Log("删除文化 {culture}", oldCulture.Name);
    }
    #endregion

    #region Save

    /// <summary>
    /// 打开模组所在的文件夹
    /// </summary>
    [RelayCommand]
    private void OpenModPath()
    {
        if (Path.Exists(ModInfo.SourcePath) is false)
        {
            DialogService.ShowMessageBox(
                this,
                "源路径为空, 请先保存模组".Translate(),
                "打开所在的文件夹失败".Translate(),
                icon: MessageBoxImage.Warning
            );
            return;
        }
        NativeUtils.OpenLink(ModInfo.SourcePath);
    }

    /// <summary>
    /// 保存
    /// </summary>
    [RelayCommand]
    private void Save()
    {
        if (ValidationData(ModInfo) is false)
            return;
        if (string.IsNullOrEmpty(ModInfo.SourcePath))
        {
            DialogService.ShowMessageBox(
                this,
                "源路径为空, 请使用 '保存至'".Translate(),
                "保存失败".Translate(),
                icon: MessageBoxImage.Warning
            );
            return;
        }
        var result = DialogService.ShowMessageBox(
            this,
            "确定保存吗".Translate(),
            "保存".Translate(),
            MessageBoxButton.YesNo
        );
        if (result is not true)
            return;
        SaveTo(ModInfo.SourcePath);
    }

    /// <summary>
    /// 保存至
    /// </summary>
    [RelayCommand]
    private void SaveTo()
    {
        if (ValidationData(ModInfo) is false)
            return;
        var saveFileDialog = DialogService.ShowOpenFolderDialog(
            this,
            new() { Title = "选择保存的文件夹".Translate() }
        );
        if (saveFileDialog is null)
            return;
        SaveTo(saveFileDialog.LocalPath);
    }
    #endregion
    /// <summary>
    /// 保存至
    /// </summary>
    /// <param name="path">路径</param>
    private void SaveTo(string path)
    {
        var pending = DialogService.ShowPendingBox(this, "保存中".Translate());
        try
        {
            ModInfo.SaveTo(path);
            if (string.IsNullOrWhiteSpace(ModInfo.SourcePath))
                ModInfo.SourcePath = path;
            pending.Close();
            DialogService.ShowMessageBox(this, "保存成功".Translate());
            this.LogIfEnabled(LogLevel.Information)?.Log("成功保存至 '{path}'", path);
        }
        catch (Exception ex)
        {
            pending.Close();
            DialogService.ShowMessageBox(this, "保存失败, 详情请查看日志".Translate());
            this.LogIfEnabled(LogLevel.Error)?.Log(ex, "保存失败");
        }
    }

    /// <summary>
    /// 验证数据
    /// </summary>
    /// <param name="model">模型</param>
    /// <returns>成功为 <see langword="true"/> 失败为 <see langword="false"/></returns>
    private bool ValidationData(ModInfoModel model)
    {
        if (ModInfo.I18nResource.DatasByKey.Count is 0)
        {
            DialogService.ShowMessageBox(
                this,
                "未添加任何语言".Translate(),
                "数据错误".Translate(),
                icon: MessageBoxImage.Warning
            );
            return false;
        }
        if (string.IsNullOrWhiteSpace(model.ID))
        {
            DialogService.ShowMessageBox(
                this,
                "ID不可为空".Translate(),
                "数据错误".Translate(),
                icon: MessageBoxImage.Warning
            );
            return false;
        }
        if (string.IsNullOrWhiteSpace(model.Author))
        {
            DialogService.ShowMessageBox(
                this,
                "作者不可为空".Translate(),
                "数据错误".Translate(),
                icon: MessageBoxImage.Warning
            );
            return false;
        }
        return true;
    }

    #endregion
    public void OnClosing(CancelEventArgs e)
    {
        if (ModInfo is null || ModInfo.I18nResource.Cultures.Count == 0)
        {
            ModInfo?.Dispose();
            return;
        }
        var result = DialogService.ShowMessageBox(
            this,
            "确认退出吗?".Translate(),
            "退出编辑".Translate(),
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning
        );
        if (result is not true)
        {
            e.Cancel = true;
        }
    }

    public void Reset()
    {
        AddCultureViewModel.ModInfo = null!;
        AddCultureViewModel.CultureName = string.Empty;
    }

    public void OnClosed()
    {
        SaveTranslationModViewModel.ModInfo = null!;
        AddCultureViewModel.ModInfo = null!;
        DialogService.Close(I18nEditViewModel);
        ModInfo?.Dispose();
        ModInfo = null!;
        foreach (var vm in ViewModels.Values.OfType<IDisposable>())
            vm.Dispose();
    }

    Task IViewClosing.OnClosingAsync(CancelEventArgs e)
    {
        return Task.CompletedTask;
    }
}
