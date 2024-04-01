using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HKW.HKWUtils.Observable;
using VPet.ModMaker.Models;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.ViewModels.ModEdit.LowTextEdit;

public class LowTextEditWindowVM : ObservableObjectX
{
    public static I18nHelper I18nData => I18nHelper.Current;
    #region Value
    public LowTextModel? OldLowText { get; set; }

    #region LowText
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private LowTextModel _lowText = new();

    public LowTextModel LowText
    {
        get => _lowText;
        set => SetProperty(ref _lowText, value);
    }
    #endregion

    #endregion

    public LowTextEditWindowVM() { }
}
