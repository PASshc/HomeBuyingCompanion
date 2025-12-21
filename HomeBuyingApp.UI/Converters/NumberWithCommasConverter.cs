using System;
using System.Globalization;
using System.Windows.Data;

namespace HomeBuyingApp.UI.Converters
{
    public class NumberWithCommasConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int intValue)
            {
                return intValue.ToString("N0", culture);
            }

            if (value is long longValue)
            {
                return longValue.ToString("N0", culture);
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                var clean = stringValue.Replace(",", "").Trim();
                if (int.TryParse(clean, NumberStyles.Integer, culture, out var intResult))
                {
                    return intResult;
                }

                if (long.TryParse(clean, NumberStyles.Integer, culture, out var longResult))
                {
                    return (int)longResult;
                }
            }

            return 0;
        }
    }
}
