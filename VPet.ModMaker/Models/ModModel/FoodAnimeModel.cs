using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using HKW.HKWUtils.Observable;
using LinePutScript;
using VPet_Simulator.Core;
using static VPet_Simulator.Core.IGameSave;

namespace VPet.ModMaker.Models.ModModel;

public class FoodAnimeModel : ObservableObjectX<FoodAnimeModel>, ICloneable<FoodAnimeModel>
{
    public FoodAnimeModel() { }

    public FoodAnimeModel(ILine line)
        : this()
    {
        foreach (var item in line.Where(i => i.Name.StartsWith('a')))
        {
            //var index = int.Parse(item.Name.Substring(1));
            var infos = item.Info.Split(',');
            var foodLocationInfo = new FoodAnimeLocationModel();
            foodLocationInfo.Duration = int.Parse(infos[0]);
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

    #region ID
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _id;

    public string ID
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }
    #endregion

    public ObservableValue<ModeType> Mode { get; }

    /// <summary>
    /// 后图像列表
    /// </summary>
    public ObservableList<ImageModel> BackImages { get; set; } = new();

    /// <summary>
    /// 前图像列表
    /// </summary>
    public ObservableList<ImageModel> FrontImages { get; set; } = new();

    /// <summary>
    /// 食物定位列表
    /// </summary>
    public ObservableList<FoodAnimeLocationModel> FoodLocations { get; } = new();

    /// <summary>
    /// 复制
    /// </summary>
    /// <returns></returns>
    public FoodAnimeModel Clone()
    {
        var model = new FoodAnimeModel();
        model.ID = ID;
        foreach (var image in FrontImages)
            model.FrontImages.Add(image.Clone());
        foreach (var image in BackImages)
            model.BackImages.Add(image.Clone());
        foreach (var foodLocation in FoodLocations)
            model.FoodLocations.Add(foodLocation.Clone());
        return model;
    }

    object ICloneable.Clone() => Clone();

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
