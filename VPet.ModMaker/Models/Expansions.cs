using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace VPet.ModMaker.Models;

public static class Extensions
{
    public static bool Contains(this string source, string value, StringComparison comparisonType)
    {
        return source.IndexOf(value, comparisonType) >= 0;
    }

    public static string GetSourceFile(this BitmapImage image)
    {
        return ((FileStream)image.StreamSource).Name;
    }

    public static void CloseString(this ImageSource source)
    {
        if (source is BitmapImage image)
        {
            image.StreamSource?.Close();
        }
    }

    public static bool TryAdd<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary,
        TKey key,
        TValue value
    )
    {
        if (dictionary.ContainsKey(key))
            return false;
        dictionary.Add(key, value);
        return true;
    }
}
