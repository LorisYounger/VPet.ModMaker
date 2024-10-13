using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HKW.HKWReactiveUI;
using HKW.HKWUtils.Extensions;
using VPet.ModMaker.Models;
using VPet.ModMaker.Models.ModModel;
using VPet_Simulator.Core;

namespace VPet.ModMaker.ViewModels.ModEdit.AnimeEdit;

public partial class SelectGraphTypeWindowVM : ViewModelBase
{
    public SelectGraphTypeWindowVM()
    {
        PropertyChanged += SelectGraphTypeWindowVM_PropertyChanged;
    }

    private void SelectGraphTypeWindowVM_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(CurrentPet))
        {
            GraphTypes = new(
                AnimeTypeModel.GraphTypes.Except(CurrentPet.Animes.Select(m => m.GraphType))
            );
            // 可添加多个项的类型
            GraphTypes.AddRange(AnimeTypeModel.HasNameAnimes);
        }
        else if (e.PropertyName == nameof(GraphType))
        {
            if (GraphType.IsHasNameAnime())
                HasNameAnime = true;
            else
                HasNameAnime = false;
        }
    }

    /// <summary>
    /// 当前宠物
    /// </summary>
    [ReactiveProperty]
    public PetModel CurrentPet { get; set; }

    /// <summary>
    /// 动画类型
    /// </summary>
    [ReactiveProperty]
    public GraphInfo.GraphType GraphType { get; set; }

    /// <summary>
    /// 动画类型列表
    /// </summary>
    [ReactiveProperty]
    public ObservableSet<GraphInfo.GraphType> GraphTypes { get; set; } = new();

    /// <summary>
    /// 动画名称
    /// </summary>
    [ReactiveProperty]
    public string AnimeName { get; set; } = string.Empty;

    /// <summary>
    /// 具有动画名称
    /// </summary>
    [ReactiveProperty]
    public bool HasNameAnime { get; set; } = true;
}
