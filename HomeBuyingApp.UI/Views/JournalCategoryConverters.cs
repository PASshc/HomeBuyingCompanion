using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using HomeBuyingApp.Core.Models;

namespace HomeBuyingApp.UI.Views
{
    /// <summary>
    /// Converts JournalCategory to an emoji icon for display.
    /// </summary>
    public class JournalCategoryToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is JournalCategory category)
            {
                return category switch
                {
                    JournalCategory.Progress => "📈",
                    JournalCategory.LessonLearned => "💡",
                    JournalCategory.MortgageInfo => "🏦",
                    JournalCategory.Decision => "⚖️",
                    JournalCategory.Research => "🔍",
                    JournalCategory.General => "📝",
                    _ => "📝"
                };
            }
            return "📝";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    /// Converts JournalCategory to a background color brush.
    /// </summary>
    public class JournalCategoryToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is JournalCategory category)
            {
                string colorCode = category switch
                {
                    JournalCategory.Progress => "#E3F2FD",      // Light Blue
                    JournalCategory.LessonLearned => "#FFF3E0", // Light Orange
                    JournalCategory.MortgageInfo => "#E8F5E9",  // Light Green
                    JournalCategory.Decision => "#F3E5F5",      // Light Purple
                    JournalCategory.Research => "#E0F7FA",      // Light Cyan
                    JournalCategory.General => "#FAFAFA",       // Light Grey
                    _ => "#FAFAFA"
                };
                return new SolidColorBrush((Color)ColorConverter.ConvertFromString(colorCode));
            }
            return new SolidColorBrush(Colors.LightGray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
