using HKW.HKWUtils.Observable;
using HKW.Models;
using LinePutScript.Localization.WPF;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels.ModEdit.WorkEdit;

public class WorkEditWindowVM
{
    public I18nHelper I18nData => I18nHelper.Current;
    #region Value
    public PetModel CurrentPet { get; set; }
    public WorkModel OldWork { get; set; }
    public ObservableValue<WorkModel> Work { get; } = new(new());
    #endregion
    public ObservableValue<double> BorderLength { get; } = new(250);
    public ObservableValue<double> LengthRatio { get; } = new(250.0 / 500.0);
    public ObservableValue<BitmapImage> Image { get; } = new();
    #region Command
    public ObservableCommand AddImageCommand { get; } = new();
    public ObservableCommand ChangeImageCommand { get; } = new();
    #endregion
    public WorkEditWindowVM()
    {
        AddImageCommand.ExecuteCommand += AddImage;
        ChangeImageCommand.ExecuteCommand += ChangeImage;
        Image.ValueChanged += Image_ValueChanged;
    }

    private void Image_ValueChanged(
        ObservableValue<BitmapImage> sender,
        ValueChangedEventArgs<BitmapImage> e
    )
    {
        //LengthRatio.EnumValue = BorderLength.EnumValue / value.PixelWidth;
    }

    public void Close()
    {
        Image.Value?.CloseStream();
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
            Image.Value = Utils.LoadImageToMemoryStream(openFileDialog.FileName);
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
            Image.Value = Utils.LoadImageToMemoryStream(openFileDialog.FileName);
        }
    }
}
