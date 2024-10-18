using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using HKW.HKWReactiveUI;
using HKW.HKWUtils.Observable;
using Mapster;
using VPet.ModMaker.ViewModels;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.Models;

/// <summary>
/// 低状态文本
/// </summary>
public partial class LowTextModel : ViewModelBase
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
    public static FrozenSet<LowText.ModeType> ModeTypes => EnumInfo<LowText.ModeType>.Values;

    /// <summary>
    /// 好感度类型
    /// </summary>
    public static FrozenSet<LowText.LikeType> LikeTypes => EnumInfo<LowText.LikeType>.Values;

    /// <summary>
    /// 体力类型
    /// </summary>
    public static FrozenSet<LowText.StrengthType> StrengthTypes =>
        EnumInfo<LowText.StrengthType>.Values;

    /// <summary>
    /// ID
    /// </summary>
    [AdaptMember(nameof(LowText.Text))]
    [ReactiveProperty]
    public string ID { get; set; } = string.Empty;

    [AdaptIgnore]
    [ReactiveProperty]
    public required I18nResource<string, string> I18nResource { get; set; }

    partial void OnI18nResourceChanged(
        I18nResource<string, string> oldValue,
        I18nResource<string, string> newValue
    )
    {
        oldValue?.I18nObjects.Remove(I18nObject);
        newValue?.I18nObjects?.Add(I18nObject);
    }

    [NotifyPropertyChangeFrom("")]
    public I18nObject<string, string> I18nObject => new(this);

    [AdaptIgnore]
    [ReactiveI18nProperty("I18nResource", nameof(I18nObject), nameof(ID))]
    public string Text
    {
        get => I18nResource.GetCurrentCultureDataOrDefault(ID);
        set => I18nResource.SetCurrentCultureData(ID, value);
    }

    /// <summary>
    /// 状态
    /// </summary>
    [AdaptMember(nameof(LowText.Mode))]
    [ReactiveProperty]
    public LowText.ModeType Mode { get; set; }

    /// <summary>
    /// 体力
    /// </summary>
    [AdaptMember(nameof(LowText.Strength))]
    [ReactiveProperty]
    public LowText.StrengthType Strength { get; set; }

    /// <summary>
    /// 好感度
    /// </summary>
    [AdaptMember(nameof(LowText.Like))]
    [ReactiveProperty]
    public LowText.LikeType Like { get; set; }

    public LowText ToLowText()
    {
        return this.Adapt<LowText>();
    }

    public void Close()
    {
        I18nResource.I18nObjects.Remove(I18nObject);
    }
}
