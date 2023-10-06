using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace VPet.ModMaker.Models;

/// <summary>
/// 可观察的枚举标签模型
/// </summary>
/// <typeparam name="T"></typeparam>
public class ObservableEnumFlags<T>
    where T : Enum
{
    /// <summary>
    /// 枚举值
    /// </summary>
    public ObservableValue<T> EnumValue { get; } = new();

    /// <summary>
    /// 添加枚举命令
    /// </summary>
    public ObservableCommand<T> AddCommand { get; } = new();

    /// <summary>
    /// 删除枚举命令
    /// </summary>
    public ObservableCommand<T> RemoveCommand { get; } = new();

    /// <summary>
    /// 枚举类型
    /// </summary>
    public Type EnumType = typeof(T);

    /// <summary>
    /// 枚举基类
    /// </summary>
    public Type UnderlyingType { get; } = Enum.GetUnderlyingType(typeof(T));

    public ObservableEnumFlags()
    {
        if (Attribute.IsDefined(EnumType, typeof(FlagsAttribute)) is false)
            throw new Exception("此枚举类型未使用特性 [Flags]");
        AddCommand.ExecuteEvent += AddCommand_ExecuteEvent;
        RemoveCommand.ExecuteEvent += RemoveCommand_ExecuteEvent;
    }

    public ObservableEnumFlags(T value)
        : this()
    {
        EnumValue.Value = value;
    }

    private void AddCommand_ExecuteEvent(T value)
    {
        if (UnderlyingType == typeof(int))
        {
            EnumValue.Value = (T)
                Enum.Parse(
                    EnumType,
                    (Convert.ToInt32(EnumValue.Value) | Convert.ToInt32(value)).ToString()
                );
        }
    }

    private void RemoveCommand_ExecuteEvent(T value)
    {
        if (UnderlyingType == typeof(int))
        {
            EnumValue.Value = (T)
                Enum.Parse(
                    EnumType,
                    (Convert.ToInt32(EnumValue.Value) & ~Convert.ToInt32(value)).ToString()
                );
        }
    }
}
