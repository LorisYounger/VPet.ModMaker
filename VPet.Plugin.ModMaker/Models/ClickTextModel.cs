using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPet_Simulator.Windows.Interface;

namespace VPet.Plugin.ModMaker.Models;

public class ClickTextModel : I18nModel<I18nClickTextModel>
{
    public string Text { get; set; }
    public ObservableValue<string> Id { get; } = new();
    public ObservableValue<ClickText.ModeType> Mode { get; } = new();

    public ObservableValue<string> Working { get; } = new();
    public ObservableValue<double> LikeMin { get; } = new();
    public ObservableValue<double> LikeMax { get; } = new();
    public ObservableValue<VPet_Simulator.Core.Main.WorkingState> WorkingState { get; } = new();
    public ObservableValue<ClickText.DayTime> DayTime { get; } = new();

    public ClickTextModel() { }

    public ClickTextModel(ClickTextModel clickText)
        : this()
    {
        Mode.Value = clickText.Mode.Value;
        Working.Value = clickText.Working.Value;
        WorkingState.Value = clickText.WorkingState.Value;
        LikeMax.Value = clickText.LikeMax.Value;
        LikeMin.Value = clickText.LikeMin.Value;
        DayTime.Value = clickText.DayTime.Value;
    }

    public ClickTextModel(ClickText clickText)
        : this()
    {
        Text = clickText.Text;
        Mode.Value = clickText.Mode;
        Working.Value = clickText.Working;
        WorkingState.Value = clickText.State;
        DayTime.Value = clickText.DaiTime;
        LikeMax.Value = clickText.LikeMax;
        LikeMin.Value = clickText.LikeMin;
    }

    public ClickText ToClickText()
    {
        return new()
        {
            Text = Text,
            Mode = Mode.Value,
            Working = Working.Value,
            State = WorkingState.Value,
            LikeMax = LikeMax.Value,
            LikeMin = LikeMin.Value,
            DaiTime = DayTime.Value,
        };
    }
}

public class I18nClickTextModel
{
    public ObservableValue<string> Text { get; } = new();
}
