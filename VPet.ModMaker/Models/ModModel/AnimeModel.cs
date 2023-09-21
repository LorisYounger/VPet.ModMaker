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

    public AnimeTypeModel(GraphInfo.GraphType graphType, string path)
    {
        GraphType.Value = graphType;
        if (
            graphType
            is GraphInfo.GraphType.Default
                or GraphInfo.GraphType.Shutdown
                or GraphInfo.GraphType.StartUP
        )
            LoadDefault(path);
        else if (
            graphType
            is GraphInfo.GraphType.Touch_Head
                or GraphInfo.GraphType.Touch_Body
                or GraphInfo.GraphType.Sleep
        )
            LoadMultiType(path);
        else
            throw new Exception();
    }

    //private void LoadSingle(string path)
    //{
    //    foreach (var dir in Directory.EnumerateDirectories(path))
    //    {
    //        var dirName = Path.GetFileName(dir);
    //        var dirInfo = dirName.Split(Utils.Separator);
    //        var mode = Enum.Parse(typeof(GameSave.ModeType), dirInfo[0], true);
    //        if (dirInfo.Length == 2) { }
    //    }
    //}

    private void LoadDefault(string path)
    {
        foreach (var dir in Directory.EnumerateDirectories(path))
        {
            var dirName = Path.GetFileName(dir);
            if (dirName.Contains(nameof(GameSave.ModeType.Happy)))
            {
                AddAnime(HappyAnimes, dir);
            }
            else if (dirName.Contains(nameof(GameSave.ModeType.Nomal)))
            {
                AddAnime(NomalAnimes, dir);
            }
            else if (dirName.Contains(nameof(GameSave.ModeType.PoorCondition)))
            {
                AddAnime(PoorConditionAnimes, dir);
            }
            else if (dirName.Contains(nameof(GameSave.ModeType.Ill)))
            {
                AddAnime(IllAnimes, dir);
            }
        }
    }

    private void LoadMultiType(string path)
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

    public void Save(string path)
    {
        if (
            GraphType.Value
            is GraphInfo.GraphType.Default
                or GraphInfo.GraphType.Shutdown
                or GraphInfo.GraphType.StartUP
        )
            SaveDefault(path, this);
        else if (
            GraphType.Value
            is GraphInfo.GraphType.Touch_Head
                or GraphInfo.GraphType.Touch_Body
                or GraphInfo.GraphType.Sleep
        )
            SaveMultiType(path, this);
    }

    void SaveMultiType(string path, AnimeTypeModel animeType)
    {
        var animePath = Path.Combine(path, animeType.GraphType.ToString());
        Directory.CreateDirectory(animePath);
        if (animeType.HappyAnimes.Count > 0)
        {
            var modePath = Path.Combine(animePath, nameof(GameSave.ModeType.Happy));
            SaveAnimes(modePath, animeType.HappyAnimes);
        }
        if (animeType.NomalAnimes.Count > 0)
        {
            var modePath = Path.Combine(animePath, nameof(GameSave.ModeType.Nomal));
            SaveAnimes(modePath, animeType.NomalAnimes);
        }
        if (animeType.PoorConditionAnimes.Count > 0)
        {
            var modePath = Path.Combine(animePath, nameof(GameSave.ModeType.PoorCondition));
            SaveAnimes(modePath, animeType.PoorConditionAnimes);
        }
        if (animeType.IllAnimes.Count > 0)
        {
            var modePath = Path.Combine(animePath, nameof(GameSave.ModeType.Ill));
            SaveAnimes(modePath, animeType.IllAnimes);
        }

        static void SaveAnimes(string animePath, ObservableCollection<AnimeModel> animes)
        {
            Directory.CreateDirectory(animePath);
            var countA = 0;
            var countB = 0;
            var countC = 0;
            foreach (var anime in animes)
            {
                if (anime.AnimeType.Value is GraphInfo.AnimatType.A_Start)
                {
                    var animatTypePath = Path.Combine(animePath, "A");
                    Directory.CreateDirectory(animatTypePath);
                    SaveImages(Path.Combine(animatTypePath, countA.ToString()), anime);
                    countA++;
                }
                else if (anime.AnimeType.Value is GraphInfo.AnimatType.B_Loop)
                {
                    var animatTypePath = Path.Combine(animePath, "B");
                    Directory.CreateDirectory(animatTypePath);
                    SaveImages(Path.Combine(animatTypePath, countB.ToString()), anime);
                    countB++;
                }
                else if (anime.AnimeType.Value is GraphInfo.AnimatType.C_End)
                {
                    var animatTypePath = Path.Combine(animePath, "C");
                    Directory.CreateDirectory(animatTypePath);
                    SaveImages(Path.Combine(animatTypePath, countC.ToString()), anime);
                    countC++;
                }
            }
        }
    }

    static void SaveDefault(string path, AnimeTypeModel animeType)
    {
        var animePath = Path.Combine(path, animeType.GraphType.ToString());
        Directory.CreateDirectory(animePath);
        if (animeType.HappyAnimes.Count > 0)
        {
            var modePath = Path.Combine(animePath, nameof(GameSave.ModeType.Happy));
            SaveAnimes(modePath, animeType.HappyAnimes);
        }
        if (animeType.NomalAnimes.Count > 0)
        {
            var modePath = Path.Combine(animePath, nameof(GameSave.ModeType.Nomal));
            SaveAnimes(modePath, animeType.NomalAnimes);
        }
        if (animeType.PoorConditionAnimes.Count > 0)
        {
            var modePath = Path.Combine(animePath, nameof(GameSave.ModeType.PoorCondition));
            SaveAnimes(modePath, animeType.PoorConditionAnimes);
        }
        if (animeType.IllAnimes.Count > 0)
        {
            var modePath = Path.Combine(animePath, nameof(GameSave.ModeType.Ill));
            SaveAnimes(modePath, animeType.IllAnimes);
        }
        static void SaveAnimes(string animePath, ObservableCollection<AnimeModel> animes)
        {
            Directory.CreateDirectory(animePath);
            var count = 0;
            foreach (var anime in animes)
            {
                SaveImages(Path.Combine(animePath, count.ToString()), anime);
                count++;
            }
        }
    }

    static void SaveImages(string imagesPath, AnimeModel model)
    {
        Directory.CreateDirectory(imagesPath);
        var imageIndex = 0;
        foreach (var image in model.Images)
        {
            File.Copy(
                image.Image.Value.GetSourceFile(),
                Path.Combine(
                    imagesPath,
                    $"{model.Id.Value}_{imageIndex:000}_{image.Duration.Value}.png"
                ),
                true
            );
            imageIndex++;
        }
    }
}

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
