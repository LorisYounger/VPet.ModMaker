using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.Models;

public class SelectTextModel : ClickTextModel
{
    public ObservableValue<int> Exp { get; } = new();
    public ObservableValue<double> Money { get; } = new();
    public ObservableValue<double> Strength { get; } = new();
    public ObservableValue<double> StrengthFood { get; } = new();
    public ObservableValue<double> StrengthDrink { get; } = new();
    public ObservableValue<double> Feeling { get; } = new();
    public ObservableValue<double> Health { get; } = new();
    public ObservableValue<double> Likability { get; } = new();
    public ObservableValue<string> Tags { get; } = new();
    public ObservableValue<string> ToTags { get; } = new();

    public SelectTextModel() { }
}
