using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using HKW.HKWUtils.Observable;
using LinePutScript.Localization.WPF;
using Microsoft.Win32;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels.ModEdit.PetEdit;

public class PetEditWindowVM : ObservableObjectX
{
    public PetEditWindowVM()
    {
        AddImageCommand.ExecuteCommand += AddImageCommand_ExecuteCommand;
        ChangeImageCommand.ExecuteCommand += ChangeImageCommand_ExecuteCommand;
        //Image.ValueChanged += Image_ValueChanged;
    }

    #region Property
    /// <summary>
    /// I18n资源
    /// </summary>
    public static I18nResource<string, string> I18nResource => ModInfoModel.Current.I18nResource;
    public PetModel? OldPet { get; set; }

    #region Pet
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private PetModel _pet = new();

    public PetModel Pet
    {
        get => _pet;
        set => SetProperty(ref _pet, value);
    }
    #endregion

    #region BorderLength
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private double _borderLength = 250;

    public double BorderLength
    {
        get => _borderLength;
        set => SetProperty(ref _borderLength, value);
    }
    #endregion
    #region LengthRatio
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private double _lengthRatio = 0.5;

    public double LengthRatio
    {
        get => _lengthRatio;
        set => SetProperty(ref _lengthRatio, value);
    }
    #endregion
    #region Image
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private BitmapImage? _image;

    public BitmapImage? Image
    {
        get => _image;
        set => SetProperty(ref _image, value);
    }
    #endregion
    #endregion

    #region Command
    public ObservableCommand AddImageCommand { get; } = new();
    public ObservableCommand ChangeImageCommand { get; } = new();
    #endregion
    private void Image_ValueChanged(
        ObservableValue<BitmapImage> sender,
        ValueChangedEventArgs<BitmapImage> e
    )
    {
        //LengthRatio.EnumValue = BorderLength.EnumValue / value.PixelWidth;
    }

    public void Close()
    {
        Image?.CloseStream();
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
            Image = NativeUtils.LoadImageToMemoryStream(openFileDialog.FileName);
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
            Image?.StreamSource?.Close();
            Image = NativeUtils.LoadImageToMemoryStream(openFileDialog.FileName);
        }
    }
}
