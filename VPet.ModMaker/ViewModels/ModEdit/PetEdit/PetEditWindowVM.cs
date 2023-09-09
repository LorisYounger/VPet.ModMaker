using HKW.HKWViewModels.SimpleObservable;
using LinePutScript.Localization.WPF;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels.ModEdit.PetEdit;

public class PetEditWindowVM
{
    public PetModel OldPet { get; set; }
    public ObservableValue<PetModel> Pet { get; } = new(new());

    public ObservableValue<double> BorderLength { get; } = new(250);
    public ObservableValue<double> LengthRatio { get; } = new(250.0 / 500.0);
    public ObservableValue<BitmapImage> Image { get; } = new();
    #region Command
    public ObservableCommand AddImageCommand { get; } = new();
    public ObservableCommand ChangeImageCommand { get; } = new();
    #endregion
    public PetEditWindowVM()
    {
        AddImageCommand.ExecuteEvent += AddImage;
        ChangeImageCommand.ExecuteEvent += ChangeImage;
        Image.ValueChanged += Image_ValueChanged;
    }

    private void Image_ValueChanged(BitmapImage oldValue, BitmapImage newValue)
    {
        //LengthRatio.Value = BorderLength.Value / value.PixelWidth;
    }

    public void Close()
    {
        Image.Value?.StreamSource?.Close();
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
            Image.Value = Utils.LoadImageToStream(openFileDialog.FileName);
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
            Image.Value?.StreamSource?.Close();
            Image.Value = Utils.LoadImageToStream(openFileDialog.FileName);
        }
    }
}
