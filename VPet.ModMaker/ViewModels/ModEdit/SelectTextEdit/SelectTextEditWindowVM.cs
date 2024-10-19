using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HKW.HKWReactiveUI;
using HKW.HKWUtils.Observable;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels.ModEdit;

public partial class SelectTextEditWindowVM : ViewModelBase
{
    /// <summary>
    /// I18n资源
    /// </summary>
    public static I18nResource<string, string> I18nResource => ModInfoModel.Current.I18nResource;
    #region Value
    public SelectTextModel? OldSelectText { get; set; }

    [ReactiveProperty]
    public SelectTextModel SelectText { get; set; } =
        new() { I18nResource = ModInfoModel.Current.I18nResource };
    #endregion
}
