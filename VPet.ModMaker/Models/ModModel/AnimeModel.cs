using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using VPet_Simulator.Core;
using static VPet_Simulator.Core.GraphInfo;

namespace VPet.ModMaker.Models.ModModel;

public class AnimeTypeModel
{
    public static ObservableCollection<GraphInfo.GraphType> GraphTypes { get; } =
        new(Enum.GetValues(typeof(GraphInfo.GraphType)).Cast<GraphInfo.GraphType>());
    public static ObservableCollection<GraphInfo.AnimatType> AnimatTypes { get; } =
        new(Enum.GetValues(typeof(GraphInfo.AnimatType)).Cast<GraphInfo.AnimatType>());
    public static ObservableCollection<GameSave.ModeType> ModeTypes { get; } =
        new(Enum.GetValues(typeof(GameSave.ModeType)).Cast<GameSave.ModeType>());

    public ObservableValue<GraphInfo.GraphType> GraphType { get; } = new();

    public ObservableCollection<AnimeModel> HappyAnimes { get; } = new();
    public ObservableCollection<AnimeModel> NomalAnimes { get; } = new();
    public ObservableCollection<AnimeModel> PoorConditionAnimes { get; } = new();
    public ObservableCollection<AnimeModel> IllAnimes { get; } = new();

    public AnimeTypeModel() { }

    public AnimeTypeModel(AnimeTypeModel model)
    {
        GraphType.Value = model.GraphType.Value;
        foreach (var anime in model.HappyAnimes)
            HappyAnimes.Add(anime.Copy());
        foreach (var anime in model.NomalAnimes)
            NomalAnimes.Add(anime.Copy());
        foreach (var anime in model.PoorConditionAnimes)
            PoorConditionAnimes.Add(anime.Copy());
        foreach (var anime in model.IllAnimes)
            IllAnimes.Add(anime.Copy());
    }

    public AnimeTypeModel(GraphInfo.GraphType graphType, string path)
    {
        GraphType.Value = graphType;
        if (graphType is GraphInfo.GraphType.Default)
            LoadDefault(path);
    }

    private void LoadDefault(string path)
    {
        foreach (var modeDir in Directory.EnumerateDirectories(path))
        {
            var mode = Enum.Parse(typeof(GameSave.ModeType), Path.GetFileName(modeDir), true);
            if (mode is GameSave.ModeType.Happy)
            {
                foreach (var imagesDir in Directory.EnumerateDirectories(modeDir))
                {
                    HappyAnimes.Add(new(imagesDir));
                }
            }
            else if (mode is GameSave.ModeType.Nomal)
            {
                foreach (var imagesDir in Directory.EnumerateDirectories(modeDir))
                {
                    NomalAnimes.Add(new(imagesDir));
                }
            }
            else if (mode is GameSave.ModeType.PoorCondition)
            {
                foreach (var imagesDir in Directory.EnumerateDirectories(modeDir))
                {
                    PoorConditionAnimes.Add(new(imagesDir));
                }
            }
            else if (mode is GameSave.ModeType.Ill)
            {
                foreach (var imagesDir in Directory.EnumerateDirectories(modeDir))
                {
                    IllAnimes.Add(new(imagesDir));
                }
            }
        }
    }
}

public class AnimeModel
{
    public ObservableValue<string> Id { get; } = new();

    public ObservableValue<GraphInfo.AnimatType> AnimeType { get; } = new();

    //public ObservableValue<GameSave.ModeType> ModeType { get; } = new();

    public ObservableCollection<ImageModel> Images { get; } = new();
    private static readonly char[] _splits = new char[] { '_' };

    public AnimeModel() { }

    public AnimeModel(string imagesPath)
    {
        foreach (var file in Directory.EnumerateFiles(imagesPath))
        {
            var info = Path.GetFileNameWithoutExtension(file)
                .Split(_splits, StringSplitOptions.RemoveEmptyEntries);
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
            model.Images.Add(image);
        return model;
    }
}
