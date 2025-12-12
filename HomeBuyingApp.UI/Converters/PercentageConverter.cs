using System;
using System.Globalization;
using System.Windows.Data;

namespace HomeBuyingApp.UI.Converters
{
    public class PercentageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
            {
                // Convert 0.065 to 6.5
                return (doubleValue * 100).ToString("0.###");
            }
            if (value is decimal decimalValue)
            {
                return (decimalValue * 100).ToString("0.###");
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                string cleanString = stringValue.Replace("%", "").Trim();
                if (decimal.TryParse(cleanString, out decimal result))
                {
                    // Convert 6.5 back to 0.065
                    return result / 100;
                }
            }
            return 0m;
        }
    }
}
