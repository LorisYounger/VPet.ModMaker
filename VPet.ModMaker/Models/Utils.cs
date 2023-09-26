using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;

namespace VPet.ModMaker.Models;

public static class Utils
{
    public const int DecodePixelWidth = 250;
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

    public static BitmapImage LoadImageToMemoryStream(string imagePath)
    {
        BitmapImage bitmapImage = new();
        bitmapImage.BeginInit();
        bitmapImage.DecodePixelWidth = DecodePixelWidth;
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
        return bitmapImage;
    }
}
