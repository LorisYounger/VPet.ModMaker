﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using HKW.HKWUtils;
using HKW.HKWUtils.Observable;

namespace VPet.ModMaker.Models.ModModel;

/// <summary>
/// 图像模型
/// </summary>
public class ImageModel : ObservableObjectX, ICloneable<ImageModel>
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

    #region Image
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private BitmapImage _image = null!;

    /// <summary>
    /// 图像
    /// </summary>
    public BitmapImage Image
    {
        get => _image;
        set => SetProperty(ref _image, value);
    }
    #endregion

    #region Duration
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private int _duration;

    /// <summary>
    /// 持续时间
    /// </summary>
    public int Duration
    {
        get => _duration;
        set => SetProperty(ref _duration, value);
    }
    #endregion

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
