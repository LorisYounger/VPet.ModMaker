using System.ComponentModel;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using HKW.HKWUtils;
using HKW.MVVM;
using HKW.WPF;
using HKW.WPF.Extensions;
using VPet.ModMaker.ViewModels;

namespace VPet.ModMaker.Models;

/// <summary>
/// 图像模型
/// </summary>
public sealed partial class ImageModel : ObservableObjectEx, ICloneable<ImageModel>, IDisposable
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
    public ImageModel(string imageFile, BitmapImage image, int duration = 100)
    {
        ImageFile = imageFile;
        Image = image;
        Duration = duration;
    }

    /// <summary>
    /// 图片路径
    /// </summary>
    public string ImageFile { get; set; }

    /// <summary>
    /// 图像
    /// </summary>
    [ObservableProperty]
    public BitmapImage Image { get; set; } = null!;

    /// <summary>
    /// 持续时间 (ms)
    /// </summary>
    [DefaultValue(100)]
    [ObservableProperty]
    public int Duration { get; set; }

    /// <summary>
    /// 载入图像
    /// </summary>
    public BitmapImage LoadImage()
    {
        return Image ??= HKWImageUtils.LoadImageToMemory(ImageFile)!;
    }

    /// <summary>
    /// 克隆图像
    /// </summary>
    /// <returns></returns>
    public ImageModel Clone()
    {
        return new(ImageFile, Duration);
    }

    object ICloneable.Clone() => Clone();

    /// <inheritdoc/>
    public void Dispose()
    {
        Image?.CloseStream();
        Image = null!;
    }
}
