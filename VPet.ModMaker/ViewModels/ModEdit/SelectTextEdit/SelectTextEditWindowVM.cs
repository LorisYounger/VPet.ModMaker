using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HKW.HKWUtils.Observable;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels.ModEdit.SelectTextEdit;

public class SelectTextEditWindowVM : ObservableObjectX
{
    /// <summary>
    /// I18n资源
    /// </summary>
    public static I18nResource<string, string> I18nResource => ModInfoModel.Current.I18nResource;
    #region Value
    public SelectTextModel? OldSelectText { get; set; }

    #region SelectText
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private SelectTextModel _selectText =
        new() { I18nResource = ModInfoModel.Current.I18nResource };

    public SelectTextModel SelectText
    {
        get => _selectText;
        set => SetProperty(ref _selectText, value);
    }
    #endregion
    #endregion
}
