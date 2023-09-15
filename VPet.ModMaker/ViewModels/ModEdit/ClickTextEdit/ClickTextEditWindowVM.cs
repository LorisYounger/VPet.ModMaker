using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPet.ModMaker.Models;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.ViewModels.ModEdit.ClickTextEdit;

public class ClickTextEditWindowVM
{
    public I18nHelper I18nData => I18nHelper.Current;

    #region Value

    public ClickTextModel OldClickText { get; set; }
    public ObservableValue<ClickTextModel> ClickText { get; } = new(new());
    #endregion
    public ClickTextEditWindowVM() { }
}
