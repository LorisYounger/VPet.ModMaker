using LinePutScript.Converter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace VPet.ModMaker.Models;

public class ModMakerHistory
{
    public BitmapImage Image { get; set; }

    [Line(ignoreCase: true)]
    public string Name { get; set; }

    private string _path;

    [Line(ignoreCase: true)]
    public string SourcePath
    {
        get => _path;
        set
        {
            _path = value;
            var imagePath = Path.Combine(_path, "icon.png");
            if (File.Exists(imagePath))
                Image = Utils.LoadImageToMemoryStream(imagePath);
        }
    }

    public string LastTimeString => LastTime.ToString("yyyy/MM/dd HH:mm:ss");

    [Line(ignoreCase: true)]
    public DateTime LastTime { get; set; }
}
