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
    public ObservableValue<string> Id { get; } = new();
    public ObservableValue<BitmapImage> Image { get; } = new();

    public ObservableValue<int> Duration { get; } = new(100);
}
