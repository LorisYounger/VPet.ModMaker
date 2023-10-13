using HKW.HKWViewModels.SimpleObservable;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using VPet.ModMaker.Models;
using System.Collections.Specialized;
using System.ComponentModel;
using VPet.ModMaker.Views.ModEdit;
using System.Windows;
using System.IO;
using LinePutScript.Localization.WPF;
using Panuon.WPF.UI;

namespace VPet.ModMaker.ViewModels.ModEdit;

public class ModEditWindowVM
{
    public ModEditWindow ModEditWindow { get; }

    #region Value
    /// <summary>
    /// 当前模组信息
    /// </summary>
    public ObservableValue<ModInfoModel> ModInfo { get; } = new(ModInfoModel.Current);

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
    /// 保存命令
    /// </summary>
    public ObservableCommand SaveCommand { get; } = new();

    /// <summary>
    /// 保存至命令
    /// </summary>
    public ObservableCommand SaveToCommand { get; } = new();
    #endregion

    public ModEditWindowVM(ModEditWindow window)
    {
        ModEditWindow = window;

        ChangeImageCommand.ExecuteEvent += ChangeImage;
        AddCultureCommand.ExecuteEvent += AddCulture;
        EditCultureCommand.ExecuteEvent += EditCulture;
        RemoveCultureCommand.ExecuteEvent += RemoveCulture;

        SaveCommand.ExecuteEvent += Save;
        SaveToCommand.ExecuteEvent += SaveTo;
    }

    /// <summary>
    /// 关闭
    /// </summary>
    public void Close()
    {
        ModInfo.Value.Image.Value?.StreamSource?.Close();
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
            ModInfo.Value.Image.Value?.StreamSource?.Close();
            ModInfo.Value.Image.Value = Utils.LoadImageToMemoryStream(openFileDialog.FileName);
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
        I18nHelper.Current.CultureNames.Add(window.ViewModel.Culture.Value);
        if (I18nHelper.Current.CultureNames.Count == 1)
            I18nHelper.Current.CultureName.Value = window.ViewModel.Culture.Value;
    }

    /// <summary>
    /// 编辑文化
    /// </summary>
    /// <param name="oldCulture">旧文化</param>
    private void EditCulture(string oldCulture)
    {
        var window = new AddCultureWindow();
        window.ViewModel.Culture.Value = oldCulture.Translate();
        window.ShowDialog();
        if (window.IsCancel)
            return;
        I18nHelper.Current.CultureNames[I18nHelper.Current.CultureNames.IndexOf(oldCulture)] =
            window.ViewModel.Culture.Value;
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
    #endregion

    #region Save
    /// <summary>
    /// 保存
    /// </summary>
    private void Save()
    {
        if (
            MessageBox.Show("确定保存吗".Translate(), "", MessageBoxButton.YesNo)
            is not MessageBoxResult.Yes
        )
            return;

        if (string.IsNullOrEmpty(ModInfo.Value.SourcePath.Value))
        {
            MessageBox.Show("源路径为空, 请使用 保存至".Translate());
            return;
        }
        try
        {
            ModInfo.Value.SaveTo(ModInfo.Value.SourcePath.Value);
        }
        catch (Exception ex)
        {
            MessageBox.Show("保存失败 错误信息:\n{0}".Translate(ex));
            return;
        }
        MessageBox.Show("保存成功".Translate());
    }

    /// <summary>
    /// 保存至
    /// </summary>
    private void SaveTo()
    {
        if (ValidationData(ModInfo.Value) is false)
            return;
        SaveFileDialog saveFileDialog =
            new()
            {
                Title = "保存 模组信息文件,并在文件夹内保存模组数据".Translate(),
                Filter = $"LPS文件|*.lps;".Translate(),
                FileName = "info.lps".Translate()
            };
        if (saveFileDialog.ShowDialog() is true)
        {
            var pending = PendingBox.Show("保存中".Translate());
            //try
            //{
            var path = Path.GetDirectoryName(saveFileDialog.FileName);
            ModInfo.Value.SaveTo(path);
            if (string.IsNullOrWhiteSpace(ModInfo.Value.SourcePath.Value))
                ModInfo.Value.SourcePath.Value = path;
            pending.Close();
            MessageBox.Show(ModEditWindow, "保存成功".Translate());
            //}
            //catch (Exception ex)
            //{
            //    pending.Close();
            //    MessageBox.Show("保存失败 错误信息:\n{0}".Translate(ex));
            //    return;
            //}
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
        if (string.IsNullOrWhiteSpace(model.Id.Value))
        {
            MessageBox.Show("Id不可为空".Translate(), "", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
        if (string.IsNullOrWhiteSpace(model.Author.Value))
        {
            MessageBox.Show("作者不可为空".Translate(), "", MessageBoxButton.OK, MessageBoxImage.Warning);
            return false;
        }
        return true;
    }
    #endregion
}
