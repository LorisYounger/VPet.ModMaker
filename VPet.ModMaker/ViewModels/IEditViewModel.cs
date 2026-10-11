using System;
using System.Collections.Generic;
using System.Text;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels;

public enum EditType
{
    Food,
}

public interface IEditViewModel
{
    public string DisplayText { get; }

    public ModInfoModel ModInfo { get; set; }
}
