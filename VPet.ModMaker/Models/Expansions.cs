using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VPet.ModMaker.Models;

public static class Extensions
{
    public static bool Contains(this string source, string value, StringComparison comparisonType)
    {
        return source.IndexOf(value, comparisonType) >= 0;
    }
}
