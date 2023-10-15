using HKW.HKWViewModels.SimpleObservable;
using LinePutScript.Localization.WPF;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// 食物图片
    /// </summary>
    public ObservableValue<BitmapImage> FoodImage { get; } = new();

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
    /// 添加图片命令
    /// </summary>
    public ObservableCommand<AnimeModel> AddImageCommand { get; } = new();

    /// <summary>
    /// 清除图片命令
    /// </summary>
    public ObservableCommand<AnimeModel> ClearImageCommand { get; } = new();

    /// <summary>
    /// 删除动画命令
    /// </summary>
    public ObservableCommand<AnimeModel> RemoveAnimeCommand { get; } = new();

    /// <summary>
    /// 删除图片命令
    /// </summary>
    public ObservableCommand<AnimeModel> RemoveImageCommand { get; } = new();
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

        PlayCommand.ExecuteEvent += PlayCommand_ExecuteEvent;
        StopCommand.ExecuteEvent += StopCommand_ExecuteEvent;
        AddImageCommand.ExecuteEvent += AddImageCommand_ExecuteEvent;
        ClearImageCommand.ExecuteEvent += ClearImageCommand_ExecuteEvent;
        RemoveAnimeCommand.ExecuteEvent += RemoveAnimeCommand_ExecuteEvent;
        RemoveImageCommand.ExecuteEvent += RemoveImageCommand_ExecuteEvent;

        Anime.ValueChanged += Anime_ValueChanged;
    }

    #region LoadAnime
    private void Anime_ValueChanged(FoodAnimeTypeModel oldValue, FoodAnimeTypeModel newValue)
    {
        CheckGraphType(newValue);
    }

    private void CheckGraphType(FoodAnimeTypeModel model)
    {
        //if (FoodAnimeTypeModel.HasMultiTypeAnimes.Contains(model.GraphType.Value))
        //    HasMultiType.Value = true;

        //if (FoodAnimeTypeModel.HasNameAnimes.Contains(model.GraphType.Value))
        //    HasAnimeName.Value = true;
    }
    #endregion
    /// <summary>
    /// 删除动画
    /// </summary>
    /// <param name="value">动画模型</param>

    private void RemoveAnimeCommand_ExecuteEvent(AnimeModel value)
    {
        if (
            MessageBox.Show("确定删除吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.Yes
        )
        {
            //if (CurrentMode is GameSave.ModeType.Happy)
            //    Anime.Value.HappyAnimes.Remove(value);
            //else if (CurrentMode is GameSave.ModeType.Nomal)
            //    Anime.Value.NomalAnimes.Remove(value);
            //else if (CurrentMode is GameSave.ModeType.PoorCondition)
            //    Anime.Value.PoorConditionAnimes.Remove(value);
            //else if (CurrentMode is GameSave.ModeType.Ill)
            //    Anime.Value.IllAnimes.Remove(value);
            value.Close();
        }
    }

    /// <summary>
    /// 清空图片
    /// </summary>
    /// <param name="value">动画模型</param>
    private void ClearImageCommand_ExecuteEvent(AnimeModel value)
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
    /// <param name="value">动画模型</param>
    private void AddImageCommand_ExecuteEvent(AnimeModel value)
    {
        OpenFileDialog openFileDialog =
            new() { Title = "选择图片".Translate(), Filter = $"图片|*.png".Translate() };
        if (openFileDialog.ShowDialog() is true)
        {
            value.Images.Add(new(Utils.LoadImageToMemoryStream(openFileDialog.FileName)));
        }
    }

    /// <summary>
    /// 添加图片
    /// </summary>
    /// <param name="model">动画模型</param>
    /// <param name="paths">路径</param>
    public void AddImages(AnimeModel model, IEnumerable<string> paths)
    {
        try
        {
            foreach (string path in paths)
            {
                if (File.Exists(path))
                {
                    model.Images.Add(new(Utils.LoadImageToMemoryStream(path)));
                }
                else if (Directory.Exists(path))
                {
                    foreach (var file in Directory.EnumerateFiles(path, "*.png"))
                    {
                        model.Images.Add(new(Utils.LoadImageToMemoryStream(path)));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show("添加失败 \n{0}".Translate(ex));
        }
    }

    /// <summary>
    /// 删除图片
    /// </summary>
    /// <param name="value">动画模型</param>
    private void RemoveImageCommand_ExecuteEvent(AnimeModel value)
    {
        CurrentFrontImageModel.Value.Close();
        value.Images.Remove(CurrentFrontImageModel.Value);
    }

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
        Reset();
    }

    /// <summary>
    /// 开始播放
    /// </summary>
    private void PlayCommand_ExecuteEvent()
    {
        if (_playing)
        {
            MessageBox.Show("正在播放".Translate());
            return;
        }
        if (CurrentAnimeModel.Value is null)
        {
            MessageBox.Show("未选中动画".Translate());
            return;
        }
        _playing = true;
        _frontPlayerTask.Start();
        _backPlayerTask.Start();
        _foodPlayerTask.Start();
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
