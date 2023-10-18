using HKW.HKWViewModels.SimpleObservable;
using LinePutScript.Localization.WPF;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using VPet.ModMaker.Models;
using VPet.ModMaker.Models.ModModel;
using VPet_Simulator.Core;

namespace VPet.ModMaker.ViewModels.ModEdit.AnimeEdit;

public class FoodAnimeEditWindowVM
{
    /// <summary>
    /// 当前宠物
    /// </summary>
    public PetModel CurrentPet { get; set; }

    /// <summary>
    /// 默认食物图片
    /// </summary>
    public static BitmapImage DefaultFoodImage { get; } =
        Utils.LoadImageToMemoryStream(
            "C:\\Users\\HKW\\Desktop\\TestPicture\\0000_core\\image\\food.png"
        );

    /// <summary>
    /// 食物图片
    /// </summary>
    public ObservableValue<BitmapImage> FoodImage { get; } = new(DefaultFoodImage);

    /// <summary>
    /// 比例
    /// </summary>
    public ObservableValue<double> LengthRatio { get; } = new(250.0 / 500.0);

    /// <summary>
    /// 旧动画
    /// </summary>
    public FoodAnimeTypeModel OldAnime { get; set; }

    /// <summary>
    /// 动画
    /// </summary>
    public ObservableValue<FoodAnimeTypeModel> Anime { get; } = new(new());

    /// <summary>
    /// 当前顶层图像模型
    /// </summary>
    public ObservableValue<ImageModel> CurrentFrontImageModel { get; } = new();

    /// <summary>
    /// 当前底层图像模型
    /// </summary>
    public ObservableValue<ImageModel> CurrentBackImageModel { get; } = new();

    /// <summary>
    /// 当前食物定位模型
    /// </summary>
    public ObservableValue<FoodLocationModel> CurrentFoodLocationModel { get; } = new();

    /// <summary>
    /// 当前动画模型
    /// </summary>
    public ObservableValue<FoodAnimeModel> CurrentAnimeModel { get; } = new();

    /// <summary>
    /// 当前模式
    /// </summary>
    public GameSave.ModeType CurrentMode { get; set; }

    /// <summary>
    /// 循环
    /// </summary>
    public ObservableValue<bool> Loop { get; } = new();

    /// <summary>
    /// 含有多个状态 参见 <see cref="AnimeTypeModel.HasMultiTypeAnimes"/>
    /// </summary>
    public ObservableValue<bool> HasMultiType { get; } = new(false);

    /// <summary>
    /// 含有动画名称 参见 <see cref="AnimeTypeModel.HasNameAnimes"/>
    /// </summary>
    public ObservableValue<bool> HasAnimeName { get; } = new(false);

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
    #endregion
    #region FoodLocation
    /// <summary>
    /// 添加食物定位命令
    /// </summary>
    public ObservableCommand<FoodAnimeModel> AddeFoodLocationCommand { get; } = new();

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

        CurrentAnimeModel.ValueChanged += CurrentAnimeModel_ValueChanged;

        PlayCommand.AsyncExecuteEvent += PlayCommand_AsyncExecuteEvent;
        StopCommand.ExecuteEvent += StopCommand_ExecuteEvent;
        ReplaceFoodImageCommand.ExecuteEvent += ChangeFoodImageCommand_ExecuteEvent;
        ResetFoodImageCommand.ExecuteEvent += ResetFoodImageCommand_ExecuteEvent;

        AddAnimeCommand.ExecuteEvent += AddAnimeCommand_ExecuteEvent;
        RemoveAnimeCommand.ExecuteEvent += RemoveAnimeCommand_ExecuteEvent;

        AddFrontImageCommand.ExecuteEvent += AddFrontImageCommand_ExecuteEvent;
        RemoveFrontImageCommand.ExecuteEvent += RemoveFrontImageCommand_ExecuteEvent;
        ClearFrontImageCommand.ExecuteEvent += ClearFrontImageCommand_ExecuteEvent;

        AddBackImageCommand.ExecuteEvent += AddBackImageCommand_ExecuteEvent;
        RemoveBackImageCommand.ExecuteEvent += RemoveBackImageCommand_ExecuteEvent;
        ClearBackImageCommand.ExecuteEvent += ClearBackImageCommand_ExecuteEvent;

        AddeFoodLocationCommand.ExecuteEvent += AddeFoodLocationCommand_ExecuteEvent;
        RemoveFoodLocationCommand.ExecuteEvent += RemoveFoodLocationCommand_ExecuteEvent;
        ClearFoodLocationCommand.ExecuteEvent += ClearFoodLocationCommand_ExecuteEvent;
    }

    private void ResetFoodImageCommand_ExecuteEvent()
    {
        if (FoodImage.Value != DefaultFoodImage)
            FoodImage.Value.CloseStream();
        FoodImage.Value = DefaultFoodImage;
    }

    private void ChangeFoodImageCommand_ExecuteEvent()
    {
        OpenFileDialog openFileDialog =
            new() { Title = "选择食物图片".Translate(), Filter = $"图片|*.png".Translate() };
        if (openFileDialog.ShowDialog() is true)
        {
            if (FoodImage.Value != DefaultFoodImage)
                FoodImage.Value.CloseStream();
            FoodImage.Value = Utils.LoadImageToMemoryStream(openFileDialog.FileName);
        }
    }

    //#region LoadAnime
    //private void Anime_ValueChanged(FoodAnimeTypeModel oldValue, FoodAnimeTypeModel newValue)
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
        if (CurrentMode is GameSave.ModeType.Happy)
            Anime.Value.HappyAnimes.Add(new());
        else if (CurrentMode is GameSave.ModeType.Nomal)
            Anime.Value.NomalAnimes.Add(new());
        else if (CurrentMode is GameSave.ModeType.PoorCondition)
            Anime.Value.PoorConditionAnimes.Add(new());
        else if (CurrentMode is GameSave.ModeType.Ill)
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
            if (CurrentMode is GameSave.ModeType.Happy)
                Anime.Value.HappyAnimes.Remove(value);
            else if (CurrentMode is GameSave.ModeType.Nomal)
                Anime.Value.NomalAnimes.Remove(value);
            else if (CurrentMode is GameSave.ModeType.PoorCondition)
                Anime.Value.PoorConditionAnimes.Remove(value);
            else if (CurrentMode is GameSave.ModeType.Ill)
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
        value.FrontImages.Remove(CurrentFrontImageModel.Value);
        CurrentFrontImageModel.Value.Close();
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

    private void ShowFrontImagesPathInfo(FoodImagesPath imagesPath)
    {
        MessageBox.Show(
            "此顶层动画源位于其它位置\n请去源位置修改此动画\n源位置 模式:{0} 索引:{1}".Translate(
                imagesPath.Mode,
                imagesPath.Index
            )
        );
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
        value.BackImages.Remove(CurrentBackImageModel.Value);
        CurrentBackImageModel.Value.Close();
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
                    newImages.Add(new(Utils.LoadImageToMemoryStream(path)));
                }
                else if (Directory.Exists(path))
                {
                    foreach (var file in Directory.EnumerateFiles(path, "*.png"))
                    {
                        newImages.Add(new(Utils.LoadImageToMemoryStream(path)));
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
        value.FoodLocations.Remove(CurrentFoodLocationModel.Value);
        CurrentFoodLocationModel.Value = null;
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
    private void CurrentAnimeModel_ValueChanged(FoodAnimeModel oldValue, FoodAnimeModel newValue)
    {
        StopCommand_ExecuteEvent();
        if (oldValue is not null)
        {
            oldValue.FrontImages.CollectionChanged -= Images_CollectionChanged;
            oldValue.BackImages.CollectionChanged -= Images_CollectionChanged;
            oldValue.FoodLocations.CollectionChanged -= Images_CollectionChanged;
        }
        if (newValue is not null)
        {
            newValue.FrontImages.CollectionChanged += Images_CollectionChanged;
            newValue.BackImages.CollectionChanged += Images_CollectionChanged;
            newValue.FoodLocations.CollectionChanged += Images_CollectionChanged;
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
        if (_playing is false)
            return;
    }

    /// <summary>
    /// 开始播放
    /// </summary>
    private async Task PlayCommand_AsyncExecuteEvent()
    {
        if (CurrentAnimeModel.Value is null)
        {
            MessageBox.Show("未选中动画".Translate());
            return;
        }
        _playing = true;
        _frontPlayerTask.Start();
        _backPlayerTask.Start();
        _foodPlayerTask.Start();
        await Task.WhenAll(_frontPlayerTask, _backPlayerTask, _foodPlayerTask);
        Reset();
    }

    /// <summary>
    /// 顶层播放
    /// </summary>
    private void FrontPlay()
    {
        do
        {
            foreach (var model in CurrentAnimeModel.Value.FrontImages)
            {
                CurrentFrontImageModel.Value = model;
                Task.Delay(model.Duration.Value).Wait();
                if (_playing is false)
                    return;
            }
        } while (Loop.Value);
    }

    /// <summary>
    /// 底层
    /// </summary>
    private void BackPlay()
    {
        do
        {
            foreach (var model in CurrentAnimeModel.Value.BackImages)
            {
                CurrentBackImageModel.Value = model;
                Task.Delay(model.Duration.Value).Wait();
                if (_playing is false)
                    return;
            }
        } while (Loop.Value);
    }

    /// <summary>
    /// 食物
    /// </summary>
    private void FoodPlay()
    {
        do
        {
            foreach (var model in CurrentAnimeModel.Value.FoodLocations)
            {
                CurrentFoodLocationModel.Value = model;
                Task.Delay(model.Duration.Value).Wait();
                if (_playing is false)
                    return;
            }
        } while (Loop.Value);
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
