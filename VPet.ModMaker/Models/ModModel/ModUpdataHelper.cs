using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using HKW.HKWUtils.Extensions;
using LinePutScript.Localization.WPF;
using VPet.ModMaker.ViewModels.ModEdit;
using VPet.ModMaker.Views;
using VPet.ModMaker.Views.ModEdit;

namespace VPet.ModMaker.Models;

/// <summary>
/// 模组升级助手
/// </summary>
public static class ModUpdataHelper
{
    public static int LastVersion => UpdataAction.Last().Key;

    /// <summary>
    /// 能否升级模组
    /// </summary>
    /// <param name="mod">模组</param>
    /// <param name="version">版本</param>
    /// <returns>可以升级为 <see langword="true"/> 不可以为 <see langword="false"/></returns>
    public static bool CanUpdata(ModInfoModel mod)
    {
        if (mod.ModVersion >= LastVersion)
            return false;
        return true;
    }

    /// <summary>
    /// 升级模组
    /// </summary>
    /// <param name="mod">模组</param>
    /// <param name="version">版本</param>
    /// <returns>可以升级为 <see langword="true"/> 不可以为 <see langword="false"/></returns>
    public static bool Updata(ModInfoModel mod)
    {
        if (CanUpdata(mod) is false)
            return false;
        foreach (var action in UpdataAction)
        {
            if (mod.ModVersion >= action.Key)
                continue;
            try
            {
                // 更新模组
                action.Value(mod);
                // 更新支持的游戏版本
                mod.GameVersion = action.Key;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    ModEditWindow.Current,
                    "模组更新失败\n当前支持的游戏版本: {0}\n目标支持的游戏版本: {1}\n{2}".Translate(
                        mod.ModVersion,
                        action.Key,
                        ex
                    )
                );
                return false;
            }
        }
        return true;
    }

    public static SortedDictionary<int, Action<ModInfoModel>> UpdataAction { get; } =
        new()
        {
            [11000] = (m) =>
            {
                foreach (var pet in m.Pets)
                {
                    // 修改宠物默认ID
                    if (pet.ID == "默认虚拟桌宠")
                        pet.ID = "vup";
                    foreach (var work in pet.Works)
                    {
                        // 修复工作溢出
                        work.FixOverLoad();
                    }
                }
            },
        };
}
