using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPet.ModMaker.Models;

public class I18nData
{
    public ObservableValue<string> Id { get; } = new();
    public ObservableCollection<string> Cultures { get; } = new();
    public ObservableCollection<ObservableValue<string>> Datas { get; } = new();
}
