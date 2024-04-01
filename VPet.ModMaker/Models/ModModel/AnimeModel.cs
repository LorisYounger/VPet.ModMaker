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
public class AnimeModel : ObservableObjectX, ICloneable<AnimeModel>
{
    public AnimeModel() { }

    public AnimeModel(string imagesPath)
        : this()
    {
        foreach (var file in Directory.EnumerateFiles(imagesPath))
        {
            var info = Path.GetFileNameWithoutExtension(file).Split(NativeUtils.Separator);
            ID = info[0];
            var duration = info.Last();
            var imageModel = new ImageModel(
                NativeUtils.LoadImageToMemoryStream(file),
                int.Parse(duration)
            );
            Images.Add(imageModel);
        }
    }

    #region ID
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _id = string.Empty;

    /// <summary>
    /// Id
    /// </summary>
    public string ID
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
    public ObservableList<ImageModel> Images { get; } = new();

    /// <summary>
    /// 复制
    /// </summary>
    /// <returns></returns>
    public AnimeModel Clone()
    {
        var model = new AnimeModel();
        model.ID = ID;
        model.AnimeType = AnimeType;
        foreach (var image in Images)
            model.Images.Add(image.Clone());
        return model;
    }

    object ICloneable.Clone() => Clone();

    /// <summary>
    /// 关闭所有图像流
    /// </summary>
    public void Close()
    {
        foreach (var image in Images)
            image.Close();
    }
}
