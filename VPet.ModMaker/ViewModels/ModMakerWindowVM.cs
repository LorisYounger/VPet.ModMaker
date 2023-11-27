using HKW.HKWViewModels.SimpleObservable;
using LinePutScript;
using LinePutScript.Converter;
using LinePutScript.Localization.WPF;
using Microsoft.Win32;
using Panuon.WPF.UI;
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
using VPet.ModMaker.Views.ModEdit.I18nEdit;

namespace VPet.ModMaker.ViewModels;

public class ModMakerWindowVM
{
    #region Value
    public ModMakerWindow ModMakerWindow { get; }

    public ModEditWindow ModEditWindow { get; private set; }

    /// <summary>
    /// 历史搜索文本
    /// </summary>
    public ObservableValue<string> HistoriesSearchText { get; } = new();

    /// <summary>
    /// 显示的历史
    /// </summary>
    public ObservableValue<ObservableCollection<ModMakeHistory>> ShowHistories { get; } = new();

    /// <summary>
    /// 历史
    /// </summary>
    public ObservableCollection<ModMakeHistory> Histories { get; } = new();
    #endregion
    #region Command
    /// <summary>
    /// 创建新模组命令
    /// </summary>
    public ObservableCommand CreateNewModCommand { get; } = new();

    /// <summary>
    /// 从文件载入模组命令
    /// </summary>
    public ObservableCommand LoadModFromFileCommand { get; } = new();

    /// <summary>
    /// 清除历史命令
    /// </summary>
    public ObservableCommand ClearHistoriesCommand { get; } = new();

    /// <summary>
    /// 删除历史命令
    /// </summary>
    public ObservableCommand<ModMakeHistory> RemoveHistoryCommand { get; } = new();
    #endregion

    public ModMakerWindowVM(ModMakerWindow window)
    {
        LoadHistories();
        ModMakerWindow = window;
        ShowHistories.Value = Histories;
        CreateNewModCommand.ExecuteEvent += CreateNewMod;
        LoadModFromFileCommand.ExecuteEvent += LoadModFromFile;
        ClearHistoriesCommand.ExecuteEvent += ClearHistories;
        RemoveHistoryCommand.ExecuteEvent += RemoveHistory;
        HistoriesSearchText.ValueChanged += HistoriesSearchText_ValueChanged;
    }

    private void HistoriesSearchText_ValueChanged(
        ObservableValue<string> sender,
        ValueChangedEventArgs<string> e
    )
    {
        if (string.IsNullOrEmpty(e.NewValue))
            ShowHistories.Value = Histories;
        else
            ShowHistories.Value = new(Histories.Where(i => i.Id.Contains(e.NewValue)));
    }

    #region History
    private void RemoveHistory(ModMakeHistory value)
    {
        Histories.Remove(value);
        SaveHistories();
    }

    /// <summary>
    /// 载入历史
    /// </summary>
    private void LoadHistories()
    {
        if (File.Exists(ModMakerInfo.HistoryFile) is false)
            return;
        var lps = new LPS(File.ReadAllText(ModMakerInfo.HistoryFile));
        foreach (var line in lps)
        {
            var history = LPSConvert.DeserializeObject<ModMakeHistory>(line);
            if (Histories.All(h => h.InfoFile != history.InfoFile))
                Histories.Add(history);
        }
    }

    /// <summary>
    /// 保存历史
    /// </summary>
    private void SaveHistories()
    {
        Directory.CreateDirectory(nameof(ModMaker));
        if (File.Exists(ModMakerInfo.HistoryFile) is false)
            File.Create(ModMakerInfo.HistoryFile).Close();

        var lps = new LPS();
        foreach (var history in Histories)
            lps.Add(LPSConvert.SerializeObjectToLine<Line>(history, nameof(history)));
        File.WriteAllText(ModMakerInfo.HistoryFile, lps.ToString());
    }

    /// <summary>
    /// 添加历史
    /// </summary>
    /// <param name="modInfo">模组信息</param>
    private void AddHistories(ModInfoModel modInfo)
    {
        if (
            Histories.FirstOrDefault(h => h.SourcePath == modInfo.SourcePath.Value)
            is ModMakeHistory history
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
    #endregion

    #region Mod
    /// <summary>
    /// 创建新模组
    /// </summary>
    public void CreateNewMod()
    {
        ModInfoModel.Current = new();
        ShowEditWindow();
    }

    /// <summary>
    /// 编辑模组
    /// </summary>
    /// <param name="modInfo">模组信息</param>
    public void EditMod(ModInfoModel modInfo)
    {
        ModInfoModel.Current = modInfo;
        ShowEditWindow();
    }

    /// <summary>
    /// 显示模组编辑窗口
    /// </summary>
    private void ShowEditWindow()
    {
        GC.Collect();
        ModMakerWindow.Hide();
        // 将当前模组添加到历史
        if (string.IsNullOrEmpty(ModInfoModel.Current.SourcePath.Value) is false)
            AddHistories(ModInfoModel.Current);
        SaveHistories();
        ModEditWindow = new();
        ModEditWindow.Show();
        ModEditWindow.InitializeData();
        ModEditWindow.Closed += (s, e) =>
        {
            var modInfo = ModInfoModel.Current;
            if (string.IsNullOrEmpty(modInfo.SourcePath.Value) is false)
            {
                AddHistories(modInfo);
                SaveHistories();
            }
            ModInfoModel.Current?.Close();
            I18nHelper.Current = new();
            ModMakerWindow.Show();
            ModMakerWindow.Activate();
            GC.Collect();
        };
    }

    /// <summary>
    /// 从文件载入模组
    /// </summary>
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

    /// <summary>
    /// 载入模组
    /// </summary>
    /// <param name="path">位置</param>
    public void LoadMod(string path)
    {
        ModLoader? loader = null;
        var pendingHandler = PendingBox.Show("载入中".Translate());
        try
        {
            loader = new ModLoader(new(path));
        }
        catch (Exception ex)
        {
            MessageBox.Show(ModMakerWindow, "模组载入失败:\n{0}".Translate(ex));
            pendingHandler.Close();
        }
        if (loader is null)
            return;
        try
        {
            var modInfo = new ModInfoModel(loader);
            EditMod(modInfo);
            pendingHandler.Close();
        }
        catch (Exception ex)
        {
            pendingHandler.Close();
            ModEditWindow?.Close();
            ModEditWindow = null;
            ModInfoModel.Current?.Close();
            I18nHelper.Current = new();
            I18nEditWindow.Current?.Close(true);
            ModMakerWindow.Show();
            ModMakerWindow.Activate();
            MessageBox.Show(ModMakerWindow, "模组载入失败:\n{0}".Translate(ex));
        }
    }
    #endregion
}
