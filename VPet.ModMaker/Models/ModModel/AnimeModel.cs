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

public class AnimeModel
{
    public static ObservableCollection<GraphInfo.GraphType> GraphTypes { get; } =
        new(Enum.GetValues(typeof(GraphInfo.GraphType)).Cast<GraphInfo.GraphType>());
    public static ObservableCollection<GraphInfo.AnimatType> AnimatTypes { get; } =
        new(Enum.GetValues(typeof(GraphInfo.AnimatType)).Cast<GraphInfo.AnimatType>());

    public static ObservableCollection<GameSave.ModeType> ModeTypes { get; } =
        new(Enum.GetValues(typeof(GameSave.ModeType)).Cast<GameSave.ModeType>());

    public ObservableValue<string> Id { get; } = new();

    public ObservableValue<GraphInfo.GraphType> GraphType { get; } = new();
    public ObservableValue<GraphInfo.AnimatType> AnimeType { get; } = new();
    public ObservableValue<GameSave.ModeType> ModeType { get; } = new();

    public ObservableCollection<ImageModels> MultiHappyImageModels = new();
    public ObservableCollection<ImageModels> MultiNomalImageModels = new();
    public ObservableCollection<ImageModels> MultiPoorConditionImageModels = new();
    public ObservableCollection<ImageModels> MultiIllImageModels = new();

    public AnimeModel() { }

    //public AnimeModel()
    //{

    //}

    public static AnimeModel? Load(string path)
    {
        var model = new AnimeModel();
        var infoFile = Path.Combine(path, ModMakerInfo.InfoFile);

        if (
            Enum.TryParse<GraphInfo.GraphType>(Path.GetFileName(path), true, out var graphType)
            is false
        )
            return null;
        if (graphType is GraphInfo.GraphType.Default)
        {
            foreach (var dir in Directory.EnumerateDirectories(path))
            {
                var dirName = Path.GetFileName(dir);
                if (
                    dirName.Contains(
                        nameof(GameSave.ModeType.Happy),
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    if (Directory.GetFiles(dir).Length == 0)
                    {
                        foreach (var imageDir in Directory.EnumerateDirectories(dir))
                        {
                            model.MultiHappyImageModels.Add(new(imageDir));
                        }
                    }
                    else
                    {
                        model.MultiHappyImageModels.Add(new(dir));
                    }
                }
                else if (
                    dirName.Contains(
                        nameof(GameSave.ModeType.Nomal),
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    if (Directory.GetFiles(dir).Length == 0)
                    {
                        foreach (var imageDir in Directory.EnumerateDirectories(dir))
                        {
                            model.MultiNomalImageModels.Add(new(imageDir));
                        }
                    }
                    else
                    {
                        model.MultiNomalImageModels.Add(new(dir));
                    }
                }
                else if (
                    dirName.Contains(
                        nameof(GameSave.ModeType.PoorCondition),
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    if (Directory.GetFiles(dir).Length == 0)
                    {
                        foreach (var imageDir in Directory.EnumerateDirectories(dir))
                        {
                            model.MultiPoorConditionImageModels.Add(new(imageDir));
                        }
                    }
                    else
                    {
                        model.MultiPoorConditionImageModels.Add(new(dir));
                    }
                }
                else if (
                    dirName.Contains(
                        nameof(GameSave.ModeType.Ill),
                        StringComparison.OrdinalIgnoreCase
                    )
                )
                {
                    if (Directory.GetFiles(dir).Length == 0)
                    {
                        foreach (var imageDir in Directory.EnumerateDirectories(dir))
                        {
                            model.MultiIllImageModels.Add(new(imageDir));
                        }
                    }
                    else
                    {
                        model.MultiIllImageModels.Add(new(dir));
                    }
                }
            }
        }
        else
            return null;

        return model;
    }
}

public class ImageModels : ObservableCollection<ImageModel>
{
    private static readonly char[] _splits = new char[] { '_' };

    public ImageModels(string imagePath)
    {
        foreach (var file in Directory.EnumerateFiles(imagePath))
        {
            var info = Path.GetFileNameWithoutExtension(file)
                .Split(_splits, StringSplitOptions.RemoveEmptyEntries);
            var id = info[0];
            var duration = info.Last();
            var imageModel = new ImageModel();
            imageModel.Id.Value = id;
            imageModel.Image.Value = Utils.LoadImageToMemoryStream(file);
            imageModel.Duration.Value = int.Parse(duration);
            Add(imageModel);
        }
    }
}
