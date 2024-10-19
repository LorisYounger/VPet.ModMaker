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

public partial class ClickTextEditWindowVM : ViewModelBase
{
    /// <summary>
    /// I18n资源
    /// </summary>
    public static I18nResource<string, string> I18nResource => ModInfoModel.Current.I18nResource;

    #region Value
    /// <summary>
    /// 旧点击文本
    /// </summary>
    public ClickTextModel? OldClickText { get; set; }

    /// <summary>
    /// 点击文本
    /// </summary>
    [ReactiveProperty]
    public ClickTextModel ClickText { get; set; } =
        new() { I18nResource = ModInfoModel.Current.I18nResource };
    #endregion
    public ClickTextEditWindowVM() { }
}
