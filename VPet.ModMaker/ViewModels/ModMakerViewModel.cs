using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using HanumanInstitute.MvvmDialogs.Wpf;
using HKW.HKWUtils;
using HKW.HKWUtils.Collections;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using HKW.MVVM;
using HKW.MVVM.SourceGenerator;
using HKW.WPF.Extensions;
using HKW.WPF.MVVMDialogs;
using LinePutScript;
using LinePutScript.Converter;
using LinePutScript.Localization.WPF;
using Microsoft.Extensions.Logging;
using VPet.ModMaker.Models;
using VPet.ModMaker.Resources;
using VPet.ModMaker.ViewModels;

namespace VPet.ModMaker.ViewModels;

/// <summary>
/// 模组制作器视图模型
/// </summary>
public partial class ModMakerViewModel : ViewModelBase
{
    public ModMakerViewModel()
    {
        Histories = new(
            [],
            [],
            f => f.ID.Contains(Search, StringComparison.CurrentCultureIgnoreCase)
        );
        LoadHistory(ModMaker.HistoryFilePath);

        this.WhenAnyValue(x => x.Search)
            .Throttle(TimeSpan.FromSeconds(0.5), ObservableSchedulers.ThreadPool)
            .DistinctUntilChanged()
            .ObserveOn(ObservableSchedulers.Current)
            .Subscribe(_ => Histories.Refresh());
    }

    #region Property

    /// <summary>
    /// 历史搜索文本
    /// </summary>
    [ObservableProperty]
    public string Search { get; set; } = string.Empty;

    /// <summary>
    /// 历史
    /// </summary>
    public FilteredListWrapper<
        ModMakeHistory,
        List<ModMakeHistory>,
        ObservableList<ModMakeHistory>
    > Histories { get; set; }

    public ModEditViewModel ModEditViewModel =>
        field ??= DialogService.CreateViewModel<ModEditViewModel>();

    public static EnumInfo<LogLevel>[] LogLevels { get; } =
    [LogLevel.Debug.GetInfo(), LogLevel.Information.GetInfo(), LogLevel.Warning.GetInfo()];

    [ObservableProperty]
    public EnumInfo<LogLevel> CurrentLogLevel { get; set; } = LogLevel.Information;

    [DIProperty]
    internal NLog.Config.LoggingConfiguration LoggingConfiguration { get; init; }

    partial void OnCurrentLogLevelChanged(EnumInfo<LogLevel> oldValue, EnumInfo<LogLevel> newValue)
    {
        AppServices.SetDebugMode(LoggingConfiguration, newValue.Value == LogLevel.Debug);
        this.LogIfEnabled(LogLevel.Information)
            ?.Log("最低日志级别 {oldValue} -> {newValue}", oldValue, newValue);
    }

    #endregion

    #region History
    /// <summary>
    /// 删除历史
    /// </summary>
    /// <param name="value">历史</param>
    [RelayCommand]
    public void RemoveHistory(ModMakeHistory value)
    {
        Histories.Remove(value);
        this.LogIfEnabled(LogLevel.Information)
            ?.Log("已删除历史记录 {historyPath}", value.SourcePath);
        SaveHistory(ModMaker.HistoryFilePath);
    }

    /// <summary>
    /// 载入历史
    /// </summary>
    public void LoadHistory(string historyFile)
    {
        if (File.Exists(historyFile) is false)
        {
            //this.LogIfEnabled(LogLevel.Warning)
            //    ?.Log("载入历史失败,历史文件 {file} 不存在", historyFile);
            File.Create(historyFile).Close();
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
            //this.LogIfEnabled(LogLevel.Debug)?.Log("添加历史 {history}", history.SourcePath);
        }
        Histories.AddRange(set.OrderByDescending(h => h.LastTime));
        //this.LogIfEnabled(LogLevel.Information)?.Log("载入历史, 数量: {count}", set.Count);
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
        this.LogIfEnabled(LogLevel.Information)?.Log("成功保存历史");
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
            Histories.Remove(history);
            Histories.Insert(0, history);
            this.LogIfEnabled(LogLevel.Information)
                ?.Log("存在相同的历史 {history}, 更新最后修改时间", modInfo.SourcePath);
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
            this.LogIfEnabled(LogLevel.Information)?.Log("添加历史 {history}", modInfo.SourcePath);
        }
        SaveHistory(ModMaker.HistoryFilePath);
    }

    /// <summary>
    /// 添加历史
    /// </summary>
    /// <param name="modPath">模组路径</param>
    public void AddHistory(string modPath)
    {
        if (Histories.FirstOrDefault(h => h.SourcePath == modPath) is ModMakeHistory history)
        {
            history.ID = modPath;
            history.LastTime = DateTime.Now;
            Histories.Remove(history);
            Histories.Insert(0, history);
            this.LogIfEnabled(LogLevel.Information)
                ?.Log("存在相同的历史 {history}, 更新最后修改时间", modPath);
        }
        else
        {
            Histories.Add(
                new()
                {
                    ID = modPath,
                    SourcePath = modPath,
                    LastTime = DateTime.Now,
                }
            );
            this.LogIfEnabled(LogLevel.Information)?.Log("添加历史 {history}", modPath);
        }
        SaveHistory(ModMaker.HistoryFilePath);
    }

    [RelayCommand]
    private void ClearHistory()
    {
        if (Histories.Count == 0)
        {
            DialogService.ShowMessageBox(this, "历史为空,不需要清空".Translate());
            return;
        }
        var result = DialogService.ShowMessageBox(
            this,
            "确定要清空吗?".Translate(),
            button: MessageBoxButton.YesNo,
            icon: MessageBoxImage.Information
        );
        if (result is not true)
            return;
        Histories.Clear();
        this.LogIfEnabled(LogLevel.Information)?.Log("历史已清空");
        if (File.Exists(ModMaker.HistoryFilePath))
            File.WriteAllText(ModMaker.HistoryFilePath, string.Empty);
    }
    #endregion

    #region Mod
    /// <summary>
    /// 创建新模组
    /// </summary>
    [RelayCommand]
    public void CreateNewMod()
    {
        EditMod(new() { GameVersion = ModUpdataHelper.LastGameVersion });
    }

    /// <summary>
    /// 编辑模组
    /// </summary>
    /// <param name="modInfo">模组信息</param>
    public void EditMod(ModInfoModel modInfo)
    {
        DialogService.Show(this, ModEditViewModel);
        ModEditViewModel.ModInfo = modInfo;
    }

    /// <summary>
    /// 从文件载入模组
    /// </summary>
    [RelayCommand]
    public void LoadModFromPath()
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
    /// 载入历史
    /// </summary>
    /// <param name="history">历史</param>
    public void LoadHistory(ModMakeHistory history)
    {
        if (Directory.Exists(history.SourcePath) is false)
        {
            var result = DialogService.ShowMessageBox(
                this,
                $"历史路径不存在, 是否删除?".Translate(),
                "数据错误".Translate(),
                MessageBoxButton.YesNo
            );
            if (result is not true)
                return;
            RemoveHistory(history);
        }
        else
            LoadMod(history.SourcePath);
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
            this.LogIfEnabled(LogLevel.Error)?.Log(ex, "模组载入失败, 路径: '{path}'", directory);
            DialogService.ShowMessageBox(
                this,
                "模组载入失败, 详情请查看日志".Translate(),
                icon: MessageBoxImage.Error
            );
        }
        if (loader is null)
            return;
        ModInfoModel? modInfo = null;
        try
        {
            modInfo = new ModInfoModel(loader);
            AddHistory(modInfo);
        }
        catch (Exception ex)
        {
            this.LogIfEnabled(LogLevel.Error)?.Log(ex, "模组载入失败, 路径: '{path}'", directory);
            DialogService.ShowMessageBox(this, "模组载入失败, 详情请查看日志".Translate());
        }
        if (modInfo is null)
            return;
        try
        {
            EditMod(modInfo);
        }
        catch (Exception ex)
        {
            this.LogIfEnabled(LogLevel.Error)
                ?.Log(ex, "模组编辑时出现错误, 路径: '{path}'", directory);
            DialogService.ShowMessageBox(this, "模组编辑时出现错误, 详情请查看日志".Translate());
        }
    }

    /// <summary>
    /// 打开模组文件夹
    /// </summary>
    /// <param name="history"></param>
    [RelayCommand]
    public void OpenModPath(ModMakeHistory history)
    {
        if (Directory.Exists(history.SourcePath) is false)
        {
            var result = DialogService.ShowMessageBox(
                this,
                $"历史路径不存在, 是否删除?".Translate(),
                "数据错误".Translate(),
                MessageBoxButton.YesNo
            );
            if (result is not true)
                return;
            RemoveHistory(history);
        }
        else
        {
            NativeUtils.OpenLink(history.SourcePath);
        }
    }

    [RelayCommand]
    public void OpenLogFile()
    {
        NativeUtils.OpenLink(ModMaker.LogFilePath);
    }

    /// <summary>
    /// 打开模组文件夹
    /// </summary>
    /// <param name="history"></param>
    [RelayCommand]
    public static void OpenModMakerPath()
    {
        NativeUtils.OpenLink(ModMaker.ModMakerBaseDirectory);
    }
    #endregion

    /// <summary>
    /// Wiki链接
    /// </summary>
    public const string WikiLink = "https://github.com/LorisYounger/VPet.ModMaker/wiki";

    [RelayCommand]
    private static void OpenWikiLink()
    {
        NativeUtils.OpenLink(WikiLink);
        //try
        //{
        //}
        //catch
        //{
        //    if (
        //        DialogService.ShowMessageBox(
        //            this,
        //            "无法打开链接,需要复制自行访问吗".Translate(),
        //            "打开链接失败".Translate(),
        //            MessageBoxButton.YesNo,
        //            icon: MessageBoxImage.Warning
        //        )
        //        is not true
        //    )
        //        return;
        //    ClipboardSetText(WikiLink);
        //    DialogService.ShowMessageBox(this, "已复制到剪贴板".Translate());
        //}
    }

    ///// <inheritdoc/>
    //protected override void Dispose(bool disposing)
    //{
    //    if (_disposed)
    //        return;
    //    base.Dispose(disposing);
    //    foreach (var history in Histories)
    //        history.Image?.CloseStreamWhenNoReference();
    //    Histories.Clear();
    //}
}
