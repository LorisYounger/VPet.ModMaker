using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HKW.HKWUtils.Observable;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels.ModEdit.SelectTextEdit;

public class SelectTextEditWindowVM : ObservableObjectX<SelectTextEditWindowVM>
{
    public static I18nHelper I18nData => I18nHelper.Current;
    #region Value
    public SelectTextModel? OldSelectText { get; set; }

    #region SelectText
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private SelectTextModel _selectText = new();

    public SelectTextModel SelectText
    {
        get => _selectText;
        set => SetProperty(ref _selectText, value);
    }
    #endregion
    #endregion
}
