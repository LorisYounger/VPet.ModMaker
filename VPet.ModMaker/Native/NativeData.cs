using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPet.ModMaker;

/// <summary>
/// 本地数据
/// </summary>
public static class NativeData
{
    /// <summary>
    /// 基准目录
    /// </summary>
    public static string BaseDirectory => AppDomain.CurrentDomain.BaseDirectory;

    /// <summary>
    /// 模组制作器基准目录
    /// </summary>
    public static string VPetHouseBaseDirectory { get; } =
        Path.Combine(BaseDirectory, nameof(ModMaker));

    /// <summary>
    /// 模组制作器目录
    /// </summary>
    public static string VPetHouseDirectory { get; } = nameof(ModMaker);
}
