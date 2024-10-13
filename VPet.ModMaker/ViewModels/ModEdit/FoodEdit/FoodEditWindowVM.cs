using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using HKW.HKWReactiveUI;
using HKW.HKWUtils.Observable;
using LinePutScript.Localization.WPF;
using Microsoft.Win32;
using VPet.ModMaker.Models;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.ViewModels.ModEdit.FoodEdit;

public partial class FoodEditWindowVM : ViewModelBase
{
    public FoodEditWindowVM()
    {
        //AddImageCommand.ExecuteCommand += AddImage;
        //ChangeImageCommand.ExecuteCommand += ChangeImage;
        //SetReferencePriceCommand.ExecuteCommand += SetReferencePrice;
    }

    public static ModInfoModel ModInfo => ModInfoModel.Current;

    /// <summary>
    /// I18n资源
    /// </summary>
    public static I18nResource<string, string> I18nResource => ModInfoModel.Current.I18nResource;

    #region Property
    public FoodModel? OldFood { get; set; }

    [ReactiveProperty]
    public FoodModel Food { get; set; } =
        new() { I18nResource = ModInfoModel.Current.I18nResource };

    //TODO: 在FoodModel设置推荐价格
    //partial void OnFoodChanged(FoodModel oldValue, FoodModel newValue)
    //{
    //    if (oldValue is not null)
    //        oldValue.PropertyChanged -= Food_PropertyChangedX;
    //    if (newValue is not null)
    //        newValue.PropertyChanged += Food_PropertyChangedX;
    //}

    //private void Food_PropertyChangedX(object? sender, PropertyChangedEventArgs e)
    //{
    //    if (e.PropertyName == nameof(FoodModel.ReferencePrice))
    //    {
    //        if (ModInfo.AutoSetFoodPrice)
    //            SetReferencePrice((double)e.NewValue!);
    //    }
    //}
    #endregion

    //#region Command
    //public ObservableCommand AddImageCommand { get; } = new();
    //public ObservableCommand ChangeImageCommand { get; } = new();
    //public ObservableCommand<double> SetReferencePriceCommand { get; } = new();
    //#endregion
    /// <summary>
    /// 设置推荐价格
    /// </summary>
    /// <param name="value"></param>
    [ReactiveCommand]
    private void SetReferencePrice(double value)
    {
        Food.Price = value;
    }

    /// <summary>
    /// 添加图像
    /// </summary>
    [ReactiveCommand]
    private void AddImage()
    {
        var openFileDialog = new OpenFileDialog()
        {
            Title = "选择图片".Translate(),
            Filter = $"图片|*.jpg;*.jpeg;*.png;*.bmp".Translate()
        };
        if (openFileDialog.ShowDialog() is true)
        {
            Food.Image = NativeUtils.LoadImageToMemoryStream(openFileDialog.FileName);
        }
    }

    /// <summary>
    /// 改变图像
    /// </summary>
    [ReactiveCommand]
    private void ChangeImage()
    {
        var openFileDialog = new OpenFileDialog()
        {
            Title = "选择图片".Translate(),
            Filter = $"图片|*.jpg;*.jpeg;*.png;*.bmp".Translate()
        };
        if (openFileDialog.ShowDialog() is true)
        {
            Food.Image?.StreamSource?.Close();
            Food.Image = NativeUtils.LoadImageToMemoryStream(openFileDialog.FileName);
        }
    }

    public void Close()
    {
        Food.Close();
    }
}
