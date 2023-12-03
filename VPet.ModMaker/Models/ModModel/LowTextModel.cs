using HKW.HKWUtils.Observable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
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

    /// <summary>
    /// Id
    /// </summary>
    public ObservableValue<string> Id { get; } = new();

    /// <summary>
    /// 状态
    /// </summary>
    public ObservableValue<LowText.ModeType> Mode { get; } = new();

    /// <summary>
    /// 体力
    /// </summary>
    public ObservableValue<LowText.StrengthType> Strength { get; } = new();

    /// <summary>
    /// 好感度
    /// </summary>
    public ObservableValue<LowText.LikeType> Like { get; } = new();

    public LowTextModel() { }

    public LowTextModel(LowTextModel lowText)
        : this()
    {
        Id.Value = lowText.Id.Value;
        Mode.Value = lowText.Mode.Value;
        Strength.Value = lowText.Strength.Value;
        Like.Value = lowText.Like.Value;

        foreach (var item in lowText.I18nDatas)
            I18nDatas[item.Key] = item.Value.Copy();
        CurrentI18nData.Value = I18nDatas[I18nHelper.Current.CultureName.Value];
    }

    public LowTextModel(LowText lowText)
        : this()
    {
        Id.Value = lowText.Text;
        Mode.Value = lowText.Mode;
        Strength.Value = lowText.Strength;
        Like.Value = lowText.Like;
    }

    public void Close() { }

    public LowText ToLowText()
    {
        return new()
        {
            Text = Id.Value,
            Mode = Mode.Value,
            Strength = Strength.Value,
            Like = Like.Value,
        };
    }
}

public class I18nLowTextModel
{
    public ObservableValue<string> Text { get; } = new();

    public I18nLowTextModel Copy()
    {
        var result = new I18nLowTextModel();
        result.Text.Value = Text.Value;
        return result;
    }
}
