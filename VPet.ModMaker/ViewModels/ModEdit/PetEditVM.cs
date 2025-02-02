﻿using System.Collections;
using System.Collections.Frozen;
using System.Reactive.Linq;
using System.Windows.Media.Imaging;
using DynamicData.Binding;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using HKW.HKWReactiveUI;
using HKW.HKWUtils;
using HKW.HKWUtils.Collections;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using HKW.MVVMDialogs;
using HKW.WPF;
using HKW.WPF.Extensions;
using HKW.WPF.MVVMDialogs;
using LinePutScript.Localization.WPF;
using ReactiveUI;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels.ModEdit;

/// <summary>
/// 宠物编辑视图模型
/// </summary>
public partial class PetEditVM : DialogViewModel, IEnableLogger<ViewModelBase>, IDisposable
{
    /// <inheritdoc/>
    public PetEditVM(ModInfoModel modInfo)
    {
        ModInfo = modInfo;
        Pets = new(
            modInfo.Pets.BaseList,
            [],
            f =>
            {
                if (ShowMainPet is false && f.FromMain)
                    return false;
                return SearchTargets.SelectedItem switch
                {
                    PetSearchTarget.ID => f.ID.Contains(Search, StringComparison.OrdinalIgnoreCase),
                    PetSearchTarget.Name
                        => f.Name.Contains(Search, StringComparison.OrdinalIgnoreCase),
                    PetSearchTarget.PetName
                        => f.PetName.Contains(Search, StringComparison.OrdinalIgnoreCase),
                    PetSearchTarget.Description
                        => f.Description.Contains(Search, StringComparison.OrdinalIgnoreCase),
                    PetSearchTarget.Tags
                        => f.Tags.Contains(Search, StringComparison.OrdinalIgnoreCase),
                    _ => false
                };
            }
        );

        this.WhenAnyValue(
                x => x.Search,
                x => x.SearchTargets.SelectedItem,
                x => x.ModInfo.I18nResource.CurrentCulture
            )
            .Throttle(TimeSpan.FromSeconds(0.5), RxApp.TaskpoolScheduler)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => Pets.Refresh())
            .Record(this);

        Closing += PetEditVM_Closing;
    }

    private void PetEditVM_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        if (DialogResult is not true)
            return;
        if (string.IsNullOrWhiteSpace(Pet.ID))
        {
            NativeUtils.DialogService.ShowMessageBoxX(
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
            NativeUtils.DialogService.ShowMessageBoxX(
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
            NativeUtils.DialogService.ShowMessageBoxX(
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
            NativeUtils.DialogService.ShowMessageBoxX(
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

    #region Property
    /// <summary>
    /// 模组信息
    /// </summary>
    public ModInfoModel ModInfo { get; set; } = null!;

    /// <summary>
    /// 宠物
    /// </summary>
    public FilterListWrapper<
        PetModel,
        ObservableList<PetModel>,
        ObservableList<PetModel>
    > Pets { get; }

    /// <summary>
    /// 搜索
    /// </summary>
    [ReactiveProperty]
    public string Search { get; set; } = string.Empty;

    /// <summary>
    /// 搜索目标
    /// </summary>
    public ObservableSelectableSetWrapper<
        PetSearchTarget,
        FrozenSet<PetSearchTarget>
    > SearchTargets { get; } = new(EnumInfo<PetSearchTarget>.Values);

    /// <summary>
    /// 显示本体宠物
    /// </summary>
    [ReactiveProperty]
    public bool ShowMainPet { get; set; }

    partial void OnShowMainPetChanged(bool oldValue, bool newValue)
    {
        ModInfo.ShowMainPet = newValue;
        Pets.Refresh();
    }

    /// <summary>
    /// I18n资源
    /// </summary>
    public I18nResource<string, string> I18nResource => ModInfo.I18nResource;

    /// <summary>
    /// 旧宠物
    /// </summary>
    public PetModel? OldPet { get; set; }

    /// <summary>
    /// 宠物
    /// </summary>
    [ReactiveProperty]
    public PetModel Pet { get; set; } = null!;

    /// <summary>
    /// 图像
    /// </summary>
    [ReactiveProperty]
    public BitmapImage? Image { get; set; }

    /// <summary>
    /// 比例
    /// </summary>
    public static double LengthRatio { get; } = 0.5;
    #endregion


    /// <summary>
    /// 添加图像
    /// </summary>
    [ReactiveCommand]
    private void AddImage()
    {
        var openFileDialog = NativeUtils.DialogService.ShowOpenFileDialog(
            this,
            new()
            {
                Title = "选择图片".Translate(),
                Filters = [new("图片".Translate(), ["jpg", "jpeg", "png", "bmp"])]
            }
        );
        if (openFileDialog is null)
            return;
        var newImage = HKWImageUtils.LoadImageToMemory(openFileDialog.LocalPath);
        if (newImage is null)
        {
            NativeUtils.DialogService.ShowMessageBoxX(
                this,
                "图片载入失败, 详情请查看日志".Translate(),
                "图片载入失败".Translate(),
                icon: MessageBoxImage.Warning
            );
            return;
        }
        Image = newImage;
    }

    /// <summary>
    /// 改变图像
    /// </summary>
    [ReactiveCommand]
    private void ChangeImage()
    {
        var openFileDialog = NativeUtils.DialogService.ShowOpenFileDialog(
            this,
            new()
            {
                Title = "选择图片".Translate(),
                Filters = [new("图片".Translate(), ["jpg", "jpeg", "png", "bmp"])]
            }
        );
        if (openFileDialog is null)
            return;
        var newImage = HKWImageUtils.LoadImageToMemory(openFileDialog.LocalPath);
        if (newImage is null)
        {
            NativeUtils.DialogService.ShowMessageBoxX(
                this,
                "图片载入失败, 详情请查看日志".Translate(),
                "图片载入失败".Translate(),
                icon: MessageBoxImage.Warning
            );
            return;
        }
        Image?.CloseStreamWhenNoReference();
        Image = newImage;
    }

    /// <summary>
    /// 添加
    /// </summary>
    [ReactiveCommand]
    private async void Add()
    {
        ModInfo.TempI18nResource.ClearCultureData();
        Pet = new() { I18nResource = ModInfo.I18nResource };
        await NativeUtils.DialogService.ShowDialogAsyncX(this, this);
        if (DialogResult is not true)
        {
            Pet.Close();
        }
        else
        {
            Pet.I18nResource.CopyDataTo(ModInfo.I18nResource, true);
            Pet.I18nResource = ModInfo.I18nResource;
            Pets.Add(Pet);
            this.LogX().Info("添加新宠物 {pet}", Pet.ID);
        }
        Reset();
    }

    /// <summary>
    /// 编辑
    /// </summary>
    /// <param name="model">模型</param>
    [ReactiveCommand]
    public async void Edit(PetModel model)
    {
        if (model.FromMain)
        {
            if (
                NativeUtils.DialogService.ShowMessageBoxX(
                    this,
                    "这是本体自带的宠物, 确定要编辑吗?".Translate(),
                    "编辑".Translate(),
                    MessageBoxButton.YesNo
                )
                is not true
            )
                return;
        }
        OldPet = model;
        var newModel = new PetModel(model) { I18nResource = ModInfo.TempI18nResource };
        model.I18nResource.CopyDataTo(
            newModel.I18nResource,
            [model.ID, model.PetNameID, model.DescriptionID],
            true
        );
        Pet = newModel;
        await NativeUtils.DialogService.ShowDialogAsync(this, this);
        if (DialogResult is not true)
        {
            newModel.I18nResource.ClearCultureData();
            newModel.CloseWhenOld();
        }
        else
        {
            OldPet.CloseWhenOld();
            newModel.I18nResource.CopyDataTo(ModInfo.I18nResource, true);
            newModel.I18nResource = ModInfo.I18nResource;
            var temp = ModInfo.CurrentPet;
            Pets[Pets.IndexOf(model)] = newModel;
            if (temp == OldPet)
                ModInfo.CurrentPet = newModel;
            this.LogX().Info("编辑宠物 {oldPet} => {newPet}", OldPet.ID, Pet.ID);
        }
        Reset();
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="list">列表</param>
    [ReactiveCommand]
    private void Remove(IList list)
    {
        var models = list.Cast<PetModel>().ToArray();
        if (
            NativeUtils.DialogService.ShowMessageBoxX(
                this,
                "确定删除已选中的 {0} 个宠物吗".Translate(models.Length),
                "删除宠物".Translate(),
                MessageBoxButton.YesNo
            )
            is not true
        )
            return;
        foreach (var model in models)
        {
            Pets.Remove(model);
            model.Close();
            this.LogX().Info("删除宠物 {pet}", model.ID);
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
        Image?.CloseStreamWhenNoReference();
        Image = null;
        ModInfo.TempI18nResource.ClearCultureData();
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (_disposed)
            return;
        base.Dispose(disposing);
        if (disposing) { }
        Reset();
        ModInfo = null!;
        _disposed = false;
    }
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
