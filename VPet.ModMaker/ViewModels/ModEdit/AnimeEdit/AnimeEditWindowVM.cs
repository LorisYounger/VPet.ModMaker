using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using HKW.HKWReactiveUI;
using HKW.HKWUtils.Observable;
using LinePutScript.Localization.WPF;
using Microsoft.Win32;
using VPet.ModMaker.Models;
using VPet.ModMaker.Models.ModModel;
using VPet_Simulator.Core;
using static VPet_Simulator.Core.IGameSave;

namespace VPet.ModMaker.ViewModels.ModEdit;

public partial class AnimeEditWindowVM : ViewModelBase
{
    public AnimeEditWindowVM()
    {
        _playerTask = new(Play);
    }

    /// <summary>
    /// 当前宠物
    /// </summary>
    public PetModel CurrentPet { get; set; } = null!;

    /// <summary>
    /// 旧动画
    /// </summary>
    public AnimeTypeModel? OldAnime { get; set; }

    /// <summary>
    /// 动画
    /// </summary>
    [ReactiveProperty]
    public AnimeTypeModel Anime { get; set; } = null!;

    /// <summary>
    /// 当前图像模型
    /// </summary>
    [ReactiveProperty]
    public ImageModel CurrentImageModel { get; set; } = null!;

    /// <summary>
    /// 当前动画模型
    /// </summary>
    [ReactiveProperty]
    public AnimeModel CurrentAnimeModel { get; set; } = null!;

    partial void OnCurrentAnimeModelChanged(AnimeModel oldValue, AnimeModel newValue)
    {
        Stop();
        if (oldValue is not null)
            oldValue.Images.CollectionChanged -= Images_CollectionChanged;
        if (newValue is not null)
            newValue.Images.CollectionChanged += Images_CollectionChanged;
    }

    /// <summary>
    /// 当前模式
    /// </summary>
    public ModeType CurrentMode { get; set; }

    /// <summary>
    /// 循环
    /// </summary>
    [ReactiveProperty]
    public bool Loop { get; set; }

    /// <summary>
    /// 含有多个状态
    /// </summary>
    [NotifyPropertyChangeFrom(nameof(Anime))]
    public bool HasMultiType => AnimeTypeModel.HasMultiTypeAnimes.Contains(Anime.GraphType);

    /// <summary>
    /// 含有动画名称
    /// </summary>
    [NotifyPropertyChangeFrom(nameof(Anime))]
    public bool HasAnimeName => AnimeTypeModel.HasNameAnimes.Contains(Anime.GraphType);

    /// <summary>
    /// 正在播放
    /// </summary>
    private bool _playing = false;

    /// <summary>
    /// 动画任务
    /// </summary>
    private Task _playerTask;

    #region AnimeCommand
    /// <summary>
    /// 添加动画
    /// </summary>
    /// <param name="value">动画模型</param>
    [ReactiveCommand]
    private void AddAnime()
    {
        if (CurrentMode is ModeType.Happy)
            Anime.HappyAnimes.Add(new());
        else if (CurrentMode is ModeType.Nomal)
            Anime.NomalAnimes.Add(new());
        else if (CurrentMode is ModeType.PoorCondition)
            Anime.PoorConditionAnimes.Add(new());
        else if (CurrentMode is ModeType.Ill)
            Anime.IllAnimes.Add(new());
    }

    /// <summary>
    /// 删除动画
    /// </summary>
    /// <param name="value">动画模型</param>
    [ReactiveCommand]
    private void RemoveAnime(AnimeModel value)
    {
        if (
            MessageBox.Show("确定删除吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.Yes
        )
        {
            if (CurrentMode is ModeType.Happy)
                Anime.HappyAnimes.Remove(value);
            else if (CurrentMode is ModeType.Nomal)
                Anime.NomalAnimes.Remove(value);
            else if (CurrentMode is ModeType.PoorCondition)
                Anime.PoorConditionAnimes.Remove(value);
            else if (CurrentMode is ModeType.Ill)
                Anime.IllAnimes.Remove(value);
            value.Close();
        }
    }
    #endregion

    #region ImageCommand

    /// <summary>
    /// 添加图片
    /// </summary>
    /// <param name="value">动画模型</param>
    [ReactiveCommand]
    private void AddImage(AnimeModel value)
    {
        OpenFileDialog openFileDialog =
            new()
            {
                Title = "选择图片".Translate(),
                Filter = $"图片|*.png".Translate(),
                Multiselect = true
            };
        if (openFileDialog.ShowDialog() is not true)
            return;
        AddImages(value.Images, openFileDialog.FileNames);
    }

    /// <summary>
    /// 删除图片
    /// </summary>
    /// <param name="value">动画模型</param>
    [ReactiveCommand]
    private void RemoveImage(AnimeModel value)
    {
        CurrentImageModel.Close();
        value.Images.Remove(CurrentImageModel);
    }

    /// <summary>
    /// 替换图片
    /// </summary>
    /// <param name="value"></param>
    /// <exception cref="NotImplementedException"></exception>
    [ReactiveCommand]
    private void ChangeImage(AnimeModel value)
    {
        OpenFileDialog openFileDialog =
            new() { Title = "选择图片".Translate(), Filter = $"图片|*.png".Translate() };
        if (openFileDialog.ShowDialog() is not true)
            return;
        BitmapImage newImage;
        try
        {
            newImage = NativeUtils.LoadImageToMemoryStream(openFileDialog.FileName);
        }
        catch (Exception ex)
        {
            MessageBox.Show("替换失败失败 \n{0}".Translate(ex));
            return;
        }
        if (newImage is null)
            return;
        CurrentImageModel.Close();
        CurrentImageModel.Image = newImage;
    }

    /// <summary>
    /// 清空图片
    /// </summary>
    /// <param name="value">动画模型</param>
    [ReactiveCommand]
    private void ClearImage(AnimeModel value)
    {
        if (
            MessageBox.Show("确定清空吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.Yes
        )
        {
            value.Close();
            value.Images.Clear();
        }
    }

    /// <summary>
    /// 添加图片
    /// </summary>
    /// <param name="images">动画</param>
    /// <param name="paths">路径</param>
    public void AddImages(ObservableList<ImageModel> images, IEnumerable<string> paths)
    {
        try
        {
            var newImages = new List<ImageModel>();
            foreach (string path in paths)
            {
                if (File.Exists(path))
                {
                    newImages.Add(new(NativeUtils.LoadImageToMemoryStream(path)));
                }
                else if (Directory.Exists(path))
                {
                    foreach (var file in Directory.EnumerateFiles(path, "*.png"))
                    {
                        newImages.Add(new(NativeUtils.LoadImageToMemoryStream(path)));
                    }
                }
            }
            foreach (var image in newImages)
                images.Add(image);
        }
        catch (Exception ex)
        {
            MessageBox.Show("添加失败 \n{0}".Translate(ex));
        }
    }
    #endregion
    #region Player

    private void Images_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Stop();
    }

    /// <summary>
    /// 停止播放
    /// </summary>
    [ReactiveCommand]
    private void Stop()
    {
        _playing = false;
    }

    /// <summary>
    /// 开始播放
    /// </summary>
    [ReactiveCommand]
    private async Task Start()
    {
        if (CurrentAnimeModel is null)
        {
            MessageBox.Show("未选中动画".Translate());
            return;
        }
        _playing = true;
        _playerTask.Start();
        await _playerTask;
        Reset();
    }

    /// <summary>
    /// 播放
    /// </summary>
    private void Play()
    {
        do
        {
            foreach (var model in CurrentAnimeModel.Images)
            {
                CurrentImageModel = model;
                Task.Delay(model.Duration).Wait();
                if (_playing is false)
                    return;
            }
        } while (Loop);
    }

    /// <summary>
    /// 重置
    /// </summary>
    private void Reset()
    {
        _playing = false;
        _playerTask = new(Play);
    }
    #endregion
}
