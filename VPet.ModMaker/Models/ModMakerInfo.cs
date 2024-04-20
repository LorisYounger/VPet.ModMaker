using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPet.ModMaker.Models.ModModel;
using VPet_Simulator.Core;

namespace VPet.ModMaker.Models;

/// <summary>
/// 模组制作器信息
/// </summary>
public static class ModMakerInfo
{
    /// <summary>
    /// 基础目录
    /// </summary>
    public const string BaseDirectory = nameof(ModMaker);

    /// <summary>
    /// 历史文件
    /// </summary>
    public const string HistoryFile = $"{BaseDirectory}\\history.lps";

    /// <summary>
    /// 信息文件
    /// </summary>
    public const string InfoFile = "info.lps";

    /// <summary>
    /// 游戏版本
    /// </summary>
    public static int GameVersion { get; set; } = 11000;

    /// <summary>
    /// 本体的宠物
    /// <para>
    /// (PetID, PetModel)
    /// </para>
    /// </summary>
    public static Dictionary<string, PetModel> MainPets { get; } = new();

    /// <summary>
    /// 本地风格
    /// </summary>
    public static NativeStyles NativeStyles { get; } = new();

    /// <summary>
    /// 是含有名称的动画
    /// </summary>
    /// <param name="graphType"></param>
    /// <returns></returns>
    public static bool IsHasNameAnime(this GraphInfo.GraphType graphType)
    {
        return AnimeTypeModel.HasNameAnimes.Contains(graphType);
    }
}
