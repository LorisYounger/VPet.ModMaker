using HanumanInstitute.MvvmDialogs;
using HKW.MVVM;
using HKW.MVVM.SourceGenerator;
using Microsoft.Extensions.Logging;

namespace VPet.ModMaker.ViewModels;

/// <summary>
/// 视图模型基类
/// </summary>
public abstract partial class ViewModelBase : ObservableObjectEx, IEnableLogger
{
    [DIProperty]
    public required IDialogService DialogService { get; init; }
}
