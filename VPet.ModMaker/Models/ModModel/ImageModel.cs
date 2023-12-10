using HKW.HKWUtils;
using HKW.HKWUtils.Observable;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace VPet.ModMaker.Models.ModModel;

/// <summary>
/// 图像模型
/// </summary>
public class ImageModel
{
    /// <summary>
    /// 图像
    /// </summary>
    public ObservableValue<BitmapImage> Image { get; } = new();

    /// <summary>
    /// 持续时间
    /// </summary>
    public ObservableValue<int> Duration { get; } = new(100);

    public ImageModel(BitmapImage image, int duration = 100)
    {
        Image.Value = image;
        Duration.Value = duration;
    }

    public ImageModel Copy()
    {
        var model = new ImageModel(Image.Value.Copy(), Duration.Value);
        return model;
    }

    public void Close()
    {
        Image.Value?.CloseStream();
    }
}
