using System.Collections;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using HKW.HKWUtils.Collections;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using HKW.Mapper;
using HKW.MVVM;
using HKW.MVVM.SourceGenerator;
using HKW.WPF;
using HKW.WPF.Extensions;
using HKW.WPF.MVVMDialogs;
using LinePutScript;
using LinePutScript.Converter;
using LinePutScript.Localization.WPF;
using Microsoft.Extensions.Logging;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels;

/// <summary>
/// 移动编辑视图模型
/// </summary>
public sealed partial class MoveEditViewModel
    : ViewModelBase,
        IEditViewModel,
        IModalDialogViewModel,
        IViewClosing,
        IDisposable
{
    /// <inheritdoc/>
    public MoveEditViewModel()
    {
        Moves = new(
            [],
            [],
            f => f.Graph.Contains(Search, StringComparison.CurrentCultureIgnoreCase)
        );
        this.WhenAnyValue(x => x.Search)
            .Throttle(TimeSpan.FromSeconds(0.5), ObservableSchedulers.ThreadPool)
            .DistinctUntilChanged()
            .ObserveOn(ObservableSchedulers.Current)
            .Subscribe(_ => Moves?.Refresh());
    }

    [NotifyPropertyChangeFrom(nameof(CurrentPet))]
    public string DisplayText => "Move ({0})".Translate(CurrentPet?.Moves.Count!);

    /// <summary>
    /// 模组信息
    /// </summary>
    [ObservableProperty]
    public ModInfoModel ModInfo { get; set; } = null!;

    partial void OnModInfoChanged(ModInfoModel oldValue, ModInfoModel newValue)
    {
        if (newValue is not null)
        {
            CurrentPet = newValue.CurrentPet;
            newValue
                .WhenAnyValue(x => x.CurrentPet)
                .Subscribe(x => CurrentPet = x)
                .DisposeWith(newValue.Disposables);
        }
        else
            CurrentPet = null;
    }

    /// <summary>
    /// 全部移动
    /// </summary>
    public ObservableFilteredListWrapper<
        MoveModel,
        ObservableList<MoveModel>,
        ObservableList<MoveModel>
    > Moves { get; }

    /// <summary>
    /// 当前宠物
    /// </summary>
    [ObservableProperty]
    public PetModel? CurrentPet { get; set; }

    partial void OnCurrentPetChanged(PetModel? oldValue, PetModel? newValue)
    {
        Moves.SourceList = null!;
        ModInfo?.CurrentPet = newValue;
        if (newValue is not null)
        {
            Moves.SourceList = newValue.Moves;
        }
    }

    /// <summary>
    /// 搜索
    /// </summary>
    [ObservableProperty]
    public string Search { get; set; } = string.Empty;

    /// <summary>
    /// 旧移动
    /// </summary>
    public MoveModel? OldMove { get; set; }

    /// <summary>
    /// 移动
    /// </summary>
    [ObservableProperty]
    public MoveModel Move { get; set; } = new();

    /// <summary>
    /// 图像
    /// </summary>
    [ObservableProperty]
    public BitmapImage? Image { get; set; }
    public bool? DialogResult { get; set; }
    public bool IsChanged { get; private set; }

    /// <summary>
    /// 改变图片
    /// </summary>
    [RelayCommand]
    private void ChangeImage()
    {
        var openFileDialog = DialogService.ShowOpenFileDialog(
            this,
            new()
            {
                Title = "选择图片".Translate(),
                Filters = [new("图片".Translate(), ModMaker.ImageExtensions)],
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
        Image?.CloseStream();
        Image = newImage;
    }

    /// <summary>
    /// 添加
    /// </summary>
    [RelayCommand]
    private void Add()
    {
        IsChanged = false;
        using var dis = Move.Changed.Subscribe(_ => IsChanged = true);
        var result = DialogService.ShowDialog(this, this);
        if (result is not true)
            return;

        Moves.Add(Move);
        this.LogIfEnabled(LogLevel.Information)?.Log("添加新移动 '{id}'", Move.Graph);
        //if (this.LogX().Level is LogLevel.Info)
        //else
        //    this.LogX()
        //        .Debug(
        //            "添加新移动 {$move}",
        //            LPSConvert.SerializeObjectToLine<Line>(Move.MapToMove(new()), "Move")
        //        );
        Reset();
    }

    /// <summary>
    /// 编辑
    /// </summary>
    /// <param name="model">模型</param>
    [RelayCommand]
    public void Edit(MoveModel model)
    {
        IsChanged = false;
        OldMove = model;
        var newModel = new MoveModel(model);
        Move = newModel;
        using var dis = newModel.Changed.Subscribe(_ => IsChanged = true);
        var result = DialogService.ShowDialog(this, this);
        if (result is not true)
            return;
        Moves[Moves.IndexOf(model)] = newModel;
        this.LogIfEnabled(LogLevel.Information)
            ?.Log("编辑移动 {oldMove} => {newMove}", OldMove.Graph, Move.Graph);
        Reset();
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="list">列表</param>
    [RelayCommand]
    private void Remove(IList list)
    {
        var models = list.Cast<MoveModel>().ToArray();
        var result = DialogService.ShowMessageBox(
            this,
            "确定删除已选中的 {0} 个移动吗".Translate(models.Length),
            "删除移动".Translate(),
            MessageBoxButton.YesNo
        );
        if (result is not true)
            return;
        foreach (var model in models)
        {
            Moves.Remove(model);
            this.LogIfEnabled(LogLevel.Information)?.Log("删除移动 '{id}'", model.Graph);
        }
    }

    /// <summary>
    /// 重置
    /// </summary>
    public void Reset()
    {
        Move = null!;
        OldMove = null!;
        DialogResult = false;
        ModInfo.TempI18nResource.ClearData();
    }

    public void OnClosing(CancelEventArgs e)
    {
        if (IsChanged)
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

        if (DialogResult is not true)
            return;
        if (string.IsNullOrWhiteSpace(Move.Graph))
        {
            DialogService.ShowMessageBox(
                this,
                "指定动画不可为空".Translate(),
                "数据错误".Translate()
            );
            e.Cancel = true;
        }
        DialogResult = e.Cancel is not true;
    }

    public Task OnClosingAsync(CancelEventArgs e)
    {
        return Task.CompletedTask;
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Image?.CloseStream();
        Image = null!;
        ModInfo = null!;
        CurrentPet = null;
    }
}
