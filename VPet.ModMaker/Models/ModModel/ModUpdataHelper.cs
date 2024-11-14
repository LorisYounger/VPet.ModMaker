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
    /// <summary>
    /// 最新游戏版本
    /// </summary>
    public static int LastGameVersion => UpdataAction.Last().Key;

    /// <summary>
    /// 能否升级模组
    /// </summary>
    /// <param name="mod">模组</param>
    /// <returns>可以升级为 <see langword="true"/> 不可以为 <see langword="false"/></returns>
    public static bool CanUpdata(ModInfoModel mod)
    {
        if (mod.GameVersion >= LastGameVersion)
            return false;
        return true;
    }

    /// <summary>
    /// 升级模组
    /// </summary>
    /// <param name="mod">模组</param>
    /// <returns>更新完成的目标版本</returns>
    public static int Updata(ModInfoModel mod)
    {
        if (CanUpdata(mod) is false)
            return mod.GameVersion;
        foreach (var action in UpdataAction)
        {
            if (mod.GameVersion >= action.Key)
                continue;
            // 更新模组
            action.Value(mod);
            // 更新支持的游戏版本
            mod.GameVersion = action.Key;
        }
        return mod.GameVersion;
    }

    /// <summary>
    /// 更新行动
    /// </summary>
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
