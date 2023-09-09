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
    public ObservableCommand AddLangCommand { get; } = new();

    public ObservableCommand<string> EditLangCommand { get; } = new();
    public ObservableCommand<string> RemoveLangCommand { get; } = new();

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
        AddLangCommand.ExecuteEvent += AddLang;
        EditLangCommand.ExecuteEvent += EditLang;
        RemoveLangCommand.ExecuteEvent += RemoveLang;
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

    private void AddLang()
    {
        var window = new Window_AddLang();
        window.ShowDialog();
        if (window.IsCancel)
            return;
        I18nHelper.Current.CultureNames.Add(window.Lang.Value);
    }

    private void EditLang(string oldLang)
    {
        var window = new Window_AddLang();
        window.Lang.Value = oldLang.Translate();
        window.ShowDialog();
        if (window.IsCancel)
            return;
        I18nHelper.Current.CultureNames[I18nHelper.Current.CultureNames.IndexOf(oldLang)] = window
            .Lang
            .Value;
        CurrentLang.Value = window.Lang.Value;
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
                ModInfo.Value.SourcePath.Value = path.Translate();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存失败 错误信息:\n{0}".Translate(ex));
                return;
            }
            MessageBox.Show("保存成功".Translate());
        }
    }
}
