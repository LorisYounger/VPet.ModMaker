﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using HKW.HKWUtils.Observable;
using LinePutScript.Localization.WPF;
using Microsoft.Win32;
using VPet.ModMaker.Models;
using VPet.ModMaker.Models.ModModel;
using VPet.ModMaker.Resources;
using VPet_Simulator.Core;
using static VPet_Simulator.Core.IGameSave;

namespace VPet.ModMaker.ViewModels.ModEdit.AnimeEdit;

public class FoodAnimeEditWindowVM : ObservableObjectX<FoodAnimeEditWindowVM>
{
    /// <summary>
    /// 当前宠物
    /// </summary>
    public PetModel CurrentPet { get; set; }

    /// <summary>
    /// 默认食物图片
    /// </summary>
    public static BitmapImage DefaultFoodImage { get; } =
        NativeUtils.LoadImageToMemoryStream(NativeResources.GetStream(NativeResources.FoodImage));

    #region FoodImage
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private BitmapImage _foodImage;

    /// <summary>
    /// 食物图片
    /// </summary>
    public BitmapImage FoodImage
    {
        get => _foodImage;
        set => SetProperty(ref _foodImage, value);
    }
    #endregion

    #region LengthRatio
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private double _lengthRatio;

    /// <summary>
    /// 比例
    /// </summary>
    public double LengthRatio
    {
        get => _lengthRatio;
        set => SetProperty(ref _lengthRatio, value);
    }
    #endregion

    /// <summary>
    /// 旧动画
    /// </summary>
    public FoodAnimeTypeModel OldAnime { get; set; }

    /// <summary>
    /// 动画
    /// </summary>
    public ObservableValue<FoodAnimeTypeModel> Anime { get; } = new(new());

    #region CurrentFrontImageModel
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ImageModel _currentFrontImageModel;

    /// <summary>
    /// 当前顶层图像模型
    /// </summary>
    public ImageModel CurrentFrontImageModel
    {
        get => _currentFrontImageModel;
        set => SetProperty(ref _currentFrontImageModel, value);
    }
    #endregion

    #region CurrentBackImageModel
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ImageModel _currentBackImageModel;

    /// <summary>
    /// 当前底层图像模型
    /// </summary>
    public ImageModel CurrentBackImageModel
    {
        get => _currentBackImageModel;
        set => SetProperty(ref _currentBackImageModel, value);
    }
    #endregion

    #region CurrentFoodLocationModel
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private FoodAnimeLocationModel _currentFoodLocationModel;

    /// <summary>
    /// 当前食物定位模型
    /// </summary>
    public FoodAnimeLocationModel CurrentFoodLocationModel
    {
        get => _currentFoodLocationModel;
        set => SetProperty(ref _currentFoodLocationModel, value);
    }
    #endregion

    #region CurrentAnimeModel
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private FoodAnimeModel _currentAnimeModel;

    /// <summary>
    /// 当前动画模型
    /// </summary>
    public FoodAnimeModel CurrentAnimeModel
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
    public ObservableCommand<FoodAnimeModel> RemoveAnimeCommand { get; } = new();

    #region FrontImage
    /// <summary>
    /// 添加顶层图片命令
    /// </summary>
    public ObservableCommand<FoodAnimeModel> AddFrontImageCommand { get; } = new();

    /// <summary>
    /// 删除顶层图片命令
    /// </summary>
    public ObservableCommand<FoodAnimeModel> RemoveFrontImageCommand { get; } = new();

    /// <summary>
    /// 清除顶层图片命令
    /// </summary>
    public ObservableCommand<FoodAnimeModel> ClearFrontImageCommand { get; } = new();

    /// <summary>
    /// 改变顶层图片命令
    /// </summary>
    public ObservableCommand<FoodAnimeModel> ChangeFrontImageCommand { get; } = new();
    #endregion

    #region BackImage
    /// <summary>
    /// 添加底层图片命令
    /// </summary>
    public ObservableCommand<FoodAnimeModel> AddBackImageCommand { get; } = new();

    /// <summary>
    /// 删除底层图片命令
    /// </summary>
    public ObservableCommand<FoodAnimeModel> RemoveBackImageCommand { get; } = new();

    /// <summary>
    /// 清除底层图片命令
    /// </summary>
    public ObservableCommand<FoodAnimeModel> ClearBackImageCommand { get; } = new();

    /// <summary>
    /// 改变底层图片命令
    /// </summary>
    public ObservableCommand<FoodAnimeModel> ChangeBackImageCommand { get; } = new();
    #endregion
    #region FoodLocation
    /// <summary>
    /// 添加食物定位命令
    /// </summary>
    public ObservableCommand<FoodAnimeModel> AddFoodLocationCommand { get; } = new();

    /// <summary>
    /// 删除食物定位命令
    /// </summary>
    public ObservableCommand<FoodAnimeModel> RemoveFoodLocationCommand { get; } = new();

    /// <summary>
    /// 清除食物定位命令
    /// </summary>
    public ObservableCommand<FoodAnimeModel> ClearFoodLocationCommand { get; } = new();
    #endregion
    /// <summary>
    /// 改变食物图片
    /// </summary>
    public ObservableCommand ReplaceFoodImageCommand { get; } = new();

    /// <summary>
    /// 重置食物图片
    /// </summary>
    public ObservableCommand ResetFoodImageCommand { get; } = new();

    #endregion

    /// <summary>
    /// 正在播放
    /// </summary>
    private bool _playing = false;

    /// <summary>
    /// 顶层动画任务
    /// </summary>
    private Task _frontPlayerTask;

    /// <summary>
    /// 底层动画任务
    /// </summary>
    private Task _backPlayerTask;

    /// <summary>
    /// 食物动画任务
    /// </summary>
    private Task _foodPlayerTask;

    public FoodAnimeEditWindowVM()
    {
        _frontPlayerTask = new(FrontPlay);
        _backPlayerTask = new(BackPlay);
        _foodPlayerTask = new(FoodPlay);
        //TODO
        //CurrentAnimeModel.ValueChanged += CurrentAnimeModel_ValueChanged;

        PlayCommand.ExecuteAsyncCommand += PlayCommand_AsyncExecuteEvent;
        StopCommand.ExecuteCommand += StopCommand_ExecuteEvent;
        ReplaceFoodImageCommand.ExecuteCommand += ChangeFoodImageCommand_ExecuteEvent;
        ResetFoodImageCommand.ExecuteCommand += ResetFoodImageCommand_ExecuteEvent;

        AddAnimeCommand.ExecuteCommand += AddAnimeCommand_ExecuteEvent;
        RemoveAnimeCommand.ExecuteCommand += RemoveAnimeCommand_ExecuteEvent;

        AddFrontImageCommand.ExecuteCommand += AddFrontImageCommand_ExecuteEvent;
        RemoveFrontImageCommand.ExecuteCommand += RemoveFrontImageCommand_ExecuteEvent;
        ClearFrontImageCommand.ExecuteCommand += ClearFrontImageCommand_ExecuteEvent;
        ChangeFrontImageCommand.ExecuteCommand += ChangeFrontImageCommand_ExecuteEvent;

        AddBackImageCommand.ExecuteCommand += AddBackImageCommand_ExecuteEvent;
        RemoveBackImageCommand.ExecuteCommand += RemoveBackImageCommand_ExecuteEvent;
        ClearBackImageCommand.ExecuteCommand += ClearBackImageCommand_ExecuteEvent;
        ChangeBackImageCommand.ExecuteCommand += ChangeBackImageCommand_ExecuteEvent;

        AddFoodLocationCommand.ExecuteCommand += AddeFoodLocationCommand_ExecuteEvent;
        RemoveFoodLocationCommand.ExecuteCommand += RemoveFoodLocationCommand_ExecuteEvent;
        ClearFoodLocationCommand.ExecuteCommand += ClearFoodLocationCommand_ExecuteEvent;
    }

    private void ResetFoodImageCommand_ExecuteEvent()
    {
        if (FoodImage != DefaultFoodImage)
            FoodImage.CloseStream();
        FoodImage = DefaultFoodImage;
    }

    private void ChangeFoodImageCommand_ExecuteEvent()
    {
        OpenFileDialog openFileDialog =
            new() { Title = "选择食物图片".Translate(), Filter = $"图片|*.png".Translate() };
        if (openFileDialog.ShowDialog() is true)
        {
            if (FoodImage != DefaultFoodImage)
                FoodImage.CloseStream();
            FoodImage = NativeUtils.LoadImageToMemoryStream(openFileDialog.FileName);
        }
    }

    //#region LoadAnime
    //private void Anime_ValueChanged(ObservableValue<FoodAnimeTypeModel> sender, ValueChangedEventArgs<FoodAnimeTypeModel> e)
    //{
    //    CheckGraphType(newValue);
    //}

    //private void CheckGraphType(FoodAnimeTypeModel model)
    //{
    //    //if (FoodAnimeTypeModel.HasMultiTypeAnimes.Contains(model.GraphType.Value))
    //    //    HasMultiType.Value = true;

    //    //if (FoodAnimeTypeModel.HasNameAnimes.Contains(model.GraphType.Value))
    //    //    HasAnimeName.Value = true;
    //}
    //#endregion
    #region AnimeCommand
    private void AddAnimeCommand_ExecuteEvent()
    {
        if (CurrentMode is ModeType.Happy)
            Anime.Value.HappyAnimes.Add(new());
        else if (CurrentMode is ModeType.Nomal)
            Anime.Value.NomalAnimes.Add(new());
        else if (CurrentMode is ModeType.PoorCondition)
            Anime.Value.PoorConditionAnimes.Add(new());
        else if (CurrentMode is ModeType.Ill)
            Anime.Value.IllAnimes.Add(new());
    }

    /// <summary>
    /// 删除动画
    /// </summary>
    /// <param name="value">动画模型</param>

    private void RemoveAnimeCommand_ExecuteEvent(FoodAnimeModel value)
    {
        if (
            MessageBox.Show("确定删除吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.Yes
        )
        {
            if (CurrentMode is ModeType.Happy)
                Anime.Value.HappyAnimes.Remove(value);
            else if (CurrentMode is ModeType.Nomal)
                Anime.Value.NomalAnimes.Remove(value);
            else if (CurrentMode is ModeType.PoorCondition)
                Anime.Value.PoorConditionAnimes.Remove(value);
            else if (CurrentMode is ModeType.Ill)
                Anime.Value.IllAnimes.Remove(value);
            value.Close();
        }
    }
    #endregion
    #region ImageCommand

    #region FrontImageCommand
    /// <summary>
    /// 添加顶层图片
    /// </summary>
    /// <param name="value">动画模型</param>
    private void AddFrontImageCommand_ExecuteEvent(FoodAnimeModel value)
    {
        OpenFileDialog openFileDialog =
            new()
            {
                Title = "选择图片".Translate(),
                Filter = $"图片|*.png".Translate(),
                Multiselect = true
            };
        if (openFileDialog.ShowDialog() is true)
        {
            AddImages(value.FrontImages, openFileDialog.FileNames);
        }
    }

    /// <summary>
    /// 删除顶层图片
    /// </summary>
    /// <param name="value">动画模型</param>
    private void RemoveFrontImageCommand_ExecuteEvent(FoodAnimeModel value)
    {
        CurrentFrontImageModel.Close();
        value.FrontImages.Remove(CurrentFrontImageModel);
    }

    /// <summary>
    /// 清空顶层图片
    /// </summary>
    /// <param name="value">动画模型</param>
    private void ClearFrontImageCommand_ExecuteEvent(FoodAnimeModel value)
    {
        if (
            MessageBox.Show("确定清空吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.Yes
        )
        {
            foreach (var image in value.FrontImages)
                image.Close();
            value.FrontImages.Clear();
        }
    }

    /// <summary>
    /// 替换顶层图片
    /// </summary>
    /// <param name="value"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void ChangeFrontImageCommand_ExecuteEvent(FoodAnimeModel value)
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
        CurrentFrontImageModel.Close();
        CurrentFrontImageModel.Image = newImage;
    }
    #endregion

    #region BackImageCommand
    /// <summary>
    /// 添加顶层图片
    /// </summary>
    /// <param name="value">动画模型</param>
    private void AddBackImageCommand_ExecuteEvent(FoodAnimeModel value)
    {
        OpenFileDialog openFileDialog =
            new()
            {
                Title = "选择图片".Translate(),
                Filter = $"图片|*.png".Translate(),
                Multiselect = true
            };
        if (openFileDialog.ShowDialog() is true)
        {
            AddImages(value.BackImages, openFileDialog.FileNames);
        }
    }

    /// <summary>
    /// 删除顶层图片
    /// </summary>
    /// <param name="value">动画模型</param>
    private void RemoveBackImageCommand_ExecuteEvent(FoodAnimeModel value)
    {
        CurrentBackImageModel.Close();
        value.BackImages.Remove(CurrentBackImageModel);
    }

    /// <summary>
    /// 清空顶层图片
    /// </summary>
    /// <param name="value">动画模型</param>
    private void ClearBackImageCommand_ExecuteEvent(FoodAnimeModel value)
    {
        if (
            MessageBox.Show("确定清空吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.Yes
        )
        {
            foreach (var image in value.BackImages)
                image.Close();
            value.BackImages.Clear();
        }
    }

    /// <summary>
    /// 替换底层图片
    /// </summary>
    /// <param name="value"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void ChangeBackImageCommand_ExecuteEvent(FoodAnimeModel value)
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
        CurrentBackImageModel.Close();
        CurrentBackImageModel.Image = newImage;
    }
    #endregion

    /// <summary>
    /// 添加图片
    /// </summary>
    /// <param name="images">动画</param>
    /// <param name="paths">路径</param>
    public void AddImages(ObservableCollection<ImageModel> images, IEnumerable<string> paths)
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
    #region FoodLocationCommand
    private void AddeFoodLocationCommand_ExecuteEvent(FoodAnimeModel value)
    {
        value.FoodLocations.Add(new());
    }

    private void RemoveFoodLocationCommand_ExecuteEvent(FoodAnimeModel value)
    {
        value.FoodLocations.Remove(CurrentFoodLocationModel);
        CurrentFoodLocationModel = null;
    }

    private void ClearFoodLocationCommand_ExecuteEvent(FoodAnimeModel value)
    {
        if (
            MessageBox.Show("确定清空吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.Yes
        )
        {
            value.FoodLocations.Clear();
        }
    }
    #endregion
    #region FrontPlayer
    private void CurrentAnimeModel_ValueChanged(
        ObservableValue<FoodAnimeModel> sender,
        ValueChangedEventArgs<FoodAnimeModel> e
    )
    {
        StopCommand_ExecuteEvent();
        if (e.OldValue is not null)
        {
            e.OldValue.FrontImages.CollectionChanged -= Images_CollectionChanged;
            e.OldValue.BackImages.CollectionChanged -= Images_CollectionChanged;
            e.OldValue.FoodLocations.CollectionChanged -= Images_CollectionChanged;
        }
        if (e.NewValue is not null)
        {
            e.NewValue.FrontImages.CollectionChanged += Images_CollectionChanged;
            e.NewValue.BackImages.CollectionChanged += Images_CollectionChanged;
            e.NewValue.FoodLocations.CollectionChanged += Images_CollectionChanged;
        }
    }

    private void Images_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        StopCommand_ExecuteEvent();
    }

    /// <summary>
    /// 停止播放
    /// </summary>
    private void StopCommand_ExecuteEvent()
    {
        _playing = false;
    }

    /// <summary>
    /// 开始播放
    /// </summary>
    private async Task PlayCommand_AsyncExecuteEvent()
    {
        if (CurrentAnimeModel is null)
        {
            MessageBox.Show("未选中动画".Translate());
            return;
        }
        _playing = true;
        do
        {
            _frontPlayerTask.Start();
            _backPlayerTask.Start();
            _foodPlayerTask.Start();
            await Task.WhenAll(_frontPlayerTask, _backPlayerTask, _foodPlayerTask);
            _frontPlayerTask = new(FrontPlay);
            _backPlayerTask = new(BackPlay);
            _foodPlayerTask = new(FoodPlay);
        } while (Loop && _playing);
    }

    /// <summary>
    /// 顶层播放
    /// </summary>
    private void FrontPlay()
    {
        foreach (var model in CurrentAnimeModel.FrontImages)
        {
            CurrentFrontImageModel = model;
            Task.Delay(model.Duration).Wait();
            if (_playing is false)
                return;
        }
    }

    /// <summary>
    /// 底层
    /// </summary>
    private void BackPlay()
    {
        foreach (var model in CurrentAnimeModel.BackImages)
        {
            CurrentBackImageModel = model;
            Task.Delay(model.Duration).Wait();
            if (_playing is false)
                return;
        }
    }

    /// <summary>
    /// 食物
    /// </summary>
    private void FoodPlay()
    {
        foreach (var model in CurrentAnimeModel.FoodLocations)
        {
            CurrentFoodLocationModel = model;
            Task.Delay(model.Duration).Wait();
            if (_playing is false)
                return;
        }
    }

    /// <summary>
    /// 重置
    /// </summary>
    private void Reset()
    {
        _playing = false;
        _frontPlayerTask = new(FrontPlay);
        _backPlayerTask = new(BackPlay);
        _foodPlayerTask = new(FoodPlay);
    }
    #endregion
}
