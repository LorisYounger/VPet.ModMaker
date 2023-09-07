using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels.ModEdit.WorkEdit;

public class WorkEditWindowVM
{
    #region Value
    public PetModel CurrentPet { get; set; }
    public WorkModel OldWork { get; set; }
    public ObservableValue<WorkModel> Work { get; } = new(new());
    #endregion
}
