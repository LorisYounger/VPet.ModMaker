using System.IO;
using System.Reactive.Linq;
using DynamicData.Binding;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.FrameworkDialogs;
using HanumanInstitute.MvvmDialogs.Wpf;
using HKW.HKWReactiveUI;
using HKW.HKWUtils;
using HKW.HKWUtils.Collections;
using HKW.HKWUtils.Extensions;
using HKW.HKWUtils.Observable;
using HKW.WPF;
using HKW.WPF.Extensions;
using HKW.WPF.MVVMDialogs;
using LinePutScript;
using LinePutScript.Converter;
using LinePutScript.Localization.WPF;
using ReactiveUI;
using Splat;
using Splat.NLog;
using VPet.ModMaker.Models;
using VPet.ModMaker.Resources;
using VPet.ModMaker.ViewModels.ModEdit;

namespace VPet.ModMaker.ViewModels;

/// <summary>
/// 模组制作器视图模型
/// </summary>
public partial class ModMakerVM : ViewModelBase
{
    private static bool _isFirst = true;

    /// <summary>
    /// 对话框服务
    /// </summary>
    public static IDialogService DialogService { get; private set; } = null!;

    /// <inheritdoc/>
    public ModMakerVM()
    {
        Initialize();
        this.LogX().Info("初始化完成".Translate());
#if !RELEASE
        HKWImageUtils.Logger = this.LogX();
        HKWImageUtils.LogStackFrame = true;
#endif
        Histories = new([], [], f => f.ID.Contains(Search, StringComparison.OrdinalIgnoreCase));
        LoadHistory(NativeData.HistoryBaseFilePath);

        this.WhenValueChanged(x => x.Search)
            .Throttle(TimeSpan.FromSeconds(0.5), RxApp.TaskpoolScheduler)
            .DistinctUntilChanged()
            .ObserveOn(RxApp.MainThreadScheduler)
            .Subscribe(_ => Histories.Refresh())
            .Record(this);
    }

    private static void Initialize()
    {
        if (_isFirst is false)
            return;
        Directory.CreateDirectory(NativeData.ModMakerBaseDirectory);
        if (File.Exists(NativeData.NLogConfigBaseFile) is false)
            NativeResources.SaveTo(NativeResources.NLogConfig, NativeData.NLogConfigBaseFile);

        LogResolver.LogFactory.Configuration = new NLog.Config.XmlLoggingConfiguration(
            Path.Combine(NativeData.NLogConfigBaseFile)
        );
        var funcLogManager = new FuncLogManager(type => new NLogLogger(LogResolver.Resolve(type)));
        LogHostX.RegisterLoggerManager(typeof(ViewModelBase), funcLogManager);

        DialogService = new DialogService(
            new DialogManagerX(viewLocator: new ViewLocator()),
            viewModelFactory: x => throw new NotImplementedException()
        );

        EnumInfo.DefaultToString = x =>
            x.IsFlagable
                ? string.Join(
                    ", ",
                    x.GetFlagInfos().Select(static i => $"{i.EnumType.Name}_{i.Value}".Translate())
                )
                : $"{x.EnumType.Name}_{x.Value}".Translate();

        _isFirst = false;
    }

    #region Property

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
        this.LogX().Info("已删除历史记录 {historyPath}", value.SourcePath);
        SaveHistory(NativeData.HistoryBaseFilePath);
    }

    /// <summary>
    /// 载入历史
    /// </summary>
    public void LoadHistory(string historyFile)
    {
        if (File.Exists(historyFile) is false)
        {
            this.LogX().Warn("载入历史失败,历史文件 {file} 不存在", historyFile);
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
            this.LogX().Debug("添加历史 {history}", history.SourcePath);
        }
        Histories.AddRange(set.OrderByDescending(h => h.LastTime));
        this.LogX().Info("载入历史, 数量: {count}", set.Count);
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
        this.LogX().Info("成功保存历史");
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
            this.LogX().Info("存在相同的历史 {history}, 更新最后修改时间", modInfo.SourcePath);
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
            this.LogX().Info("添加历史 {history}", modInfo.SourcePath);
        }
        SaveHistory(NativeData.HistoryBaseFilePath);
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
            this.LogX().Info("存在相同的历史 {history}, 更新最后修改时间", modPath);
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
            this.LogX().Info("添加历史 {history}", modPath);
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
        this.LogX().Info("历史已清空");
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
        EditMod(new() { GameVersion = ModUpdataHelper.LastGameVersion });
    }

    /// <summary>
    /// 编辑模组
    /// </summary>
    /// <param name="modInfo">模组信息</param>
    public void EditMod(ModInfoModel modInfo)
    {
        var view = DialogService.DialogManager.FindViewByViewModel(this)!;
        view.Hide();
        var vm = new ModEditVM(modInfo);
        DialogService.Show(vm).ShowViewOnSelfClose(view);
        vm.CheckModInfo();
    }

    /// <summary>
    /// 从文件载入模组
    /// </summary>
    [ReactiveCommand]
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
            if (
                DialogService.ShowMessageBoxX(
                    this,
                    $"历史路径不存在, 是否删除?".Translate(),
                    "数据错误".Translate(),
                    MessageBoxButton.YesNo
                )
                is not true
            )
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
            this.LogX().Error(ex, "模组载入失败, 路径: {path}", directory);
            DialogService.ShowMessageBoxX(
                this,
                "模组载入失败, 详情请查看日志".Translate(),
                icon: MessageBoxImage.Error
            );
        }
        if (loader is null)
            return;
        try
        {
            var modInfo = new ModInfoModel(loader);
            AddHistory(modInfo);
            EditMod(modInfo);
        }
        catch (Exception ex)
        {
            this.LogX().Error(ex, "模组载入失败, 路径: {path}", directory);
            DialogService.ShowMessageBoxX(this, "模组载入失败, 详情请查看日志".Translate());
        }
    }

    /// <summary>
    /// 打开模组文件夹
    /// </summary>
    /// <param name="history"></param>
    [ReactiveCommand]
    public void OpenModPath(ModMakeHistory history)
    {
        if (Directory.Exists(history.SourcePath) is false)
        {
            if (
                DialogService.ShowMessageBoxX(
                    this,
                    $"历史路径不存在, 是否删除?".Translate(),
                    "数据错误".Translate(),
                    MessageBoxButton.YesNo
                )
                is not true
            )
                return;
            RemoveHistory(history);
        }
        else
        {
            NativeUtils.OpenLink(history.SourcePath);
        }
    }

    /// <summary>
    /// 打开模组文件夹
    /// </summary>
    /// <param name="history"></param>
    [ReactiveCommand]
    public void OpenModMakerPath(ModMakeHistory history)
    {
        NativeUtils.OpenLink(NativeData.ModMakerBaseDirectory);
    }
    #endregion

    /// <summary>
    /// Wiki链接
    /// </summary>
    public const string WikiLink = "https://github.com/LorisYounger/VPet.ModMaker/wiki";

    [ReactiveCommand]
    private void OpenWikiLink()
    {
        try
        {
            NativeUtils.OpenLink(WikiLink);
        }
        catch
        {
            if (
                DialogService.ShowMessageBoxX(
                    this,
                    "无法打开链接,需要复制自行访问吗".Translate(),
                    "打开链接失败".Translate(),
                    MessageBoxButton.YesNo,
                    icon: MessageBoxImage.Warning
                )
                is not true
            )
                return;
            NativeUtils.ClipboardSetText(WikiLink);
            DialogService.ShowMessageBoxX(this, "已复制到剪贴板".Translate());
        }
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (_disposed)
            return;
        base.Dispose(disposing);
        foreach (var history in Histories)
            history.Image?.CloseStreamWhenNoReference();
        Histories.Clear();
    }
}
