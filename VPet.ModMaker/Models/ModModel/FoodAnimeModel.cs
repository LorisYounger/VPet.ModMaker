using HKW.HKWUtils.Observable;
using LinePutScript;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using VPet_Simulator.Core;

namespace VPet.ModMaker.Models.ModModel;

public class FoodAnimeModel
{
    /// <summary>
    /// Id
    /// </summary>
    public ObservableValue<string> Id { get; } = new();

    public ObservableValue<GameSave.ModeType> Mode { get; }

    /// <summary>
    /// 后图像列表
    /// </summary>
    public ObservableCollection<ImageModel> BackImages { get; set; } = new();

    /// <summary>
    /// 前图像列表
    /// </summary>
    public ObservableCollection<ImageModel> FrontImages { get; set; } = new();

    /// <summary>
    /// 食物定位列表
    /// </summary>
    public ObservableCollection<FoodLocationModel> FoodLocations { get; } = new();

    public FoodAnimeModel() { }

    public FoodAnimeModel(ILine line)
        : this()
    {
        foreach (var item in line.Where(i => i.Name.StartsWith("a")))
        {
            //var index = int.Parse(item.Name.Substring(1));
            var infos = item.Info.Split(',');
            var foodLocationInfo = new FoodLocationModel();
            foodLocationInfo.Duration.Value = int.Parse(infos[0]);
            if (infos.Length > 1)
            {
                foodLocationInfo.Rect = new(
                    double.Parse(infos[1]),
                    double.Parse(infos[2]),
                    double.Parse(infos[3]),
                    double.Parse(infos[3])
                );
            }
            if (infos.Length > 4)
                foodLocationInfo.Rotate.Value = double.Parse(infos[4]);
            if (infos.Length > 5)
                foodLocationInfo.Opacity.Value = double.Parse(infos[5]);
            FoodLocations.Add(foodLocationInfo);
        }
    }

    /// <summary>
    /// 复制
    /// </summary>
    /// <returns></returns>
    public FoodAnimeModel Copy()
    {
        var model = new FoodAnimeModel();
        model.Id.Value = Id.Value;
        foreach (var image in FrontImages)
            model.FrontImages.Add(image.Copy());
        foreach (var image in BackImages)
            model.BackImages.Add(image.Copy());
        foreach (var foodLocation in FoodLocations)
            model.FoodLocations.Add(foodLocation.Copy());
        return model;
    }

    /// <summary>
    /// 关闭所有图像流
    /// </summary>
    public void Close()
    {
        foreach (var image in FrontImages)
            image.Close();
        foreach (var image in BackImages)
            image.Close();
    }
}

public class FoodImagesPath
{
    public ObservableValue<GameSave.ModeType> Mode { get; } = new();

    public ObservableValue<int> Index { get; } = new();
}
