using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HKW.HKWReactiveUI;
using HKW.HKWUtils.Observable;
using VPet.ModMaker.Models;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.ViewModels.ModEdit;

public partial class LowTextEditWindowVM : ViewModelBase
{
    /// <summary>
    /// I18n资源
    /// </summary>
    public static I18nResource<string, string> I18nResource => ModInfoModel.Current.I18nResource;

    public LowTextModel? OldLowText { get; set; }

    [ReactiveProperty]
    public LowTextModel LowText { get; set; } =
        new() { I18nResource = ModInfoModel.Current.I18nResource };

    public LowTextEditWindowVM() { }
}
