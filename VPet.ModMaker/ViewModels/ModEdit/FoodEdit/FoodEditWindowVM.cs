using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using HKW.HKWUtils.Observable;
using LinePutScript.Localization.WPF;
using Microsoft.Win32;
using VPet.ModMaker.Models;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.ViewModels.ModEdit.FoodEdit;

public class FoodEditWindowVM : ObservableObjectX<FoodEditWindowVM>
{
    public static ModInfoModel ModInfo => ModInfoModel.Current;
    public static I18nHelper I18nData => I18nHelper.Current;
    #region Value
    public FoodModel OldFood { get; set; }

    #region Food
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private FoodModel _food;

    public FoodModel Food
    {
        get => _food;
        set => SetProperty(ref _food, value);
    }
    #endregion
    #endregion

    #region Command
    public ObservableCommand AddImageCommand { get; } = new();
    public ObservableCommand ChangeImageCommand { get; } = new();
    public ObservableCommand<double> SetReferencePriceCommand { get; } = new();
    #endregion

    public FoodEditWindowVM()
    {
        AddImageCommand.ExecuteCommand += AddImage;
        ChangeImageCommand.ExecuteCommand += ChangeImage;
        SetReferencePriceCommand.ExecuteCommand += SetReferencePriceToPrice;
        //TODO
        //Food.Value.ReferencePrice.ValueChanged += ReferencePrice_ValueChanged;
    }

    private void ReferencePrice_ValueChanged(
        ObservableValue<double> sender,
        ValueChangedEventArgs<double> e
    )
    {
        if (ModInfo.AutoSetFoodPrice)
        {
            SetReferencePriceToPrice(e.NewValue);
        }
    }

    private void SetReferencePriceToPrice(double value)
    {
        Food.Price = value;
    }

    public void Close()
    {
        Food.Close();
    }

    private void AddImage()
    {
        OpenFileDialog openFileDialog =
            new()
            {
                Title = "选择图片".Translate(),
                Filter = $"图片|*.jpg;*.jpeg;*.png;*.bmp".Translate()
            };
        if (openFileDialog.ShowDialog() is true)
        {
            Food.Image = NativeUtils.LoadImageToMemoryStream(openFileDialog.FileName);
        }
    }

    private void ChangeImage()
    {
        OpenFileDialog openFileDialog =
            new()
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
}
