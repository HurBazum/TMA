using System.Globalization;
using System.Windows.Data;

namespace TMA.UI.Infrastructure.Converters;
enum Months
{
    Январь,
    Февраль,
    Март,
    Апрель,
    Май,
    Июнь,
    Июль,
    Август,
    Сентябрь,
    Октябрь,
    Ноябрь,
    Декабрь
}
public class MonthConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => Enum.GetName(typeof(Months), (int)value - 1);

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => (Months)value + 1;
}