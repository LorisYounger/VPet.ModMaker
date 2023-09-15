using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels.ModEdit.SelectTextEdit;

public class SelectTextEditWindowVM
{
    public I18nHelper I18nData => I18nHelper.Current;
    #region Value
    public SelectTextModel OldSelectText { get; set; }
    public ObservableValue<SelectTextModel> SelectText { get; } = new(new());
    #endregion
}
