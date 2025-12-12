using System;
using System.Globalization;
using System.Windows.Data;

namespace HomeBuyingApp.UI.Converters
{
    public class CurrencyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal decimalValue)
            {
                return decimalValue.ToString("C0", culture); // C0 for no decimal places if preferred, or C2
            }
            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                // Remove currency symbol and commas
                string cleanString = stringValue
                    .Replace(culture.NumberFormat.CurrencySymbol, "")
                    .Replace(",", "")
                    .Trim();

                if (decimal.TryParse(cleanString, out decimal result))
                {
                    return result;
                }
            }
            return 0m;
        }
    }
}
