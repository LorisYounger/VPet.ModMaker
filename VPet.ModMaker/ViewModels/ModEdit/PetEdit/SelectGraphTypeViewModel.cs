using CommunityToolkit.Mvvm.ComponentModel;
using HanumanInstitute.MvvmDialogs;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using HKW.MVVM.SourceGenerator;
using VPet_Simulator.Core;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels;

/// <summary>
/// 选择图像类型视图模型
/// </summary>
public partial class SelectGraphTypeViewModel : ViewModelBase, IModalDialogViewModel
{
    /// <summary>
    /// 当前宠物
    /// </summary>
    [ObservableProperty]
    public PetModel CurrentPet { get; set; } = null!;


    partial void OnCurrentPetChanged(PetModel oldValue, PetModel newValue)
    {
        if (newValue is not null)
        {
            AnimeName = string.Empty;
            DialogResult = false;
            GraphTypes = new(
                AnimeTypeModel.GraphTypes.Except(CurrentPet.Animes.Select(m => m.GraphType))
            );
            // 可添加多个项的类型
            GraphTypes.AddRange(AnimeTypeModel.NamedAnimes);
        }
    }

    /// <summary>
    /// 动画类型
    /// </summary>
    [ObservableProperty]
    public GraphInfo.GraphType GraphType { get; set; }

    /// <summary>
    /// 动画类型列表
    /// </summary>
    [ObservableProperty]
    public ObservableSet<GraphInfo.GraphType> GraphTypes { get; set; } = [];

    /// <summary>
    /// 动画名称
    /// </summary>
    [ObservableProperty]
    public string AnimeName { get; set; } = string.Empty;

    /// <summary>
    /// 具有动画名称
    /// </summary>
    [NotifyPropertyChangeFrom(nameof(GraphType))]
    public bool HasNameAnime => GraphType.IsNamedGraph();

    public bool? DialogResult { get; set; }
}
