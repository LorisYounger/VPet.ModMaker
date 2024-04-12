using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;
using LinePutScript.Converter;

namespace VPet.ModMaker.Models;

/// <summary>
/// 模组制作历史
/// </summary>
public class ModMakeHistory : IEquatable<ModMakeHistory>
{
    public ModMakeHistory() { }

    /// <summary>
    /// 图片
    /// </summary>
    public BitmapImage? Image { get; set; }

    /// <summary>
    /// Id
    /// </summary>
    [Line(ignoreCase: true)]
    public string ID { get; set; } = string.Empty;

    /// <summary>
    /// 路径
    /// </summary>
    private string _sourcePath = string.Empty;

    /// <summary>
    /// 资源路径
    /// </summary>
    [Line(ignoreCase: true)]
    public string SourcePath
    {
        get => _sourcePath;
        set
        {
            if (string.IsNullOrWhiteSpace(_sourcePath) is false)
                Image?.CloseStream();
            _sourcePath = value;
            var imagePath = Path.Combine(_sourcePath, "icon.png");
            if (File.Exists(imagePath))
                Image = NativeUtils.LoadImageToMemoryStream(imagePath);
        }
    }

    /// <summary>
    /// 模组信息文件
    /// </summary>
    public string InfoFile => Path.Combine(SourcePath, "info.lps");

    /// <summary>
    /// 最后编辑时间
    /// </summary>
    [Line(ignoreCase: true)]
    public DateTime LastTime { get; set; } = DateTime.Now;

    public bool Equals(ModMakeHistory? other)
    {
        return SourcePath.Equals(other?.SourcePath);
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return Equals(obj as ModMakeHistory);
    }

    public override int GetHashCode()
    {
        return SourcePath.GetHashCode();
    }
}
