using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using HKW.HKWReactiveUI;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using HKW.MVVMDialogs;
using HKW.WPF;
using HKW.WPF.Extensions;
using HKW.WPF.MVVMDialogs;
using LinePutScript.Localization.WPF;
using Panuon.WPF.UI;
using ReactiveUI;
using Splat;
using VPet.ModMaker.Models;
using VPet.ModMaker.Views.ModEdit;

namespace VPet.ModMaker.ViewModels.ModEdit;

/// <summary>
/// 模组编辑视图模型
/// </summary>
public partial class ModEditVM : ViewModelBase
{
    private static IDialogService DialogService => Locator.Current.GetService<IDialogService>()!;

    /// <inheritdoc/>
    public ModEditVM() { }

    #region Property
    /// <summary>
    /// 当前模组信息
    /// </summary>
    [ReactiveProperty]
    public ModInfoModel ModInfo { get; set; } = null!;

    partial void OnModInfoChanged(ModInfoModel oldValue, ModInfoModel newValue)
    {
        if (newValue is not null)
        {
            if (ModInfo.I18nResource.Cultures.Count == 0)
            {
                DialogService.ShowMessageBoxX(
                    this,
                    "未添加任何文化,请添加文化".Translate(),
                    "缺少文化".Translate(),
                    icon: MessageBoxImage.Information
                );
                AddCulture();
                if (ModInfo.I18nResource.Cultures.Count == 0)
                {
                    DialogService.ShowMessageBoxX(
                        this,
                        "未设置文化, 将退出编辑".Translate(),
                        "数据错误".Translate(),
                        icon: MessageBoxImage.Warning
                    );
                    MessageBus.Current.SendMessage<ModInfoModel?>(null);
                    return;
                }
            }
            // 更新模组
            if (ModUpdataHelper.CanUpdata(ModInfo) is false)
                return;
            if (
                DialogService.ShowMessageBoxX(
                    this,
                    "是否更新模组\n当前版本: {0}\n最新版本: {1}".Translate(
                        ModInfo.ModVersion,
                        ModUpdataHelper.LastGameVersion
                    ),
                    "更新模组".Translate(),
                    MessageBoxButton.YesNo
                )
                is true
            )
            {
                try
                {
                    var version = ModUpdataHelper.Updata(ModInfo);
                    DialogService.ShowMessageBoxX(this, "更新成功更新至版本 {0}, 请手动保存".Translate(version));
                    this.Log().Info("更新成功更新至版本 {version}", version);
                }
                catch (Exception ex)
                {
                    DialogService.ShowMessageBoxX(this, "模组更新失败, 详情请查看日志".Translate());
                    this.Log().Error(ex, "模组更新失败");
                }
            }
        }
    }
    #endregion

    /// <summary>
    /// 保存为翻译模组
    /// </summary>
    [ReactiveCommand]
    private async void SaveAsTranslationMod()
    {
        await DialogService.ShowDialogAsync<SaveTranslationModVM>(
            this,
            new SaveTranslationModVM(ModInfo)
        );
    }

    /// <summary>
    /// 可以进行I18n编辑
    /// </summary>
    [ReactiveProperty]
    public bool I18nEditCanExecute { get; set; } = true;

    /// <summary>
    /// 编辑I18n数据
    /// </summary>
    [ReactiveCommand(nameof(I18nEditCanExecute))]
    private void EditI18n()
    {
        var vm = new I18nEditVM(ModInfo);
        DialogService.Show<I18nEditVM>(null, vm);
        vm.Closing += I18nEdit_Closing;
        I18nEditCanExecute = false;
    }

    private void I18nEdit_Closing(object? sender, CancelEventArgs e)
    {
        if (sender is not I18nEditVM vm)
            return;
        vm.Closing -= I18nEdit_Closing;
        I18nEditCanExecute = true;
    }

    /// <summary>
    /// 关闭
    /// </summary>
    public void Close()
    {
        ModInfo.Close();
        this.Log().Debug("剩余缓存图像数量: {count}", HKWImageUtils.ImageByPath.Count);
    }

    /// <summary>
    /// 改变图片
    /// </summary>
    [ReactiveCommand]
    private void ChangeImage()
    {
        var openFileDialog = DialogService.ShowOpenFileDialog(
            this,
            new()
            {
                Title = "选择图片".Translate(),
                Filters = [new("图片".Translate(), ["jpg", "jpeg", "png", "bmp"])]
            }
        );
        if (openFileDialog is null)
            return;
        var newImage = HKWImageUtils.LoadImageToMemory(openFileDialog.LocalPath);
        if (newImage is null)
        {
            DialogService.ShowMessageBoxX(
                this,
                "图片载入失败, 详情请查看日志".Translate(),
                "图片载入失败".Translate(),
                icon: MessageBoxImage.Warning
            );
            return;
        }
        ModInfo.Image?.CloseStreamWhenNoReference();
        ModInfo.Image = newImage;
    }

    #region Culture
    /// <summary>
    /// 添加文化
    /// </summary>
    [ReactiveCommand]
    public void AddCulture()
    {
        var vm = DialogService.ShowDialogX(this, new AddCultureVM(ModInfo));
        if (vm.DialogResult is not true)
            return;
        ModInfo.I18nResource.AddCulture(vm.CultureName);
        ModInfo.I18nResource.SetCurrentCulture(vm.CultureName);
        this.Log().Info("添加文化 {culture}", vm.CultureName);
    }

    /// <summary>
    /// 编辑文化
    /// </summary>
    /// <param name="oldCulture">旧文化</param>
    [ReactiveCommand]
    private void EditCulture(CultureInfo oldCulture)
    {
        var vm = DialogService.ShowDialogX(
            this,
            new AddCultureVM(ModInfo) { CultureName = oldCulture.Name }
        );
        if (vm.DialogResult is not true)
            return;
        ModInfo.I18nResource.ReplaceCulture(oldCulture, new(vm.CultureName));
        this.Log().Info("重命名文化 {oldCulture} => {newCulture}", oldCulture, vm.CultureName);
    }

    /// <summary>
    /// 删除文化
    /// </summary>
    /// <param name="oldCulture">旧文化</param>
    [ReactiveCommand]
    private void RemoveCulture(CultureInfo oldCulture)
    {
        if (
            DialogService.ShowMessageBoxX(
                this,
                "确定删除文化 \"{0}\" 吗".Translate(oldCulture),
                "删除文化".Translate(),
                MessageBoxButton.YesNo
            )
            is not true
        )
            return;
        ModInfo.I18nResource.RemoveCulture(oldCulture);
        this.Log().Info("删除文化 {culture}", oldCulture);
    }
    #endregion

    #region Save

    /// <summary>
    /// 打开模组所在的文件夹
    /// </summary>
    [ReactiveCommand]
    private void OpenModPath()
    {
        if (Path.Exists(ModInfo.SourcePath) is false)
        {
            DialogService.ShowMessageBoxX(
                this,
                "源路径为空, 请先保存模组".Translate(),
                "打开所在文件夹失败".Translate(),
                icon: MessageBoxImage.Warning
            );
            return;
        }
        NativeUtils.OpenLink(ModInfo.SourcePath);
    }

    /// <summary>
    /// 保存
    /// </summary>
    [ReactiveCommand]
    private void Save()
    {
        if (ValidationData(ModInfo) is false)
            return;
        if (string.IsNullOrEmpty(ModInfo.SourcePath))
        {
            DialogService.ShowMessageBoxX(
                this,
                "源路径为空, 请使用 \"保存至\"".Translate(),
                "保存失败".Translate(),
                icon: MessageBoxImage.Warning
            );
            return;
        }
        if (
            DialogService.ShowMessageBoxX(
                this,
                "确定保存吗".Translate(),
                "保存".Translate(),
                MessageBoxButton.YesNo
            )
            is not true
        )
            return;
        SaveTo(ModInfo.SourcePath);
    }

    /// <summary>
    /// 保存至
    /// </summary>
    [ReactiveCommand]
    private void SaveTo()
    {
        if (ValidationData(ModInfo) is false)
            return;
        var saveFileDialog = DialogService.ShowOpenFolderDialog(
            this,
            new() { Title = "选择保存的文件夹".Translate(), }
        );
        if (saveFileDialog is null)
            return;
        SaveTo(saveFileDialog.LocalPath);
    }

    /// <summary>
    /// 保存至
    /// </summary>
    /// <param name="path">路径</param>
    private void SaveTo(string path)
    {
        var pending = PendingBox.Show("保存中".Translate());
        try
        {
            ModInfo.SaveTo(path);
            if (string.IsNullOrWhiteSpace(ModInfo.SourcePath))
                ModInfo.SourcePath = path;
            pending.Close();
            DialogService.ShowMessageBoxX(this, "保存成功".Translate());
            this.Log().Info("成功保存至 {path}", path);
        }
        catch (Exception ex)
        {
            pending.Close();
            DialogService.ShowMessageBoxX(this, "保存失败, 详情请查看日志".Translate());
            this.Log().Error(ex, "保存失败");
            return;
        }
    }

    /// <summary>
    /// 验证数据
    /// </summary>
    /// <param name="model">模型</param>
    /// <returns>成功为 <see langword="true"/> 失败为 <see langword="false"/></returns>
    private bool ValidationData(ModInfoModel model)
    {
        if (ModInfo.I18nResource.CultureDatas.HasValue() is false)
        {
            DialogService.ShowMessageBoxX(
                this,
                "未添加任何语言".Translate(),
                "数据错误".Translate(),
                icon: MessageBoxImage.Warning
            );
            return false;
        }
        if (string.IsNullOrWhiteSpace(model.ID))
        {
            DialogService.ShowMessageBoxX(
                this,
                "ID不可为空".Translate(),
                "数据错误".Translate(),
                icon: MessageBoxImage.Warning
            );
            return false;
        }
        if (string.IsNullOrWhiteSpace(model.Author))
        {
            DialogService.ShowMessageBoxX(
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
}
