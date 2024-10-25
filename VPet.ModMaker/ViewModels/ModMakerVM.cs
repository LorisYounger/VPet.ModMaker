using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using DynamicData.Binding;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using HKW.HKWReactiveUI;
using HKW.HKWUtils.Collections;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using HKW.WPF.MVVMDialogs;
using LinePutScript;
using LinePutScript.Converter;
using LinePutScript.Localization.WPF;
using Microsoft.Win32;
using ReactiveUI;
using Splat;
using VPet.ModMaker.Models;
using VPet.ModMaker.Resources;
using VPet.ModMaker.ViewModels.ModEdit;

namespace VPet.ModMaker.ViewModels;

public partial class ModMakerVM : ViewModelBase
{
    private static bool _isFirst = true;

    private static IDialogService DialogService => Locator.Current.GetService<IDialogService>()!;

    public ModMakerVM()
    {
        Initialize();
        this.Log().Info("初始化完成");
        Histories = new([], [], f => f.ID.Contains(Search, StringComparison.OrdinalIgnoreCase));
        LoadHistory(NativeData.HistoryBaseFilePath);

        this.WhenValueChanged(x => x.Search)
            .Throttle(TimeSpan.FromSeconds(1), RxApp.TaskpoolScheduler)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => Histories.Refresh());
    }

    private static void Initialize()
    {
        if (_isFirst is false)
            return;
        Directory.CreateDirectory(NativeData.ModMakerBaseDirectory);
        var configPath = Path.Combine(NativeData.ModMakerBaseDirectory, "NLog.config");
        if (File.Exists(configPath) is false)
            NativeResources.SaveTo(NativeResources.NLogConfig, configPath);
        DependencyInjection.Initialize();
        EnumInfo.DefaultToString = x => $"{x.EnumType.Name}_{x.Value}".Translate();
        EnumInfo<FoodSearchTarget>.Initialize();
        _isFirst = false;
    }

    #region Property

    /// <summary>
    /// 当前模组
    /// </summary>
    [ReactiveProperty]
    public ModInfoModel ModInfo { get; set; } = null!;

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

    #region History
    /// <summary>
    /// 删除历史
    /// </summary>
    /// <param name="value">历史</param>
    [ReactiveCommand]
    public void RemoveHistory(ModMakeHistory value)
    {
        Histories.Remove(value);
        this.Log().Info("已删除历史记录 {historyPath}", value.SourcePath);
        SaveHistory(NativeData.HistoryBaseFilePath);
    }

    /// <summary>
    /// 载入历史
    /// </summary>
    public void LoadHistory(string historyFile)
    {
        this.Log().Info("开始载入历史");
        if (File.Exists(historyFile) is false)
        {
            this.Log().Info("载入历史失败,历史文件 {file} 不存在", historyFile);
            return;
        }
        var lps = new LPS(File.ReadAllText(historyFile));
        var set = new HashSet<ModMakeHistory>();
        foreach (var line in lps)
        {
            if (LPSConvert.DeserializeObject<ModMakeHistory>(line) is not ModMakeHistory history)
                continue;
            history.ID ??= string.Empty;
            set.Add(history);
            this.Log().Debug("添加历史 {history}", history.SourcePath);
        }
        Histories.AddRange(set.OrderByDescending(h => h.LastTime));
        this.Log().Info("已成功载入历史 {count} 个", set.Count);
    }

    /// <summary>
    /// 保存历史
    /// </summary>
    public void SaveHistory(string historyFile)
    {
        Directory.CreateDirectory(nameof(ModMaker));
        if (File.Exists(historyFile) is false)
            File.Create(historyFile).Close();

        var lps = new LPS();
        foreach (var history in Histories)
            lps.Add(LPSConvert.SerializeObjectToLine<Line>(history, nameof(history)));
        File.WriteAllText(historyFile, lps.ToString());
        this.Log().Info("已成功保存历史");
    }

    /// <summary>
    /// 添加历史
    /// </summary>
    /// <param name="modInfo">模组信息</param>
    public void AddHistory(ModInfoModel modInfo)
    {
        if (
            Histories.FirstOrDefault(h => h.SourcePath == modInfo.SourcePath)
            is ModMakeHistory history
        )
        {
            history.ID = modInfo.ID;
            history.LastTime = DateTime.Now;
            this.Log().Info("已存在相同的历史 {history}, 更新最后修改时间", modInfo.SourcePath);
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
            this.Log().Info("添加历史 {history}", modInfo.SourcePath);
        }
        SaveHistory(NativeData.HistoryBaseFilePath);
    }

    [ReactiveCommand]
    private void ClearHistory()
    {
        if (Histories.HasValue() is false)
        {
            DialogService.ShowMessageBoxX(this, "历史为空,不需要清空");
            return;
        }

        if (
            DialogService.ShowMessageBoxX(
                this,
                "确定要清空吗?".Translate(),
                button: MessageBoxButton.YesNo,
                icon: MessageBoxImage.Information
            )
            is not true
        )
            return;
        Histories.Clear();
        this.Log().Info("历史已清空");
        if (File.Exists(NativeData.HistoryBaseFilePath))
            File.WriteAllText(NativeData.HistoryBaseFilePath, string.Empty);
    }
    #endregion

    #region Mod
    /// <summary>
    /// 创建新模组
    /// </summary>
    [ReactiveCommand]
    public void CreateNewMod()
    {
        ModInfo = new();
        MessageBus.Current.SendMessage(ModInfo);
    }

    /// <summary>
    /// 编辑模组
    /// </summary>
    /// <param name="modInfo">模组信息</param>
    public void EditMod(ModInfoModel modInfo)
    {
        ModInfo = modInfo;
        MessageBus.Current.SendMessage(ModInfo);
    }

    /// <summary>
    /// 从文件载入模组
    /// </summary>
    [ReactiveCommand]
    public void LoadModFromFile()
    {
        var openFileDialog = DialogService.ShowOpenFolderDialog(
            this,
            new() { Title = "模组信息文件夹".Translate() }
        );
        if (openFileDialog is not null)
        {
            LoadMod(openFileDialog.LocalPath);
        }
    }

    /// <summary>
    /// 载入模组
    /// </summary>
    /// <param name="directory">目录</param>
    public void LoadMod(string directory)
    {
        ModLoader? loader = null;
        try
        {
            loader = new ModLoader(new(directory));
        }
        catch (Exception ex)
        {
            DialogService.ShowMessageBoxX(
                this,
                "模组载入失败, 详情请查看日志".Translate(),
                icon: MessageBoxImage.Error
            );
            this.Log().Error("模组载入失败, 路径: {path}", directory, ex);
        }
        if (loader is null)
            return;
        try
        {
            var modInfo = new ModInfoModel(loader);
            EditMod(modInfo);
        }
        catch (Exception ex)
        {
            ModInfo?.Close();
            ModInfo = null!;
            DialogService.ShowMessageBoxX(this, "模组载入失败, 详情请查看日志".Translate());
            this.Log().Error("模组载入失败, 路径: {path}", directory, ex);
        }
    }
    #endregion
}
