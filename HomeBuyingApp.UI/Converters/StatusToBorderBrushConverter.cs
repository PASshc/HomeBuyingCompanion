using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using HomeBuyingApp.Core.Models;

namespace HomeBuyingApp.UI.Converters
{
    public class StatusToBorderBrushConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PropertyStatus status)
            {
                return status switch
                {
                    PropertyStatus.Interested => new SolidColorBrush(Color.FromRgb(76, 175, 80)),      // Green
                    PropertyStatus.PendingVisit => new SolidColorBrush(Color.FromRgb(33, 150, 243)),   // Blue
                    PropertyStatus.OfferMade => new SolidColorBrush(Color.FromRgb(255, 152, 0)),       // Orange
                    PropertyStatus.OfferRejected => new SolidColorBrush(Color.FromRgb(244, 67, 54)),   // Red
                    PropertyStatus.Closed => new SolidColorBrush(Color.FromRgb(156, 39, 176)),         // Purple
                    PropertyStatus.PropertyInspection => new SolidColorBrush(Color.FromRgb(0, 188, 212)), // Cyan
                    PropertyStatus.NotInterested => new SolidColorBrush(Color.FromRgb(158, 158, 158)), // Gray
                    PropertyStatus.Researching => new SolidColorBrush(Color.FromRgb(255, 235, 59)),    // Yellow
                    _ => new SolidColorBrush(Colors.Transparent)
                };
            }

            // Handle string status from ViewModel
            if (value is string statusStr)
            {
                return statusStr switch
                {
                    "Interested" => new SolidColorBrush(Color.FromRgb(76, 175, 80)),
                    "Pending Visit" => new SolidColorBrush(Color.FromRgb(33, 150, 243)),
                    "Offer Made" => new SolidColorBrush(Color.FromRgb(255, 152, 0)),
                    "Offer Rejected" => new SolidColorBrush(Color.FromRgb(244, 67, 54)),
                    "Closed" => new SolidColorBrush(Color.FromRgb(156, 39, 176)),
                    "Property Inspection" => new SolidColorBrush(Color.FromRgb(0, 188, 212)),
                    "Not Interested" => new SolidColorBrush(Color.FromRgb(158, 158, 158)),
                    "Researching" => new SolidColorBrush(Color.FromRgb(255, 235, 59)),
                    _ => new SolidColorBrush(Colors.Transparent)
                };
            }

            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
