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
        // 没有 Text
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
