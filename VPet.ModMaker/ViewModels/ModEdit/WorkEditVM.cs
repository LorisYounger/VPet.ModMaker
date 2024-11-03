using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using DynamicData.Binding;
using HKW.HKWReactiveUI;
using HKW.HKWUtils.Collections;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using HKW.WPF.Extensions;
using LinePutScript.Localization.WPF;
using Microsoft.Win32;
using ReactiveUI;
using VPet.ModMaker.Models;
using VPet.ModMaker.Models.ModModel;
using VPet.ModMaker.Native;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.ViewModels.ModEdit;

public partial class WorkEditVM : ViewModelBase
{
    public WorkEditVM()
    {
        //ModInfo = modInfo;
        Works = new([], [], f => f.ID.Contains(Search, StringComparison.OrdinalIgnoreCase));

        if (ModInfo.Pets.HasValue())
            CurrentPet = ModInfo.Pets.FirstOrDefault(
                m => m.FromMain is false && m.Works.HasValue(),
                ModInfo.Pets.First()
            );

        ModInfo
            .WhenValueChanged(x => x.ShowMainPet)
            .Throttle(TimeSpan.FromSeconds(1), RxApp.TaskpoolScheduler)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ =>
            {
                if (CurrentPet?.FromMain is false)
                    CurrentPet = null!;
            });

        this.WhenValueChanged(x => x.Search)
            .Throttle(TimeSpan.FromSeconds(1), RxApp.TaskpoolScheduler)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => Works.Refresh());
    }

    #region Property
    /// <summary>
    /// 模组信息
    /// </summary>
    [ReactiveProperty]
    public ModInfoModel ModInfo { get; set; } = null!;

    /// <summary>
    /// I18n资源
    /// </summary>
    public I18nResource<string, string> I18nResource => ModInfo.I18nResource;

    /// <summary>
    /// 当前宠物
    /// </summary>
    [ReactiveProperty]
    public PetModel CurrentPet { get; set; } = null!;

    partial void OnCurrentPetChanged(PetModel oldValue, PetModel newValue)
    {
        Works.Clear();
        if (oldValue is not null)
            Works.BaseList.BindingList(oldValue.Works, true);
        if (newValue is null)
            return;
        Works.AddRange(CurrentPet.Works);
        Works.BaseList.BindingList(CurrentPet.Works);
    }

    /// <summary>
    /// 全部工作
    /// </summary>
    public FilterListWrapper<
        WorkModel,
        ObservableList<WorkModel>,
        ObservableList<WorkModel>
    > Works { get; set; } = null!;

    [ReactiveProperty]
    public string Search { get; set; } = string.Empty;

    /// <summary>
    /// 旧工作
    /// </summary>
    public WorkModel? OldWork { get; set; }

    [ReactiveProperty]
    public WorkModel Work { get; set; } = null!;

    partial void OnWorkChanged(WorkModel oldValue, WorkModel newValue)
    {
        if (oldValue is not null)
        {
            oldValue.PropertyChanged -= Work_PropertyChanged;
        }
        if (newValue is not null)
        {
            newValue.PropertyChanged -= Work_PropertyChanged;
            newValue.PropertyChanged += Work_PropertyChanged;
            SetGraphImage(newValue);
        }
    }

    private void Work_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is not WorkModel workModel)
            return;
        if (e.PropertyName == nameof(WorkModel.Graph))
        {
            SetGraphImage(workModel);
        }
    }

    [ReactiveProperty]
    public double BorderLength { get; set; } = 250;

    [ReactiveProperty]
    public double LengthRatio { get; set; } = 250 / 500;

    /// <summary>
    /// 图片
    /// </summary>
    [ReactiveProperty]
    public BitmapImage? Image { get; set; }
    #endregion

    /// <summary>
    /// 修复超模
    /// </summary>
    [ReactiveCommand]
    private void FixOverLoad()
    {
        Work.FixOverLoad();
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
    /// 设置图片
    /// </summary>
    /// <param name="workModel">工作模型</param>
    private void SetGraphImage(WorkModel workModel)
    {
        if (CurrentPet is null)
            return;
        var graph = workModel.Graph;
        Image?.CloseStream();
        Image = null;
        // 随机挑一张图片
        if (
            CurrentPet.Animes.FirstOrDefault(
                a =>
                    a.GraphType is VPet_Simulator.Core.GraphInfo.GraphType.Work
                    && a.Name.Equals(graph, StringComparison.OrdinalIgnoreCase),
                null!
            )
            is not AnimeTypeModel anime
        )
            return;
        if (anime.HappyAnimes.HasValue())
        {
            Image = anime.HappyAnimes.Random().Images.Random().Image.CloneStream();
        }
        else if (anime.NomalAnimes.HasValue())
        {
            Image = anime.NomalAnimes.Random().Images.Random().Image.CloneStream();
        }
        else if (anime.PoorConditionAnimes.HasValue())
        {
            Image = anime.PoorConditionAnimes.Random().Images.Random().Image.CloneStream();
        }
        else if (anime.IllAnimes.HasValue())
        {
            Image = anime.IllAnimes.Random().Images.Random().Image.CloneStream();
        }
    }

    /// <summary>
    /// 添加
    /// </summary>
    [ReactiveCommand]
    private void Add()
    {
        //var window = new WorkEditWindow();
        //var vm = window.ViewModel;
        //vm.CurrentPet = CurrentPet;
        //window.ShowDialog();
        //if (window.IsCancel)
        //    return;
        //Works.Add(vm.Work);
    }

    /// <summary>
    /// 编辑
    /// </summary>
    /// <param name="model">模型</param>
    [ReactiveCommand]
    public void Edit(WorkModel model)
    {
        //var window = new WorkEditWindow();
        //var vm = window.ViewModel;
        //vm.CurrentPet = CurrentPet;
        //vm.OldWork = model;
        //var newModel = vm.Work = new(model) { I18nResource = ModInfo.TempI18nResource };
        //model.I18nResource.CopyDataTo(newModel.I18nResource, model.ID, true);
        //window.ShowDialog();
        //if (window.IsCancel)
        //{
        //    newModel.I18nResource.ClearCultureData();
        //    newModel.Close();
        //    return;
        //}
        //newModel.I18nResource.CopyDataTo(ModInfo.I18nResource, true);
        //newModel.I18nResource = ModInfo.I18nResource;
        //Works[Works.IndexOf(model)] = newModel;
        //model.Close();
    }

    /// <summary>
    /// 删除
    /// </summary>
    /// <param name="model">模型</param>
    [ReactiveCommand]
    private void Remove(WorkModel model)
    {
        //if (MessageBox.Show("确定删除吗".Translate(), "", MessageBoxButton.YesNo) is MessageBoxResult.No)
        //    return;
        //Works.Remove(model);
        //model.Close();
    }

    public void Close()
    {
        Image?.CloseStream();
    }
}
