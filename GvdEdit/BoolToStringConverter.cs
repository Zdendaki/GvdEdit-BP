using System;
using System.Globalization;
using System.Windows.Data;

namespace GvdEdit
{
    public class BoolToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool booleanValue)
            {
                return booleanValue ? "ano" : "ne";
            }
            return string.Empty;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                return stringValue.Equals("ano", StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }
    }
}
