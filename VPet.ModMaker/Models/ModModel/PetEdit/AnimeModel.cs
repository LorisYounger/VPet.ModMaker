using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using HKW.HKWUtils;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using HKW.MVVM;
using VPet_Simulator.Core;
using VPet.ModMaker.ViewModels;

namespace VPet.ModMaker.Models;

/// <summary>
/// 动画模型
/// </summary>
public sealed partial class AnimeModel : ObservableModelBase, ICloneable<AnimeModel>, IDisposable
{
    /// <inheritdoc/>
    public AnimeModel() { }

    /// <inheritdoc/>
    /// <param name="imagesPath">图像路径</param>
    public AnimeModel(string imagesPath)
    {
        foreach (var file in Directory.EnumerateFiles(imagesPath))
        {
            var info = Path.GetFileNameWithoutExtension(file).Split(NativeUtils.Underscore);
            ID = info[0];
            var duration = info[^1];
            if (int.TryParse(duration, out var result) is false)
                result = 100;
            var imageModel = new ImageModel(file, result);
            Images.Add(imageModel);
        }
    }

    /// <summary>
    /// ID
    /// </summary>
    [ObservableProperty]
    public string ID { get; set; } = string.Empty;

    /// <summary>
    /// 动画类型
    /// </summary>
    [ObservableProperty]
    public GraphInfo.AnimatType AnimeType { get; set; }

    /// <summary>
    /// 图像列表
    /// </summary>
    public ObservableList<ImageModel> Images { get; } = [];

    /// <summary>
    /// 载入动画
    /// </summary>
    public void LoadAnime()
    {
        if (Images.FirstOrDefault()?.Image is not null)
            return;
        foreach (var image in Images)
            image.LoadImage();
    }

    /// <summary>
    /// 复制
    /// </summary>
    /// <returns></returns>
    public AnimeModel Clone()
    {
        var model = new AnimeModel { ID = ID, AnimeType = AnimeType };
        foreach (var image in Images)
            model.Images.Add(image.Clone());
        return model;
    }

    object ICloneable.Clone() => Clone();

    /// <inheritdoc/>
    public void Dispose()
    {
        foreach (var image in Images)
            image.Dispose();
        Images.Clear();
    }
}
