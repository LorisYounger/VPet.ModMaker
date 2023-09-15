using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPet.ModMaker.Models;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.ViewModels.ModEdit.LowTextEdit;

public class LowTextEditWindowVM
{
    public I18nHelper I18nData => I18nHelper.Current;
    #region Value
    public LowTextModel OldLowText { get; set; }
    public ObservableValue<LowTextModel> LowText { get; } = new(new());

    #endregion

    public LowTextEditWindowVM() { }
}
