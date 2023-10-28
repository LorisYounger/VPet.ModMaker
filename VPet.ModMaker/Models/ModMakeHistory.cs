using LinePutScript.Converter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace VPet.ModMaker.Models;

/// <summary>
/// 模组制作历史
/// </summary>
public class ModMakeHistory
{
    /// <summary>
    /// 图片
    /// </summary>
    public BitmapImage Image { get; set; }

    /// <summary>
    /// Id
    /// </summary>
    [Line(ignoreCase: true)]
    public string Id { get; set; }

    /// <summary>
    /// 路径
    /// </summary>
    private string _path;

    /// <summary>
    /// 资源路径
    /// </summary>
    [Line(ignoreCase: true)]
    public string SourcePath
    {
        get => _path;
        set
        {
            _path = value;
            var imagePath = Path.Combine(_path, "icon.png");
            if (File.Exists(imagePath))
                Image = Utils.LoadImageToMemoryStream(imagePath);
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
}
