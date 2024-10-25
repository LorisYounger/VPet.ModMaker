using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using HKW.WPF.MVVMDialogs;
using HKW.WPF.MVVMDialogs.Windows;
using VPet.ModMaker.Models.ModModel;
using VPet.ModMaker.ViewModels.ModEdit;
using VPet.ModMaker.Views.ModEdit;
using VPet_Simulator.Core;

namespace VPet.ModMaker.Native;

/// <summary>
/// 拓展
/// </summary>
public static class NativeExtensions
{
    /// <summary>
    /// 是含有名称的动画
    /// </summary>
    /// <param name="graphType"></param>
    /// <returns></returns>
    public static bool IsHasNameAnime(this GraphInfo.GraphType graphType)
    {
        return AnimeTypeModel.HasNameAnimes.Contains(graphType);
    }

    ///// <summary>
    /////
    ///// </summary>
    ///// <param name="source"></param>
    ///// <param name="value"></param>
    ///// <param name="comparisonType"></param>
    ///// <returns></returns>
    //public static bool Contains(this string source, string value, StringComparison comparisonType)
    //{
    //    return source.IndexOf(value, comparisonType) >= 0;
    //}

    //public static string GetSourceFile(this BitmapImage image)
    //{
    //    return ((FileStream)image.StreamSource).Name;
    //}

    /// <summary>
    /// 图像复制
    /// </summary>
    /// <param name="image">图像</param>
    /// <returns>复制的图像</returns>
    public static BitmapImage CloneStream(this BitmapImage image)
    {
        if (image is null)
            return null!;
        BitmapImage newImage = new();
        newImage.BeginInit();
        newImage.DecodePixelWidth = image.DecodePixelWidth;
        newImage.DecodePixelHeight = image.DecodePixelHeight;
        try
        {
            var ms = new MemoryStream();
            var position = image.StreamSource.Position;
            image.StreamSource.Seek(0, SeekOrigin.Begin);
            image.StreamSource.CopyTo(ms);
            image.StreamSource.Seek(position, SeekOrigin.Begin);
            newImage.StreamSource = ms;
        }
        finally
        {
            newImage.EndInit();
        }
        return newImage;
    }

    /// <summary>
    /// 保存至Png图片
    /// </summary>
    /// <param name="image">图片资源</param>
    /// <param name="path">路径</param>
    //public static void SaveToPng(this BitmapImage image, string path)
    //{
    //    if (image is null)
    //        return;
    //    if (path.EndsWith(".png") is false)
    //        path += ".png";
    //    var encoder = new PngBitmapEncoder();
    //    var stream = image.StreamSource;
    //    // 保存位置
    //    var position = stream.Position;
    //    // 必须要重置位置, 否则EndInit将出错
    //    stream.Seek(0, SeekOrigin.Begin);
    //    encoder.Frames.Add(BitmapFrame.Create(image.StreamSource));
    //    // 恢复位置
    //    stream.Seek(position, SeekOrigin.Begin);
    //    using var fs = new FileStream(path, FileMode.Create);
    //    encoder.Save(fs);
    //}
    public static void SaveToPng(this BitmapImage image, string path)
    {
        if (image is null)
            return;
        if (path.EndsWith(".png") is false)
            path += ".png";
        var stream = image.StreamSource;
        // 保存位置
        var position = stream.Position;
        // 必须要重置位置, 否则EndInit将出错
        stream.Seek(0, SeekOrigin.Begin);
        using var fs = new FileStream(path, FileMode.Create);
        stream.CopyTo(fs);
        // 恢复位置
        stream.Seek(position, SeekOrigin.Begin);
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

    /// <summary>
    /// 注册添加文化, 使用默认对话框
    /// </summary>
    /// <param name="strongViewLocator">强视图定位器</param>
    public static void RegisterAddCultureDialog(this StrongViewLocatorX strongViewLocator)
    {
        strongViewLocator.RegisterDialogX<AddCultureVM, AddCulturePage, DialogWindowX>();
    }
}
