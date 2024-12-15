using System.Collections.Specialized;
using System.IO;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using HKW.HKWReactiveUI;
using HKW.HKWUtils.Observable;
using HKW.MVVMDialogs;
using HKW.WPF;
using HKW.WPF.MVVMDialogs;
using LinePutScript.Localization.WPF;
using VPet.ModMaker.Models;
using VPet.ModMaker.Models.ModModel;
using static VPet_Simulator.Core.IGameSave;

namespace VPet.ModMaker.ViewModels.ModEdit;

/// <summary>
/// 动画编辑视图模型
/// </summary>
public partial class AnimeEditVM : DialogViewModel
{
    /// <inheritdoc/>
    public AnimeEditVM(AnimeTypeModel anime)
    {
        Anime = anime;
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
    public bool HasMultiType => AnimeTypeModel.HasMultiTypeAnimes.Contains(Anime.GraphType);

    /// <summary>
    /// 含有动画名称
    /// </summary>
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
            ModMakerVM.DialogService.ShowMessageBoxX(
                this,
                "确认删除动画 \"{0}\"".Translate(value.ID),
                "删除动画".Translate(),
                MessageBoxButton.YesNo
            )
            is not true
        )
            return;
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
    #endregion

    #region ImageCommand

    /// <summary>
    /// 添加图片
    /// </summary>
    /// <param name="value">动画模型</param>
    [ReactiveCommand]
    private void AddImage(AnimeModel value)
    {
        var openFilesDialog = ModMakerVM.DialogService.ShowOpenFilesDialog(
            this,
            new()
            {
                Title = "选择图片".Translate(),
                Filters = [new("图片".Translate(), ["jpg", "jpeg", "png", "bmp"])]
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
        var openFileDialog = ModMakerVM.DialogService.ShowOpenFileDialog(
            this,
            new() { Title = "选择图片".Translate(), Filters = [new("图片".Translate(), ["png"])] }
        );
        if (openFileDialog is null)
            return;
        var newImage = HKWImageUtils.LoadImageToMemory(openFileDialog.LocalPath);
        if (newImage is null)
        {
            ModMakerVM.DialogService.ShowMessageBoxX(
                this,
                "图片载入失败, 详情请查看日志".Translate(),
                "图片载入失败".Translate(),
                icon: MessageBoxImage.Warning
            );
            return;
        }
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
            ModMakerVM.DialogService.ShowMessageBoxX(
                this,
                "确定清空图像吗".Translate(),
                "清空图像".Translate(),
                MessageBoxButton.YesNo
            )
            is not true
        )
            return;
        value.Close();
        value.Images.Clear();
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
            ModMakerVM.DialogService.ShowMessageBoxX(
                this,
                "图片载入失败, 数量 {0}, 详情请查看日志".Translate(failCount),
                "图片载入失败".Translate(),
                icon: MessageBoxImage.Warning
            );
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
            ModMakerVM.DialogService.ShowMessageBoxX(
                this,
                "未选中动画".Translate(),
                "播放失败".Translate(),
                icon: MessageBoxImage.Warning
            );
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
