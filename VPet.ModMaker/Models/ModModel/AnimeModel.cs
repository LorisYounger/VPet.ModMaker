using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using HKW.HKWUtils.Observable;
using VPet_Simulator.Core;

namespace VPet.ModMaker.Models.ModModel;

/// <summary>
/// 动画模型
/// </summary>
public class AnimeModel : ObservableObjectX<AnimeModel>
{
    #region Id
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _id;

    /// <summary>
    /// Id
    /// </summary>
    public string Id
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }
    #endregion

    #region AnimeType
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private GraphInfo.AnimatType _animeType;

    /// <summary>
    /// 动画类型
    /// </summary>
    public GraphInfo.AnimatType AnimeType
    {
        get => _animeType;
        set => SetProperty(ref _animeType, value);
    }
    #endregion

    /// <summary>
    /// 图像列表
    /// </summary>
    public ObservableCollection<ImageModel> Images { get; } = new();

    public AnimeModel() { }

    public AnimeModel(string imagesPath)
        : this()
    {
        foreach (var file in Directory.EnumerateFiles(imagesPath))
        {
            var info = Path.GetFileNameWithoutExtension(file).Split(NativeUtils.Separator);
            Id = info[0];
            var duration = info.Last();
            var imageModel = new ImageModel(
                NativeUtils.LoadImageToMemoryStream(file),
                int.Parse(duration)
            );
            Images.Add(imageModel);
        }
    }

    /// <summary>
    /// 复制
    /// </summary>
    /// <returns></returns>
    public AnimeModel Copy()
    {
        var model = new AnimeModel();
        model.Id = Id;
        model.AnimeType = AnimeType;
        foreach (var image in Images)
            model.Images.Add(image.Copy());
        return model;
    }

    /// <summary>
    /// 关闭所有图像流
    /// </summary>
    public void Close()
    {
        foreach (var image in Images)
            image.Close();
    }
}
