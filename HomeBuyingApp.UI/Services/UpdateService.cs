using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace HomeBuyingApp.UI.Services
{
    public class UpdateInfo
    {
        public string LatestVersion { get; set; } = string.Empty;
        public string CurrentVersion { get; set; } = string.Empty;
        public bool UpdateAvailable { get; set; }
        public string DownloadUrl { get; set; } = string.Empty;
        public string ReleaseNotes { get; set; } = string.Empty;
    }

    public interface IUpdateService
    {
        Task<UpdateInfo> CheckForUpdatesAsync();
        string GetCurrentVersion();
    }

    public class UpdateService : IUpdateService
    {
        private const string GitHubApiUrl = "https://api.github.com/repos/PASshc/HomeBuyingCompanion/releases/latest";
        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        static UpdateService()
        {
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "HomeBuyingApp");
        }

        public string GetCurrentVersion()
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            return version?.ToString(3) ?? "0.0.0";
        }

        public async Task<UpdateInfo> CheckForUpdatesAsync()
        {
            var updateInfo = new UpdateInfo
            {
                CurrentVersion = GetCurrentVersion()
            };

            try
            {
                var response = await _httpClient.GetAsync(GitHubApiUrl);
                
                if (!response.IsSuccessStatusCode)
                {
                    return updateInfo;
                }

                var json = await response.Content.ReadAsStringAsync();
                using var document = JsonDocument.Parse(json);
                var root = document.RootElement;

                // Get latest version (tag_name like "v5.2.0")
                var tagName = root.GetProperty("tag_name").GetString() ?? "";
                updateInfo.LatestVersion = tagName.TrimStart('v');

                // Get download URL
                if (root.TryGetProperty("html_url", out var htmlUrl))
                {
                    updateInfo.DownloadUrl = htmlUrl.GetString() ?? "";
                }

                // Get release notes
                if (root.TryGetProperty("body", out var body))
                {
                    updateInfo.ReleaseNotes = body.GetString() ?? "";
                }

                // Compare versions
                updateInfo.UpdateAvailable = IsNewerVersion(updateInfo.LatestVersion, updateInfo.CurrentVersion);
            }
            catch
            {
                // Silently fail - don't interrupt user experience
                // Update check failure is not critical
            }

            return updateInfo;
        }

        private bool IsNewerVersion(string latestVersion, string currentVersion)
        {
            try
            {
                var latest = Version.Parse(latestVersion);
                var current = Version.Parse(currentVersion);
                return latest > current;
            }
            catch
            {
                return false;
            }
        }
    }
}
