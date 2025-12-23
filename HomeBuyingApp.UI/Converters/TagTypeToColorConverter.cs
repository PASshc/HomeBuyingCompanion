using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using HomeBuyingApp.Core.Models;

namespace HomeBuyingApp.UI.Converters
{
    public class TagTypeToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is TagType tagType)
            {
                return tagType switch
                {
                    TagType.Pro => new SolidColorBrush(Color.FromRgb(34, 139, 34)),      // Forest Green
                    TagType.Con => new SolidColorBrush(Color.FromRgb(178, 34, 34)),       // Firebrick Red
                    TagType.Neutral => new SolidColorBrush(Color.FromRgb(70, 130, 180)),  // Steel Blue
                    _ => new SolidColorBrush(Colors.Gray)
                };
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
