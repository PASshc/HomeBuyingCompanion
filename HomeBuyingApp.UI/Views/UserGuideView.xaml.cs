using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace HomeBuyingApp.UI.Views
{
    public partial class UserGuideView : UserControl
    {
        public UserGuideView()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, RequestNavigateEventArgs e)
        {
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = e.Uri.AbsoluteUri,
                    UseShellExecute = true
                });
                e.Handled = true;
            }
            catch
            {
                MessageBox.Show($"Could not open link: {e.Uri.AbsoluteUri}", 
                    "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }
}
