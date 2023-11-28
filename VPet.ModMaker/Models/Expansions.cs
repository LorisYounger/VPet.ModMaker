﻿using System;
using System.Collections;
using System.Collections.Generic;
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
using VPet.ModMaker.Models.ModModel;
using VPet_Simulator.Core;

namespace VPet.ModMaker.Models;

/// <summary>
/// 拓展
/// </summary>
public static class Extensions
{
    /// <summary>
    ///
    /// </summary>
    /// <param name="source"></param>
    /// <param name="value"></param>
    /// <param name="comparisonType"></param>
    /// <returns></returns>
    public static bool Contains(this string source, string value, StringComparison comparisonType)
    {
        return source.IndexOf(value, comparisonType) >= 0;
    }

    //public static string GetSourceFile(this BitmapImage image)
    //{
    //    return ((FileStream)image.StreamSource).Name;
    //}

    /// <summary>
    /// 关闭流
    /// </summary>
    /// <param name="source">图像资源</param>
    public static void CloseStream(this ImageSource source)
    {
        if (source is BitmapImage image)
        {
            image.StreamSource?.Close();
        }
    }

    /// <summary>
    /// 图像复制
    /// </summary>
    /// <param name="image">图像</param>
    /// <returns>复制的图像</returns>
    public static BitmapImage Copy(this BitmapImage image)
    {
        if (image is null)
            return null;
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

    /// <summary>
    /// 保存至Png图片
    /// </summary>
    /// <param name="image">图片资源</param>
    /// <param name="path">路径</param>
    public static void SaveToPng(this BitmapImage image, string path)
    {
        if (image is null)
            return;
        if (path.EndsWith(".png") is false)
            path += ".png";
        var encoder = new PngBitmapEncoder();
        var stream = image.StreamSource;
        // 保存位置
        var position = stream.Position;
        // 必须要重置位置, 否则EndInit将出错
        stream.Seek(0, SeekOrigin.Begin);
        encoder.Frames.Add(BitmapFrame.Create(image.StreamSource));
        // 恢复位置
        stream.Seek(position, SeekOrigin.Begin);
        using var fs = new FileStream(path, FileMode.Create);
        encoder.Save(fs);
    }

    /// <summary>
    /// 尝试添加
    /// </summary>
    /// <typeparam name="TKey">键类型</typeparam>
    /// <typeparam name="TValue"></typeparam>
    /// <param name="dictionary"></param>
    /// <param name="key">键</param>
    /// <param name="value">值</param>
    /// <returns>成功为 <see langword="true"/> 失败为 <see langword="false"/></returns>
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
    /// 是含有名称的动画
    /// </summary>
    /// <param name="graphType"></param>
    /// <returns></returns>
    public static bool IsHasNameAnime(this GraphInfo.GraphType graphType)
    {
        return AnimeTypeModel.HasNameAnimes.Contains(graphType);
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

    public static T? FindVisualChild<T>(this DependencyObject obj)
        where T : DependencyObject
    {
        if (obj is null)
            return null;
        var count = VisualTreeHelper.GetChildrenCount(obj);
        for (int i = 0; i < count; i++)
        {
            var child = VisualTreeHelper.GetChild(obj, i);
            if (child is T t)
                return t;
            if (FindVisualChild<T>(child) is T childItem)
                return childItem;
        }
        return null;
    }

    public static T FindParent<T>(this DependencyObject obj)
        where T : class
    {
        while (obj != null)
        {
            if (obj is T)
                return obj as T;
            obj = VisualTreeHelper.GetParent(obj);
        }
        return null;
    }

    public static string GetFullInfo(this CultureInfo cultureInfo)
    {
        return $"{cultureInfo.DisplayName} [{cultureInfo.Name}]";
    }

    /// <summary>
    /// 尝试使用索引获取值
    /// </summary>
    /// <typeparam name="T">值类型</typeparam>
    /// <param name="list">列表</param>
    /// <param name="index">索引</param>
    /// <param name="value">值</param>
    /// <returns>成功为 <see langword="true"/> 失败为 <see langword="false"/></returns>
    public static bool TryGetValue<T>(this IList<T> list, int index, out T value)
    {
        value = default;
        if (index < 0 || index >= list.Count)
            return false;
        value = list[index];
        return true;
    }

    /// <summary>
    /// 尝试使用索引获取值
    /// </summary>
    /// <typeparam name="T">值类型</typeparam>
    /// <param name="list">列表</param>
    /// <param name="index">索引</param>
    /// <param name="value">值</param>
    /// <returns>成功为 <see langword="true"/> 失败为 <see langword="false"/></returns>
    public static bool TryGetValue<T>(this IList list, int index, out object value)
    {
        value = default;
        if (index < 0 || index >= list.Count)
            return false;
        value = list[index];
        return true;
    }

    /// <summary>
    /// 获取目标
    /// </summary>
    /// <typeparam name="T">类型</typeparam>
    /// <param name="weakReference">弱引用</param>
    /// <returns>获取成功返回目标值, 获取失败则返回 <see langword="null"/></returns>
    public static T? GetTarget<T>(this WeakReference<T> weakReference)
        where T : class
    {
        return weakReference.TryGetTarget(out var t) ? t : null;
    }

    /// <summary>
    /// 枚举出带有索引值的枚举值
    /// </summary>
    /// <typeparam name="T">值类型</typeparam>
    /// <param name="collection">集合</param>
    /// <returns>带有索引的枚举值</returns>
    public static IEnumerable<ItemInfo<T>> Enumerate<T>(this IEnumerable<T> collection)
    {
        var index = 0;
        foreach (var item in collection)
            yield return new(index++, item);
    }
}

/// <summary>
/// 项信息
/// </summary>
/// <typeparam name="T"></typeparam>
[DebuggerDisplay("[{Index}, {Value}]")]
public readonly struct ItemInfo<T>
{
    /// <summary>
    /// 索引值
    /// </summary>
    public int Index { get; }

    /// <summary>
    /// 值
    /// </summary>
    public T Value { get; }

    /// <inheritdoc/>
    /// <param name="value">值</param>
    /// <param name="index">索引值</param>
    public ItemInfo(int index, T value)
    {
        Index = index;
        Value = value;
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        return $"[{Index}, {Value}]";
    }
}
