using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using LinePutScript.Localization.WPF;
using Microsoft.Win32;
using VPet.ModMaker.Models;
using VPet.ModMaker.Models.ModModel;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.ViewModels.ModEdit.WorkEdit;

public class WorkEditWindowVM : ObservableObjectX<WorkEditWindowVM>
{
    public WorkEditWindowVM()
    {
        PropertyChangedX += WorkEditWindowVM_PropertyChangedX;
        Work.PropertyChanged += NewWork_PropertyChanged;
        AddImageCommand.ExecuteCommand += AddImageCommand_ExecuteCommand;
        ChangeImageCommand.ExecuteCommand += ChangeImageCommand_ExecuteCommand;
        FixOverLoadCommand.ExecuteCommand += FixOverLoadCommand_ExecuteCommand;
    }

    private void WorkEditWindowVM_PropertyChangedX(
        WorkEditWindowVM sender,
        PropertyChangedXEventArgs e
    )
    {
        if (e.PropertyName == nameof(Work))
        {
            var newWork = e.NewValue as WorkModel;
            var oldWork = e.OldValue as WorkModel;
            if (oldWork is not null)
            {
                oldWork.PropertyChanged -= NewWork_PropertyChanged;
            }
            if (newWork is not null)
            {
                newWork.PropertyChanged -= NewWork_PropertyChanged;
                newWork.PropertyChanged += NewWork_PropertyChanged;
                SetGraphImage(newWork);
            }
        }
    }

    private void NewWork_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not WorkModel workModel)
            return;
        if (e.PropertyName == nameof(WorkModel.Graph))
        {
            SetGraphImage(workModel);
        }
    }

    private void SetGraphImage(WorkModel workModel)
    {
        if (CurrentPet is null)
            return;
        var graph = workModel.Graph;
        Image?.CloseStream();
        Image = null;
        // 随机挑一张图片
        if (
            CurrentPet.Animes.FirstOrDefault(
                a =>
                    a.GraphType is VPet_Simulator.Core.GraphInfo.GraphType.Work
                    && a.Name.Equals(graph, StringComparison.OrdinalIgnoreCase),
                null!
            )
            is not AnimeTypeModel anime
        )
            return;
        if (anime.HappyAnimes.HasValue())
        {
            Image = anime.HappyAnimes.Random().Images.Random().Image.CloneStream();
        }
        else if (anime.NomalAnimes.HasValue())
        {
            Image = anime.NomalAnimes.Random().Images.Random().Image.CloneStream();
        }
        else if (anime.PoorConditionAnimes.HasValue())
        {
            Image = anime.PoorConditionAnimes.Random().Images.Random().Image.CloneStream();
        }
        else if (anime.IllAnimes.HasValue())
        {
            Image = anime.IllAnimes.Random().Images.Random().Image.CloneStream();
        }
    }

    public static ModInfoModel ModInfo => ModInfoModel.Current;
    public static I18nHelper I18nData => I18nHelper.Current;
    #region Property
    public PetModel CurrentPet { get; set; } = null!;
    public WorkModel? OldWork { get; set; }

    #region Work
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private WorkModel _work = new();

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
    #region Image
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private BitmapImage? _image;

    /// <summary>
    /// 图片
    /// </summary>
    public BitmapImage? Image
    {
        get => _image;
        set => SetProperty(ref _image, value);
    }
    #endregion
    #region Command
    public ObservableCommand AddImageCommand { get; } = new();
    public ObservableCommand ChangeImageCommand { get; } = new();

    public ObservableCommand FixOverLoadCommand { get; } = new();
    #endregion
    private void FixOverLoadCommand_ExecuteCommand()
    {
        Work.FixOverLoad();
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
            Image?.CloseStream();
            Image = NativeUtils.LoadImageToMemoryStream(openFileDialog.FileName);
        }
    }

    public void Close()
    {
        Image?.CloseStream();
    }
}
