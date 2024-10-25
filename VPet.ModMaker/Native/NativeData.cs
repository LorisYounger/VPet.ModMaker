using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VPet.ModMaker.Models;

namespace VPet.ModMaker;

/// <summary>
/// 本地数据
/// </summary>
public static class NativeData
{
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

    /// <summary>
    /// 历史文件基准目录
    /// </summary>
    public static string HistoryBaseFilePath { get; } =
        Path.Combine(BaseDirectory, HistoryFileName);

    #endregion

    #region Path
    /// <summary>
    /// 模组制作器目录
    /// </summary>
    public static string Directory { get; } = nameof(ModMaker);

    /// <summary>
    /// 历史文件目录
    /// </summary>
    public static string HistoryFilePath { get; } = Path.Combine(Directory, HistoryFileName);
    #endregion
    /// <summary>
    /// 本地风格
    /// </summary>
    public static NativeStyles NativeStyles { get; } = new();

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

    /// <summary>
    /// 本体的宠物
    /// <para>
    /// (PetID, PetModel)
    /// </para>
    /// </summary>
    public static Dictionary<string, PetModel> MainPets { get; } = [];
}
