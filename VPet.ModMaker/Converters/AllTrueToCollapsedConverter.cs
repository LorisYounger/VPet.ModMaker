using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace VPet.ModMaker.Converters;

/// <summary>
/// 全部为真时设置隐藏转换器
/// </summary>
public class AllTrueToCollapsedConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length == 0)
            throw new ArgumentException("No values", nameof(values));
        return values.All(i => i is true) ? Visibility.Collapsed : Visibility.Visible;
    }

    public object[] ConvertBack(
        object value,
        Type[] targetTypes,
        object parameter,
        CultureInfo culture
    )
    {
        throw new NotImplementedException();
    }
}
