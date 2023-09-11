using HKW.HKWViewModels.SimpleObservable;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPet.ModMaker.Models;

public class EnumFlagsVM<T>
    where T : Enum
{
    public ObservableValue<T> EnumValue { get; } = new();

    public ObservableCommand<T> AddCommand { get; } = new();
    public ObservableCommand<T> RemoveCommand { get; } = new();

    public Type EnumType = typeof(T);
    public Type UnderlyingType { get; } = Enum.GetUnderlyingType(typeof(T));

    public EnumFlagsVM()
    {
        AddCommand.ExecuteEvent += AddCommand_ExecuteEvent;
        RemoveCommand.ExecuteEvent += RemoveCommand_ExecuteEvent;
    }

    public EnumFlagsVM(T value)
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
