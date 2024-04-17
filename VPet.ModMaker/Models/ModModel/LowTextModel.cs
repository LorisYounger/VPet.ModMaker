using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using HKW.HKWUtils.Observable;
using Mapster;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.Models;

/// <summary>
/// 低状态文本
/// </summary>
public class LowTextModel : ObservableObjectX
{
    public LowTextModel() { }

    public LowTextModel(LowTextModel lowText)
        : this()
    {
        lowText.Adapt(this);
    }

    public LowTextModel(LowText lowText)
        : this()
    {
        lowText.Adapt(this);
    }

    /// <summary>
    /// 状态类型
    /// </summary>
    public static FrozenSet<LowText.ModeType> ModeTypes { get; } =
        Enum.GetValues<LowText.ModeType>().ToFrozenSet();

    /// <summary>
    /// 好感度类型
    /// </summary>
    public static FrozenSet<LowText.LikeType> LikeTypes { get; } =
        Enum.GetValues<LowText.LikeType>().ToFrozenSet();

    /// <summary>
    /// 体力类型
    /// </summary>
    public static FrozenSet<LowText.StrengthType> StrengthTypes { get; } =
        Enum.GetValues<LowText.StrengthType>().ToFrozenSet();

    #region ID

    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _id = string.Empty;

    /// <summary>
    /// ID
    /// </summary>
    [AdaptMember(nameof(LowText.Text))]
    public string ID
    {
        get => _id;
        set => SetProperty(ref _id, value);
    }
    #endregion

    #region I18nData
    [AdaptIgnore]
    private I18nResource<string, string> _i18nResource = null!;

    [AdaptIgnore]
    public required I18nResource<string, string> I18nResource
    {
        get => _i18nResource;
        set
        {
            if (_i18nResource is not null)
                I18nResource.I18nObjectInfos.Remove(this);
            _i18nResource = value;
            InitializeI18nResource();
        }
    }

    public void InitializeI18nResource()
    {
        I18nResource.I18nObjectInfos.Add(
            this,
            new(this, OnPropertyChanged, [(nameof(ID), ID, nameof(Text), true)])
        );
    }

    [AdaptIgnore]
    public string Text
    {
        get => I18nResource.GetCurrentCultureDataOrDefault(ID);
        set => I18nResource.SetCurrentCultureData(ID, value);
    }
    #endregion

    #region Mode
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private LowText.ModeType _mode;

    /// <summary>
    /// 状态
    /// </summary>
    [AdaptMember(nameof(LowText.Mode))]
    public LowText.ModeType Mode
    {
        get => _mode;
        set => SetProperty(ref _mode, value);
    }
    #endregion

    #region Strength
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private LowText.StrengthType _strength;

    /// <summary>
    /// 体力
    /// </summary>
    [AdaptMember(nameof(LowText.Strength))]
    public LowText.StrengthType Strength
    {
        get => _strength;
        set => SetProperty(ref _strength, value);
    }
    #endregion

    #region Like
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private LowText.LikeType _like;

    /// <summary>
    /// 好感度
    /// </summary>
    [AdaptMember(nameof(LowText.Like))]
    public LowText.LikeType Like
    {
        get => _like;
        set => SetProperty(ref _like, value);
    }
    #endregion

    public LowText ToLowText()
    {
        return this.Adapt<LowText>();
    }

    public void Close()
    {
        I18nResource.I18nObjectInfos.Remove(this);
    }
}
