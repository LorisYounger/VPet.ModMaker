using HKW.HKWViewModels.SimpleObservable;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using VPet_Simulator.Core;

namespace VPet.ModMaker.Models.ModModel;

public class AnimeModel
{
    public ObservableValue<string> Id { get; } = new();

    public ObservableValue<GraphInfo.AnimatType> AnimeType { get; } = new();

    public ObservableCollection<ImageModel> Images { get; } = new();

    public AnimeModel() { }

    public AnimeModel(string imagesPath)
    {
        foreach (var file in Directory.EnumerateFiles(imagesPath))
        {
            var info = Path.GetFileNameWithoutExtension(file).Split(Utils.Separator);
            Id.Value = info[0];
            var duration = info.Last();
            var imageModel = new ImageModel(
                Utils.LoadImageToMemoryStream(file),
                int.Parse(duration)
            );
            Images.Add(imageModel);
        }
    }

    public AnimeModel Copy()
    {
        var model = new AnimeModel();
        model.Id.Value = Id.Value;
        model.AnimeType.Value = AnimeType.Value;
        foreach (var image in Images)
            model.Images.Add(image.Copy());
        return model;
    }

    public void Close()
    {
        foreach (var image in Images)
            image.Close();
    }
}
