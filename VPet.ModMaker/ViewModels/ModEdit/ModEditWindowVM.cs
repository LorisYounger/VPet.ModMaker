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

namespace VPet.ModMaker.ViewModels.ModEdit;

public class ModEditWindowVM
{
    public ModEditWindow ModEditWindow { get; }

    #region Value
    public ObservableValue<ModInfoModel> ModInfo { get; } = new(ModInfoModel.Current);
    public ObservableValue<string> CurrentLang { get; } = new();
    public I18nHelper I18nData => I18nHelper.Current;
    #endregion

    #region Command
    public ObservableCommand AddImageCommand { get; } = new();
    public ObservableCommand ChangeImageCommand { get; } = new();
    public ObservableCommand AddCultureCommand { get; } = new();

    public ObservableCommand<string> EditCultureCommand { get; } = new();
    public ObservableCommand<string> RemoveCultureCommand { get; } = new();

    public ObservableCommand SaveCommand { get; } = new();
    public ObservableCommand SaveToCommand { get; } = new();
    #endregion

    public ModEditWindowVM() { }

    public ModEditWindowVM(ModEditWindow window)
    {
        ModEditWindow = window;
        CurrentLang.ValueChanged += CurrentLang_ValueChanged;

        AddImageCommand.ExecuteEvent += AddImage;
        ChangeImageCommand.ExecuteEvent += ChangeImage;
        AddCultureCommand.ExecuteEvent += AddCulture;
        EditCultureCommand.ExecuteEvent += EditCulture;
        RemoveCultureCommand.ExecuteEvent += RemoveLang;
        SaveCommand.ExecuteEvent += Save;
        SaveToCommand.ExecuteEvent += SaveTo;
    }

    private void CurrentLang_ValueChanged(string oldValue, string newValue)
    {
        if (newValue is null)
            return;
        ModInfo.Value.CurrentI18nData.Value = ModInfo.Value.I18nDatas[newValue];
    }

    public void Close()
    {
        ModInfo.Value.Image.Value?.StreamSource?.Close();
    }

    private void AddImage()
    {
        OpenFileDialog openFileDialog =
            new()
            {
                Title = "选择图片".Translate(),
                Filter = $"图片|*.jpg;*.jpeg;*.png;*.bmp".Translate()
            };
        if (openFileDialog.ShowDialog() is true)
        {
            ModInfo.Value.Image.Value = Utils.LoadImageToStream(openFileDialog.FileName);
        }
    }

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
            ModInfo.Value.Image.Value = Utils.LoadImageToStream(openFileDialog.FileName);
        }
    }

    private void AddCulture()
    {
        var window = new AddCultureWindow();
        window.ShowDialog();
        if (window.IsCancel)
            return;
        I18nHelper.Current.CultureNames.Add(window.ViewModel.Culture.Value);
        if (I18nHelper.Current.CultureNames.Count == 1)
            I18nHelper.Current.CultureName.Value = window.ViewModel.Culture.Value;
    }

    private void EditCulture(string oldLang)
    {
        var window = new AddCultureWindow();
        window.ViewModel.Culture.Value = oldLang.Translate();
        window.ShowDialog();
        if (window.IsCancel)
            return;
        I18nHelper.Current.CultureNames[I18nHelper.Current.CultureNames.IndexOf(oldLang)] = window
            .ViewModel
            .Culture
            .Value;
        CurrentLang.Value = window.ViewModel.Culture.Value;
    }

    private void RemoveLang(string oldLang)
    {
        if (
            MessageBox.Show("确定删除吗".Translate(), "".Translate(), MessageBoxButton.YesNo)
            is MessageBoxResult.No
        )
            return;
        I18nHelper.Current.CultureNames.Remove(oldLang);
    }

    private void Save()
    {
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
            try
            {
                var path = Path.GetDirectoryName(saveFileDialog.FileName);
                ModInfo.Value.SaveTo(path);
                if (string.IsNullOrWhiteSpace(ModInfo.Value.SourcePath.Value))
                    ModInfo.Value.SourcePath.Value = path;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败 错误信息:\n{0}".Translate(ex));
                return;
            }
            MessageBox.Show("保存成功".Translate());
        }
    }

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
}
