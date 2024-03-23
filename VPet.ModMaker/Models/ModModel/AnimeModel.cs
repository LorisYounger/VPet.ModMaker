using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using HKW.HKWUtils.Observable;
using VPet_Simulator.Core;

namespace VPet.ModMaker.Models.ModModel;

/// <summary>
/// 动画模型
/// </summary>
public class AnimeModel
{
    /// <summary>
    /// Id
    /// </summary>
    public ObservableValue<string> Id { get; } = new();

    /// <summary>
    /// 动画类型
    /// </summary>
    public ObservableValue<GraphInfo.AnimatType> AnimeType { get; } = new();

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
            Id.Value = info[0];
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
        model.Id.Value = Id.Value;
        model.AnimeType.Value = AnimeType.Value;
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
