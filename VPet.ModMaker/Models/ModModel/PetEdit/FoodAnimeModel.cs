using CommunityToolkit.Mvvm.ComponentModel;
using HKW.HKWUtils;
using HKW.HKWUtils.Observable;
using HKW.MVVM;
using LinePutScript;
using VPet.ModMaker.ViewModels;

namespace VPet.ModMaker.Models;

/// <summary>
/// 食物动画模型
/// </summary>
public sealed partial class FoodAnimeModel
    : ObservableModelBase,
        ICloneable<FoodAnimeModel>,
        IDisposable
{
    /// <inheritdoc/>
    public FoodAnimeModel() { }

    /// <inheritdoc/>
    /// <param name="line">行</param>
    public FoodAnimeModel(ILine line)
        : this()
    {
        foreach (var item in line.Where(i => i.Name.StartsWith('a')))
        {
            var infos = item.Info.Split(',');
            var foodLocationInfo = new FoodAnimeLocationModel { Duration = int.Parse(infos[0]) };
            if (infos.Length > 1)
            {
                foodLocationInfo.RectangleLocation = new(
                    double.Parse(infos[1]),
                    double.Parse(infos[2]),
                    double.Parse(infos[3]),
                    double.Parse(infos[3])
                );
            }
            if (infos.Length > 4)
                foodLocationInfo.Rotate = double.Parse(infos[4]);
            if (infos.Length > 5)
                foodLocationInfo.Opacity = double.Parse(infos[5]);
            FoodLocations.Add(foodLocationInfo);
        }
    }

    /// <summary>
    /// ID
    /// </summary>
    [ObservableProperty]
    public string ID { get; set; } = string.Empty;

    /// <summary>
    /// 前图像列表
    /// </summary>
    public ObservableList<ImageModel> FrontImages { get; set; } = [];

    /// <summary>
    /// 后图像列表
    /// </summary>
    public ObservableList<ImageModel> BackImages { get; set; } = [];

    /// <summary>
    /// 食物定位列表
    /// </summary>
    public ObservableList<FoodAnimeLocationModel> FoodLocations { get; } = [];

    /// <summary>
    /// 载入动画
    /// </summary>
    public void LoadAnime()
    {
        if (BackImages.FirstOrDefault()?.Image is null)
        {
            foreach (var image in BackImages)
                image.LoadImage();
        }
        if (FrontImages.FirstOrDefault()?.Image is null)
        {
            foreach (var image in FrontImages)
                image.LoadImage();
        }
    }

    /// <inheritdoc/>
    public FoodAnimeModel Clone()
    {
        var model = new FoodAnimeModel() { ID = ID };
        foreach (var image in FrontImages)
            model.FrontImages.Add(image.Clone());
        foreach (var image in BackImages)
            model.BackImages.Add(image.Clone());
        foreach (var foodLocation in FoodLocations)
            model.FoodLocations.Add(foodLocation.Clone());
        return model;
    }

    object ICloneable.Clone() => Clone();

    /// <inheritdoc/>
    public void Dispose()
    {
        foreach (var image in FrontImages)
            image.Dispose();
        FrontImages.Clear();
        foreach (var image in BackImages)
            image.Dispose();
        BackImages.Clear();
    }
}
