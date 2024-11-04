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
using HKW.WPF.Extensions;
using HKW.WPF.MVVMDialogs;
using LinePutScript.Localization.WPF;
using Panuon.WPF.UI;
using ReactiveUI;
using Splat;
using VPet.ModMaker.Models;
using VPet.ModMaker.Views.ModEdit;

namespace VPet.ModMaker.ViewModels.ModEdit;

public partial class ModEditVM : ViewModelBase
{
    private static IDialogService DialogService => Locator.Current.GetService<IDialogService>()!;

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
            if (ModInfo.I18nResource.Cultures.HasValue() is false)
            {
                if (
                    DialogService.ShowMessageBoxX(
                        "未添加任何文化,确定要添加文化吗?".Translate(),
                        "缺少文化".Translate(),
                        MessageBoxButton.YesNo
                    )
                    is not true
                )
                    return;
                AddCulture();
                if (
                    ModInfo.I18nResource.Cultures.HasValue() is false
                    || DialogService.ShowMessageBoxX(
                        "需要将文化 {0} 设为主要文化吗?".Translate(ModInfo.I18nResource.Cultures.First().Name),
                        "设置主要文化".Translate(),
                        MessageBoxButton.YesNo
                    )
                        is not true
                )
                    return;
                SetMainCulture(ModInfo.I18nResource.Cultures.First().Name);
            }
            // 更新模组
            if (ModUpdataHelper.CanUpdata(ModInfo) is false)
                return;

            if (
                DialogService.ShowMessageBoxX(
                    this,
                    "是否更新模组\n当前版本: {0}\n最新版本: {1}".Translate(
                        ModInfo.ModVersion,
                        ModUpdataHelper.LastVersion
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
                    this.Log().Error("更新成功更新至版本 {version}", version);
                }
                catch (Exception ex)
                {
                    DialogService.ShowMessageBoxX(this, "模组更新失败, 详情请查看日志".Translate());
                    this.Log().Error("模组更新失败", ex);
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
    /// 编辑I18n数据
    /// </summary>
    [ReactiveCommand]
    private async void EditI18n()
    {
        await DialogService.ShowDialogAsync<I18nEditVM>(this, new I18nEditVM(ModInfo));
    }

    /// <summary>
    /// 关闭
    /// </summary>
    public void Close()
    {
        ModInfo.Image?.CloseStream();
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

        ModInfo.Image?.CloseStream();
        ModInfo.Image = NativeUtils.LoadImageToMemoryStream(openFileDialog.LocalPath);
    }

    #region Culture
    /// <summary>
    /// 添加文化
    /// </summary>
    [ReactiveCommand]
    public async void AddCulture()
    {
        var vm = await DialogService.ShowDialogAsyncX(this, new AddCultureVM(ModInfo));
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
    private async void EditCulture(string oldCulture)
    {
        var vm = await DialogService.ShowDialogAsyncX(
            this,
            new AddCultureVM(ModInfo) { CultureName = oldCulture }
        );
        if (vm.DialogResult is not true)
            return;
        ModInfo.I18nResource.ReplaceCulture(oldCulture, vm.CultureName);
        this.Log().Info("重命名文化 {oldCulture} => {newCulture}", oldCulture, vm.CultureName);
    }

    /// <summary>
    /// 删除文化
    /// </summary>
    /// <param name="oldCulture">旧文化</param>
    [ReactiveCommand]
    private void RemoveCulture(string oldCulture)
    {
        if (
            DialogService.ShowMessageBoxX(
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

    /// <summary>
    /// 设置主文化
    /// </summary>
    /// <param name="culture">文化名称</param>
    [ReactiveCommand]
    public void SetMainCulture(string culture)
    {
        if (
            DialogService.ShowMessageBoxX(
                this,
                "!!!注意!!!\n此操作会将所有ID设为当前文化的翻译内容,仅适用于初次设置多文化的模组\n确定要继续吗?".Translate(),
                "设置主文化".Translate(),
                MessageBoxButton.YesNo
            )
            is not true
        )
            return;
        foreach (var datas in ModInfo.I18nResource.CultureDatas)
        {
            ModInfo.I18nResource.SetCurrentCultureData(datas.Key, datas.Key);
        }
        ModInfo.RefreshAllID();
        this.Log().Info("设置主要文化为 {culture}", culture);
    }
    #endregion

    #region Save
    /// <summary>
    /// 保存
    /// </summary>
    [ReactiveCommand]
    private void Save()
    {
        if (ValidationData(ModInfo) is false)
            return;
        if (
            DialogService.ShowMessageBoxX(
                "确定保存吗".Translate(),
                "保存".Translate(),
                MessageBoxButton.YesNo
            )
            is not true
        )
            return;
        if (string.IsNullOrEmpty(ModInfo.SourcePath))
        {
            DialogService.ShowMessageBoxX("源路径为空, 请使用 保存至".Translate());
            return;
        }
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
            DialogService.ShowMessageBoxX("保存失败, 详情请查看日志".Translate());
            this.Log().Error("保存失败", ex);
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
