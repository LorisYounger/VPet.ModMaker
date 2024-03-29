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
using HKW.HKWUtils.Observable;
using LinePutScript.Localization.WPF;
using Microsoft.Win32;
using VPet.ModMaker.Models;
using VPet.ModMaker.Models.ModModel;
using VPet_Simulator.Core;
using static VPet_Simulator.Core.IGameSave;

namespace VPet.ModMaker.ViewModels.ModEdit.AnimeEdit;

public class AnimeEditWindowVM : ObservableObjectX<AnimeEditWindowVM>
{
    public AnimeEditWindowVM()
    {
        _playerTask = new(Play);
        PropertyChangedX += AnimeEditWindowVM_PropertyChangedX;

        PlayCommand.ExecuteAsyncCommand += PlayCommand_ExecuteAsyncCommand;
        StopCommand.ExecuteCommand += StopCommand_ExecuteCommand;
        AddAnimeCommand.ExecuteCommand += AddAnimeCommand_ExecuteCommand;
        RemoveAnimeCommand.ExecuteCommand += RemoveAnimeCommand_ExecuteCommand;
        AddImageCommand.ExecuteCommand += AddImageCommand_ExecuteCommand;
        RemoveImageCommand.ExecuteCommand += RemoveImageCommand_ExecuteCommand;
        ChangeImageCommand.ExecuteCommand += ChangeImageCommand_ExecuteCommand;
        ClearImageCommand.ExecuteCommand += ClearImageCommand_ExecuteCommand;
    }

    private void AnimeEditWindowVM_PropertyChangedX(
        AnimeEditWindowVM sender,
        PropertyChangedXEventArgs e
    )
    {
        if (e.PropertyName == nameof(CurrentAnimeModel))
        {
            var newModel = e.NewValue as AnimeModel;
            var oldModel = e.OldValue as AnimeModel;
            StopCommand_ExecuteCommand();
            if (oldModel is not null)
                oldModel.Images.CollectionChanged -= Images_CollectionChanged;
            if (newModel is not null)
                newModel.Images.CollectionChanged += Images_CollectionChanged;
        }
    }

    /// <summary>
    /// 当前宠物
    /// </summary>
    public PetModel CurrentPet { get; set; } = null!;

    /// <summary>
    /// 旧动画
    /// </summary>
    public AnimeTypeModel? OldAnime { get; set; }

    #region Anime
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private AnimeTypeModel _anime = new();

    /// <summary>
    /// 动画
    /// </summary>
    public AnimeTypeModel Anime
    {
        get => _anime;
        set
        {
            if (SetProperty(ref _anime, value) is false)
                return;
            CheckGraphType(Anime);
        }
    }
    #endregion

    #region CurrentImageModel
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ImageModel _currentImageModel = null!;

    /// <summary>
    /// 当前图像模型
    /// </summary>
    public ImageModel CurrentImageModel
    {
        get => _currentImageModel;
        set => SetProperty(ref _currentImageModel, value);
    }
    #endregion

    #region CurrentAnimeModel
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private AnimeModel _currentAnimeModel = null!;

    /// <summary>
    /// 当前动画模型
    /// </summary>

    public AnimeModel CurrentAnimeModel
    {
        get => _currentAnimeModel;
        set => SetProperty(ref _currentAnimeModel, value);
    }
    #endregion

    /// <summary>
    /// 当前模式
    /// </summary>
    public ModeType CurrentMode { get; set; }

    #region Loop
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private bool _loop;

    /// <summary>
    /// 循环
    /// </summary>
    public bool Loop
    {
        get => _loop;
        set => SetProperty(ref _loop, value);
    }
    #endregion

    #region HasMultiType
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private bool _hasMultiType;

    /// <summary>
    /// 含有多个状态 参见 <see cref="AnimeTypeModel.HasMultiTypeAnimes"/>
    /// </summary>
    public bool HasMultiType
    {
        get => _hasMultiType;
        set => SetProperty(ref _hasMultiType, value);
    }
    #endregion

    #region HasAnimeName
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private bool _hasAnimeName;

    /// <summary>
    /// 含有动画名称 参见 <see cref="AnimeTypeModel.HasNameAnimes"/>
    /// </summary>
    public bool HasAnimeName
    {
        get => _hasAnimeName;
        set => SetProperty(ref _hasAnimeName, value);
    }
    #endregion

    #region Command
    /// <summary>
    /// 播放命令
    /// </summary>
    public ObservableCommand PlayCommand { get; } = new();

    /// <summary>
    /// 停止命令
    /// </summary>
    public ObservableCommand StopCommand { get; } = new();

    /// <summary>
    /// 添加动画命令
    /// </summary>
    public ObservableCommand AddAnimeCommand { get; } = new();

    /// <summary>
    /// 删除动画命令
    /// </summary>
    public ObservableCommand<AnimeModel> RemoveAnimeCommand { get; } = new();

    /// <summary>
    /// 添加图片命令
    /// </summary>
    public ObservableCommand<AnimeModel> AddImageCommand { get; } = new();

    /// <summary>
    /// 删除图片命令
    /// </summary>
    public ObservableCommand<AnimeModel> RemoveImageCommand { get; } = new();

    /// <summary>
    /// 替换图片命令
    /// </summary>
    public ObservableCommand<AnimeModel> ChangeImageCommand { get; } = new();

    /// <summary>
    /// 清除图片命令
    /// </summary>
    public ObservableCommand<AnimeModel> ClearImageCommand { get; } = new();
    #endregion

    /// <summary>
    /// 正在播放
    /// </summary>
    private bool _playing = false;

    /// <summary>
    /// 动画任务
    /// </summary>
    private Task _playerTask;

    #region LoadAnime

    private void CheckGraphType(AnimeTypeModel model)
    {
        if (AnimeTypeModel.HasMultiTypeAnimes.Contains(model.GraphType))
            HasMultiType = true;

        if (AnimeTypeModel.HasNameAnimes.Contains(model.GraphType))
            HasAnimeName = true;
    }
    #endregion

    #region AnimeCommand
    /// <summary>
    /// 添加动画
    /// </summary>
    /// <param name="value">动画模型</param>
    private void AddAnimeCommand_ExecuteCommand()
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

    private void RemoveAnimeCommand_ExecuteCommand(AnimeModel value)
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
    private void AddImageCommand_ExecuteCommand(AnimeModel value)
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
    private void RemoveImageCommand_ExecuteCommand(AnimeModel value)
    {
        CurrentImageModel.Close();
        value.Images.Remove(CurrentImageModel);
    }

    /// <summary>
    /// 替换图片
    /// </summary>
    /// <param name="value"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void ChangeImageCommand_ExecuteCommand(AnimeModel value)
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
    private void ClearImageCommand_ExecuteCommand(AnimeModel value)
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
        StopCommand_ExecuteCommand();
    }

    /// <summary>
    /// 停止播放
    /// </summary>
    private void StopCommand_ExecuteCommand()
    {
        _playing = false;
    }

    /// <summary>
    /// 开始播放
    /// </summary>
    private async Task PlayCommand_ExecuteAsyncCommand()
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
