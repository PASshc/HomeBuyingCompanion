using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using HomeBuyingApp.UI.ViewModels;

namespace HomeBuyingApp.UI.Views
{
    public partial class PropertyListView : UserControl
    {
        public PropertyListView()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                string url = e.Uri.ToString();
                // Handle URLs missing the protocol (e.g. "www.example.com")
                if (!url.StartsWith("http://", System.StringComparison.OrdinalIgnoreCase) && 
                    !url.StartsWith("https://", System.StringComparison.OrdinalIgnoreCase))
                {
                    url = "https://" + url;
                }

                Process.Start(new ProcessStartInfo(url) { UseShellExecute = true });
            }
            catch
            {
                // Handle invalid URLs or other errors silently
            }
            e.Handled = true;
        }

        private void ClearFilters_Click(object sender, RoutedEventArgs e)
        {
            if (DataContext is PropertyListViewModel viewModel)
            {
                viewModel.FilterCity = string.Empty;
                viewModel.FilterState = string.Empty;
            }
        }
    }
}
