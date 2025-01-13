using System.IO;
using HanumanInstitute.MvvmDialogs;

namespace VPet.ModMaker;

/// <summary>
/// 工具
/// </summary>
internal static class NativeUtils
{
    /// <summary>
    /// 对话框服务
    /// </summary>
    public static IDialogService DialogService { get; set; } = null!;

    /// <summary>
    /// 粘贴板复制文本
    /// </summary>
    public static Action<string> ClipboardSetText { get; set; } = null!;

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
