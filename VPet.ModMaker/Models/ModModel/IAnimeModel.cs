using VPet_Simulator.Core;

namespace VPet.ModMaker.Models.ModModel;

/// <summary>
/// 动画模型接口
/// </summary>
public interface IAnimeModel : IDisposable
{
    /// <summary>
    /// ID
    /// </summary>
    string ID { get; set; }

    /// <summary>
    /// 动画名称
    /// </summary>
    string Name { get; set; }

    /// <summary>
    /// 动画类型
    /// </summary>
    GraphInfo.GraphType GraphType { get; }

    /// <summary>
    /// 清除
    /// </summary>
    void Clear();

    /// <summary>
    /// 关闭
    /// </summary>
    void Close();

    /// <summary>
    /// 保存
    /// </summary>
    /// <param name="path"></param>
    void Save(string path);
}
