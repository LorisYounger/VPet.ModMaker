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

public class ModMaker : MainPlugin
{
    //public ILine Set;
    public override string PluginName => "ModMaker";

    public ModMakerWindow Maker = null!;

    public ModMaker(IMainWindow mainwin)
        : base(mainwin) { }

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
        Application.Current.Resources.MergedDictionaries.Add(ModMakerInfo.NativeStyles);
    }

    public override void Setting()
    {
        if (Maker == null)
        {
            // 载入ModMaker资源
            Maker = new ModMakerWindow();
            // 设置游戏版本
            ModMakerInfo.GameVersion = MW.version;
            // 载入本体宠物
            foreach (var pet in MW.Pets)
                ModMakerInfo.MainPets.TryAdd(
                    pet.Name,
                    new(pet, true) { I18nResource = ModInfoModel.Current.I18nResource }
                );
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
