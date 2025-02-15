﻿using System.Windows.Media.Imaging;
using HKW.HKWReactiveUI;
using HKW.HKWUtils;
using HKW.WPF;
using HKW.WPF.Extensions;
using VPet.ModMaker.ViewModels;

namespace VPet.ModMaker.Models.ModModel;

/// <summary>
/// 图像模型
/// </summary>
public partial class ImageModel : ViewModelBase, ICloneable<ImageModel>
{
    /// <inheritdoc/>
    /// <param name="imageFile">图像文件</param>
    /// <param name="duration">间隔</param>/>
    public ImageModel(string imageFile, int duration = 100)
    {
        ImageFile = string.Intern(imageFile);
        Duration = duration;
    }

    /// <inheritdoc/>
    /// <param name="image">图像</param>
    /// <param name="duration">间隔</param>/>
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

    /// <summary>
    /// 载入图像
    /// </summary>
    public void LoadImage()
    {
        Image = HKWImageUtils.LoadImageToMemory(ImageFile)!;
    }

    /// <summary>
    /// 克隆图像
    /// </summary>
    /// <returns></returns>
    public ImageModel Clone()
    {
        Image ??= HKWImageUtils.LoadImageToMemory(ImageFile)!;
        var model = new ImageModel(Image, Duration);
        Image.AddReferenceCount();
        return model;
    }

    object ICloneable.Clone() => Clone();

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (_disposed)
            return;
        base.Dispose(disposing);
        if (disposing)
        {
            Image?.CloseStreamWhenNoReference();
        }
    }
}
