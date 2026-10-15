using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using HanumanInstitute.MvvmDialogs;
using HKW.HKWUtils;
using HKW.HKWUtils.Collections;
using HKW.WPF.Extensions;
using LinePutScript.Localization.WPF;
using Microsoft.Extensions.DependencyInjection;
using VPet_Simulator.Windows.Interface;
using VPet.ModMaker.Models;
using VPet.ModMaker.ViewModels;
using VPet.ModMaker.Views;

namespace VPet.ModMaker;

/// <summary>
/// 模组制作器
/// </summary>
public class ModMaker : MainPlugin
{
    /// <inheritdoc/>
    public ModMaker(IMainWindow mainwin)
        : base(mainwin) { }

    /// <summary>
    /// 名称
    /// </summary>
    public override string PluginName => "ModMaker";

    /// <summary>
    /// 单例
    /// </summary>
    public ModMakerWindow? ModMakerWindow { get; private set; }

    /// <summary>
    /// 服务提供商
    /// </summary>
    public IServiceProvider Services => field ??= new AppServices().BuildServiceProvider();

    /// <summary>
    /// 载入插件
    /// </summary>
    public override void LoadPlugin()
    {
        //Set = MW.Set.FindLine("ModMaker");
        if (MW is not null)
        {
            var modset = MW.Main.ToolBar.MenuMODConfig;
            ModMaker.GameVersion = MW.version;
            modset.Visibility = Visibility.Visible;
            var menuset = new MenuItem()
            {
                Header = "Mod制作器".Translate(),
                HorizontalContentAlignment = HorizontalAlignment.Center,
            };
            menuset.Click += (s, e) =>
            {
                Setting();
            };
            modset.Items.Add(menuset);

            //载入本体宠物
            foreach (var pet in MW.Pets)
            {
                try
                {
                    MainPets.TryAdd(pet.Name, new(pet, I18nResource, true));
                }
                catch (Exception ex)
                {
                    //this.LogX().Warn(ex, "载入本体宠物错误, PetID: {id}", pet.Name);
                }
            }
        }

        if (
            Application.Current.Resources.MergedDictionaries.Contains(ModMaker.NativeStyles)
            is false
        )
            Application.Current.Resources.MergedDictionaries.Add(ModMaker.NativeStyles);
    }

    /// <summary>
    /// I18n资源
    /// </summary>
    public static ObservableI18nResource<string, string> I18nResource { get; } =
        new("MainPet") { GetDefaultValue = (s, _) => s };

    /// <inheritdoc/>
    public override void Setting()
    {
        if (ModMakerWindow is null)
        {
            var vm = Services.GetService<ModMakerViewModel>()!;
            Services.GetService<IDialogService>()!.Show(null, vm);
            ModMakerWindow = (ModMakerWindow)
                vm.DialogService.DialogManager.FindViewByViewModel(vm)!.RefObj;
            ModMakerWindow?.Closed += Maker_Closed;
        }
        else
        {
            ModMakerWindow.RestoredAndAlert();
        }
    }

    private void Maker_Closed(object? sender, EventArgs e)
    {
        I18nResource.ClearData();
        ModMakerWindow?.Closed -= Maker_Closed;
        ModMakerWindow = null;
    }

    #region BasePath
    /// <summary>
    /// 基准目录
    /// </summary>
    public static string BaseDirectory => AppDomain.CurrentDomain.BaseDirectory;

    /// <summary>
    /// 模组制作器基准目录
    /// </summary>
    public static string ModMakerBaseDirectory { get; } =
        Path.Combine(BaseDirectory, nameof(ModMaker));

    #endregion

    #region Path
    /// <summary>
    /// 历史文件位置
    /// </summary>
    public static string HistoryFilePath { get; } =
        Path.Combine(ModMakerBaseDirectory, HistoryFileName);

    /// <summary>
    /// 日志文件位置
    /// </summary>
    public static string LogFilePath { get; } = Path.Combine(ModMakerBaseDirectory, "ModMaker.log");

    #endregion
    /// <summary>
    /// 本地风格
    /// </summary>
    public static NativeStyles NativeStyles { get; } = [];

    /// <summary>
    /// 信息文件
    /// </summary>
    public const string InfoFileName = "info.lps";

    /// <summary>
    /// 历史文件
    /// </summary>
    public const string HistoryFileName = "history.lps";

    /// <summary>
    /// 游戏版本
    /// </summary>
    public static int GameVersion { get; set; } = 11000;

    public static ReadOnlyList<string> ImageExtensions { get; } =
        new(["jpg", "jpeg", "png", "bmp"]);

    public static ReadOnlyList<string> PngExtensions { get; } = new(["png"]);

    public static ReadOnlyList<string> FontExtensions { get; } = new(["ttf"]);

    /// <summary>
    /// 本体的宠物
    /// <para>
    /// (PetID, PetModel)
    /// </para>
    /// </summary>
    public static Dictionary<string, PetModel> MainPets { get; } = [];

    static ModMaker()
    {
#if DEBUG
        var en = CultureInfo.GetCultureInfo("en");
        var hans = CultureInfo.GetCultureInfo("zh-Hans");
        var hant = CultureInfo.GetCultureInfo("zh-Hant");
        I18nResource.AddCulture(en);
        I18nResource.AddCulture(hans);
        I18nResource.AddCulture(hant);
        I18nResource.CurrentCulture = en;
        // 本体宠物测试
        var pet = new PetModel()
        {
            I18nResource = I18nResource,
            ID = "TestMainPet",
            FromMain = true,
        };
        var res = pet.I18nResource;

        res.SetDatas(
            [
                KeyValuePair.Create(pet.ID, "Pet[en]"),
                KeyValuePair.Create(pet.PetNameID, "PetName[en]"),
                KeyValuePair.Create(pet.DescriptionID, "Description[en]"),
            ],
            en
        );
        res.SetDatas(
            [
                KeyValuePair.Create(pet.ID, "Pet[zh-Hans]"),
                KeyValuePair.Create(pet.PetNameID, "PetName[zh-Hans]"),
                KeyValuePair.Create(pet.DescriptionID, "Description[zh-Hans]"),
            ],
            hans
        );
        res.SetDatas(
            [
                KeyValuePair.Create(pet.ID, "Pet[zh-Hant]"),
                KeyValuePair.Create(pet.PetNameID, "PetName[zh-Hant]"),
                KeyValuePair.Create(pet.DescriptionID, "Description[zh-Hant]"),
            ],
            hant
        );
        MainPets.Add(pet.ID, pet);
#endif
    }
}
