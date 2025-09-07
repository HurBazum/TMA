using System.Globalization;
using System.Windows.Data;

namespace TMA.UI.Infrastructure.Converters;

public class PriorityValueConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return value switch
        {
            0 => "Normal",
            1 => "Medium",
            2 => "High",
            _ => throw new ArgumentNullException(nameof(value))
        };
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => value switch 
    {
        "Normal" => 0,
        "Medium" => 1,
        "High" => 2,
        _ => throw new ArgumentNullException($"{nameof(value)}")
    };
}