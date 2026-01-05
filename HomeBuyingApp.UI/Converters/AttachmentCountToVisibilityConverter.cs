using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace HomeBuyingApp.UI.Converters
{
    /// <summary>
    /// Converts an attachment count to visibility. Shows when count > 0.
    /// </summary>
    public class AttachmentCountToVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is int count)
            {
                return count > 0 ? Visibility.Visible : Visibility.Collapsed;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
