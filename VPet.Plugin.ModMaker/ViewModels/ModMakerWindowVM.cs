using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPet.Plugin.ModMaker.Models;
using VPet.Plugin.ModMaker.Views;
using VPet.Plugin.ModMaker.Views.ModEdit;

namespace VPet.Plugin.ModMaker.ViewModels;

public class ModMakerWindowVM
{
    public ModMakerWindow ModMakerWindow { get; }

    public ModEditWindow ModEditWindow { get; private set; }

    public ObservableCommand CreateNewModCommand { get; set; } = new();

    public ObservableValue<string> ModFilterText { get; } = new();

    public ObservableCollection<ModInfoModel> ShowMods { get; set; }
    public ObservableCollection<ModInfoModel> Mods { get; } = new();

    public ModMakerWindowVM() { }

    public ModMakerWindowVM(ModMakerWindow window)
    {
        LoadMods();
        ModMakerWindow = window;
        ShowMods = Mods;
        CreateNewModCommand.ExecuteAction = CreateNewMod;
        ModFilterText.ValueChanged += ModFilterText_ValueChanged;
    }

    private void LoadMods()
    {
        var dic = Directory.CreateDirectory(ModMakerInfo.BaseDirectory);
        foreach (var modDic in dic.EnumerateDirectories())
        {
            var mod = new ModLoader(modDic);
            if (mod.SuccessLoad is false)
                continue;
            var modModel = new ModInfoModel(mod);
            Mods.Add(modModel);
            if (mod.OtherI18nDatas.Count == 0)
                continue;
        }
    }

    private void ModFilterText_ValueChanged(string value)
    {
        if (string.IsNullOrEmpty(value))
            ShowMods = Mods;
        else
            ShowMods = new(Mods.Where(i => i.Id.Value.Contains(value)));
    }

    public void CreateNewMod()
    {
        // I18nHelper.Current = new();
        ModEditWindow = new();
        ModEditWindow.Show();
        ModMakerWindow.Hide();
        ModEditWindow.Closed += (s, e) =>
        {
            ModMakerWindow.Close();
        };
    }
}
