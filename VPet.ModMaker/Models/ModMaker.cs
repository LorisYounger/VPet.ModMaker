using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using LinePutScript;
using LinePutScript.Localization.WPF;
using VPet.ModMaker.Views;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.Models;

/// <summary>
/// 模组制作器
/// </summary>
public class ModMaker : MainPlugin
{
    /// <summary>
    /// 名称
    /// </summary>
    public override string PluginName => "ModMaker";

    /// <summary>
    /// 单例
    /// </summary>
    public ModMakerWindow Maker = null!;

    /// <inheritdoc/>
    public ModMaker(IMainWindow mainwin)
        : base(mainwin) { }

    /// <summary>
    /// 载入插件
    /// </summary>
    public override void LoadPlugin()
    {
        //Set = MW.Set.FindLine("ModMaker");
        MenuItem modset = MW.Main.ToolBar.MenuMODConfig;
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
        Application.Current.Resources.MergedDictionaries.Add(NativeData.NativeStyles);
    }

    /// <summary>
    /// I18n资源
    /// </summary>
    public static I18nResource<string, string> I18nResource { get; } =
        new() { FillDefaultValueToNewCulture = true, DefaultValue = string.Empty };

    /// <inheritdoc/>
    public override void Setting()
    {
        if (Maker is null)
        {
            // 载入ModMaker资源
            Maker = new ModMakerWindow();
            // 设置游戏版本
            NativeData.GameVersion = MW.version;
            // 载入本体宠物
            foreach (var pet in MW.Pets)
                NativeData.MainPets.TryAdd(pet.Name, new(pet, I18nResource, true));
            //Maker.ModMaker = this;
            Maker.Show();
            Maker.Closed += Maker_Closed;
        }
        else
        {
            Maker.Activate();
        }
    }

    private void Maker_Closed(object? sender, EventArgs e)
    {
        Maker = null!;
    }
}
