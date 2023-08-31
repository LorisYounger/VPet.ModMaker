using HKW.HKWViewModels.SimpleObservable;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using VPet.ModMaker.Models;
using VPet.ModMaker.Views;
using VPet.ModMaker.Views.ModEdit;

namespace VPet.ModMaker.ViewModels;

public class ModMakerWindowVM
{
    #region Value
    public ModMakerWindow ModMakerWindow { get; }

    public ModEditWindow ModEditWindow { get; private set; }

    public ObservableValue<string> ModFilterText { get; } = new();

    public ObservableCollection<ModInfoModel> ShowMods { get; set; }
    public ObservableCollection<ModInfoModel> Mods { get; } = new();
    #endregion
    #region Command
    public ObservableCommand CreateNewModCommand { get; } = new();
    public ObservableCommand LoadModFromFileCommand { get; } = new();
    #endregion
    public ModMakerWindowVM() { }

    public ModMakerWindowVM(ModMakerWindow window)
    {
        LoadMods();
        ModMakerWindow = window;
        ShowMods = Mods;
        CreateNewModCommand.ExecuteAction = CreateNewMod;
        LoadModFromFileCommand.ExecuteAction = LoadModFromFile;
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
        }
    }

    private void ModFilterText_ValueChanged(string value)
    {
        if (string.IsNullOrEmpty(value))
            ShowMods = Mods;
        else
            ShowMods = new(Mods.Where(i => i.Name.Value.Contains(value)));
    }

    public void CreateNewMod()
    {
        ModInfoModel.Current = new();
        // I18nHelper.Current = new();
        ModEditWindow = new();
        ModEditWindow.Show();
        ModMakerWindow.Hide();
        ModEditWindow.Closed += (s, e) =>
        {
            ModMakerWindow.Close();
        };
    }

    public void LoadModFromFile()
    {
        OpenFileDialog openFileDialog =
            new()
            {
                Title = "模组信息文件",
                Filter = $"LPS文件|*.lps;",
                FileName = "info.lps"
            };
        if (openFileDialog.ShowDialog() is true)
        {
            try
            {
                var path = Path.GetDirectoryName(openFileDialog.FileName);
                var mod = new ModLoader(new DirectoryInfo(path));
                if (mod.SuccessLoad is false)
                {
                    MessageBox.Show("模组载入失败");
                    return;
                }
                ModInfoModel.Current = new ModInfoModel(mod);
                CreateNewMod();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"模组载入失败:\n{ex}");
            }
        }
    }
}
