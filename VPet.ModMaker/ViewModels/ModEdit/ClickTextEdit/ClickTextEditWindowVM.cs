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

namespace VPet.ModMaker.ViewModels.ModEdit.ClickTextEdit;

public class ClickTextEditWindowVM : ObservableObjectX<ClickTextEditWindowVM>
{
    public I18nHelper I18nData => I18nHelper.Current;

    #region Value
    /// <summary>
    /// 旧点击文本
    /// </summary>
    public ClickTextModel OldClickText { get; set; }

    #region ClickText
    [DebuggerBrowsable(DebuggerBrowsableState.Never)]
    private ClickTextModel _clickText = new();

    /// <summary>
    /// 点击文本
    /// </summary>
    public ClickTextModel ClickText
    {
        get => _clickText;
        set => SetProperty(ref _clickText, value);
    }
    #endregion
    #endregion
    public ClickTextEditWindowVM() { }
}
