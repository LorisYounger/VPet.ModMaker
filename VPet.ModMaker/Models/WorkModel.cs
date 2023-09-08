﻿using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace VPet.ModMaker.Models;

public class WorkModel : I18nModel<I18nWorkModel>
{
    public static ObservableCollection<VPet_Simulator.Core.GraphHelper.Work.WorkType> WorkTypes { get; } =
        new(
            Enum.GetValues(typeof(VPet_Simulator.Core.GraphHelper.Work.WorkType))
                .Cast<VPet_Simulator.Core.GraphHelper.Work.WorkType>()
        );

    //public VPet_Simulator.Core.GraphHelper.Work
    public ObservableValue<VPet_Simulator.Core.GraphHelper.Work.WorkType> WorkType { get; } =
        new(VPet_Simulator.Core.GraphHelper.Work.WorkType.Work);

    public ObservableValue<string> Id { get; } = new();

    public ObservableValue<string> Graph { get; } = new();
    public ObservableValue<double> MoneyLevel { get; } = new();
    public ObservableValue<double> MoneyBase { get; } = new();
    public ObservableValue<double> StrengthFood { get; } = new();
    public ObservableValue<double> StrengthDrink { get; } = new();
    public ObservableValue<double> Feeling { get; } = new();
    public ObservableValue<int> LevelLimit { get; } = new();
    public ObservableValue<int> Time { get; } = new();
    public ObservableValue<double> FinishBonus { get; } = new();

    public ObservableValue<SolidColorBrush> BorderBrush { get; } =
        new(new((Color)ColorConverter.ConvertFromString("#FF0290D5")));
    public ObservableValue<SolidColorBrush> Background { get; } =
        new(new((Color)ColorConverter.ConvertFromString("#FF81d4fa")));
    public ObservableValue<SolidColorBrush> Foreground { get; } =
        new(new((Color)ColorConverter.ConvertFromString("#FF0286C6")));
    public ObservableValue<SolidColorBrush> ButtonBackground { get; } =
        new(new((Color)ColorConverter.ConvertFromString("#AA0286C6")));
    public ObservableValue<SolidColorBrush> ButtonForeground { get; } =
        new(new((Color)ColorConverter.ConvertFromString("#FFffffff")));

    public ObservableValue<double> Left { get; } = new(100);
    public ObservableValue<double> Top { get; } = new(160);
    public ObservableValue<double> Width { get; } = new(300);

    public WorkModel() { }

    public WorkModel(WorkModel model)
        : this()
    {
        WorkType.Value = model.WorkType.Value;
        Id.Value = model.Id.Value;
        Graph.Value = model.Graph.Value;
        MoneyLevel.Value = model.MoneyLevel.Value;
        MoneyBase.Value = model.MoneyBase.Value;
        StrengthFood.Value = model.StrengthFood.Value;
        StrengthDrink.Value = model.StrengthDrink.Value;
        Feeling.Value = model.Feeling.Value;
        LevelLimit.Value = model.LevelLimit.Value;
        Time.Value = model.Time.Value;
        FinishBonus.Value = model.FinishBonus.Value;

        BorderBrush.Value = model.BorderBrush.Value;
        Background.Value = model.Background.Value;
        ButtonBackground.Value = model.ButtonBackground.Value;
        ButtonForeground.Value = model.ButtonForeground.Value;
        Foreground.Value = model.Foreground.Value;

        Left.Value = model.Left.Value;
        Top.Value = model.Top.Value;
        Width.Value = model.Width.Value;

        foreach (var item in model.I18nDatas)
            I18nDatas[item.Key] = item.Value.Copy();
        CurrentI18nData.Value = I18nDatas[I18nHelper.Current.CultureName.Value];
    }

    public WorkModel(VPet_Simulator.Core.GraphHelper.Work work)
        : this()
    {
        WorkType.Value = work.Type;
        Id.Value = work.Name;
        Graph.Value = work.Graph;
        MoneyLevel.Value = work.MoneyLevel;
        MoneyBase.Value = work.MoneyBase;
        StrengthFood.Value = work.StrengthFood;
        StrengthDrink.Value = work.StrengthDrink;
        Feeling.Value = work.Feeling;
        LevelLimit.Value = work.LevelLimit;
        Time.Value = work.Time;
        FinishBonus.Value = work.FinishBonus;

        BorderBrush.Value = new((Color)ColorConverter.ConvertFromString("#FF" + work.BorderBrush));
        Background.Value = new((Color)ColorConverter.ConvertFromString("#FF" + work.Background));
        Foreground.Value = new((Color)ColorConverter.ConvertFromString("#FF" + work.Foreground));
        ButtonBackground.Value = new(
            (Color)ColorConverter.ConvertFromString("#AA" + work.ButtonBackground)
        );
        ButtonForeground.Value = new(
            (Color)ColorConverter.ConvertFromString("#FF" + work.ButtonForeground)
        );

        Left.Value = work.Left;
        Top.Value = work.Top;
        Width.Value = work.Width;
    }

    public VPet_Simulator.Core.GraphHelper.Work ToWork()
    {
        return new()
        {
            Type = WorkType.Value,
            Name = Id.Value,
            Graph = Graph.Value,
            MoneyLevel = MoneyLevel.Value,
            MoneyBase = MoneyBase.Value,
            StrengthFood = StrengthFood.Value,
            StrengthDrink = StrengthDrink.Value,
            Feeling = Feeling.Value,
            LevelLimit = LevelLimit.Value,
            Time = Time.Value,
            FinishBonus = FinishBonus.Value,
            //
            BorderBrush = BorderBrush.Value.ToString().Substring(3),
            Background = Background.Value.ToString().Substring(3),
            ButtonBackground = ButtonBackground.Value.ToString().Substring(3),
            ButtonForeground = ButtonForeground.Value.ToString().Substring(3),
            Foreground = Foreground.Value.ToString().Substring(3),
            //
            Left = Left.Value,
            Top = Top.Value,
            Width = Width.Value,
        };
    }
}

public class I18nWorkModel
{
    public ObservableValue<string> Name { get; } = new();

    public I18nWorkModel Copy()
    {
        var result = new I18nWorkModel();
        result.Name.Value = Name.Value;
        return result;
    }
}