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
    public static BitmapImage LoadImageToStream(string imagePath)
    {
        BitmapImage bitmapImage = new();
        bitmapImage.BeginInit();
        try
        {
            bitmapImage.StreamSource = new StreamReader(imagePath).BaseStream;
        }
        finally
        {
            bitmapImage.EndInit();
        }
        return bitmapImage;
    }

    public static BitmapImage LoadImageToStream(this BitmapImage image)
    {
        return LoadImageToStream(image.GetSourceFile());
    }

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
        return bitmapImage;
    }

    public static BitmapImage LoadImageToMemoryStream(this BitmapImage image)
    {
        return LoadImageToMemoryStream(image.GetSourceFile());
    }
}
