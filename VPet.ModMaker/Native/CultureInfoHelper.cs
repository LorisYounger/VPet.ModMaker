using System.Globalization;
using System.IO;
using HKW.HKWUtils.Extensions;

namespace VPet.ModMaker;

/// <summary>
/// 自定义文化信息（用于支持非标准 LCID / 自定义名称的文化）
/// </summary>
public sealed class CustomCultureInfo : CultureInfo
{
    private readonly string _name;

    /// <inheritdoc/>
    public CustomCultureInfo(string name)
        : base(string.Empty)
    {
        _name = name;
    }

    /// <inheritdoc/>
    public override string Name => _name;

    /// <inheritdoc/>
    public override string DisplayName => _name;

    /// <inheritdoc/>
    public override string NativeName => _name;

    /// <inheritdoc/>
    public override string EnglishName => _name;

    /// <inheritdoc/>
    public override string ToString() => _name;

    /// <inheritdoc/>
    public override bool Equals(object? value)
    {
        if (ReferenceEquals(this, value))
            return true;
        if (value is CultureInfo other)
            return string.Equals(_name, other.Name, StringComparison.OrdinalIgnoreCase);
        return false;
    }

    /// <inheritdoc/>
    public override int GetHashCode() =>
        StringComparer.OrdinalIgnoreCase.GetHashCode(_name);
}

/// <summary>
/// 文化信息辅助工具
/// </summary>
public static class CultureInfoHelper
{
    private static readonly char[] InvalidLpsChars = ['#', ':', '|'];

    /// <summary>
    /// 校验文化名称是否合法（可安全用于目录名、文件名及 LPS 键）
    /// </summary>
    public static bool IsValidCultureName(string? name)
    {
        if (string.IsNullOrWhiteSpace(name))
            return false;
        var trimmed = name.Trim();
        if (trimmed is "." or "..")
            return false;
        if (trimmed.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
            return false;
        if (trimmed.IndexOfAny(InvalidLpsChars) >= 0)
            return false;
        return true;
    }

    /// <summary>
    /// 尝试获取标准文化信息（仅当标准文化名称与输入一致时返回 true）
    /// </summary>
    public static bool TryGetStandardCultureInfo(string? name, out CultureInfo culture)
    {
        culture = null!;
        if (string.IsNullOrWhiteSpace(name))
            return false;
        var trimmed = name.Trim();
        if (
            CultureInfo.TryGetCultureInfo(trimmed, out var info)
            && string.Equals(info.Name, trimmed, StringComparison.OrdinalIgnoreCase)
        )
        {
            culture = info;
            return true;
        }
        return false;
    }

    /// <summary>
    /// 获取标准文化或创建自定义文化
    /// </summary>
    public static CultureInfo GetOrCreateCultureInfo(string name)
    {
        var trimmed = name.Trim();
        if (TryGetStandardCultureInfo(trimmed, out var culture))
            return culture;
        return new CustomCultureInfo(trimmed);
    }
}
