﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using HKW.HKWReactiveUI;
using HKW.HKWUtils;
using HKW.HKWUtils.Observable;
using VPet.ModMaker.ViewModels;

namespace VPet.ModMaker.Models.ModModel;

/// <summary>
/// 图像模型
/// </summary>
public partial class ImageModel : ViewModelBase, ICloneable<ImageModel>
{
    public ImageModel(string imageFile, int duration = 100)
    {
        ImageFile = imageFile;
        Duration = duration;
    }

    public ImageModel(BitmapImage image, int duration = 100)
    {
        Image = image;
        Duration = duration;
    }

    /// <summary>
    /// 图片路径
    /// </summary>
    public string ImageFile { get; set; } = string.Empty;

    /// <summary>
    /// 图像
    /// </summary>
    [ReactiveProperty]
    public BitmapImage Image { get; set; } = null!;

    /// <summary>
    /// 持续时间
    /// </summary>
    [ReactiveProperty]
    public int Duration { get; set; } = 100;

    public void LoadImage()
    {
        Image = NativeUtils.LoadImageToMemoryStream(ImageFile);
    }

    public ImageModel Clone()
    {
        var model = new ImageModel(
            Image?.CloneStream() ?? NativeUtils.LoadImageToMemoryStream(ImageFile),
            Duration
        );
        return model;
    }

    object ICloneable.Clone() => Clone();

    public void Close()
    {
        Image?.CloseStream();
    }
}
