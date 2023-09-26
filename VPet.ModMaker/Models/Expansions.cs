using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace VPet.ModMaker.Models;

public static class Extensions
{
    public static bool Contains(this string source, string value, StringComparison comparisonType)
    {
        return source.IndexOf(value, comparisonType) >= 0;
    }

    //public static string GetSourceFile(this BitmapImage image)
    //{
    //    return ((FileStream)image.StreamSource).Name;
    //}

    public static void CloseStream(this ImageSource source)
    {
        if (source is BitmapImage image)
        {
            image.StreamSource?.Close();
        }
    }

    public static BitmapImage Copy(this BitmapImage image)
    {
        BitmapImage newImage = new();
        newImage.BeginInit();
        newImage.DecodePixelWidth = image.DecodePixelWidth;
        newImage.DecodePixelHeight = image.DecodePixelHeight;
        try
        {
            using var bitmap = new Bitmap(image.StreamSource);
            var ms = new MemoryStream();
            bitmap.Save(ms, ImageFormat.Png);
            image.StreamSource.CopyTo(ms);
            newImage.StreamSource = ms;
        }
        finally
        {
            newImage.EndInit();
        }
        return newImage;
    }

    public static void SaveToPng(this BitmapSource image, string path)
    {
        if (path.EndsWith(".png") is false)
            path += ".png";
        var encoder = new PngBitmapEncoder();
        encoder.Frames.Add(BitmapFrame.Create(image));
        using var fs = new FileStream(path, FileMode.Create);
        encoder.Save(fs);
    }

    public static bool TryAdd<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary,
        TKey key,
        TValue value
    )
    {
        if (dictionary.ContainsKey(key))
            return false;
        dictionary.Add(key, value);
        return true;
    }

    /// <summary>
    /// 流内容对比
    /// </summary>
    /// <param name="source">原始流</param>
    /// <param name="target">目标流</param>
    /// <param name="bufferLength">缓冲区大小 (越大速度越快(流内容越大效果越明显), 但会提高内存占用 (bufferSize = bufferLength * sizeof(long) * 2))</param>
    /// <returns>内容相同为 <see langword="true"/> 否则为 <see langword="false"/></returns>
    public static bool ContentsEqual(this Stream source, Stream target, int bufferLength = 8)
    {
        int bufferSize = bufferLength * sizeof(long);
        var sourceBuffer = new byte[bufferSize];
        var targetBuffer = new byte[bufferSize];
        while (true)
        {
            int sourceCount = ReadFullBuffer(source, sourceBuffer);
            int targetCount = ReadFullBuffer(target, targetBuffer);
            if (sourceCount != targetCount)
                return false;
            if (sourceCount == 0)
                return true;

            for (int i = 0; i < sourceCount; i += sizeof(long))
                if (BitConverter.ToInt64(sourceBuffer, i) != BitConverter.ToInt64(targetBuffer, i))
                    return false;
        }
        static int ReadFullBuffer(Stream stream, byte[] buffer)
        {
            int bytesRead = 0;
            while (bytesRead < buffer.Length)
            {
                int read = stream.Read(buffer, bytesRead, buffer.Length - bytesRead);
                if (read == 0)
                    return bytesRead;
                bytesRead += read;
            }
            return bytesRead;
        }
    }
}
