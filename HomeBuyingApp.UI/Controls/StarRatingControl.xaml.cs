using System.Windows;
using System.Windows.Controls;

namespace HomeBuyingApp.UI.Controls
{
    public partial class StarRatingControl : UserControl
    {
        public static readonly DependencyProperty RatingProperty =
            DependencyProperty.Register(
                nameof(Rating),
                typeof(decimal),
                typeof(StarRatingControl),
                new FrameworkPropertyMetadata(0m, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnRatingChanged));

        public decimal Rating
        {
            get => (decimal)GetValue(RatingProperty);
            set => SetValue(RatingProperty, value);
        }

        public StarRatingControl()
        {
            InitializeComponent();
            UpdateStarDisplay();
        }

        private static void OnRatingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is StarRatingControl control)
            {
                control.UpdateStarDisplay();
            }
        }

        private void Star_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is string tagStr && int.TryParse(tagStr, out int starValue))
            {
                // If clicking the same star that's currently the rating, clear it (set to 0)
                if ((int)Rating == starValue)
                {
                    Rating = 0;
                }
                else
                {
                    Rating = starValue;
                }
            }
        }

        private void UpdateStarDisplay()
        {
            int rating = (int)Rating;
            
            Star1.Content = rating >= 1 ? "★" : "☆";
            Star2.Content = rating >= 2 ? "★" : "☆";
            Star3.Content = rating >= 3 ? "★" : "☆";
            Star4.Content = rating >= 4 ? "★" : "☆";
            Star5.Content = rating >= 5 ? "★" : "☆";
        }
    }
}
