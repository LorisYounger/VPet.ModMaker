using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace VPet.ModMaker.Models;

/// <summary>
/// 工具
/// </summary>
public static class Utils
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

    /// <summary>
    /// 载入图片至内存流
    /// </summary>
    /// <param name="imagePath"></param>
    /// <returns></returns>
    public static BitmapImage LoadImageToMemoryStream(string imagePath)
    {
        BitmapImage bitmapImage = new();
        bitmapImage.BeginInit();
        try
        {
            var ms = new MemoryStream();
            using var sr = new StreamReader(imagePath);
            sr.BaseStream.CopyTo(ms);
            bitmapImage.StreamSource = ms;
        }
        finally
        {
            bitmapImage.EndInit();
        }
        if (bitmapImage.PixelWidth > DecodePixelWidth)
            bitmapImage.DecodePixelWidth = DecodePixelWidth;
        return bitmapImage;
    }
}
