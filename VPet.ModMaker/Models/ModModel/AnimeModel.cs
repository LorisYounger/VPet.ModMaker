using System.IO;
using HKW.HKWReactiveUI;
using HKW.HKWUtils;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using VPet.ModMaker.ViewModels;
using VPet_Simulator.Core;

namespace VPet.ModMaker.Models.ModModel;

/// <summary>
/// 动画模型
/// </summary>
public partial class AnimeModel : ViewModelBase, ICloneable<AnimeModel>
{
    /// <inheritdoc/>
    public AnimeModel() { }

    /// <inheritdoc/>
    /// <param name="imagesPath">图像路径</param>
    public AnimeModel(string imagesPath)
    {
        foreach (var file in Directory.EnumerateFiles(imagesPath))
        {
            var info = Path.GetFileNameWithoutExtension(file).Split(NativeUtils.Separator);
            ID = info[0];
            var duration = info.Last();
            if (int.TryParse(duration, out var result) is false)
                result = 100;
            var imageModel = new ImageModel(file, result);
            Images.Add(imageModel);
        }
    }

    /// <summary>
    /// ID
    /// </summary>
    [ReactiveProperty]
    public string ID { get; set; } = string.Empty;

    /// <summary>
    /// 动画类型
    /// </summary>
    [ReactiveProperty]
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
        if (Images.HasValue() is false || Images.First().Image is not null)
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
    protected override void Dispose(bool disposing)
    {
        if (_disposed)
            return;
        base.Dispose(disposing);
        if (disposing)
        {
            foreach (var image in Images)
                image.Dispose();
        }
    }
}
