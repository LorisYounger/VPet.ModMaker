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
using System.Windows;
using System.Windows.Media.Imaging;
using HKW.HKWReactiveUI;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using LinePutScript.Localization.WPF;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using Panuon.WPF.UI;
using VPet.ModMaker.Models;
using VPet.ModMaker.Views.ModEdit;
using VPet.ModMaker.Views.ModEdit.I18nEdit;

namespace VPet.ModMaker.ViewModels.ModEdit;

public partial class ModEditWindowVM : ViewModelBase
{
    public ModEditWindowVM(ModEditWindow window)
    {
        ModEditWindow = window;
    }

    public ModEditWindow ModEditWindow { get; }

    #region Property
    /// <summary>
    /// 当前模组信息
    /// </summary>
    [ReactiveProperty]
    public ModInfoModel ModInfo { get; set; } = ModInfoModel.Current;

    /// <summary>
    /// I18n资源
    /// </summary>
    public I18nResource<string, string> I18nResource => ModInfo.I18nResource;
    #endregion

    /// <summary>
    /// 保存为翻译模组
    /// </summary>
    [ReactiveCommand]
    private void SaveAsTranslationMod()
    {
        if (ValidationData(ModInfo) is false)
            return;
        var window = new SaveTranslationModWindow();
        window.ShowDialog();
    }

    /// <summary>
    /// 编辑I18n数据
    /// </summary>
    [ReactiveCommand]
    private void EditI18n()
    {
        ModEditWindow.I18nEditWindow.ShowOrActivate();
    }

    /// <summary>
    /// 关闭
    /// </summary>
    public void Close()
    {
        ModInfo.Image?.StreamSource?.Close();
    }

    /// <summary>
    /// 改变图片
    /// </summary>
    [ReactiveCommand]
    private void ChangeImage()
    {
        OpenFileDialog openFileDialog =
            new()
            {
                Title = "选择图片".Translate(),
                Filter = $"图片|*.jpg;*.jpeg;*.png;*.bmp".Translate()
            };
        if (openFileDialog.ShowDialog() is true)
        {
            ModInfo.Image?.StreamSource?.Close();
            ModInfo.Image = NativeUtils.LoadImageToMemoryStream(openFileDialog.FileName);
        }
    }

    #region Culture
    /// <summary>
    /// 添加文化
    /// </summary>
    [ReactiveCommand]
    public void AddCulture()
    {
        var window = new AddCultureWindow();
        window.ShowDialog();
        if (window.IsCancel)
            return;
        I18nResource.AddCulture(window.ViewModel.CultureName);
        I18nResource.SetCurrentCulture(window.ViewModel.CultureName);
    }

    /// <summary>
    /// 编辑文化
    /// </summary>
    /// <param name="oldCulture">旧文化</param>
    [ReactiveCommand]
    private void EditCulture(string oldCulture)
    {
        var window = new AddCultureWindow();
        window.ViewModel.CultureName = oldCulture.Translate();
        window.ShowDialog();
        if (window.IsCancel)
            return;
        I18nResource.ReplaceCulture(oldCulture, window.ViewModel.CultureName);
    }

    /// <summary>
    /// 删除文化
    /// </summary>
    /// <param name="oldCulture">旧文化</param>
    [ReactiveCommand]
    private void RemoveCulture(string oldCulture)
    {
        if (
            MessageBox.Show(
                "确定删除文化 {0} 吗".Translate(oldCulture),
                "".Translate(),
                MessageBoxButton.YesNo
            ) is MessageBoxResult.No
        )
            return;
        I18nResource.RemoveCulture(oldCulture);
    }

    /// <summary>
    /// 设置主文化
    /// </summary>
    /// <param name="culture"></param>
    [ReactiveCommand]
    public void SetMainCulture(string culture)
    {
        if (
            MessageBox.Show(
                ModEditWindow,
                "!!!注意!!!\n此操作会将所有ID设为当前文化的翻译内容,仅适用于初次设置多文化的模组\n确定要继续吗?".Translate(),
                "",
                MessageBoxButton.YesNo
            )
            is not MessageBoxResult.Yes
        )
            return;
        foreach (var datas in I18nResource.CultureDatas)
        {
            I18nResource.SetCurrentCultureData(datas.Key, datas.Key);
        }
        ModInfo.RefreshAllID();
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
            MessageBox.Show("确定保存吗".Translate(), "", MessageBoxButton.YesNo)
            is not MessageBoxResult.Yes
        )
            return;
        if (string.IsNullOrEmpty(ModInfo.SourcePath))
        {
            MessageBox.Show("源路径为空, 请使用 保存至".Translate());
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
        var dialog = new VistaFolderBrowserDialog();
        if (dialog.ShowDialog() is not true)
            return;
        SaveTo(dialog.SelectedPath);
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
            MessageBox.Show(ModEditWindow, "保存成功".Translate());
        }
        catch (Exception ex)
        {
            pending.Close();
            MessageBox.Show("保存失败 错误信息:\n{0}".Translate(ex));
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
        if (I18nResource.CultureDatas.HasValue() is false)
        {
            MessageBox.Show(
                ModEditWindow,
                "未添加任何语言".Translate(),
                "",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
            return false;
        }
        if (string.IsNullOrWhiteSpace(model.ID))
        {
            MessageBox.Show(
                ModEditWindow,
                "ID不可为空".Translate(),
                "",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
            return false;
        }
        if (string.IsNullOrWhiteSpace(model.Author))
        {
            MessageBox.Show(
                ModEditWindow,
                "作者不可为空".Translate(),
                "",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
            return false;
        }
        return true;
    }
    #endregion
}
