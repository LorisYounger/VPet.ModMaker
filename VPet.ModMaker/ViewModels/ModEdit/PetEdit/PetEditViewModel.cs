using System.Collections;
using System.Collections.Frozen;
using System.Collections.Immutable;
using System.ComponentModel;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using HKW.HKWUtils;
using HKW.HKWUtils.Collections;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using HKW.MVVM;
using HKW.MVVM.SourceGenerator;
using HKW.WPF;
using HKW.WPF.Extensions;
using HKW.WPF.MVVMDialogs;
using LinePutScript.Localization.WPF;
using Microsoft.Extensions.Logging;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels;

/// <summary>
/// 宠物编辑视图模型
/// </summary>
public partial class PetEditViewModel
    : ViewModelBase,
        IEditViewModel,
        IModalDialogViewModel,
        IViewClosing
{
    /// <inheritdoc/>
    public PetEditViewModel()
    {
        Pets = new(
            [],
            [],
            f =>
            {
                if (ShowMainPet is false && f.FromMain)
                    return false;
                return CurrentSearchTarget.Value switch
                {
                    PetSearchTarget.ID => f.ID.Contains(
                        Search,
                        StringComparison.CurrentCultureIgnoreCase
                    ),
                    PetSearchTarget.Name => f.Name.Contains(
                        Search,
                        StringComparison.CurrentCultureIgnoreCase
                    ),
                    PetSearchTarget.PetName => f.PetName.Contains(
                        Search,
                        StringComparison.CurrentCultureIgnoreCase
                    ),
                    PetSearchTarget.Description => f.Description.Contains(
                        Search,
                        StringComparison.CurrentCultureIgnoreCase
                    ),
                    PetSearchTarget.Tags => f.Tags.Contains(
                        Search,
                        StringComparison.CurrentCultureIgnoreCase
                    ),
                    _ => false,
                };
            }
        );
        this.WhenAnyValue(x => x.Search, x => x.CurrentSearchTarget)
            .Throttle(TimeSpan.FromSeconds(0.5), ObservableSchedulers.ThreadPool)
            .DistinctUntilChanged()
            .ObserveOn(ObservableSchedulers.Current)
            .Subscribe(_ => Pets?.Refresh());
    }

    [NotifyPropertyChangeFrom(nameof(ModInfo))]
    public string DisplayText => "Pet ({0})".Translate(ModInfo?.Pets.FilteredList.Count!);

    #region Property
    /// <summary>
    /// 模组信息
    /// </summary>
    [ObservableProperty]
    public ModInfoModel ModInfo { get; set; } = null!;

    partial void OnModInfoChanged(ModInfoModel oldValue, ModInfoModel newValue)
    {
        Pets.SourceList = null!;
        if (newValue is not null)
        {
            Pets.SourceList = newValue.Pets.FilteredList;
            newValue
                .Pets.FilteredList.WhenAnyValue(x => x.Count)
                .Subscribe(_ => OnPropertyChanged(nameof(DisplayText)))
                .DisposeWith(newValue.Disposables);
        }
    }

    /// <summary>
    /// 宠物
    /// </summary>
    public ObservableFilteredListWrapper<
        PetModel,
        ObservableList<PetModel>,
        ObservableList<PetModel>
    > Pets { get; }

    /// <summary>
    /// 搜索
    /// </summary>
    [ObservableProperty]
    public string Search { get; set; } = string.Empty;

    /// <summary>
    /// 搜索目标
    /// </summary>
    public static ImmutableArray<EnumInfo<PetSearchTarget>> SearchTargets =>
        EnumInfo<PetSearchTarget>.StaticInfos;

    [ObservableProperty]
    public EnumInfo<PetSearchTarget> CurrentSearchTarget { get; set; } = PetSearchTarget.ID;

    /// <summary>
    /// 显示本体宠物
    /// </summary>
    [ObservableProperty]
    public bool ShowMainPet { get; set; }

    partial void OnShowMainPetChanged(bool oldValue, bool newValue)
    {
        ModInfo.ShowMainPet = newValue;
        Pets.Refresh();
    }

    /// <summary>
    /// 旧宠物
    /// </summary>
    public PetModel? OldPet { get; set; }

    /// <summary>
    /// 宠物
    /// </summary>
    [ObservableProperty]
    public PetModel Pet { get; set; } = null!;

    /// <summary>
    /// 图像
    /// </summary>
    [ObservableProperty]
    public BitmapImage? Image { get; set; }

    /// <summary>
    /// 比例
    /// </summary>
    public static double LengthRatio { get; } = 0.5;

    public bool? DialogResult { get; set; }
    #endregion


    /// <summary>
    /// 添加图像
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
        Pet = new() { I18nResource = ModInfo.TempI18nResource };
        var result = DialogService.ShowDialog(this, this);
        if (result is not true)
        {
            Pet.Dispose();
        }
        else
        {
            Pet.I18nResource.CopyDataTo(ModInfo.I18nResource);
            Pet.I18nResource = ModInfo.I18nResource;
            Pets.Add(Pet);
            ModInfo.Pets.Refresh();
            this.LogIfEnabled(LogLevel.Information)?.Log("添加新宠物 '{id}'", Pet.ID);
        }
        Reset();
    }

    /// <summary>
    /// 编辑
    /// </summary>
    /// <param name="model">模型</param>
    [RelayCommand]
    public void Edit(PetModel model)
    {
        if (
            model.FromMain
            && DialogService.ShowMessageBox(
                this,
                "这是本体自带的宠物, 确定要编辑吗?".Translate(),
                "编辑".Translate(),
                MessageBoxButton.YesNo
            )
                is not true
        )
            return;
        OldPet = model;
        var newModel = new PetModel(model) { I18nResource = ModInfo.TempI18nResource };
        model.I18nResource.CopyDataTo(
            newModel.I18nResource,
            model.ID,
            model.PetNameID,
            model.DescriptionID
        );
        Pet = newModel;
        var result = DialogService.ShowDialog(this, this);
        if (result is not true)
        {
            newModel.Dispose();
        }
        else
        {
            OldPet.Dispose();
            newModel.I18nResource.CopyDataTo(ModInfo.I18nResource);
            newModel.I18nResource = ModInfo.I18nResource;
            var temp = ModInfo.CurrentPet;
            Pets[Pets.IndexOf(model)] = newModel;
            if (temp == OldPet)
                ModInfo.CurrentPet = newModel;
            this.LogIfEnabled(LogLevel.Information)
                ?.Log("编辑宠物 {oldPet} => {newPet}", OldPet.ID, Pet.ID);
        }
        Reset();
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="list">列表</param>
    [RelayCommand]
    private void Remove(IList list)
    {
        var models = list.Cast<PetModel>().ToArray();
        var result = DialogService.ShowMessageBox(
            this,
            "确定删除已选中的 {0} 个宠物吗".Translate(models.Length),
            "删除宠物".Translate(),
            MessageBoxButton.YesNo
        );
        if (result is not true)
            return;
        foreach (var model in models)
        {
            Pets.Remove(model);
            model.DisposeAll();
            this.LogIfEnabled(LogLevel.Information)?.Log("删除宠物 '{id}'", model.ID);
        }
    }

    /// <summary>
    /// 重置
    /// </summary>
    public void Reset()
    {
        Pet = null!;
        OldPet = null!;
        DialogResult = false;
        Image?.CloseStream();
        Image = null;
        ModInfo.TempI18nResource.ClearData();
    }

    public void OnClosing(CancelEventArgs e)
    {
        if (DialogResult is not true)
            return;
        if (string.IsNullOrWhiteSpace(Pet.ID))
        {
            DialogService.ShowMessageBox(
                this,
                "ID不可为空".Translate(),
                "数据错误".Translate(),
                MessageBoxButton.Ok,
                MessageBoxImage.Warning
            );
            e.Cancel = true;
        }
        else if (Pet.Name is null)
        {
            DialogService.ShowMessageBox(
                this,
                "名称不可为空".Translate(),
                "数据错误".Translate(),
                MessageBoxButton.Ok,
                MessageBoxImage.Warning
            );
            e.Cancel = true;
        }
        else if (Pet.PetName is null)
        {
            DialogService.ShowMessageBox(
                this,
                "宠物名称不可为空".Translate(),
                "数据错误".Translate(),
                MessageBoxButton.Ok,
                MessageBoxImage.Warning
            );
            e.Cancel = true;
        }
        else if (OldPet?.ID != Pet.ID && ModInfo.Pets.Any(i => i.ID == Pet.ID))
        {
            DialogService.ShowMessageBox(
                this,
                "此ID已存在".Translate(),
                "数据错误".Translate(),
                MessageBoxButton.Ok,
                MessageBoxImage.Warning
            );
            e.Cancel = true;
        }
        DialogResult = e.Cancel is not true;
    }

    public Task OnClosingAsync(CancelEventArgs e)
    {
        return Task.CompletedTask;
    }

    ///// <inheritdoc/>
    //protected override void Dispose(bool disposing)
    //{
    //    if (_disposed)
    //        return;
    //    base.Dispose(disposing);
    //    if (disposing) { }
    //    Reset();
    //    ModInfo = null!;
    //    _disposed = false;
    //}
}

/// <summary>
/// 宠物搜索目标
/// </summary>
public enum PetSearchTarget
{
    /// <summary>
    /// ID
    /// </summary>
    ID,

    /// <summary>
    /// 名称
    /// </summary>
    Name,

    /// <summary>
    /// 宠物名称
    /// </summary>
    PetName,

    /// <summary>
    /// 描述
    /// </summary>
    Description,

    /// <summary>
    /// 标签
    /// </summary>
    Tags,
}
