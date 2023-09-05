using HKW.HKWViewModels.SimpleObservable;
using LinePutScript;
using LinePutScript.Converter;
using LinePutScript.Localization.WPF;
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

    public ObservableValue<string> HistoriesFilterText { get; } = new();

    public ObservableCollection<ModInfoModel> Mods { get; } = new();
    public ObservableValue<ObservableCollection<ModMakerHistory>> ShowHistories { get; } = new();
    public ObservableCollection<ModMakerHistory> Histories { get; } = new();
    #endregion
    #region Command
    public ObservableCommand CreateNewModCommand { get; } = new();
    public ObservableCommand LoadModFromFileCommand { get; } = new();
    public ObservableCommand ClearHistoriesCommand { get; } = new();
    #endregion
    public ModMakerWindowVM() { }

    public ModMakerWindowVM(ModMakerWindow window)
    {
        LoadHistories();
        ModMakerWindow = window;
        ShowHistories.Value = Histories;
        CreateNewModCommand.ExecuteAction = CreateNewMod;
        LoadModFromFileCommand.ExecuteAction = LoadModFromFile;
        ClearHistoriesCommand.ExecuteAction = ClearHistories;
        HistoriesFilterText.ValueChanged += ModFilterText_ValueChanged;
    }

    private void LoadHistories()
    {
        if (File.Exists(ModMakerInfo.HistoryFile) is false)
            return;
        var lps = new LPS(File.ReadAllText(ModMakerInfo.HistoryFile));
        foreach (var line in lps)
            Histories.Add(LPSConvert.DeserializeObject<ModMakerHistory>(line));
    }

    private void SaveHistories()
    {
        if (File.Exists(ModMakerInfo.HistoryFile) is false)
            File.Create(ModMakerInfo.HistoryFile).Close();
        var lps = new LPS();
        foreach (var history in Histories)
            lps.Add(
                new Line(nameof(history))
                {
                    new Sub("Name", history.Name),
                    new Sub("SourcePath", history.SourcePath),
                    new Sub("LastTime", history.LastTimeString)
                }
            );
        File.WriteAllText(ModMakerInfo.HistoryFile, lps.ToString());
    }

    private void AddHistories(ModInfoModel modInfo)
    {
        if (Histories.FirstOrDefault(h => h.Name == modInfo.Name.Value) is ModMakerHistory history)
        {
            history.SourcePath = modInfo.SourcePath.Value;
            history.LastTime = DateTime.Now;
        }
        else
        {
            Histories.Add(
                new()
                {
                    Name = modInfo.Name.Value,
                    SourcePath = modInfo.SourcePath.Value,
                    LastTime = DateTime.Now,
                }
            );
        }
    }

    private void ModFilterText_ValueChanged(string value)
    {
        if (string.IsNullOrEmpty(value))
            ShowHistories.Value = Histories;
        else
            ShowHistories.Value = new(Histories.Where(i => i.Name.Contains(value)));
    }

    public void CreateNewMod()
    {
        ModInfoModel.Current ??= new();
        ModEditWindow = new();
        ModEditWindow.Show();
        ModMakerWindow.Hide();
        ModEditWindow.Closed += (s, e) =>
        {
            var modInfo = ModInfoModel.Current;
            if (string.IsNullOrEmpty(modInfo.SourcePath.Value) is false)
                AddHistories(modInfo);
            SaveHistories();
            ModMakerWindow.Close();
        };
    }

    private void ClearHistories()
    {
        ShowHistories.Value.Clear();
        Histories.Clear();
    }

    public void LoadModFromFile()
    {
        OpenFileDialog openFileDialog =
            new()
            {
                Title = "模组信息文件".Translate(),
                Filter = $"LPS文件|*.lps;".Translate(),
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
                    MessageBox.Show("模组载入失败".Translate());
                    return;
                }
                ModInfoModel.Current = new ModInfoModel(mod);
                CreateNewMod();
            }
            catch (Exception ex)
            {
                MessageBox.Show("模组载入失败:\n{0}".Translate(ex));
            }
        }
    }
}
