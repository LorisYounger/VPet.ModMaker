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
    public FoodEditWindowVM()
    {
        AddImageCommand.ExecuteCommand += AddImageCommand_ExecuteCommand;
        ChangeImageCommand.ExecuteCommand += ChangeImageCommand_ExecuteCommand;
        SetReferencePriceCommand.ExecuteCommand += SetReferencePriceCommand_ExecuteCommand;
    }

    public static ModInfoModel ModInfo => ModInfoModel.Current;
    public static I18nHelper I18nData => I18nHelper.Current;

    #region Property
    public FoodModel? OldFood { get; set; }

    #region Food
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private FoodModel _food = new();

    public FoodModel Food
    {
        get => _food;
        set
        {
            if (_food is not null)
                _food.PropertyChangedX -= Food_PropertyChangedX;
            if (SetProperty(ref _food!, value) is false)
                return;
            Food.PropertyChangedX += Food_PropertyChangedX;
        }
    }

    private void Food_PropertyChangedX(I18nModel<I18nFoodModel> sender, PropertyChangedXEventArgs e)
    {
        if (e.PropertyName == nameof(FoodModel.ReferencePrice))
        {
            if (ModInfo.AutoSetFoodPrice)
                SetReferencePriceCommand_ExecuteCommand((double)e.NewValue!);
        }
    }
    #endregion
    #endregion

    #region Command
    public ObservableCommand AddImageCommand { get; } = new();
    public ObservableCommand ChangeImageCommand { get; } = new();
    public ObservableCommand<double> SetReferencePriceCommand { get; } = new();
    #endregion

    private void SetReferencePriceCommand_ExecuteCommand(double value)
    {
        Food.Price = value;
    }

    public void Close()
    {
        Food.Close();
    }

    private void AddImageCommand_ExecuteCommand()
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

    private void ChangeImageCommand_ExecuteCommand()
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
}
