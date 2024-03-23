using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using HKW.HKWUtils.Observable;
using LinePutScript.Localization.WPF;
using Microsoft.Win32;
using VPet.ModMaker.Models;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.ViewModels.ModEdit.WorkEdit;

public class WorkEditWindowVM
{
    public static ModInfoModel ModInfo => ModInfoModel.Current;
    public static I18nHelper I18nData => I18nHelper.Current;
    #region Value
    public PetModel CurrentPet { get; set; }
    public WorkModel OldWork { get; set; }
    public ObservableValue<WorkModel> Work { get; } = new(new());
    #endregion
    public ObservableValue<double> BorderLength { get; } = new(250);
    public ObservableValue<double> LengthRatio { get; } = new(250.0 / 500.0);
    #region Command
    public ObservableCommand AddImageCommand { get; } = new();
    public ObservableCommand ChangeImageCommand { get; } = new();

    public ObservableCommand FixOverLoadCommand { get; } = new();
    #endregion
    public WorkEditWindowVM()
    {
        AddImageCommand.ExecuteCommand += AddImage;
        ChangeImageCommand.ExecuteCommand += ChangeImage;
        FixOverLoadCommand.ExecuteCommand += FixOverLoadCommand_ExecuteCommand;
    }

    private void FixOverLoadCommand_ExecuteCommand()
    {
        var work = Work.Value.ToWork();
        work.FixOverLoad();
        Work.Value = new(work);
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
            Work.Value.Image.Value = NativeUtils.LoadImageToMemoryStream(openFileDialog.FileName);
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
            Work.Value.Image.Value?.CloseStream();
            Work.Value.Image.Value = NativeUtils.LoadImageToMemoryStream(openFileDialog.FileName);
        }
    }
}
