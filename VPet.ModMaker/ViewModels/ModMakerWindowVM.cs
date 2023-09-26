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

    public ObservableValue<string> HistoriesSearchText { get; } = new();

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
        CreateNewModCommand.ExecuteEvent += CreateNewMod;
        LoadModFromFileCommand.ExecuteEvent += LoadModFromFile;
        ClearHistoriesCommand.ExecuteEvent += ClearHistories;
        HistoriesSearchText.ValueChanged += ModSearchText_ValueChanged;
    }

    private void LoadHistories()
    {
        if (File.Exists(ModMakerInfo.HistoryFile) is false)
            return;
        var lps = new LPS(File.ReadAllText(ModMakerInfo.HistoryFile));
        foreach (var line in lps)
        {
            var history = LPSConvert.DeserializeObject<ModMakerHistory>(line);
            if (Histories.All(h => h.InfoFile != history.InfoFile))
                Histories.Add(history);
        }
    }

    private void SaveHistories()
    {
        Directory.CreateDirectory(nameof(ModMaker));
        if (File.Exists(ModMakerInfo.HistoryFile) is false)
            File.Create(ModMakerInfo.HistoryFile).Close();
        var lps = new LPS();
        foreach (var history in Histories)
            lps.Add(
                new Line(nameof(history))
                {
                    new Sub("Id", history.Id),
                    new Sub("SourcePath", history.SourcePath),
                    new Sub("LastTime", history.LastTimeString)
                }
            );
        File.WriteAllText(ModMakerInfo.HistoryFile, lps.ToString());
    }

    private void AddHistories(ModInfoModel modInfo)
    {
        if (
            Histories.FirstOrDefault(h => h.SourcePath == modInfo.SourcePath.Value)
            is ModMakerHistory history
        )
        {
            history.Id = modInfo.Id.Value;
            history.SourcePath = modInfo.SourcePath.Value;
            history.LastTime = DateTime.Now;
        }
        else
        {
            Histories.Add(
                new()
                {
                    Id = modInfo.Id.Value,
                    SourcePath = modInfo.SourcePath.Value,
                    LastTime = DateTime.Now,
                }
            );
        }
    }

    private void ModSearchText_ValueChanged(string oldValue, string newValue)
    {
        if (string.IsNullOrEmpty(newValue))
            ShowHistories.Value = Histories;
        else
            ShowHistories.Value = new(Histories.Where(i => i.Id.Contains(newValue)));
    }

    public void CreateNewMod()
    {
        ModInfoModel.Current = new();
        ShowEditWindow();
    }

    public void EditMod(ModInfoModel modInfo)
    {
        ModInfoModel.Current = modInfo;
        ShowEditWindow();
    }

    private void ShowEditWindow()
    {
        if (string.IsNullOrEmpty(ModInfoModel.Current.SourcePath.Value) is false)
            AddHistories(ModInfoModel.Current);
        SaveHistories();
        ModEditWindow = new();
        ModEditWindow.Show();
        ModMakerWindow.Hide();
        ModEditWindow.Closed += (s, e) =>
        {
            var modInfo = ModInfoModel.Current;
            if (string.IsNullOrEmpty(modInfo.SourcePath.Value) is false)
                AddHistories(modInfo);
            SaveHistories();
            ModInfoModel.Current.Close();
            ModInfoModel.Current = null;
            I18nHelper.Current = new();
            ModMakerWindow.Show();
        };
    }

    private void ClearHistories()
    {
        if (
            MessageBox.Show("确定要清空吗?".Translate(), "", MessageBoxButton.YesNo)
            is not MessageBoxResult.Yes
        )
            return;
        ShowHistories.Value.Clear();
        Histories.Clear();
        File.WriteAllText(ModMakerInfo.HistoryFile, string.Empty);
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
            LoadMod(Path.GetDirectoryName(openFileDialog.FileName));
        }
    }

    public void LoadMod(string path)
    {
        try
        {
            var mod = new ModLoader(new DirectoryInfo(path));
            if (mod.SuccessLoad is false)
            {
                MessageBox.Show("模组载入失败".Translate());
                return;
            }
            var modInfo = new ModInfoModel(mod);
            EditMod(modInfo);
        }
        catch (Exception ex)
        {
            MessageBox.Show("模组载入失败:\n{0}".Translate(ex));
        }
    }
}
