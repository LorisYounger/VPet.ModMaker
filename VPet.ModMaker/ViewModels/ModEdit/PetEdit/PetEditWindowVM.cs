﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using HKW.HKWReactiveUI;
using HKW.HKWUtils.Observable;
using LinePutScript.Localization.WPF;
using Microsoft.Win32;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels.ModEdit;

public partial class PetEditWindowVM : ViewModelBase
{
    public PetEditWindowVM() { }

    #region Property
    /// <summary>
    /// I18n资源
    /// </summary>
    public static I18nResource<string, string> I18nResource => ModInfoModel.Current.I18nResource;
    public PetModel? OldPet { get; set; }

    [ReactiveProperty]
    public PetModel Pet { get; set; } = new() { I18nResource = ModInfoModel.Current.I18nResource };

    [ReactiveProperty]
    public double BorderLength { get; set; } = 250;

    [ReactiveProperty]
    public double LengthRatio { get; set; } = 0.5;

    [ReactiveProperty]
    public BitmapImage? Image { get; set; }
    #endregion

    public void Close()
    {
        Image?.CloseStream();
    }

    /// <summary>
    /// 添加图片
    /// </summary>
    [ReactiveCommand]
    private void AddImage()
    {
        var openFileDialog = new OpenFileDialog()
        {
            Title = "选择图片".Translate(),
            Filter = $"图片|*.jpg;*.jpeg;*.png;*.bmp".Translate()
        };
        if (openFileDialog.ShowDialog() is true)
        {
            Image = NativeUtils.LoadImageToMemoryStream(openFileDialog.FileName);
        }
    }

    /// <summary>
    /// 改变图片
    /// </summary>
    [ReactiveCommand]
    private void ChangeImage()
    {
        var openFileDialog = new OpenFileDialog()
        {
            Title = "选择图片".Translate(),
            Filter = $"图片|*.jpg;*.jpeg;*.png;*.bmp".Translate()
        };
        if (openFileDialog.ShowDialog() is true)
        {
            Image?.CloseStream();
            Image = NativeUtils.LoadImageToMemoryStream(openFileDialog.FileName);
        }
    }
}
