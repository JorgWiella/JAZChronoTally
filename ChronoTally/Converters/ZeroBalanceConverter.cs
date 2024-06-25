using System;
using System.Globalization;
using System.Windows.Data;

namespace ChronoTally.Converters
{
    public class ZeroBalanceConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
            {
                return Math.Abs(doubleValue - 40.0) < 0.01;
            }
            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
