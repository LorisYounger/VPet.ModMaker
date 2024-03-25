using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using HKW.HKWUtils.Observable;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.Models;

/// <summary>
/// 低状态文本
/// </summary>
public class LowTextModel : I18nModel<I18nLowTextModel>
{
    /// <summary>
    /// 状态类型
    /// </summary>
    public static ObservableCollection<LowText.ModeType> ModeTypes { get; } =
        new(Enum.GetValues(typeof(LowText.ModeType)).Cast<LowText.ModeType>());

    /// <summary>
    /// 好感度类型
    /// </summary>
    public static ObservableCollection<LowText.LikeType> LikeTypes { get; } =
        new(Enum.GetValues(typeof(LowText.LikeType)).Cast<LowText.LikeType>());

    /// <summary>
    /// 体力类型
    /// </summary>
    public static ObservableCollection<LowText.StrengthType> StrengthTypes { get; } =
        new(Enum.GetValues(typeof(LowText.StrengthType)).Cast<LowText.StrengthType>());

    #region Id
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private string _id = string.Empty;

    /// <summary>
    /// Id
    /// </summary>
    public string Id
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

    public LowText.LikeType Like
    {
        get => _like;
        set => SetProperty(ref _like, value);
    }
    #endregion

    public LowTextModel() { }

    public LowTextModel(LowTextModel lowText)
        : this()
    {
        Id = lowText.Id;
        Mode = lowText.Mode;
        Strength = lowText.Strength;
        Like = lowText.Like;

        foreach (var item in lowText.I18nDatas)
            I18nDatas[item.Key] = item.Value.Copy();
        CurrentI18nData = I18nDatas[I18nHelper.Current.CultureName];
    }

    public LowTextModel(LowText lowText)
        : this()
    {
        Id = lowText.Text;
        Mode = lowText.Mode;
        Strength = lowText.Strength;
        Like = lowText.Like;
    }

    public void Close() { }

    public LowText ToLowText()
    {
        return new()
        {
            Text = Id,
            Mode = Mode,
            Strength = Strength,
            Like = Like,
        };
    }
}

public class I18nLowTextModel : ObservableObjectX<I18nLowTextModel>
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

    public I18nLowTextModel Copy()
    {
        var result = new I18nLowTextModel();
        result.Text = Text;
        return result;
    }
}
