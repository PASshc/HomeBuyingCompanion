using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace HomeBuyingApp.UI.Views
{
    public partial class PropertyDetailView : UserControl
    {
        public PropertyDetailView()
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
    }
}
