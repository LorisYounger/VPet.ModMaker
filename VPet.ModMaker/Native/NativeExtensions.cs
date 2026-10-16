using System.IO;
using System.Windows.Media.Imaging;
using VPet_Simulator.Core;
using VPet.ModMaker.Models;

namespace VPet.ModMaker;

/// <summary>
/// 拓展
/// </summary>
public static class NativeExtensions
{
    /// <summary>
    /// 是可以命名的动作类型, 例如 Work_Read, Work_Clean
    /// </summary>
    /// <param name="graphType">动作类型</param>
    /// <returns>是否支持</returns>
    public static bool IsNamedGraph(this GraphInfo.GraphType graphType)
    {
        return graphType
            is GraphInfo.GraphType.Common
                or GraphInfo.GraphType.Work
                or GraphInfo.GraphType.Idel
                or GraphInfo.GraphType.Move
                or GraphInfo.GraphType.Say;
    }

    /// <summary>
    /// 是只有一个状态的动作, 即不支持 <see cref="ModeType"/> 多状态
    /// </summary>
    /// <param name="graphType">动作类型</param>
    /// <returns>是否是</returns>
    public static bool IsSingleTypeGraph(this GraphInfo.GraphType graphType)
    {
        return graphType
            is GraphInfo.GraphType.Default
                or GraphInfo.GraphType.Shutdown
                or GraphInfo.GraphType.StartUP
                or GraphInfo.GraphType.Switch_Up
                or GraphInfo.GraphType.Switch_Down
                or GraphInfo.GraphType.Switch_Thirsty
                or GraphInfo.GraphType.Switch_Hunger
                or GraphInfo.GraphType.Raised_Dynamic;
    }

    /// <summary>
    /// 是支持 <see cref="ModeType"/> 状态的动作
    /// </summary>
    /// <param name="graphType">动作类型</param>
    /// <returns>是否是</returns>
    public static bool IsMultiTypeGraph(this GraphInfo.GraphType graphType)
    {
        return graphType
            is GraphInfo.GraphType.Default
                or GraphInfo.GraphType.Touch_Head
                or GraphInfo.GraphType.Touch_Body
                or GraphInfo.GraphType.Sleep
                or GraphInfo.GraphType.Raised_Static
                or GraphInfo.GraphType.StateONE
                or GraphInfo.GraphType.StateTWO
                or GraphInfo.GraphType.Common
                or GraphInfo.GraphType.Work
                or GraphInfo.GraphType.Idel
                or GraphInfo.GraphType.Move
                or GraphInfo.GraphType.Say
                or GraphInfo.GraphType.SideHide_Left_Main
                or GraphInfo.GraphType.SideHide_Left_Rise
                or GraphInfo.GraphType.SideHide_Right_Main
                or GraphInfo.GraphType.SideHide_Right_Rise;
    }

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
}
