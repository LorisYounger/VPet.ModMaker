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
public class LowTextModel : I18nModel<I18nLowTextModel>
{
    public LowTextModel() { }

    public LowTextModel(LowTextModel lowText)
        : this()
    {
        lowText.Adapt(this);

        foreach (var item in lowText.I18nDatas)
            I18nDatas[item.Key] = item.Value.Clone();
        CurrentI18nData = I18nDatas[I18nHelper.Current.CultureName];
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
}

public class I18nLowTextModel : ObservableObjectX, ICloneable<I18nLowTextModel>
{
    #region Text
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _text = string.Empty;

    public string Text
    {
        get => _text;
        set => SetProperty(ref _text, value);
    }
    #endregion

    public I18nLowTextModel Clone()
    {
        return this.Adapt<I18nLowTextModel>();
    }

    object ICloneable.Clone() => Clone();
}
