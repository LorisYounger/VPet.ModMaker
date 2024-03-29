using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HKW.HKWUtils.Extensions;
using VPet.ModMaker.Models;
using VPet.ModMaker.Models.ModModel;
using VPet_Simulator.Core;

namespace VPet.ModMaker.ViewModels.ModEdit.AnimeEdit;

public class SelectGraphTypeWindowVM : ObservableObjectX<SelectGraphTypeWindowVM>
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

    #region CurrentPet
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private PetModel _currentPet = null!;

    /// <summary>
    /// 当前宠物
    /// </summary>
    public PetModel CurrentPet
    {
        get => _currentPet;
        set => SetProperty(ref _currentPet, value);
    }
    #endregion
    #region GraphType
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private GraphInfo.GraphType _graphType;

    /// <summary>
    /// 动画类型
    /// </summary>
    public GraphInfo.GraphType GraphType
    {
        get => _graphType;
        set => SetProperty(ref _graphType, value);
    }
    #endregion
    #region GraphTypes
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ObservableSet<GraphInfo.GraphType> _graphTypes = new();

    /// <summary>
    /// 动画类型列表
    /// </summary>
    public ObservableSet<GraphInfo.GraphType> GraphTypes
    {
        get => _graphTypes;
        set => SetProperty(ref _graphTypes, value);
    }
    #endregion
    #region AnimeName
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _animeName = string.Empty;

    /// <summary>
    /// 动画名称
    /// </summary>
    public string AnimeName
    {
        get => _animeName;
        set => SetProperty(ref _animeName, value);
    }
    #endregion

    #region HasNameAnime
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private bool _hasNameAnime = true;

    /// <summary>
    /// 具有动画名称
    /// </summary>
    public bool HasNameAnime
    {
        get => _hasNameAnime;
        set => SetProperty(ref _hasNameAnime, value);
    }
    #endregion
}
