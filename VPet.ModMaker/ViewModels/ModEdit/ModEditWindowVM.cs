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
        foreach (var lang in ModInfo.Value.I18nDatas)
        {
            if (I18nHelper.Current.CultureNames.Contains(lang.Key) is false)
                I18nHelper.Current.CultureNames.Add(lang.Key);
        }
        if (I18nHelper.Current.CultureNames.Count > 0)
        {
            I18nHelper.Current.CultureName.Value = I18nHelper.Current.CultureNames.First();
            foreach (var i18n in ModInfo.Value.OtherI18nDatas)
            {
                foreach (var food in ModInfo.Value.Foods)
                {
                    var foodI18n = food.I18nDatas[i18n.Key];
                    if (i18n.Value.TryGetValue(food.Name, out var i18nName))
                        foodI18n.Name.Value = i18nName;
                    if (i18n.Value.TryGetValue(food.Description, out var i18nDescription))
                        foodI18n.Description.Value = i18nDescription;
                }
                foreach (var lowText in ModInfo.Value.LowTexts)
                {
                    var lowTextI18n = lowText.I18nDatas[i18n.Key];
                    if (i18n.Value.TryGetValue(lowText.Text, out var text))
                        lowTextI18n.Text.Value = text;
                }
                foreach (var clickText in ModInfo.Value.ClickTexts)
                {
                    var clickTextI18n = clickText.I18nDatas[i18n.Key];
                    if (i18n.Value.TryGetValue(clickText.Text, out var text))
                        clickTextI18n.Text.Value = text;
                }
            }
        }

        ModEditWindow = window;

        I18nHelper.Current.AddLang += I18nData_AddLang;
        I18nHelper.Current.RemoveLang += I18nData_RemoveLang;
        I18nHelper.Current.ReplaceLang += I18nData_ReplaceLang;
        CurrentLang.ValueChanged += CurrentLang_ValueChanged;

        AddImageCommand.ExecuteAction += AddImage;
        ChangeImageCommand.ExecuteAction += ChangeImage;
        AddLangCommand.ExecuteAction += AddLang;
        EditLangCommand.ExecuteAction += EditLang;
        RemoveLangCommand.ExecuteAction += RemoveLang;
        SaveCommand.ExecuteAction += Save;
        SaveToCommand.ExecuteAction += SaveTo;
    }

    private void I18nData_AddLang(string lang)
    {
        ModInfo.Value.I18nDatas.Add(lang, new());
    }

    private void I18nData_RemoveLang(string lang)
    {
        ModInfo.Value.I18nDatas.Remove(lang);
    }

    private void I18nData_ReplaceLang(string oldLang, string newLang)
    {
        var info = ModInfo.Value.I18nDatas[oldLang];
        ModInfo.Value.I18nDatas.Remove(oldLang);
        ModInfo.Value.I18nDatas.Add(newLang, info);
    }

    private void CurrentLang_ValueChanged(string value)
    {
        if (value is null)
            return;
        ModInfo.Value.CurrentI18nData.Value = ModInfo.Value.I18nDatas[value];
    }

    public void Close()
    {
        ModInfo.Value.ModImage.Value?.StreamSource?.Close();
    }

    private void AddImage()
    {
        OpenFileDialog openFileDialog =
            new() { Title = "选择图片", Filter = $"图片|*.jpg;*.jpeg;*.png;*.bmp" };
        if (openFileDialog.ShowDialog() is true)
        {
            ModInfo.Value.ModImage.Value = Utils.LoadImageToStream(openFileDialog.FileName);
        }
    }

    private void ChangeImage()
    {
        OpenFileDialog openFileDialog =
            new() { Title = "选择图片", Filter = $"图片|*.jpg;*.jpeg;*.png;*.bmp" };
        if (openFileDialog.ShowDialog() is true)
        {
            ModInfo.Value.ModImage.Value?.StreamSource?.Close();
            ModInfo.Value.ModImage.Value = Utils.LoadImageToStream(openFileDialog.FileName);
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
        window.Lang.Value = oldLang;
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
        if (MessageBox.Show("确定删除吗", "", MessageBoxButton.YesNo) is MessageBoxResult.No)
            return;
        I18nHelper.Current.CultureNames.Remove(oldLang);
    }

    private void Save()
    {
        return;
    }

    private void SaveTo()
    {
        return;
    }
}
