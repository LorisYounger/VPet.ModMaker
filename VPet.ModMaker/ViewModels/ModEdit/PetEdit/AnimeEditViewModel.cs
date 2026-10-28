using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using HKW.HKWUtils.Observable;
using HKW.WPF;
using HKW.WPF.Extensions;
using HKW.WPF.MVVMDialogs;
using LinePutScript.Localization.WPF;
using VPet.ModMaker.Models;
using static VPet_Simulator.Core.IGameSave;

namespace VPet.ModMaker.ViewModels;

/// <summary>
/// 动画编辑视图模型
/// </summary>
public sealed partial class AnimeEditViewModel
    : ViewModelBase,
        IModalDialogViewModel,
        IDisposable,
        IViewClosing
{
    /// <inheritdoc/>
    public AnimeEditViewModel() { }

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
    public AnimeTypeModel Anime { get; set; } = null!;

    /// <summary>
    /// 当前动画模型
    /// </summary>
    [ObservableProperty]
    public AnimeModel CurrentAnimeModel { get; set; } = null!;

    partial void OnCurrentAnimeModelChanged(AnimeModel oldValue, AnimeModel newValue)
    {
        _ = Reset();
        if (oldValue is not null)
            oldValue.Images.CollectionChanged -= Images_CollectionChanged;
        if (newValue is not null)
        {
            newValue.Images.CollectionChanged += Images_CollectionChanged;
            CurrentImageModel = newValue.Images.FirstOrDefault()!;
        }
    }

    /// <summary>
    /// 当前图像模型
    /// </summary>
    [ObservableProperty]
    public ImageModel CurrentImageModel { get; set; } = null!;

    /// <summary>
    /// 当前模式
    /// </summary>
    public ModeType CurrentMode { get; set; }

    /// <summary>
    /// 循环
    /// </summary>
    [ObservableProperty]
    public bool Loop { get; set; }

    /// <summary>
    /// 含有多个状态
    /// </summary>
    public bool HasMultiType => Anime?.GraphType.IsMultiTypeGraph() is true;

    /// <summary>
    /// 含有动画名称
    /// </summary>
    public bool HasAnimeName => Anime?.GraphType.IsNamedGraph() is true;
    public bool? DialogResult { get; set; }

    #region AnimeCommand
    /// <summary>
    /// 添加动画
    /// </summary>
    [RelayCommand]
    private void AddAnime(ModeType animeMode)
    {
        if (animeMode is ModeType.Happy)
            Anime.HappyAnimes.Add(new());
        else if (animeMode is ModeType.Nomal)
            Anime.NomalAnimes.Add(new());
        else if (animeMode is ModeType.PoorCondition)
            Anime.PoorConditionAnimes.Add(new());
        else if (animeMode is ModeType.Ill)
            Anime.IllAnimes.Add(new());
    }

    /// <summary>
    /// 删除动画
    /// </summary>
    /// <param name="value">动画模型</param>
    [RelayCommand]
    private void RemoveAnime(AnimeModel value)
    {
        var result = DialogService.ShowMessageBox(
            this,
            "确认删除动画 '{0}'".Translate(value.ID),
            "删除动画".Translate(),
            MessageBoxButton.YesNo
        );
        if (result is not true)
            return;
        if (CurrentMode is ModeType.Happy)
            Anime.HappyAnimes.Remove(value);
        else if (CurrentMode is ModeType.Nomal)
            Anime.NomalAnimes.Remove(value);
        else if (CurrentMode is ModeType.PoorCondition)
            Anime.PoorConditionAnimes.Remove(value);
        else if (CurrentMode is ModeType.Ill)
            Anime.IllAnimes.Remove(value);
        value.Dispose();
    }
    #endregion

    #region ImageCommand

    /// <summary>
    /// 添加图片
    /// </summary>
    /// <param name="value">动画模型</param>
    [RelayCommand]
    private void AddImage(AnimeModel value)
    {
        var openFilesDialog = DialogService.ShowOpenFilesDialog(
            this,
            new()
            {
                Title = "选择图片".Translate(),
                Filters = [new("图片".Translate(), ModMaker.PngExtensions)],
            }
        );
        if (openFilesDialog is null)
            return;
        AddImages(value.Images, openFilesDialog.Select(x => x.LocalPath));
    }

    /// <summary>
    /// 删除图片
    /// </summary>
    /// <param name="value">动画模型</param>
    [RelayCommand]
    private void RemoveImage(ImageModel value)
    {
        value.Dispose();
        CurrentAnimeModel.Images.Remove(value);
    }

    /// <summary>
    /// 替换图片
    /// </summary>
    /// <param name="value"></param>
    [RelayCommand]
    private void ChangeImage(ImageModel value)
    {
        var openFileDialog = DialogService.ShowOpenFileDialog(
            this,
            new()
            {
                Title = "选择图片".Translate(),
                Filters = [new("图片".Translate(), ModMaker.PngExtensions)],
            }
        );
        if (openFileDialog is null)
            return;
        var newImage = HKWImageUtils.LoadImageToMemory(openFileDialog.LocalPath);
        if (newImage is null)
        {
            DialogService.ShowMessageBox(
                this,
                "图片载入失败, 详情请查看日志".Translate(),
                "图片载入失败".Translate(),
                icon: MessageBoxImage.Warning
            );
            return;
        }
        value.Image.CloseStream();
        value.Image = newImage;
    }

    /// <summary>
    /// 清空图片
    /// </summary>
    /// <param name="value">动画模型</param>
    [RelayCommand]
    private void ClearImage(AnimeModel value)
    {
        var result = DialogService.ShowMessageBox(
            this,
            "确定清空图像吗".Translate(),
            "清空图像".Translate(),
            MessageBoxButton.YesNo
        );
        if (result is not true)
            return;
        value.Dispose();
    }

    /// <summary>
    /// 添加图片
    /// </summary>
    /// <param name="images">动画</param>
    /// <param name="paths">路径</param>
    public void AddImages(ObservableList<ImageModel> images, IEnumerable<string> paths)
    {
        var failCount = 0;
        foreach (string path in paths)
        {
            if (File.Exists(path))
            {
                var image = HKWImageUtils.LoadImageToMemory(path);
                if (image is null)
                    failCount++;
                else
                    images.Add(new(path, image));
            }
            else if (Directory.Exists(path))
            {
                foreach (var file in Directory.EnumerateFiles(path, "*.png"))
                {
                    var image = HKWImageUtils.LoadImageToMemory(file);
                    if (image is null)
                        failCount++;
                    else
                        images.Add(new(path, image));
                }
            }
        }
        if (failCount > 0)
        {
            DialogService.ShowMessageBox(
                this,
                "图片载入失败, 数量 {0}, 详情请查看日志".Translate(failCount),
                "图片载入失败".Translate(),
                icon: MessageBoxImage.Warning
            );
        }
    }

    public void OnClosing(CancelEventArgs e)
    {
        var result = DialogService.ShowMessageBox(
            this,
            "编辑完成, 是否保存?".Translate(),
            "编辑完成".Translate(),
            MessageBoxButton.YesNoCancel,
            MessageBoxImage.Information
        );
        if (result is null)
        {
            e.Cancel = true;
            return;
        }
        DialogResult = result;
    }

    public Task OnClosingAsync(CancelEventArgs e)
    {
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        if (_disposed)
            return;

        _disposed = true;
        SetPlaying(false);

        var cts = _cts;
        cts.Cancel();

        if (_playTask is { } task)
        {
            _ = task.ContinueWith(
                _ => cts.Dispose(),
                CancellationToken.None,
                TaskContinuationOptions.ExecuteSynchronously,
                TaskScheduler.Default
            );
        }
        else
        {
            cts.Dispose();
        }
    }

    #endregion
    #region Player

    private void Images_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Stop();
    }

    private readonly Lock _playLock = new();
    private TaskCompletionSource<bool> _resumeSignal = NewResumeSignal();

    private CancellationTokenSource _cts = new();
    private Task? _playTask;

    [ObservableProperty]
    public bool Playing { get; private set; }
    private bool _disposed;

    private static TaskCompletionSource<bool> NewResumeSignal() =>
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    /// <summary>
    /// 设置播放/暂停状态，并在开始播放时唤醒等待中的协程
    /// </summary>
    private void SetPlaying(bool playing)
    {
        lock (_playLock)
        {
            Playing = playing;

            if (playing)
            {
                _resumeSignal.TrySetResult(true);
            }
            else if (_resumeSignal.Task.IsCompleted)
            {
                _resumeSignal = NewResumeSignal();
            }
        }
    }

    /// <summary>
    /// 暂停时异步等待，直到再次开始播放
    /// </summary>
    private async Task WaitUntilPlayingAsync(CancellationToken token)
    {
        while (true)
        {
            Task waitTask;

            lock (_playLock)
            {
                if (Playing)
                    return;

                waitTask = _resumeSignal.Task;
            }

            await waitTask.WaitAsync(token);
        }
    }

    /// <summary>
    /// 停止播放（暂停）
    /// </summary>
    [RelayCommand]
    private void Stop()
    {
        SetPlaying(false);
    }

    /// <summary>
    /// 开始或继续播放
    /// </summary>
    [RelayCommand]
    private void Start()
    {
        if (_disposed)
            return;

        if (CurrentAnimeModel is null)
        {
            DialogService.ShowMessageBox(
                this,
                "未选中动画".Translate(),
                "播放失败".Translate(),
                icon: MessageBoxImage.Warning
            );
            return;
        }

        SetPlaying(true);

        // 已有播放任务时只唤醒它，不重复创建任务
        if (_playTask is { IsCompleted: false })
            return;

        _cts.Dispose();
        _cts = new CancellationTokenSource();
        _playTask = PlayAsync(_cts.Token);
    }

    /// <summary>
    /// 播放
    /// </summary>
    private async Task PlayAsync(CancellationToken token)
    {
        try
        {
            var anime = CurrentAnimeModel;
            if (anime is null)
                return;

            while (true)
            {
                foreach (var model in anime.Images)
                {
                    await WaitUntilPlayingAsync(token);

                    CurrentImageModel = model;
                    await Task.Delay(model.Duration, token);
                }

                if (!Loop)
                    break;

                // 一轮结束时如果已暂停，等到再次开始后再进入下一轮
                await WaitUntilPlayingAsync(token);
            }
        }
        catch (OperationCanceledException) when (token.IsCancellationRequested)
        {
            // Reset 或 Dispose 时结束播放
        }
        finally
        {
            SetPlaying(false);
        }
    }

    /// <summary>
    /// 重置并停止当前播放
    /// </summary>
    [RelayCommand]
    private async Task Reset()
    {
        SetPlaying(false);

        var oldCts = _cts;
        oldCts.Cancel(false);

        if (_playTask is not null)
            await _playTask;

        oldCts.Dispose();
        _cts = new CancellationTokenSource();
        _playTask = null;
    }

    #endregion
}
