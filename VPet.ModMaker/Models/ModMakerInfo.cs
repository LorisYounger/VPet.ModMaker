using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
