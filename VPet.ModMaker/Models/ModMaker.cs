using LinePutScript;
using LinePutScript.Localization.WPF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using VPet_Simulator.Windows.Interface;
using VPet.ModMaker.Views;

namespace VPet.ModMaker.Models;

public class ModMaker : MainPlugin
{
    public ILine Set;
    public override string PluginName => "ModMaker";

    public ModMakerWindow Maker;

    public ModMaker(IMainWindow mainwin)
        : base(mainwin) { }

    public override void LoadPlugin()
    {
        Set = MW.Set.FindLine("ModMaker");
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
        Application.Current.Resources.MergedDictionaries.Add(Utils.ModMakerStyles);
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
            {
                var petModel = new PetModel();
                petModel.SourceId = pet.Name;
                petModel.Id.Value = pet.Name + " (来自本体)".Translate();
                ModMakerInfo.Pets.Add(petModel);
            }
            //Maker.ModMaker = this;
            Maker.Show();
            Maker.Closed += Maker_Closed;
        }
        else
        {
            Maker.Activate();
        }
    }

    private void Maker_Closed(object sender, EventArgs e)
    {
        Maker = null;
    }
}
