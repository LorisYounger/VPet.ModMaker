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
    /// <summary>
    /// I18n资源
    /// </summary>
    public static I18nResource<string, string> I18nResource => ModInfoModel.Current.I18nResource;
    #region Value
    public LowTextModel? OldLowText { get; set; }

    #region LowText
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private LowTextModel _lowText = new() { I18nResource = ModInfoModel.Current.I18nResource };

    public LowTextModel LowText
    {
        get => _lowText;
        set => SetProperty(ref _lowText, value);
    }
    #endregion

    #endregion

    public LowTextEditWindowVM() { }
}
