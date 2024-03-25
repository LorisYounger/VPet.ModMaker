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
using HKW.HKWUtils.Observable;
using LinePutScript.Localization.WPF;
using Microsoft.Win32;
using Ookii.Dialogs.Wpf;
using Panuon.WPF.UI;
using VPet.ModMaker.Models;
using VPet.ModMaker.Views.ModEdit;
using VPet.ModMaker.Views.ModEdit.I18nEdit;

namespace VPet.ModMaker.ViewModels.ModEdit;

public class ModEditWindowVM : ObservableObjectX<ModEditWindowVM>
{
    public ModEditWindow ModEditWindow { get; }

    #region Value
    /// <summary>
    /// 当前模组信息
    /// </summary>
    #region ModInfo
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ModInfoModel _modInfo = ModInfoModel.Current;

    public ModInfoModel ModInfo
    {
        get => _modInfo;
        set => SetProperty(ref _modInfo, value);
    }
    #endregion

    /// <summary>
    /// I18n数据
    /// </summary>
    public I18nHelper I18nData => I18nHelper.Current;
    #endregion

    #region Command

    /// <summary>
    /// 改变图片命令
    /// </summary>
    public ObservableCommand ChangeImageCommand { get; } = new();

    /// <summary>
    /// 添加文化命令
    /// </summary>
    public ObservableCommand AddCultureCommand { get; } = new();

    /// <summary>
    /// 编辑文化命令
    /// </summary>
    public ObservableCommand<string> EditCultureCommand { get; } = new();

    /// <summary>
    /// 删除文化命令
    /// </summary>
    public ObservableCommand<string> RemoveCultureCommand { get; } = new();

    /// <summary>
    /// 设置主要文化命令
    /// </summary>
    public ObservableCommand<string> SetMainCultureCommand { get; } = new();

    /// <summary>
    /// 保存命令
    /// </summary>
    public ObservableCommand SaveCommand { get; } = new();

    /// <summary>
    /// 保存至命令
    /// </summary>
    public ObservableCommand SaveToCommand { get; } = new();

    /// <summary>
    /// 编辑多语言内容
    /// </summary>
    public ObservableCommand EditI18nCommand { get; } = new();

    /// <summary>
    /// 保存为翻译模组
    /// </summary>
    public ObservableCommand SaveAsTranslationModCommand { get; } = new();
    #endregion

    public ModEditWindowVM(ModEditWindow window)
    {
        new I18nEditWindow();
        ModEditWindow = window;
        ChangeImageCommand.ExecuteCommand += ChangeImage;
        AddCultureCommand.ExecuteCommand += AddCulture;
        EditCultureCommand.ExecuteCommand += EditCulture;
        RemoveCultureCommand.ExecuteCommand += RemoveCulture;
        EditI18nCommand.ExecuteCommand += EditI18n;
        SetMainCultureCommand.ExecuteCommand += SetMainCulture;

        SaveCommand.ExecuteCommand += Save;
        SaveToCommand.ExecuteCommand += SaveTo;
        SaveAsTranslationModCommand.ExecuteCommand += SaveAsTranslationMod;
    }

    private void SaveAsTranslationMod()
    {
        if (ValidationData(ModInfo) is false)
            return;
        var window = new SaveTranslationModWindow();
        window.ShowDialog();
    }

    private void EditI18n()
    {
        I18nEditWindow.Current.Visibility = Visibility.Visible;
        I18nEditWindow.Current.Activate();
    }

    /// <summary>
    /// 关闭
    /// </summary>
    public void Close()
    {
        ModInfo.Image?.StreamSource?.Close();
        I18nEditWindow.Current?.Close(true);
    }

    /// <summary>
    /// 改变图片
    /// </summary>
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
    public void AddCulture()
    {
        var window = new AddCultureWindow();
        window.ShowDialog();
        if (window.IsCancel)
            return;
        I18nHelper.Current.CultureNames.Add(window.ViewModel.Culture);
        if (I18nHelper.Current.CultureNames.Count == 1)
            I18nHelper.Current.CultureName = window.ViewModel.Culture;
    }

    /// <summary>
    /// 编辑文化
    /// </summary>
    /// <param name="oldCulture">旧文化</param>
    private void EditCulture(string oldCulture)
    {
        var window = new AddCultureWindow();
        window.ViewModel.Culture = oldCulture.Translate();
        window.ShowDialog();
        if (window.IsCancel)
            return;
        I18nHelper.Current.CultureNames[I18nHelper.Current.CultureNames.IndexOf(oldCulture)] =
            window.ViewModel.Culture;
    }

    /// <summary>
    /// 删除文化
    /// </summary>
    /// <param name="oldCulture">旧文化</param>
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
        I18nHelper.Current.CultureNames.Remove(oldCulture);
    }

    public void SetMainCulture(string culture)
    {
        if (
            MessageBox.Show(
                "!!!注意!!!\n此操作会将所有Id设为当前文化的翻译内容,仅适用于初次设置多文化的模组\n确定要继续吗?".Translate(),
                "",
                MessageBoxButton.YesNo
            )
            is not MessageBoxResult.Yes
        )
            return;
        ModInfo.I18nDatas[culture].Name = ModInfo.Id;
        ModInfo.I18nDatas[culture].Description = ModInfo.DescriptionId;
        foreach (var food in ModInfo.Foods)
        {
            food.I18nDatas[culture].Name = food.Id;
            food.I18nDatas[culture].Description = food.DescriptionId;
        }
        foreach (var text in ModInfo.LowTexts)
            text.I18nDatas[culture].Text = text.Id;
        foreach (var text in ModInfo.ClickTexts)
            text.I18nDatas[culture].Text = text.Id;
        foreach (var text in ModInfo.SelectTexts)
        {
            text.I18nDatas[culture].Text = text.Id;
            text.I18nDatas[culture].Choose = text.ChooseId;
        }
        foreach (var pet in ModInfo.Pets)
        {
            pet.I18nDatas[culture].Name = pet.ID;
            pet.I18nDatas[culture].PetName = pet.PetNameId;
            pet.I18nDatas[culture].Description = pet.DescriptionId;
            foreach (var work in pet.Works)
                work.I18nDatas[culture].Name = work.Id;
        }
    }
    #endregion

    #region Save
    /// <summary>
    /// 保存
    /// </summary>
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
    /// <param name="path"></param>
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
        if (I18nHelper.Current.CultureNames.Count == 0)
        {
            MessageBox.Show(
                "未添加任何语言".Translate(),
                "",
                MessageBoxButton.OK,
                MessageBoxImage.Warning
            );
            return false;
        }
        if (string.IsNullOrWhiteSpace(model.Id))
        {
            MessageBox.Show("Id不可为空".Translate(), "", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
        if (string.IsNullOrWhiteSpace(model.Author))
        {
            MessageBox.Show("作者不可为空".Translate(), "", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
        return true;
    }
    #endregion
}
