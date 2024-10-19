using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Reflection;

namespace VPet.ModMaker.Resources;

/// <summary>
/// 本地资源
/// </summary>
internal class NativeResources
{
    /// <summary>
    /// 资源基路径
    /// </summary>
    public const string ResourcePath = $"{nameof(VPet)}.{nameof(ModMaker)}.{nameof(Resources)}";

    /// <summary>
    /// 食物图片
    /// </summary>
    public const string FoodImage = $"{ResourcePath}.food.png";

    /// <summary>
    /// NLog配置文件
    /// </summary>
    public const string NLogConfig = $"{ResourcePath}.NLog.config";

    #region Native
    private static readonly Assembly _assembly = Assembly.GetExecutingAssembly();

    /// <summary>
    /// 获取资源流
    /// </summary>
    /// <param name="resourceName">资源名</param>
    /// <returns>资源流</returns>
    public static Stream GetStream(string resourceName) =>
        _assembly.GetManifestResourceStream(resourceName)!;

    /// <summary>
    /// 尝试获取资源流
    /// </summary>
    /// <param name="resourceName">资源名</param>
    /// <param name="resourceStream">资源流</param>
    /// <returns>成功为 <see langword="true"/> 失败为 <see langword="false"/></returns>
    public static bool TryGetStream(
        string resourceName,
        [MaybeNullWhen(false)] out Stream resourceStream
    )
    {
        resourceStream = null;
        if (_assembly.GetManifestResourceStream(resourceName) is not Stream stream)
            return false;
        resourceStream = stream;
        return true;
    }

    /// <summary>
    /// 将流保存至文件
    /// </summary>
    /// <param name="resourceName">资源名</param>
    /// <param name="path">文件路径</param>
    /// <returns>成功为 <see langword="true"/> 失败为 <see langword="false"/></returns>
    public static bool SaveTo(string resourceName, string path)
    {
        if (_assembly.GetManifestResourceStream(resourceName) is not Stream stream)
            return false;
        using var sr = new StreamReader(stream);
        using var sw = new StreamWriter(path);
        sr.BaseStream.CopyTo(sw.BaseStream);
        return true;
    }
    #endregion
}
