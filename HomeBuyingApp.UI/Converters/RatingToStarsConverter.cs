using System;
using System.Globalization;
using System.Windows.Data;

namespace HomeBuyingApp.UI.Converters
{
    /// <summary>
    /// Converts a decimal rating (0-5) to a visual star representation.
    /// Full stars: ★, Empty stars: ☆
    /// </summary>
    public class RatingToStarsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            decimal rating = 0;
            
            if (value is decimal d)
                rating = d;
            else if (value is double dbl)
                rating = (decimal)dbl;
            else if (value is int i)
                rating = i;
            else if (value is float f)
                rating = (decimal)f;

            if (rating <= 0)
                return "☆☆☆☆☆";

            int fullStars = (int)Math.Floor(rating);
            bool hasHalf = (rating - fullStars) >= 0.5m;
            
            string result = new string('★', Math.Min(fullStars, 5));
            
            // For half stars, we'll just round up to show the user's intent
            if (hasHalf && fullStars < 5)
                result += "★";
            
            // Fill remaining with empty stars
            int totalShown = result.Length;
            result += new string('☆', 5 - totalShown);
            
            return result;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
