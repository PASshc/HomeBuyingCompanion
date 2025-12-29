using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HomeBuyingApp.UI.Services;
using HomeBuyingApp.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace HomeBuyingApp.UI;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly IUpdateService _updateService;
    private readonly IBackupService? _backupService;
    private UpdateInfo? _latestUpdateInfo;

    public MainWindow(IUpdateService updateService, IBackupService? backupService = null)
    {
        InitializeComponent();
        _updateService = updateService;
        _backupService = backupService;
        
        var version = _updateService.GetCurrentVersion();
        Title = $"Home Buying App v{version}";
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        // Auto-check for updates on launch
        await CheckForUpdatesAsync(showMessageIfNone: false);
    }

    private async Task CheckForUpdatesAsync(bool showMessageIfNone = true)
    {
        try
        {
            _latestUpdateInfo = await _updateService.CheckForUpdatesAsync();

            if (_latestUpdateInfo.UpdateAvailable)
            {
                UpdateMessage.Text = $"Version {_latestUpdateInfo.LatestVersion} is available! (You have {_latestUpdateInfo.CurrentVersion})";
                DownloadLink.NavigateUri = new Uri(_latestUpdateInfo.DownloadUrl);
                UpdateBanner.Visibility = Visibility.Visible;
            }
            else if (showMessageIfNone)
            {
                MessageBox.Show($"You are running the latest version ({_latestUpdateInfo.CurrentVersion})!", 
                    "Up to Date", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch
        {
            if (showMessageIfNone)
            {
                MessageBox.Show("Unable to check for updates. Please check your internet connection.", 
                    "Update Check Failed", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }
    }

    private void DismissUpdate_Click(object sender, RoutedEventArgs e)
    {
        UpdateBanner.Visibility = Visibility.Collapsed;
    }

    private async void CheckForUpdates_Click(object sender, RoutedEventArgs e)
    {
        await CheckForUpdatesAsync(showMessageIfNone: true);
    }

    private void UserGuide_Click(object sender, RoutedEventArgs e)
    {
        UserGuideTab.IsSelected = true;
    }

    private void About_Click(object sender, RoutedEventArgs e)
    {
        var version = _updateService.GetCurrentVersion();
        var aboutMessage = $@"Home Buying Companion

Version: {version}
Framework: .NET 8.0
Database: SQLite

A privacy-first desktop application for tracking your home buying journey.

© 2024-2025
GitHub: github.com/PASshc/HomeBuyingCompanion
License: MIT";

        MessageBox.Show(aboutMessage, "About Home Buying Companion", 
            MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void BackupDatabase_Click(object sender, RoutedEventArgs e)
    {
        if (_backupService == null)
        {
            MessageBox.Show("Backup service not available.", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "Backup files (*.zip)|*.zip",
            FileName = $"HomeBuyingApp_Backup_{DateTime.Now:yyyyMMdd_HHmmss}.zip"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                _backupService.CreateBackupAsync(dialog.FileName).Wait();
                MessageBox.Show("Backup created successfully!", "Success", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Backup failed: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void RestoreDatabase_Click(object sender, RoutedEventArgs e)
    {
        if (_backupService == null)
        {
            MessageBox.Show("Backup service not available.", "Error", 
                MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }

        var result = MessageBox.Show(
            "Restoring a backup will replace your current database. This action cannot be undone.\n\nDo you want to continue?",
            "Confirm Restore", MessageBoxButton.YesNo, MessageBoxImage.Warning);

        if (result == MessageBoxResult.Yes)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Backup files (*.zip)|*.zip"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    _backupService.RestoreBackupAsync(dialog.FileName).Wait();
                    MessageBox.Show("Restore completed successfully! Please restart the application.", 
                        "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    Application.Current.Shutdown();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Restore failed: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }

    private void Exit_Click(object sender, RoutedEventArgs e)
    {
        Application.Current.Shutdown();
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