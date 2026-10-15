using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.Models;

public sealed partial class FontModel : ObservableModelBase
{
    public FontModel(IFont font)
    {
        ID = font.Name;
        Font = font.Font;
        Path = font.Path;
    }

    [ObservableProperty]
    public string ID { get; set; }

    public FontFamily Font { get; }

    public string Path { get; }
}
