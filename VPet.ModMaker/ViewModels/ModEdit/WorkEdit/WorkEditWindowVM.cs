using System;
using System.Collections.Generic;
using System.Diagnostics;
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

public class WorkEditWindowVM : ObservableObjectX<WorkEditWindowVM>
{
    public static ModInfoModel ModInfo => ModInfoModel.Current;
    public static I18nHelper I18nData => I18nHelper.Current;
    #region Value
    public PetModel CurrentPet { get; set; }
    public WorkModel OldWork { get; set; }

    #region Work
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private WorkModel _work;

    public WorkModel Work
    {
        get => _work;
        set => SetProperty(ref _work, value);
    }
    #endregion
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
    private double _lengthRatio = 250 / 500;

    public double LengthRatio
    {
        get => _lengthRatio;
        set => SetProperty(ref _lengthRatio, value);
    }
    #endregion
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
        //var work = Work.ToWork();
        //work.FixOverLoad();
        //Work = new(work);
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
            Work.Image = NativeUtils.LoadImageToMemoryStream(openFileDialog.FileName);
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
            Work.Image?.CloseStream();
            Work.Image = NativeUtils.LoadImageToMemoryStream(openFileDialog.FileName);
        }
    }
}
