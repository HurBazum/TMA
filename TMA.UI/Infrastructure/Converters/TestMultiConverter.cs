using System.Globalization;
using System.Windows.Data;

namespace TMA.UI.Infrastructure.Converters;

public class TestMultiConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        return $"{values[0]}_{values[1]}";
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}