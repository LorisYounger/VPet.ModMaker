using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HKW.HKWReactiveUI;
using HKW.HKWUtils.Observable;
using Mapster;
using VPet.ModMaker.Models;
using VPet.ModMaker.ViewModels;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.Models;

/// <summary>
/// 选择文本模型
/// </summary>
public partial class SelectTextModel : ViewModelBase
{
    public SelectTextModel() { }

    public SelectTextModel(SelectTextModel model)
        : this()
    {
        //model.Adapt(this);
        //Like.Min = -100;
        ID = model.ID;
        Mode.Value = model.Mode.Value;
        Tags = model.Tags;
        ToTags = model.ToTags;
        Like = model.Like.Clone();
        Health = model.Health.Clone();
        Level = model.Level.Clone();
        Money = model.Money.Clone();
        Food = model.Food.Clone();
        Drink = model.Drink.Clone();
        Feel = model.Feel.Clone();
        Strength = model.Strength.Clone();
    }

    public SelectTextModel(SelectText text)
        : this()
    {
        ID = text.Text;
        ChooseID = text.Choose ?? string.Empty;
        Mode.Value = text.Mode;
        Tags = text.Tags is null ? string.Empty : string.Join(", ", text.Tags);
        ToTags = text.ToTags is null ? string.Empty : string.Join(", ", text.ToTags);
        Like = new(text.LikeMin, text.LikeMax);
        Health = new(text.HealthMin, text.HealthMax);
        Level = new(text.LevelMin, text.LevelMax);
        Money = new(text.MoneyMin, text.MoneyMax);
        Food = new(text.FoodMin, text.FoodMax);
        Drink = new(text.DrinkMin, text.DrinkMax);
        Feel = new(text.FeelMin, text.FeelMax);
        Strength = new(text.StrengthMin, text.StrengthMax);
    }

    /// <summary>
    /// 模式类型
    /// </summary>
    public static FrozenSet<ClickText.ModeType> ModeTypes => ClickTextModel.ModeTypes;

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
    [ReactiveProperty]
    public string ID { get; set; } = string.Empty;

    /// <summary>
    /// 选择Id
    /// </summary>
    [ReactiveProperty]
    public string ChooseID { get; set; } = string.Empty;

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
        //        [(nameof(ID), ID, nameof(Text)), (nameof(ChooseID), ChooseID, nameof(Choose))],
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

    [AdaptIgnore]
    public string Choose
    {
        get => I18nResource.GetCurrentCultureDataOrDefault(ChooseID);
        set => I18nResource.SetCurrentCultureData(ChooseID, value);
    }
    #endregion

    /// <summary>
    /// 宠物状态
    /// </summary>
    public ObservableEnum<ClickText.ModeType> Mode { get; } =
        new(
            ClickText.ModeType.Happy
                | ClickText.ModeType.Nomal
                | ClickText.ModeType.PoorCondition
                | ClickText.ModeType.Ill,
            (v, f) => v |= f,
            (v, f) => v &= f
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

    public void RefreshID()
    {
        ChooseID = $"{ID}_{nameof(ChooseID)}";
    }

    private static readonly char[] _splitChars = [',', ' '];

    public SelectText ToSelectText()
    {
        return new()
        {
            Text = ID,
            Choose = ChooseID,
            Mode = Mode.Value,
            Tags = new(Tags.Split(_splitChars, StringSplitOptions.RemoveEmptyEntries)),
            ToTags = new(ToTags.Split(_splitChars, StringSplitOptions.RemoveEmptyEntries)),
            LikeMax = Like.Max,
            LikeMin = Like.Min,
            HealthMin = Health.Min,
            HealthMax = Health.Max,
            LevelMin = Level.Min,
            LevelMax = Level.Max,
            MoneyMin = Money.Min,
            MoneyMax = Money.Max,
            FoodMin = Food.Min,
            FoodMax = Food.Max,
            DrinkMin = Drink.Min,
            DrinkMax = Drink.Max,
            FeelMin = Feel.Min,
            FeelMax = Feel.Max,
            StrengthMin = Strength.Min,
            StrengthMax = Strength.Max,
        };
    }

    public void Close()
    {
        //I18nResource.I18nObjectInfos.Remove(this);
    }
}
