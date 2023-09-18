using HKW.HKWViewModels.SimpleObservable;
using LinePutScript.Localization.WPF;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VPet.ModMaker.Models;
using VPet.ModMaker.Models.ModModel;
using VPet_Simulator.Core;

namespace VPet.ModMaker.ViewModels.ModEdit.AnimeEdit;

public class AnimeEditWindowVM
{
    public PetModel CurrentPet { get; set; }
    public AnimeTypeModel OldAnime { get; set; }
    public ObservableValue<AnimeTypeModel> Anime { get; } = new(new());
    public ObservableValue<ImageModel> CurrentImageModel { get; } = new();
    public GameSave.ModeType CurrentMode { get; set; }
    #region Command
    public ObservableCommand PlayCommand { get; } = new();
    public ObservableCommand PauseCommand { get; } = new();

    public ObservableCommand<AnimeModel> AddImageCommand { get; } = new();
    public ObservableCommand<AnimeModel> ClearImageCommand { get; } = new();
    public ObservableCommand<AnimeModel> RemoveAnimeCommand { get; } = new();
    public ObservableCommand<AnimeModel> RemoveImageCommand { get; } = new();
    #endregion

    public bool _pause = false;
    public Task _playerTask;

    public AnimeEditWindowVM()
    {
        //_playerTask = new(Play);
        //PlayCommand.ExecuteEvent += PlayCommand_ExecuteEvent;
        //PauseCommand.ExecuteEvent += PauseCommand_ExecuteEvent;
        AddImageCommand.ExecuteEvent += AddImageCommand_ExecuteEvent;
        ClearImageCommand.ExecuteEvent += ClearImageCommand_ExecuteEvent;
        RemoveAnimeCommand.ExecuteEvent += RemoveAnimeCommand_ExecuteEvent;
        RemoveImageCommand.ExecuteEvent += RemoveImageCommand_ExecuteEvent;
    }

    private void RemoveImageCommand_ExecuteEvent(AnimeModel value)
    {
        value.Images.Remove(CurrentImageModel.Value);
    }

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
        }
    }

    private void ClearImageCommand_ExecuteEvent(AnimeModel value)
    {
        if (
            MessageBox.Show("确定清空吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.Yes
        )
            value.Images.Clear();
    }

    private void AddImageCommand_ExecuteEvent(AnimeModel value)
    {
        OpenFileDialog openFileDialog =
            new()
            {
                Title = "选择图片".Translate(),
                Filter = $"图片|*.jpg;*.jpeg;*.png;*.bmp".Translate()
            };
        if (openFileDialog.ShowDialog() is true)
        {
            value.Images.Add(new(Utils.LoadImageToStream(openFileDialog.FileName)));
        }
    }

    private void PauseCommand_ExecuteEvent()
    {
        //_pause = true;
    }

    private void PlayCommand_ExecuteEvent()
    {
        //_playerTask.Start();
    }

    private void Play()
    {
        //while (_pause is false)
        //{
        //    foreach (var model in Anime.Value.ImageModels)
        //    {
        //        Anime.Value.CurrentImageModel.Value = model;
        //        Task.Delay(model.Duration.Value).Wait();
        //    }
        //}
    }
}
