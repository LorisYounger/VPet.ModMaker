using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace VPet.ModMaker.Models.ModModel;

public class AnimeModel
{
    public ObservableValue<ImageModel> CurrentImageModel { get; } = new();
    public ObservableCollection<ImageModel> ImageModels { get; } = new();
}
