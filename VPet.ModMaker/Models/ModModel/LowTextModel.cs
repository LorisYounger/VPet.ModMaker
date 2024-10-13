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

    /// <summary>
    /// ID
    /// </summary>
    [AdaptMember(nameof(LowText.Text))]
    [ReactiveProperty]
    public string ID { get; set; } = string.Empty;

    #region I18nData
    [AdaptIgnore]
    private I18nResource<string, string> _i18nResource = null!;

    [AdaptIgnore]
    public required I18nResource<string, string> I18nResource
    {
        get => _i18nResource;
        set
        {
            //TODO:
            //if (_i18nResource is not null)
            //    I18nResource.I18nObjectInfos.Remove(this);
            //_i18nResource = value;
            //InitializeI18nResource();
        }
    }

    public void InitializeI18nResource()
    {
        //I18nResource?.I18nObjectInfos.Add(
        //    this,
        //    new I18nObjectInfo<string, string>(this, OnPropertyChanged).AddPropertyInfo(
        //        nameof(ID),
        //        ID,
        //        nameof(Text),
        //        true
        //    )
        //);
    }

    [AdaptIgnore]
    public string Text
    {
        get => I18nResource.GetCurrentCultureDataOrDefault(ID);
        set => I18nResource.SetCurrentCultureData(ID, value);
    }
    #endregion


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
        //TODO:
        //I18nResource.I18nObjectInfos.Remove(this);
    }
}
