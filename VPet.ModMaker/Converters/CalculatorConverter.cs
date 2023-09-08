using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace VPet.ModMaker.Converters;

/// <summary>
/// 计算器转换器
/// <para>示例:
/// <code><![CDATA[
/// <MultiBinding Converter="{StaticResource CalculatorConverter}">
///   <Binding Path="Num1" />
///   <Binding Source="+" />
///   <Binding Path="Num2" />
///   <Binding Source="-" />
///   <Binding Path="Num3" />
///   <Binding Source="*" />
///   <Binding Path="Num4" />
///   <Binding Source="/" />
///   <Binding Path="Num5" />
/// </MultiBinding>
/// ]]></code></para>
/// </summary>
/// <exception cref="Exception">绑定的数量不正确</exception>
public class CalculatorConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (System.Convert.ToBoolean(values.Length & 1) is false)
            throw new Exception("Parameter error: Incorrect quantity");
        if (values.Length == 1)
            return values[0];
        bool isNumber = false;
        double result = (double)values[0];
        char currentOperator = '0';
        for (int i = 1; i < values.Length - 1; i++)
        {
            if (isNumber is false)
            {
                currentOperator = ((string)values[i])[0];
                isNumber = true;
            }
            else
            {
                var value = System.Convert.ToDouble(values[i]);
                result = Operation(result, currentOperator, value);
                isNumber = false;
            }
        }
        return Operation(result, currentOperator, System.Convert.ToDouble(values.Last()));
    }

    public static double Operation(double value1, char operatorChar, double value2)
    {
        return operatorChar switch
        {
            '+' => value1 + value2,
            '-' => value1 - value2,
            '*' => value1 * value2,
            '/' => value1 / value2,
            _ => throw new NotImplementedException(),
        };
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
