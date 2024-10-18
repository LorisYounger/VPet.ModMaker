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
using VPet_Simulator.Core;

namespace HKW.HKWUtils;

/// <summary>
/// 拓展
/// </summary>
public static class NativeExtensions
{
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
    /// 关闭流
    /// </summary>
    /// <param name="source">图像资源</param>
    public static void CloseStream(this ImageSource source)
    {
        if (source is not BitmapImage image)
            return;
        image.StreamSource?.Close();
    }

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

    public static void SetDataContext<T>(this Window window, Action? closedAction = null)
        where T : new()
    {
        window.DataContext = new T();
        window.Closed += (s, e) =>
        {
            try
            {
                closedAction?.Invoke();
                window.DataContext = null;
            }
            catch { }
        };
    }

    public static void SetDataContext(
        this Window window,
        object viewModel,
        Action? closedAction = null
    )
    {
        window.DataContext = viewModel;
        window.Closed += (s, e) =>
        {
            try
            {
                closedAction?.Invoke();
                window.DataContext = null;
            }
            catch { }
        };
    }

    private static Dictionary<Window, WindowCloseState> _windowCloseStates = new();

    /// <summary>
    /// 设置关闭状态
    /// </summary>
    /// <param name="window"></param>
    /// <param name="state">关闭状态</param>
    public static void SetCloseState(this Window window, WindowCloseState state)
    {
        window.Closing -= WindowCloseState_Closing;
        window.Closing += WindowCloseState_Closing;
        _windowCloseStates[window] = state;
    }

    /// <summary>
    /// 强制关闭
    /// </summary>
    /// <param name="window"></param>
    public static void CloseX(this Window? window)
    {
        if (window is null)
            return;
        _windowCloseStates.Remove(window);
        window.Closing -= WindowCloseState_Closing;
        window.Close();
    }

    /// <summary>
    /// 显示或者聚焦
    /// </summary>
    /// <param name="window"></param>
    public static void ShowOrActivate(this Window? window)
    {
        if (window is null)
            return;
        if (window.IsVisible is false)
            window.Show();
        window.Activate();
    }

    private static void WindowCloseState_Closing(object sender, CancelEventArgs e)
    {
        if (sender is not Window window)
            return;
        if (_windowCloseStates.TryGetValue(window, out var state) is false)
            return;
        if (state is WindowCloseState.Close)
            return;
        e.Cancel = true;
        window.Visibility =
            state is WindowCloseState.Hidden ? Visibility.Hidden : Visibility.Collapsed;
        return;
    }
}

public enum WindowCloseState
{
    Close,
    Hidden,
    Collapsed
}
