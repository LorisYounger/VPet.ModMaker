using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using HKW.HKWReactiveUI;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using LinePutScript.Localization.WPF;
using Microsoft.Win32;
using VPet.ModMaker.Models;
using VPet.ModMaker.Models.ModModel;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.ViewModels.ModEdit;

public partial class WorkEditWindowVM : ViewModelBase
{
    public WorkEditWindowVM()
    {
        //PropertyChangedX += WorkEditWindowVM_PropertyChangedX;
        //Work.PropertyChanged += NewWork_PropertyChanged;
    }

    /// <summary>
    /// I18n资源
    /// </summary>
    public static I18nResource<string, string> I18nResource => ModInfoModel.Current.I18nResource;
    #region Property
    public PetModel CurrentPet { get; set; } = null!;
    public WorkModel? OldWork { get; set; }

    [ReactiveProperty]
    public WorkModel Work { get; set; } =
        new() { I18nResource = ModInfoModel.Current.I18nResource };

    partial void OnWorkChanged(WorkModel oldValue, WorkModel newValue)
    {
        if (oldValue is not null)
        {
            oldValue.PropertyChanged -= Work_PropertyChanged;
        }
        if (newValue is not null)
        {
            newValue.PropertyChanged -= Work_PropertyChanged;
            newValue.PropertyChanged += Work_PropertyChanged;
            SetGraphImage(newValue);
        }
    }

    private void Work_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not WorkModel workModel)
            return;
        if (e.PropertyName == nameof(WorkModel.Graph))
        {
            SetGraphImage(workModel);
        }
    }

    [ReactiveProperty]
    public double BorderLength { get; set; } = 250;

    [ReactiveProperty]
    public double LengthRatio { get; set; } = 250 / 500;

    /// <summary>
    /// 图片
    /// </summary>
    [ReactiveProperty]
    public BitmapImage? Image { get; set; }
    #endregion

    /// <summary>
    /// 修复超模
    /// </summary>
    [ReactiveCommand]
    private void FixOverLoad()
    {
        Work.FixOverLoad();
    }

    /// <summary>
    /// 添加图片
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
            Image = NativeUtils.LoadImageToMemoryStream(openFileDialog.FileName);
        }
    }

    /// <summary>
    /// 改变图片
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
            Image?.CloseStream();
            Image = NativeUtils.LoadImageToMemoryStream(openFileDialog.FileName);
        }
    }

    /// <summary>
    /// 设置图片
    /// </summary>
    /// <param name="workModel">工作模型</param>
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

    public void Close()
    {
        Image?.CloseStream();
    }
}
