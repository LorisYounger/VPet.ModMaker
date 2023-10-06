using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPet.ModMaker.Models;

namespace VPet.ModMaker.ViewModels.ModEdit;

public class AddCultureWindowVM
{
    /// <summary>
    /// 显示的文化
    /// </summary>
    public ObservableValue<ObservableCollection<string>> ShowCultures { get; } = new();

    /// <summary>
    /// 全部文化
    /// </summary>
    public static ObservableCollection<string> AllCultures { get; set; } =
        new(LinePutScript.Localization.WPF.LocalizeCore.AvailableCultures);

    /// <summary>
    /// 当前文化
    /// </summary>
    public ObservableValue<string> Culture { get; } = new();

    /// <summary>
    /// 搜索文化
    /// </summary>
    public ObservableValue<string> Search { get; } = new();

    public AddCultureWindowVM()
    {
        ShowCultures.Value = AllCultures;
        Search.ValueChanged += Search_ValueChanged;
    }

    private void Search_ValueChanged(string oldValue, string newValue)
    {
        if (string.IsNullOrWhiteSpace(newValue))
        {
            ShowCultures.Value = AllCultures;
        }
        else
        {
            ShowCultures.Value = new(
                AllCultures.Where(s => s.Contains(newValue, StringComparison.OrdinalIgnoreCase))
            );
        }
    }
}
