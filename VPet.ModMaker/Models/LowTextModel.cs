using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.Models;

public class LowTextModel : I18nModel<I18nLowTextModel>
{
    public string Text { get; set; }
    public ObservableValue<string> Id { get; } = new();
    public ObservableValue<LowText.ModeType> Mode { get; } = new();
    public ObservableValue<LowText.StrengthType> Strength { get; } = new();
    public ObservableValue<LowText.LikeType> Like { get; } = new();

    public LowTextModel() { }

    public LowTextModel(LowTextModel lowText)
        : this()
    {
        Mode.Value = lowText.Mode.Value;
        Strength.Value = lowText.Strength.Value;
        Like.Value = lowText.Like.Value;
        foreach (var item in lowText.I18nDatas)
        {
            I18nDatas[item.Key] = item.Value;
            I18nDatas[item.Key].Text.Value = lowText.I18nDatas[item.Key].Text.Value;
        }
        CurrentI18nData.Value = I18nDatas[I18nHelper.Current.CultureName.Value];
    }

    public LowTextModel(LowText lowText)
        : this()
    {
        Text = lowText.Text;
        Mode.Value = lowText.Mode;
        Strength.Value = lowText.Strength;
        Like.Value = lowText.Like;
    }

    public void Close() { }

    public LowText ToLowText()
    {
        return new()
        {
            Text = Text,
            Mode = Mode.Value,
            Strength = Strength.Value,
            Like = Like.Value,
        };
    }
}

public class I18nLowTextModel
{
    public ObservableValue<string> Text { get; } = new();
}
