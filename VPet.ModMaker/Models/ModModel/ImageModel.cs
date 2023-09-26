using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace VPet.ModMaker.Models.ModModel;

public class ImageModel
{
    public ObservableValue<BitmapImage> Image { get; } = new();
    public ObservableValue<int> Duration { get; } = new(100);

    public ImageModel(BitmapImage image, int duration = 100)
    {
        Image.Value = image;
        Duration.Value = duration;
    }

    public ImageModel Copy()
    {
        var model = new ImageModel(Image.Value, Duration.Value);
        return model;
    }

    public void Close()
    {
        Image.Value?.CloseStream();
    }
}
