using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using DynamicData.Binding;
using HKW.HKWReactiveUI;
using HKW.HKWUtils.Collections;
using HKW.HKWUtils.Observable;
using HKW.WPF.Extensions;
using LinePutScript.Localization.WPF;
using Microsoft.Win32;
using ReactiveUI;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels.ModEdit;

public partial class PetEditVM : ViewModelBase
{
    public PetEditVM(ModInfoModel modInfo)
    {
        ModInfo = modInfo;
        Pet = new() { I18nResource = ModInfo.I18nResource };
        Pets = new(
            new(ModInfo.Pets),
            [],
            f =>
            {
                if (ShowMainPet is false && f.FromMain)
                    return false;
                return f.ID.Contains(Search, StringComparison.OrdinalIgnoreCase);
            }
        );
        Pets.BaseList.WhenValueChanged(x => x.Count)
            .Throttle(TimeSpan.FromSeconds(1), RxApp.TaskpoolScheduler)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => Pets.Refresh());

        this.WhenValueChanged(x => x.Search)
            .Throttle(TimeSpan.FromSeconds(1), RxApp.TaskpoolScheduler)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => Pets.Refresh());
    }

    #region Property
    /// <summary>
    /// 模组信息
    /// </summary>
    public ModInfoModel ModInfo { get; }

    public FilterListWrapper<
        PetModel,
        ObservableList<PetModel>,
        ObservableList<PetModel>
    > Pets { get; set; }

    [ReactiveProperty]
    public string Search { get; set; } = string.Empty;

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
    public PetModel? OldPet { get; set; }

    [ReactiveProperty]
    public PetModel Pet { get; set; }

    [ReactiveProperty]
    public double BorderLength { get; set; } = 250;

    [ReactiveProperty]
    public double LengthRatio { get; set; } = 0.5;

    [ReactiveProperty]
    public BitmapImage? Image { get; set; }
    #endregion

    public void Close()
    {
        Image?.CloseStream();
    }

    /// <summary>
    /// 添加图片
    /// </summary>
    [ReactiveCommand]
    private void AddImage()
    {
        var openFileDialog = new OpenFileDialog()
        {
            Title = "选择图片".Translate(),
            Filter = $"图片|*.jpg;*.jpeg;*.png;*.bmp".Translate()
        };
        if (openFileDialog.ShowDialog() is true)
        {
            Image = NativeUtils.LoadImageToMemoryStream(openFileDialog.FileName);
        }
    }

    /// <summary>
    /// 改变图片
    /// </summary>
    [ReactiveCommand]
    private void ChangeImage()
    {
        var openFileDialog = new OpenFileDialog()
        {
            Title = "选择图片".Translate(),
            Filter = $"图片|*.jpg;*.jpeg;*.png;*.bmp".Translate()
        };
        if (openFileDialog.ShowDialog() is true)
        {
            Image?.CloseStream();
            Image = NativeUtils.LoadImageToMemoryStream(openFileDialog.FileName);
        }
    }

    /// <summary>
    /// 添加
    /// </summary>
    [ReactiveCommand]
    private void Add()
    {
        //var window = new PetEditWindow();
        //var vm = window.ViewModel;
        //window.ShowDialog();
        //if (window.IsCancel)
        //    return;
        //Pets.Add(vm.Pet);
    }

    /// <summary>
    /// 编辑
    /// </summary>
    /// <param name="model">模型</param>
    [ReactiveCommand]
    public void Edit(PetModel model)
    {
        //if (model.FromMain)
        //{
        //    if (
        //        MessageBox.Show("这是本体自带的宠物, 确定要编辑吗".Translate(), "", MessageBoxButton.YesNo)
        //        is not MessageBoxResult.Yes
        //    )
        //        return;
        //}
        //var window = new PetEditWindow();
        //var vm = window.ViewModel;
        //vm.OldPet = model;
        //var newModel = vm.Pet = new(model) { I18nResource = ModInfo.TempI18nResource };
        //model.I18nResource.CopyDataTo(
        //    newModel.I18nResource,
        //    [model.ID, model.PetNameID, model.DescriptionID],
        //    true
        //);
        //window.ShowDialog();
        //if (window.IsCancel)
        //{
        //    newModel.I18nResource.ClearCultureData();
        //    newModel.CloseI18nResource();
        //    return;
        //}
        //newModel.I18nResource.CopyDataTo(ModInfoModel.Current.I18nResource, true);
        //newModel.I18nResource = ModInfoModel.Current.I18nResource;
        //if (model.FromMain)
        //{
        //    var index = Pets.IndexOf(model);
        //    Pets.Remove(model);
        //    Pets.Insert(index, newModel);
        //}
        //else
        //{
        //    Pets[Pets.IndexOf(model)] = newModel;
        //}
        //model.CloseI18nResource();
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="model">模型</param>
    [ReactiveCommand]
    private void Remove(PetModel model)
    {
        //if (model.FromMain)
        //{
        //    MessageBox.Show("这是本体自带的宠物, 无法删除".Translate());
        //    return;
        //}
        //if (MessageBox.Show("确定删除吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.No)
        //    return;
        //Pets.Remove(model);
        //model.CloseI18nResource();
    }
}
