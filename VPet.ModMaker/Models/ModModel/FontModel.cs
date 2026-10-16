using System.IO;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using VPet_Simulator.Windows.Interface;

namespace VPet.ModMaker.Models;

public sealed partial class FontModel : ObservableModelBase
{
    public FontModel(string file)
    {
        Name = Path.GetFileNameWithoutExtension(file);
        var familyName = Path.GetDirectoryName(file) + @"\#" + Name;
        Font = new FontFamily(@"file:///" + familyName);
        FullName = file;
        SupportedCultures = string.Join(", ", Font.FamilyNames.Keys);
    }

    [ObservableProperty]
    public string Name { get; set; }

    public FontFamily Font { get; }

    public string FullName { get; }

    public string SupportedCultures { get; }

    public static string SampleText { get; } =
        "abcdefghijklmnopqrstuvwxyz ABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890.:,; ' \" (!?) +-*/=";
}
