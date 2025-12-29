using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using HomeBuyingApp.Core.Models;

namespace HomeBuyingApp.UI.Converters
{
    /// <summary>
    /// Converts PropertyStatus to a background color for the status cell.
    /// Uses lighter colors suitable for text backgrounds.
    /// </summary>
    public class StatusToBackgroundConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PropertyStatus status)
            {
                return status switch
                {
                    PropertyStatus.Interested => new SolidColorBrush(Color.FromRgb(200, 230, 201)),      // Light Green
                    PropertyStatus.PendingVisit => new SolidColorBrush(Color.FromRgb(187, 222, 251)),    // Light Blue
                    PropertyStatus.OfferMade => new SolidColorBrush(Color.FromRgb(255, 224, 178)),       // Light Orange
                    PropertyStatus.OfferRejected => new SolidColorBrush(Color.FromRgb(255, 205, 210)),   // Light Red
                    PropertyStatus.Closed => new SolidColorBrush(Color.FromRgb(225, 190, 231)),          // Light Purple
                    PropertyStatus.PropertyInspection => new SolidColorBrush(Color.FromRgb(178, 235, 242)), // Light Cyan
                    PropertyStatus.NotInterested => new SolidColorBrush(Color.FromRgb(224, 224, 224)),   // Light Gray
                    PropertyStatus.Researching => new SolidColorBrush(Color.FromRgb(255, 249, 196)),     // Light Yellow
                    _ => new SolidColorBrush(Colors.Transparent)
                };
            }

            // Handle string status from ViewModel
            if (value is string statusStr)
            {
                return statusStr switch
                {
                    "Interested" => new SolidColorBrush(Color.FromRgb(200, 230, 201)),
                    "Pending Visit" => new SolidColorBrush(Color.FromRgb(187, 222, 251)),
                    "Offer Made" => new SolidColorBrush(Color.FromRgb(255, 224, 178)),
                    "Offer Rejected" => new SolidColorBrush(Color.FromRgb(255, 205, 210)),
                    "Closed" => new SolidColorBrush(Color.FromRgb(225, 190, 231)),
                    "Property Inspection" => new SolidColorBrush(Color.FromRgb(178, 235, 242)),
                    "Not Interested" => new SolidColorBrush(Color.FromRgb(224, 224, 224)),
                    "Researching" => new SolidColorBrush(Color.FromRgb(255, 249, 196)),
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
