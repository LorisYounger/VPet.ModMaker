using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace VPet.ModMaker.Converters;

/// <summary>
/// 任意为假时设置显示转换器
/// </summary>
public class AnyFalseToVisibleConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length == 0)
            throw new ArgumentException("No values", nameof(values));
        return values.Any(i => i is not true) ? Visibility.Visible : Visibility.Collapsed;
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
