using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HKW.HKWMapper;
using HKW.HKWReactiveUI;
using HKW.HKWUtils.Observable;
using VPet.ModMaker.Models;
using VPet.ModMaker.ViewModels;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.Models;

/// <summary>
/// 选择文本模型
/// </summary>
[MapTo(typeof(SelectText), MapperConfig = typeof(SelectTextModelMapToSelectTextConfig))]
[MapFrom(typeof(SelectText), MapperConfig = typeof(SelectTextModelMapFromSelectTextConfig))]
[MapFrom(
    typeof(SelectTextModel),
    MapperConfig = typeof(SelectTextModelMapFromSelectTextModelConfig)
)]
public partial class SelectTextModel : ViewModelBase
{
    /// <inheritdoc/>
    public SelectTextModel() { }

    /// <inheritdoc/>
    /// <param name="model">选择文本模型</param>
    public SelectTextModel(SelectTextModel model)
        : this()
    {
        this.MapFromSelectTextModel(model);
    }

    /// <inheritdoc/>
    /// <param name="text">选择文本</param>
    /// <param name="i18nResource">I18n资源</param>
    [SetsRequiredMembers]
    public SelectTextModel(SelectText text, I18nResource<string, string> i18nResource)
        : this()
    {
        this.MapFromSelectText(text);
        if (text.Choose != ChooseID)
            i18nResource.ReplaceCultureDataKey(text.Choose, ChooseID);
        I18nResource = i18nResource;
    }

    /// <summary>
    /// 模式类型
    /// </summary>
    public static FrozenSet<ClickText.ModeType> ModeTypes => EnumInfo<ClickText.ModeType>.Values;

    /// <summary>
    /// 标签
    /// </summary>
    [ReactiveProperty]
    public string Tags { get; set; } = string.Empty;

    /// <summary>
    /// 跳转标签
    /// </summary>
    [ReactiveProperty]
    public string ToTags { get; set; } = string.Empty;

    /// <summary>
    /// ID
    /// </summary>
    [SelectTextModelMapToSelectTextProperty(nameof(SelectText.Text))]
    [SelectTextModelMapFromSelectTextProperty(nameof(SelectText.Text))]
    [ReactiveProperty]
    public string ID { get; set; } = string.Empty;

    /// <summary>
    /// 选择ID
    /// </summary>
    [NotifyPropertyChangeFrom(nameof(ID))]
    public string ChooseID => $"{ID}_Choose";

    /// <summary>
    /// I18n资源
    /// </summary>
    [MapIgnoreProperty]
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

    /// <summary>
    /// I18n对象
    /// </summary>
    [NotifyPropertyChangeFrom("")]
    public I18nObject<string, string> I18nObject => new(this);

    /// <summary>
    /// 文本
    /// </summary>
    [MapIgnoreProperty]
    [ReactiveI18nProperty("I18nResource", nameof(I18nObject), nameof(ID))]
    public string Text
    {
        get => I18nResource.GetCurrentCultureDataOrDefault(ID);
        set => I18nResource.SetCurrentCultureData(ID, value);
    }

    /// <summary>
    /// 选择
    /// </summary>
    [MapIgnoreProperty]
    [ReactiveI18nProperty("I18nResource", nameof(I18nObject), nameof(ChooseID))]
    public string Choose
    {
        get => I18nResource.GetCurrentCultureDataOrDefault(ChooseID);
        set => I18nResource.SetCurrentCultureData(ChooseID, value);
    }

    /// <summary>
    /// 宠物状态
    /// </summary>
    [SelectTextModelMapToSelectTextProperty(nameof(SelectText.Mode))]
    [SelectTextModelMapFromSelectTextProperty(nameof(SelectText.Mode))]
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
    public ObservableRange<double> Like { get; } = new(0, int.MaxValue);

    /// <summary>
    /// 健康度
    /// </summary>
    public ObservableRange<double> Health { get; } = new(0, int.MaxValue);

    /// <summary>
    /// 等级
    /// </summary>
    public ObservableRange<double> Level { get; } = new(0, int.MaxValue);

    /// <summary>
    /// 金钱
    /// </summary>
    public ObservableRange<double> Money { get; } = new(int.MinValue, int.MaxValue);

    /// <summary>
    /// 饱食度
    /// </summary>
    public ObservableRange<double> Food { get; } = new(0, int.MaxValue);

    /// <summary>
    /// 口渴度
    /// </summary>
    public ObservableRange<double> Drink { get; } = new(0, int.MaxValue);

    /// <summary>
    /// 心情值
    /// </summary>
    public ObservableRange<double> Feel { get; } = new(0, int.MaxValue);

    /// <summary>
    /// 体力值
    /// </summary>
    public ObservableRange<double> Strength { get; } = new(0, int.MaxValue);

    /// <inheritdoc/>
    public void Close()
    {
        I18nResource.I18nObjects.Remove(I18nObject);
        I18nObject.Close();
    }
}

internal class SelectTextModelMapFromSelectTextModelConfig
    : MapperConfig<SelectTextModel, SelectTextModel>
{
    public SelectTextModelMapFromSelectTextModelConfig()
    {
        AddMap(
            x => x.Mode,
            (s, t) =>
            {
                s.Mode = t.Mode.Clone();
            }
        );
        AddMap(
            x => x.Like,
            (s, t) =>
            {
                s.Like.SetValue(t.Like);
            }
        );
        AddMap(
            x => x.Health,
            (s, t) =>
            {
                s.Health.SetValue(t.Health);
            }
        );
        AddMap(
            x => x.Level,
            (s, t) =>
            {
                s.Level.SetValue(t.Level);
            }
        );
        AddMap(
            x => x.Money,
            (s, t) =>
            {
                s.Money.SetValue(t.Money);
            }
        );
        AddMap(
            x => x.Food,
            (s, t) =>
            {
                s.Food.SetValue(t.Food);
            }
        );
        AddMap(
            x => x.Drink,
            (s, t) =>
            {
                s.Drink.SetValue(t.Drink);
            }
        );
        AddMap(
            x => x.Feel,
            (s, t) =>
            {
                s.Feel.SetValue(t.Feel);
            }
        );
        AddMap(
            x => x.Strength,
            (s, t) =>
            {
                s.Strength.SetValue(t.Strength);
            }
        );
    }
}

internal class SelectTextModelMapToSelectTextConfig : MapperConfig<SelectTextModel, SelectText>
{
    public SelectTextModelMapToSelectTextConfig()
    {
        AddMap(
            x => x.Tags,
            (s, t) =>
            {
                t.Tags = new(
                    s.Tags.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim())
                );
            }
        );
        AddMap(
            x => x.ToTags,
            (s, t) =>
            {
                t.ToTags = new(
                    s.ToTags.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim())
                );
            }
        );
        AddMap(
            x => x.Mode,
            (s, t) =>
            {
                t.Mode = s.Mode.Value;
            }
        );
        AddMap(
            x => x.Like,
            (s, t) =>
            {
                t.LikeMax = s.Like.Max;
                t.LikeMin = s.Like.Min;
            }
        );
        AddMap(
            x => x.Health,
            (s, t) =>
            {
                t.HealthMax = s.Health.Max;
                t.HealthMin = s.Health.Min;
            }
        );
        AddMap(
            x => x.Level,
            (s, t) =>
            {
                t.LevelMax = s.Level.Max;
                t.LevelMin = s.Level.Min;
            }
        );
        AddMap(
            x => x.Money,
            (s, t) =>
            {
                t.MoneyMax = s.Money.Max;
                t.MoneyMin = s.Money.Min;
            }
        );
        AddMap(
            x => x.Food,
            (s, t) =>
            {
                t.FoodMax = s.Food.Max;
                t.FoodMin = s.Food.Min;
            }
        );
        AddMap(
            x => x.Drink,
            (s, t) =>
            {
                t.DrinkMax = s.Drink.Max;
                t.DrinkMin = s.Drink.Min;
            }
        );
        AddMap(
            x => x.Feel,
            (s, t) =>
            {
                t.FeelMax = s.Feel.Max;
                t.FeelMin = s.Feel.Min;
            }
        );
        AddMap(
            x => x.Strength,
            (s, t) =>
            {
                t.StrengthMax = s.Strength.Max;
                t.StrengthMin = s.Strength.Min;
            }
        );
    }
}

internal class SelectTextModelMapFromSelectTextConfig : MapperConfig<SelectTextModel, SelectText>
{
    public SelectTextModelMapFromSelectTextConfig()
    {
        AddMap(
            x => x.Tags,
            (s, t) =>
            {
                s.Tags = string.Join(",", s.Tags);
            }
        );
        AddMap(
            x => x.ToTags,
            (s, t) =>
            {
                s.ToTags = string.Join(",", s.ToTags);
            }
        );
        AddMap(
            x => x.Mode,
            (s, t) =>
            {
                s.Mode = new(t.Mode);
            }
        );
        AddMap(
            x => x.Like,
            (s, t) =>
            {
                s.Like.SetValue(t.LikeMin, t.LikeMax);
            }
        );
        AddMap(
            x => x.Health,
            (s, t) =>
            {
                s.Health.SetValue(t.HealthMin, t.HealthMax);
            }
        );
        AddMap(
            x => x.Level,
            (s, t) =>
            {
                s.Level.SetValue(t.LevelMin, t.LevelMax);
            }
        );
        AddMap(
            x => x.Money,
            (s, t) =>
            {
                s.Money.SetValue(t.MoneyMin, t.MoneyMax);
            }
        );
        AddMap(
            x => x.Food,
            (s, t) =>
            {
                s.Food.SetValue(t.FoodMin, t.FoodMax);
            }
        );
        AddMap(
            x => x.Drink,
            (s, t) =>
            {
                s.Drink.SetValue(t.DrinkMin, t.DrinkMax);
            }
        );
        AddMap(
            x => x.Feel,
            (s, t) =>
            {
                s.Feel.SetValue(t.FeelMin, t.FeelMax);
            }
        );
        AddMap(
            x => x.Strength,
            (s, t) =>
            {
                s.Strength.SetValue(t.StrengthMin, t.StrengthMax);
            }
        );
    }
}
