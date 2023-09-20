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
        else if (graphType is GraphInfo.GraphType.Touch_Head or GraphInfo.GraphType.Touch_Body)
            LoadMultiTypeAnime(path);
        else
            throw new Exception();
    }

    public static AnimeTypeModel? Create(GraphInfo.GraphType graphType, string path)
    {
        try
        {
            var model = new AnimeTypeModel(graphType, path);
            return model;
        }
        catch
        {
            return null;
        }
    }

    private void LoadDefault(string path)
    {
        foreach (var dir in Directory.EnumerateDirectories(path))
        {
            var mode = Enum.Parse(typeof(GameSave.ModeType), Path.GetFileName(dir), true);
            if (mode is GameSave.ModeType.Happy)
            {
                AddAnime(HappyAnimes, dir);
            }
            else if (mode is GameSave.ModeType.Nomal)
            {
                AddAnime(NomalAnimes, dir);
            }
            else if (mode is GameSave.ModeType.PoorCondition)
            {
                AddAnime(PoorConditionAnimes, dir);
            }
            else if (mode is GameSave.ModeType.Ill)
            {
                AddAnime(IllAnimes, dir);
            }
        }
    }

    private void LoadMultiTypeAnime(string path)
    {
        foreach (var dir in Directory.EnumerateDirectories(path))
        {
            var dirName = Path.GetFileName(dir);
            var dirInfo = dirName.Split(Utils.Separator, StringSplitOptions.RemoveEmptyEntries);
            if (dirInfo.Length == 2)
            {
                // 判断 A_Happy 类型文件夹
                var typeName = dirInfo[0];
                var modeName = dirInfo[1];
                var type = GetAnimatType(typeName[0]);
                var mode = Enum.Parse(typeof(GameSave.ModeType), Path.GetFileName(modeName), true);
                if (mode is GameSave.ModeType.Happy)
                {
                    AddAnime(HappyAnimes, dir, type);
                }
                else if (mode is GameSave.ModeType.Nomal)
                {
                    AddAnime(NomalAnimes, dir, type);
                }
                else if (mode is GameSave.ModeType.PoorCondition)
                {
                    AddAnime(PoorConditionAnimes, dir, type);
                }
                else if (mode is GameSave.ModeType.Ill)
                {
                    AddAnime(IllAnimes, dir, type);
                }
            }
            else
            {
                // 判断 Happy/A 型文件夹
                var mode = Enum.Parse(typeof(GameSave.ModeType), Path.GetFileName(dirName), true);
                foreach (var typePath in Directory.EnumerateDirectories(dir))
                {
                    var type = GetAnimatType(Path.GetFileName(typePath)[0]);
                    if (mode is GameSave.ModeType.Happy)
                    {
                        AddAnime(HappyAnimes, dir, type);
                    }
                    else if (mode is GameSave.ModeType.Nomal)
                    {
                        AddAnime(NomalAnimes, dir, type);
                    }
                    else if (mode is GameSave.ModeType.PoorCondition)
                    {
                        AddAnime(PoorConditionAnimes, dir, type);
                    }
                    else if (mode is GameSave.ModeType.Ill)
                    {
                        AddAnime(IllAnimes, dir, type);
                    }
                }
            }
        }
    }

    private static GraphInfo.AnimatType GetAnimatType(char c)
    {
        return c switch
        {
            'A' => GraphInfo.AnimatType.A_Start,
            'B' => GraphInfo.AnimatType.B_Loop,
            'C' => GraphInfo.AnimatType.C_End,
            _ => GraphInfo.AnimatType.Single,
        };
    }

    public static void AddAnime(
        ObservableCollection<AnimeModel> collection,
        string path,
        GraphInfo.AnimatType animatType = AnimatType.Single
    )
    {
        if (Directory.EnumerateFiles(path).Any())
        {
            var animeModel = new AnimeModel(path);
            animeModel.AnimeType.Value = animatType;
            collection.Add(animeModel);
        }
        else
        {
            foreach (var imagesDir in Directory.EnumerateDirectories(path))
            {
                var animeModel = new AnimeModel(imagesDir);
                animeModel.AnimeType.Value = animatType;
                collection.Add(animeModel);
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

    public AnimeModel() { }

    public AnimeModel(string imagesPath)
    {
        foreach (var file in Directory.EnumerateFiles(imagesPath))
        {
            var info = Path.GetFileNameWithoutExtension(file).Split(Utils.Separator);
            Id.Value = info[0];
            var duration = info.Last();
            var imageModel = new ImageModel(Utils.LoadImageToStream(file), int.Parse(duration));
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

    public void Close()
    {
        foreach (var image in Images)
            image.Close();
    }
}
