using System.Windows;
using System.Windows.Controls;
using HKW.HKWReactiveUI;
using HKW.HKWUtils;
using LinePutScript.Localization.WPF;
using VPet.ModMaker.ViewModels;
using VPet.ModMaker.Views;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.Models;

/// <summary>
/// 模组制作器
/// </summary>
public class ModMaker : MainPlugin, IEnableLogger<ViewModelBase>
{
    /// <summary>
    /// 名称
    /// </summary>
    public override string PluginName => "ModMaker";

    /// <summary>
    /// 单例
    /// </summary>
    public ModMakerWindow ModMakerWindow = null!;

    /// <inheritdoc/>
    public ModMaker(IMainWindow mainwin)
        : base(mainwin) { }

    /// <summary>
    /// 载入插件
    /// </summary>
    public override void LoadPlugin()
    {
        //Set = MW.Set.FindLine("ModMaker");
        var modset = MW.Main.ToolBar.MenuMODConfig;
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

        if (
            Application.Current.Resources.MergedDictionaries.Contains(NativeData.NativeStyles)
            is false
        )
            Application.Current.Resources.MergedDictionaries.Add(NativeData.NativeStyles);
    }

    /// <summary>
    /// I18n资源
    /// </summary>
    public static I18nResource<string, string> I18nResource { get; } =
        new() { FillDefaultValueToData = true, DefaultValue = string.Empty };

    /// <inheritdoc/>
    public override void Setting()
    {
        if (ModMakerWindow is null)
        {
            // 载入ModMaker资源
            ModMakerWindow = new ModMakerWindow();
            // 设置游戏版本
            NativeData.GameVersion = MW.version;
            // 载入本体宠物
            foreach (var pet in MW.Pets)
            {
                try
                {
                    NativeData.MainPets.TryAdd(pet.Name, new(pet, I18nResource, true));
                }
                catch (Exception ex)
                {
                    this.LogX().Warn(ex, "载入本体宠物错误, PetID: {id}", pet.Name);
                }
            }
            ModMakerWindow.Show();
            ModMakerWindow.Closed += Maker_Closed;
        }
        else
        {
            ModMakerWindow.Activate();
        }
    }

    private void Maker_Closed(object? sender, EventArgs e)
    {
        ModMakerWindow.Closed -= Maker_Closed;
        ModMakerWindow = null!;
    }
}
