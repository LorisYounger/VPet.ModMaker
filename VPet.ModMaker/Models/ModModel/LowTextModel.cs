using System.Collections.Frozen;
using System.Collections.Immutable;
using CommunityToolkit.Mvvm.ComponentModel;
using HKW.HKWUtils;
using HKW.Mapper;
using HKW.MVVM;
using VPet_Simulator.Windows.Interface;
using VPet.ModMaker.ViewModels;

namespace VPet.ModMaker.Models;

/// <summary>
/// 低状态文本
/// </summary>
[MapTarget(typeof(LowText))]
[MapTarget(typeof(LowTextModel))]
public sealed partial class LowTextModel : ObservableModelBase, IDisposable
{
    /// <inheritdoc/>
    public LowTextModel(ObservableI18nResource<string, string> i18nResource)
    {
        I18nResource = i18nResource;
    }

    /// <inheritdoc/>
    /// <param name="lowText">低状态文本模型</param>
    public LowTextModel(LowTextModel lowText, ObservableI18nResource<string, string> i18nResource)
    {
        this.MapFrom(lowText);
        I18nResource = i18nResource;
    }

    /// <inheritdoc/>
    /// <param name="lowText">低状态文本</param>
    public LowTextModel(LowText lowText, ObservableI18nResource<string, string> i18nResource)
    {
        this.MapFrom(lowText);
        I18nResource = i18nResource;
    }

    /// <summary>
    /// 状态类型
    /// </summary>
    public static ImmutableArray<LowText.ModeType> ModeTypes =>
        EnumInfo<LowText.ModeType>.StaticValues;

    /// <summary>
    /// 好感度类型
    /// </summary>
    public static ImmutableArray<LowText.LikeType> LikeTypes =>
        EnumInfo<LowText.LikeType>.StaticValues;

    /// <summary>
    /// 体力类型
    /// </summary>
    public static ImmutableArray<LowText.StrengthType> StrengthTypes =>
        EnumInfo<LowText.StrengthType>.StaticValues;

    /// <summary>
    /// ID
    /// </summary>
    [ObservableProperty]
    [MapProperty(typeof(LowText), nameof(LowText.Text))]
    public string ID { get; set; } = string.Empty;

    /// <summary>
    /// I18n资源
    /// </summary>
    [MapIgnoreProperty]
    [ObservableProperty]
    public ObservableI18nResource<string, string> I18nResource { get; set; }

    partial void OnI18nResourceChanged(
        ObservableI18nResource<string, string> oldValue,
        ObservableI18nResource<string, string> newValue
    )
    {
        I18nObserver.Resource = newValue;
    }

    [MapIgnoreProperty]
    public I18nObserver<LowTextModel, string, string> I18nObserver =>
        field ??= new(I18nResource, this, (s, p) => s.OnPropertyChanged(p))
        {
            new(s => s.ID, nameof(Text)),
        };

    /// <summary>
    /// 点击文本
    /// </summary>
    [MapIgnoreProperty]
    public string Text
    {
        get => I18nObserver.GetDataOrDefault(ID);
        set => I18nObserver.SetData(ID, value);
    }

    /// <summary>
    /// 状态
    /// </summary>
    [ObservableProperty]
    public LowText.ModeType Mode { get; set; }

    /// <summary>
    /// 体力
    /// </summary>
    [ObservableProperty]
    public LowText.StrengthType Strength { get; set; }

    /// <summary>
    /// 好感度
    /// </summary>
    [ObservableProperty]
    public LowText.LikeType Like { get; set; }

    /// <inheritdoc/>
    public void Dispose()
    {
        I18nObserver?.Dispose();
    }
}
