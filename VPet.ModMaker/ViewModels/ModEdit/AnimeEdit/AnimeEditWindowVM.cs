using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPet.ModMaker.Models;
using VPet.ModMaker.Models.ModModel;

namespace VPet.ModMaker.ViewModels.ModEdit.AnimeEdit;

public class AnimeEditWindowVM
{
    public AnimeModel OldAnime { get; set; }
    public ObservableValue<AnimeModel> Anime { get; } = new(new());

    public ObservableValue<ImageModel> CurrentImageModel { get; } = new();
    #region Command
    public ObservableCommand PlayCommand { get; } = new();
    public ObservableCommand PauseCommand { get; } = new();
    #endregion

    public bool _pause = false;
    public Task _playerTask;

    public AnimeEditWindowVM()
    {
        //foreach (
        //    var file in Directory.EnumerateFiles(
        //        @"C:\Users\HKW\Desktop\TestPicture\0000_core\pet\vup\Default\Happy\1"
        //    )
        //)
        //{
        //    Anime.Value.MultiImageModels.Add(new(Utils.LoadImageToMemoryStream(file)));
        //}
        _playerTask = new(Play);
        PlayCommand.ExecuteEvent += PlayCommand_ExecuteEvent;
        PauseCommand.ExecuteEvent += PauseCommand_ExecuteEvent;
    }

    private void PauseCommand_ExecuteEvent()
    {
        _pause = true;
    }

    private void PlayCommand_ExecuteEvent()
    {
        _playerTask.Start();
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
