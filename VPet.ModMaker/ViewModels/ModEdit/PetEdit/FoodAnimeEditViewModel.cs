using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Windows.Media.Imaging;
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
using VPet.ModMaker.Resources;
using static VPet_Simulator.Core.IGameSave;

namespace VPet.ModMaker.ViewModels;

/// <summary>
/// 食物动画编辑视图模型
/// </summary>
public sealed partial class FoodAnimeEditViewModel
    : ViewModelBase,
        IModalDialogViewModel,
        IDisposable,
        IViewClosing
{
    /// <inheritdoc/>
    public FoodAnimeEditViewModel()
    {
        FoodImage = DefaultFoodImage;
    }

    /// <summary>
    /// 当前宠物
    /// </summary>
    public PetModel CurrentPet { get; set; } = null!;

    /// <summary>
    /// 默认食物图片
    /// </summary>
    public static BitmapImage DefaultFoodImage { get; } =
        HKWImageUtils.LoadImage(NativeResources.GetStream(NativeResources.FoodImage))!;

    /// <summary>
    /// 食物图片
    /// </summary>
    [ObservableProperty]
    public BitmapImage FoodImage { get; set; }

    /// <summary>
    /// 比例
    /// </summary>
    [ObservableProperty]
    public double LengthRatio { get; set; } = 0.5;

    /// <summary>
    /// 旧动画
    /// </summary>
    public FoodAnimeTypeModel? OldAnime { get; set; } = null!;

    /// <summary>
    /// 动画
    /// </summary>
    [ObservableProperty]
    public FoodAnimeTypeModel Anime { get; set; } = null!;

    /// <summary>
    /// 当前顶层图像模型
    /// </summary>
    [ObservableProperty]
    public ImageModel CurrentFrontImage { get; set; } = null!;

    /// <summary>
    /// 当前底层图像模型
    /// </summary>
    [ObservableProperty]
    public ImageModel CurrentBackImage { get; set; } = null!;

    /// <summary>
    /// 当前食物定位模型
    /// </summary>
    [ObservableProperty]
    public FoodAnimeLocationModel CurrentFoodLocation { get; set; } = null!;

    /// <summary>
    /// 当前动画模型
    /// </summary>
    [ObservableProperty]
    public FoodAnimeModel CurrentAnimeModel { get; set; } = null!;

    partial void OnCurrentAnimeModelChanged(FoodAnimeModel oldValue, FoodAnimeModel newValue)
    {
        _ = Reset();
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

    /// <summary>
    /// 当前模式
    /// </summary>
    public ModeType CurrentMode { get; set; }

    /// <summary>
    /// 循环
    /// </summary>
    [ObservableProperty]
    public bool Loop { get; set; }
    public bool? DialogResult { get; set; }

    #region Command

    [RelayCommand]
    private void ResetFoodImage()
    {
        if (FoodImage != DefaultFoodImage)
            FoodImage?.CloseStream();
        FoodImage = DefaultFoodImage;
    }

    [RelayCommand]
    private void ChangeFoodImage()
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
        if (FoodImage != DefaultFoodImage)
            FoodImage?.CloseStream();
        var image = HKWImageUtils.LoadImageToMemory(openFileDialog.LocalPath);
        if (image is null)
        {
            DialogService.ShowMessageBox(
                this,
                "图片载入失败, 详情请查看日志".Translate(),
                "图片载入失败".Translate(),
                icon: MessageBoxImage.Warning
            );
            return;
        }
        else
            FoodImage = image;
    }

    #region AnimeCommand
    [RelayCommand]
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
    [RelayCommand]
    private void RemoveAnime(FoodAnimeModel value)
    {
        var result = DialogService.ShowMessageBox(
            this,
            "确定删除动画 '{0}'".Translate(value.ID),
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

    #region FrontImageCommand
    /// <summary>
    /// 添加顶层图片
    /// </summary>
    /// <param name="value">动画模型</param>
    [RelayCommand]
    private void AddFrontImage(FoodAnimeModel value)
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
        AddImages(value.FrontImages, openFilesDialog.Select(x => x.LocalPath));
    }

    /// <summary>
    /// 删除顶层图片
    /// </summary>
    /// <param name="value">动画模型</param>
    [RelayCommand]
    private void RemoveFrontImage(FoodAnimeModel value)
    {
        CurrentFrontImage.Dispose();
        value.FrontImages.Remove(CurrentFrontImage);
    }

    /// <summary>
    /// 清空顶层图片
    /// </summary>
    /// <param name="value">动画模型</param>
    [RelayCommand]
    private void ClearFrontImage(FoodAnimeModel value)
    {
        var result = DialogService.ShowMessageBox(
            this,
            "确定清空吗".Translate(),
            "清空图像".Translate(),
            MessageBoxButton.YesNo
        );
        if (result is not true)
            return;
        foreach (var image in value.FrontImages)
            image.Dispose();
        value.FrontImages.Clear();
    }

    /// <summary>
    /// 替换顶层图片
    /// </summary>
    /// <param name="value">动画模型</param>
    [RelayCommand]
    private void ChangeFrontImage(FoodAnimeModel value)
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
        CurrentFrontImage.Dispose();
        CurrentFrontImage.Image = newImage;
    }
    #endregion

    #region BackImageCommand
    /// <summary>
    /// 添加底层图片
    /// </summary>
    /// <param name="value">动画模型</param>
    [RelayCommand]
    private void AddBackImage(FoodAnimeModel value)
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
        AddImages(value.BackImages, openFilesDialog.Select(x => x.LocalPath));
    }

    /// <summary>
    /// 删除底层图片
    /// </summary>
    /// <param name="value">动画模型</param>
    [RelayCommand]
    private void RemoveBackImage(FoodAnimeModel value)
    {
        CurrentBackImage.Dispose();
        value.BackImages.Remove(CurrentBackImage);
    }

    /// <summary>
    /// 清空底层图片
    /// </summary>
    /// <param name="value">动画模型</param>
    [RelayCommand]
    private void ClearBackImage(FoodAnimeModel value)
    {
        var result = DialogService.ShowMessageBox(
            this,
            "确定清空图片吗".Translate(),
            "清空图片".Translate(),
            MessageBoxButton.YesNo
        );
        if (result is not null)
            return;
        foreach (var image in value.BackImages)
            image.Dispose();
        value.BackImages.Clear();
    }

    /// <summary>
    /// 替换底层图片
    /// </summary>
    /// <param name="value"></param>
    /// <exception cref="NotImplementedException"></exception>
    [RelayCommand]
    private void ChangeBackImage(FoodAnimeModel value)
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
        CurrentBackImage.Dispose();
        CurrentBackImage.Image = newImage;
    }
    #endregion

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
                    images.Add(new(image));
            }
            else if (Directory.Exists(path))
            {
                foreach (var file in Directory.EnumerateFiles(path, "*.png"))
                {
                    var image = HKWImageUtils.LoadImageToMemory(file);
                    if (image is null)
                        failCount++;
                    else
                        images.Add(new(image));
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
    #endregion
    #region FoodLocationCommand
    [RelayCommand]
    private void AddeFoodLocation(FoodAnimeModel value)
    {
        value.FoodLocations.Add(new());
    }

    [RelayCommand]
    private void RemoveFoodLocation(FoodAnimeModel value)
    {
        value.FoodLocations.Remove(CurrentFoodLocation);
        CurrentFoodLocation = null!;
    }

    [RelayCommand]
    private void ClearFoodLocation(FoodAnimeModel value)
    {
        var result = DialogService.ShowMessageBox(
            this,
            "确定清空图像吗".Translate(),
            "清空图像".Translate(),
            MessageBoxButton.YesNo
        );
        if (result is not true)
            return;
        value.FoodLocations.Clear();
    }
    #endregion

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
    /// 暂停播放
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

        // 已有播放协程时只唤醒它，避免重复启动多个播放循环
        if (_playTask is { IsCompleted: false })
            return;

        _cts.Dispose();
        _cts = new CancellationTokenSource();

        _playTask = PlayLoopAsync(_cts.Token);
    }

    private async Task PlayLoopAsync(CancellationToken token)
    {
        try
        {
            do
            {
                await Task.WhenAll(
                    FrontPlayAsync(token),
                    BackPlayAsync(token),
                    FoodPlayAsync(token)
                );

                if (!Loop)
                    break;

                await WaitUntilPlayingAsync(token);
            } while (true);
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
    /// 顶层播放
    /// </summary>
    private async Task FrontPlayAsync(CancellationToken token)
    {
        var anime = CurrentAnimeModel;
        if (anime is null)
            return;

        foreach (var model in anime.FrontImages)
        {
            await WaitUntilPlayingAsync(token);

            CurrentFrontImage = model;
            await Task.Delay(model.Duration, token);
        }
    }

    /// <summary>
    /// 底层播放
    /// </summary>
    private async Task BackPlayAsync(CancellationToken token)
    {
        var anime = CurrentAnimeModel;
        if (anime is null)
            return;

        foreach (var model in anime.BackImages)
        {
            await WaitUntilPlayingAsync(token);

            CurrentBackImage = model;
            await Task.Delay(model.Duration, token);
        }
    }

    /// <summary>
    /// 食物播放
    /// </summary>
    private async Task FoodPlayAsync(CancellationToken token)
    {
        var anime = CurrentAnimeModel;
        if (anime is null)
            return;

        foreach (var model in anime.FoodLocations)
        {
            await WaitUntilPlayingAsync(token);

            CurrentFoodLocation = model;
            await Task.Delay(model.Duration, token);
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
    #endregion
}
