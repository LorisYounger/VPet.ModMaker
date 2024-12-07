using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace VPet.ModMaker;

/// <summary>
/// 工具
/// </summary>
public static class NativeUtils
{
    /// <summary>
    /// 粘贴板复制文本
    /// </summary>
    public static Action<string> ClipboardSetText { get; set; } = null!;

    /// <summary>
    /// 解码像素宽度
    /// </summary>
    public const int DecodePixelWidth = 250;

    /// <summary>
    /// 解码像素高度
    /// </summary>
    public const int DecodePixelHeight = 250;

    /// <summary>
    /// 分隔符
    /// </summary>
    public static char[] Separator { get; } = ['_'];

    /// <summary>
    /// 打开文件
    /// </summary>
    /// <param name="filePath">文件路径</param>
    public static void OpenLink(string filePath)
    {
        System
            .Diagnostics.Process.Start(
                new System.Diagnostics.ProcessStartInfo(filePath) { UseShellExecute = true }
            )
            ?.Close();
    }

    /// <summary>
    /// 从资源管理器打开文件
    /// </summary>
    /// <param name="filePath">文件路径</param>
    public static void OpenFileInExplorer(string filePath)
    {
        System
            .Diagnostics.Process.Start("Explorer", $"/select,{Path.GetFullPath(filePath)}")
            ?.Close();
    }
}
