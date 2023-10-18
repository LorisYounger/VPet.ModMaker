using HKW.HKWViewModels.SimpleObservable;
using LinePutScript.Localization.WPF;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using VPet.ModMaker.Models;
using VPet.ModMaker.Models.ModModel;
using VPet_Simulator.Core;

namespace VPet.ModMaker.ViewModels.ModEdit.AnimeEdit;

public class AnimeEditWindowVM
{
    /// <summary>
    /// 当前宠物
    /// </summary>
    public PetModel CurrentPet { get; set; }

    /// <summary>
    /// 旧动画
    /// </summary>
    public AnimeTypeModel OldAnime { get; set; }

    /// <summary>
    /// 动画
    /// </summary>
    public ObservableValue<AnimeTypeModel> Anime { get; } = new(new());

    /// <summary>
    /// 当前图像模型
    /// </summary>
    public ObservableValue<ImageModel> CurrentImageModel { get; } = new();

    /// <summary>
    /// 当前动画模型
    /// </summary>
    public ObservableValue<AnimeModel> CurrentAnimeModel { get; } = new();

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

    public AnimeEditWindowVM()
    {
        _playerTask = new(Play);

        CurrentAnimeModel.ValueChanged += CurrentAnimeModel_ValueChanged;

        PlayCommand.AsyncExecuteEvent += PlayCommand_AsyncExecuteEvent;
        StopCommand.ExecuteEvent += StopCommand_ExecuteEvent;
        AddAnimeCommand.ExecuteEvent += AddAnimeCommand_ExecuteEvent;
        RemoveAnimeCommand.ExecuteEvent += RemoveAnimeCommand_ExecuteEvent;
        AddImageCommand.ExecuteEvent += AddImageCommand_ExecuteEvent;
        RemoveImageCommand.ExecuteEvent += RemoveImageCommand_ExecuteEvent;
        ChangeImageCommand.ExecuteEvent += ChangeImageCommand_ExecuteEvent;
        ClearImageCommand.ExecuteEvent += ClearImageCommand_ExecuteEvent;

        Anime.ValueChanged += Anime_ValueChanged;
    }

    #region LoadAnime
    private void Anime_ValueChanged(AnimeTypeModel oldValue, AnimeTypeModel newValue)
    {
        CheckGraphType(newValue);
    }

    private void CheckGraphType(AnimeTypeModel model)
    {
        if (AnimeTypeModel.HasMultiTypeAnimes.Contains(model.GraphType.Value))
            HasMultiType.Value = true;

        if (AnimeTypeModel.HasNameAnimes.Contains(model.GraphType.Value))
            HasAnimeName.Value = true;
    }
    #endregion

    #region AnimeCommand
    /// <summary>
    /// 添加动画
    /// </summary>
    /// <param name="value">动画模型</param>
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

    private void RemoveAnimeCommand_ExecuteEvent(AnimeModel value)
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

    /// <summary>
    /// 添加图片
    /// </summary>
    /// <param name="value">动画模型</param>
    private void AddImageCommand_ExecuteEvent(AnimeModel value)
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
    private void RemoveImageCommand_ExecuteEvent(AnimeModel value)
    {
        value.Images.Remove(CurrentImageModel.Value);
        CurrentImageModel.Value.Close();
    }

    /// <summary>
    /// 替换图片
    /// </summary>
    /// <param name="value"></param>
    /// <exception cref="NotImplementedException"></exception>
    private void ChangeImageCommand_ExecuteEvent(AnimeModel value)
    {
        OpenFileDialog openFileDialog =
            new() { Title = "选择图片".Translate(), Filter = $"图片|*.png".Translate() };
        if (openFileDialog.ShowDialog() is not true)
            return;
        BitmapImage newImage;
        try
        {
            newImage = Utils.LoadImageToMemoryStream(openFileDialog.FileName);
        }
        catch (Exception ex)
        {
            MessageBox.Show("替换失败失败 \n{0}".Translate(ex));
            return;
        }
        if (newImage is null)
            return;
        CurrentImageModel.Value.Close();
        CurrentImageModel.Value.Image.Value = newImage;
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
    #region Player
    private void CurrentAnimeModel_ValueChanged(AnimeModel oldValue, AnimeModel newValue)
    {
        StopCommand_ExecuteEvent();
        if (oldValue is not null)
            oldValue.Images.CollectionChanged -= Images_CollectionChanged;
        if (newValue is not null)
            newValue.Images.CollectionChanged += Images_CollectionChanged;
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
        _playerTask.Start();
        await Task.WhenAll(_playerTask);
        Reset();
    }

    /// <summary>
    /// 播放
    /// </summary>
    private void Play()
    {
        do
        {
            foreach (var model in CurrentAnimeModel.Value.Images)
            {
                CurrentImageModel.Value = model;
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
        _playerTask = new(Play);
    }
    #endregion
}
