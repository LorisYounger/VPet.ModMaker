using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using CommunityToolkit.Mvvm.ComponentModel;
using HKW.HKWUtils;
using HKW.HKWUtils.Observable;
using HKW.Mapper;
using HKW.MVVM;
using HKW.MVVM.SourceGenerator;
using VPet_Simulator.Windows.Interface;
using VPet.ModMaker.ViewModels;

namespace VPet.ModMaker.Models;

/// <summary>
/// 选择文本模型
/// </summary>
[MapTarget(typeof(SelectText))]
[MapTarget(typeof(SelectTextModel))]
public sealed partial class SelectTextModel : ObservableModelBase, IDisposable
{
    /// <inheritdoc/>
    public SelectTextModel() { }

    /// <inheritdoc/>
    /// <param name="model">选择文本模型</param>
    public SelectTextModel(SelectTextModel model)
        : this()
    {
        this.MapFrom(model);
    }

    /// <inheritdoc/>
    /// <param name="text">选择文本</param>
    /// <param name="i18nResource">I18n资源</param>
    [SetsRequiredMembers]
    public SelectTextModel(SelectText text, ObservableI18nResource<string, string> i18nResource)
        : this()
    {
        this.MapFrom(text);
        I18nResource = i18nResource;
    }

    /// <summary>
    /// 模式类型
    /// </summary>
    public static ImmutableArray<ClickText.ModeType> ModeTypes =>
        EnumInfo<ClickText.ModeType>.StaticValues;

    /// <summary>
    /// 标签
    /// </summary>
    [ObservableProperty]
    [MapProperty(typeof(SelectText), typeof(StringSpiltToListConverter))]
    public string Tags { get; set; } = string.Empty;

    /// <summary>
    /// 跳转标签
    /// </summary>
    [ObservableProperty]
    [MapProperty(typeof(SelectText), typeof(StringSpiltToListConverter))]
    public string ToTags { get; set; } = string.Empty;

    /// <summary>
    /// ID
    /// </summary>
    [ObservableProperty]
    [MapProperty(typeof(SelectText), nameof(SelectText.Text))]
    public string ID { get; set; } = string.Empty;

    /// <summary>
    /// 选择ID
    /// </summary>
    [ObservableProperty]
    [MapProperty(typeof(SelectText), nameof(SelectText.Choose))]
    public string ChooseID { get; set; } = string.Empty;

    /// <summary>
    /// I18n资源
    /// </summary>
    [MapIgnoreProperty]
    [ObservableProperty]
    public required ObservableI18nResource<string, string> I18nResource { get; set; }

    partial void OnI18nResourceChanged(
        ObservableI18nResource<string, string> oldValue,
        ObservableI18nResource<string, string> newValue
    )
    {
        I18nObserver.Resource = newValue;
    }

    [MapIgnoreProperty]
    public I18nObserver<SelectTextModel, string, string> I18nObserver =>
        field ??= new(I18nResource, this, (s, p) => s.OnPropertyChanged(p))
        {
            new(s => s.ID, nameof(Text)),
            new(s => s.ChooseID, nameof(Choose)),
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
    /// 选择文本
    /// </summary>
    [MapIgnoreProperty]
    public string Choose
    {
        get => I18nObserver.GetDataOrDefault(ChooseID);
        set => I18nObserver.SetData(ChooseID, value);
    }

    /// <summary>
    /// 宠物状态
    /// </summary>
    [MapProperty(typeof(SelectText), typeof(ObservableEnumConverter))]
    public ObservableEnum<ClickText.ModeType> Mode { get; set; } =
        new(
            ClickText.ModeType.Happy
                | ClickText.ModeType.Nomal
                | ClickText.ModeType.PoorCondition
                | ClickText.ModeType.Ill
        );

    /// <summary>
    /// 好感度
    /// </summary>
    [MapCompositeProperty(
        typeof(SelectText),
        typeof(ObservableRangeConverter<double>),
        nameof(SelectText.LikeMin),
        nameof(SelectText.LikeMax)
    )]
    public ObservableRange<double> Like { get; set; } = new(0, int.MaxValue);

    /// <summary>
    /// 健康度
    /// </summary>
    [MapCompositeProperty(
        typeof(SelectText),
        typeof(ObservableRangeConverter<double>),
        nameof(SelectText.HealthMin),
        nameof(SelectText.HealthMax)
    )]
    public ObservableRange<double> Health { get; set; } = new(0, int.MaxValue);

    /// <summary>
    /// 等级
    /// </summary>
    [MapCompositeProperty(
        typeof(SelectText),
        typeof(ObservableRangeConverter<double>),
        nameof(SelectText.LevelMin),
        nameof(SelectText.LevelMax)
    )]
    public ObservableRange<double> Level { get; set; } = new(0, int.MaxValue);

    /// <summary>
    /// 金钱
    /// </summary>
    [MapCompositeProperty(
        typeof(SelectText),
        typeof(ObservableRangeConverter<double>),
        nameof(SelectText.MoneyMin),
        nameof(SelectText.MoneyMax)
    )]
    public ObservableRange<double> Money { get; set; } = new(int.MinValue, int.MaxValue);

    /// <summary>
    /// 饱食度
    /// </summary>
    [MapCompositeProperty(
        typeof(SelectText),
        typeof(ObservableRangeConverter<double>),
        nameof(SelectText.FoodMin),
        nameof(SelectText.FoodMax)
    )]
    public ObservableRange<double> Food { get; set; } = new(0, int.MaxValue);

    /// <summary>
    /// 口渴度
    /// </summary>
    [MapCompositeProperty(
        typeof(SelectText),
        typeof(ObservableRangeConverter<double>),
        nameof(SelectText.DrinkMin),
        nameof(SelectText.DrinkMax)
    )]
    public ObservableRange<double> Drink { get; set; } = new(0, int.MaxValue);

    /// <summary>
    /// 心情值
    /// </summary>
    [MapCompositeProperty(
        typeof(SelectText),
        typeof(ObservableRangeConverter<double>),
        nameof(SelectText.FeelMin),
        nameof(SelectText.FeelMax)
    )]
    public ObservableRange<double> Feel { get; set; } = new(0, int.MaxValue);

    /// <summary>
    /// 体力值
    /// </summary>
    [MapCompositeProperty(
        typeof(SelectText),
        typeof(ObservableRangeConverter<double>),
        nameof(SelectText.StrengthMin),
        nameof(SelectText.StrengthMax)
    )]
    public ObservableRange<double> Strength { get; set; } = new(0, int.MaxValue);

    /// <inheritdoc/>
    public void Dispose()
    {
        I18nObserver?.Dispose();
    }
}

public sealed class StringSpiltToListConverter : IMapConverter<string, List<string>>
{
    public List<string> Convert(object source, string value)
    {
        return value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToList();
    }

    public string ConvertBack(object target, List<string> value)
    {
        return string.Join(", ", value);
    }
}
