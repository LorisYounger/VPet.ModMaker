using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPet.ModMaker.Models;

[DebuggerDisplay("{Id}, Count = {Datas.Count}")]
public class I18nData
{
    public ObservableValue<string> Id { get; } = new();
    public ObservableCollection<ObservableValue<string>> Datas { get; } = new();
}
