using LinePutScript.Converter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPet.ModMaker.Models;

public class ModMakeHistory
{
    [Line(ignoreCase: true)]
    public string Name { get; set; }

    [Line(ignoreCase: true)]
    public string Path { get; set; }

    [Line(ignoreCase: true)]
    public DateTime LastOpenTime { get; set; }
}
