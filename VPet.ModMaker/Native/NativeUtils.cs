using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;

namespace HKW.HKWUtils;

/// <summary>
/// 工具
/// </summary>
public static class NativeUtils
{
    /// <summary>
    /// 解码像素宽度
    /// </summary>
    public const int DecodePixelWidth = 250;

    /// <summary>
    /// 解码像素高度
    /// </summary>
    public const int DecodePixelHeight = 250;
    public static char[] Separator { get; } = new char[] { '_' };

    //public static BitmapImage LoadImageToStream(string imagePath)
    //{
    //    BitmapImage bitmapImage = new();
    //    bitmapImage.BeginInit();
    //    bitmapImage.DecodePixelWidth = DecodePixelWidth;
    //    try
    //    {
    //        bitmapImage.StreamSource = new StreamReader(imagePath).BaseStream;
    //    }
    //    finally
    //    {
    //        bitmapImage.EndInit();
    //    }
    //    return bitmapImage;
    //}

    ///// <summary>
    ///// 载入图片至内存流
    ///// </summary>
    ///// <param name="imagePath">图片路径</param>
    ///// <returns></returns>
    //public static BitmapImage? LoadImageToMemoryStream(string imagePath)
    //{
    //    var bitmapImage = new BitmapImage();
    //    bitmapImage.BeginInit();
    //    try
    //    {
    //        var bytes = File.ReadAllBytes(imagePath);
    //        bitmapImage.StreamSource = new MemoryStream(bytes);
    //        bitmapImage.DecodePixelWidth = DecodePixelWidth;
    //    }
    //    catch
    //    {
    //        bitmapImage = null!;
    //    }
    //    finally
    //    {
    //        bitmapImage.EndInit();
    //    }
    //    return bitmapImage;
    //}

    ///// <summary>
    ///// 载入图片至内存流
    ///// </summary>
    ///// <param name="imageStream">图片流</param>
    ///// <returns></returns>
    //public static BitmapImage LoadImageToMemoryStream(Stream imageStream)
    //{
    //    BitmapImage bitmapImage = new();
    //    bitmapImage.BeginInit();
    //    try
    //    {
    //        bitmapImage.StreamSource = imageStream;
    //        bitmapImage.DecodePixelWidth = DecodePixelWidth;
    //    }
    //    finally
    //    {
    //        bitmapImage.EndInit();
    //    }
    //    return bitmapImage;
    //}

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
