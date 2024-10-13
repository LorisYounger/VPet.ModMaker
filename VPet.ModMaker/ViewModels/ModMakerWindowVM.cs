using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using HKW.HKWReactiveUI;
using HKW.HKWUtils.Collections;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using LinePutScript;
using LinePutScript.Converter;
using LinePutScript.Localization.WPF;
using Microsoft.Win32;
using Panuon.WPF.UI;
using VPet.ModMaker.Models;
using VPet.ModMaker.Views;
using VPet.ModMaker.Views.ModEdit;
using VPet.ModMaker.Views.ModEdit.I18nEdit;

namespace VPet.ModMaker.ViewModels;

public partial class ModMakerWindowVM : ViewModelBase
{
    public ModMakerWindowVM(ModMakerWindow window)
    {
        Histories = new([], [], f => f.ID.Contains(Search, StringComparison.OrdinalIgnoreCase));
        LoadHistories();
        ModMakerWindow = window;
        PropertyChanged += ModMakerWindowVM_PropertyChanged;
    }

    private void ModMakerWindowVM_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(Search))
        {
            Histories.Refresh();
        }
    }

    #region Property
    public ModMakerWindow ModMakerWindow { get; } = null!;

    public ModEditWindow ModEditWindow { get; private set; } = null!;

    /// <summary>
    /// 历史搜索文本
    /// </summary>
    [ReactiveProperty]
    public string Search { get; set; } = string.Empty;

    /// <summary>
    /// 历史
    /// </summary>
    public FilterListWrapper<
        ModMakeHistory,
        List<ModMakeHistory>,
        ObservableList<ModMakeHistory>
    > Histories { get; set; }

    #endregion
    //#region Command
    ///// <summary>
    ///// 创建新模组命令
    ///// </summary>
    //public ObservableCommand CreateNewModCommand { get; } = new();

    ///// <summary>
    ///// 从文件载入模组命令
    ///// </summary>
    //public ObservableCommand LoadModFromFileCommand { get; } = new();

    ///// <summary>
    ///// 清除历史命令
    ///// </summary>
    //public ObservableCommand ClearHistoriesCommand { get; } = new();

    ///// <summary>
    ///// 删除历史命令
    ///// </summary>
    //public ObservableCommand<ModMakeHistory> RemoveHistoryCommand { get; } = new();
    //#endregion

    #region History
    /// <summary>
    /// 删除历史
    /// </summary>
    /// <param name="value">历史</param>
    [ReactiveCommand]
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
        var set = new HashSet<ModMakeHistory>();
        foreach (var line in lps)
        {
            if (LPSConvert.DeserializeObject<ModMakeHistory>(line) is not ModMakeHistory history)
                continue;
            history.ID ??= string.Empty;
            set.Add(history);
        }
        Histories.AddRange(set.OrderByDescending(h => h.LastTime));
    }

    /// <summary>
    /// 保存历史
    /// </summary>
    public void SaveHistories()
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
            Histories.FirstOrDefault(h => h.SourcePath == modInfo.SourcePath)
            is ModMakeHistory history
        )
        {
            history.ID = modInfo.ID;
            history.SourcePath = modInfo.SourcePath;
            history.LastTime = DateTime.Now;
        }
        else
        {
            Histories.Add(
                new()
                {
                    ID = modInfo.ID,
                    SourcePath = modInfo.SourcePath,
                    LastTime = DateTime.Now,
                }
            );
        }
    }

    [ReactiveCommand]
    private void ClearHistories()
    {
        if (
            MessageBox.Show("确定要清空吗?".Translate(), "", MessageBoxButton.YesNo)
            is not MessageBoxResult.Yes
        )
            return;
        Histories.Clear();
        if (File.Exists(ModMakerInfo.HistoryFile))
            File.WriteAllText(ModMakerInfo.HistoryFile, string.Empty);
    }
    #endregion

    #region Mod
    /// <summary>
    /// 创建新模组
    /// </summary>
    [ReactiveCommand]
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
        if (string.IsNullOrEmpty(ModInfoModel.Current.SourcePath) is false)
            AddHistories(ModInfoModel.Current);
        SaveHistories();
        ModEditWindow = new();
        ModEditWindow.Show();
        ModEditWindow.InitializeData();
        ModEditWindow.Closed += (s, e) =>
        {
            var modInfo = ModInfoModel.Current;
            if (string.IsNullOrEmpty(modInfo.SourcePath) is false)
            {
                AddHistories(modInfo);
                SaveHistories();
            }
            ModInfoModel.Current?.Close();
            ModMakerWindow.Show();
            ModMakerWindow.Activate();
            GC.Collect();
        };
    }

    /// <summary>
    /// 从文件载入模组
    /// </summary>
    [ReactiveCommand]
    public void LoadModFromFile()
    {
        var openFileDialog = new OpenFileDialog()
        {
            Title = "模组信息文件".Translate(),
            Filter = $"LPS文件|*.lps;".Translate(),
            FileName = "info.lps"
        };
        if (openFileDialog.ShowDialog() is true)
        {
            LoadMod(Path.GetDirectoryName(openFileDialog.FileName)!);
        }
    }

    /// <summary>
    /// 载入模组
    /// </summary>
    /// <param name="path">位置</param>
    public void LoadMod(string path)
    {
        ModLoader? loader = null;
        var pendingHandler = PendingBox.Show(ModMakerWindow, "载入中".Translate());
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
            pendingHandler.Hide();
            EditMod(modInfo);
            // 更新模组
            if (ModUpdataHelper.CanUpdata(modInfo))
            {
                if (
                    MessageBox.Show(
                        ModEditWindow.Current,
                        "是否更新模组\n当前版本: {0}\n最新版本: {1}".Translate(
                            modInfo.ModVersion,
                            ModUpdataHelper.LastVersion
                        ),
                        "更新模组".Translate(),
                        MessageBoxButton.YesNo
                    ) is MessageBoxResult.Yes
                )
                {
                    if (ModUpdataHelper.Updata(modInfo))
                        MessageBox.Show(ModEditWindow.Current, "更新完成, 请手动保存".Translate());
                }
            }
            pendingHandler.Close();
        }
        catch (Exception ex)
        {
            pendingHandler.Close();
            ModEditWindow?.Close();
            ModEditWindow = null!;
            ModInfoModel.Current?.Close();
            ModMakerWindow.ShowOrActivate();
            MessageBox.Show(ModMakerWindow, "模组载入失败:\n{0}".Translate(ex));
            GC.Collect();
        }
    }
    #endregion
}
