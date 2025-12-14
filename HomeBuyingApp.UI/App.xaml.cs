using System;
using System.IO;
using System.Windows;
using HomeBuyingApp.Core.Services;
using HomeBuyingApp.Infrastructure.Data;
using HomeBuyingApp.Infrastructure.Services;
using HomeBuyingApp.UI.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace HomeBuyingApp.UI
{
    public partial class App : Application
    {
        private IServiceProvider _serviceProvider;

        public App()
        {
            var services = new ServiceCollection();
            ConfigureServices(services);
            _serviceProvider = services.BuildServiceProvider();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // Set up data directory in AppData to persist data across updates
            string appFolderName = "HomeBuyingApp";
            
#if DEBUG
            // Isolate Dev data from Prod data
            appFolderName = "HomeBuyingApp_Dev";
#endif

            string appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appFolderName);
            if (!Directory.Exists(appDataPath))
            {
                Directory.CreateDirectory(appDataPath);
            }
            
            string dbPath = Path.Combine(appDataPath, "homebuying.db");

            // Migration: If DB exists in the old location (app folder) and not in new, move/copy it
            string oldDbPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "homebuying.db");
            if (File.Exists(oldDbPath) && !File.Exists(dbPath))
            {
                try 
                {
                    File.Copy(oldDbPath, dbPath);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to migrate database: {ex.Message}", "Migration Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }

            // Database
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            // Services
            services.AddSingleton<IMortgageCalculatorService, MortgageCalculatorService>();
            services.AddScoped<IPropertyService, PropertyService>();
            services.AddScoped<ICsvService, CsvService>();
            services.AddScoped<IWebScraperService, WebScraperService>();
            services.AddSingleton<IBackupService>(provider => new BackupService(dbPath, Path.Combine(appDataPath, "Attachments")));

            // ViewModels
            services.AddTransient<MortgageCalculatorViewModel>();
            services.AddTransient<MortgageComparisonViewModel>();
            services.AddTransient<PropertyListViewModel>();
            services.AddTransient<MainViewModel>();

            // Views
            services.AddTransient<MainWindow>();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Global exception handling
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;

            // Ensure database is created
            using (var scope = _serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                context.Database.EnsureCreated();

                // Schema update for v1.5.0 features
                try
                {
                    var columns = new[]
                    {
                        "CalcDownPaymentAmount TEXT",
                        "CalcInterestRate TEXT",
                        "CalcLoanTermYears INTEGER",
                        "CalcPropertyTaxAnnualAmount TEXT",
                        "CalcHomeownerInsuranceAnnualAmount TEXT",
                        "CalcPmiRate TEXT",
                        "CalcHoaMonthlyAmount TEXT",
                        "ViewingDate TEXT",
                        "InspectionDate TEXT",
                        "ClosingDate TEXT",
                        "HasAC INTEGER",
                        "HasLandscape INTEGER",
                        "HasPool INTEGER",
                        "HasJacuzzi INTEGER",
                        "HasLanai INTEGER",
                        "HasCarpet INTEGER",
                        "HasTile INTEGER",
                        "HasWoodFlooring INTEGER",
                        "HasOven INTEGER DEFAULT 0",
                        "HasRefrigerator INTEGER DEFAULT 0",
                        "HasWasherDryer INTEGER DEFAULT 0",
                        "OtherFeatures TEXT DEFAULT ''"
                    };

                    foreach (var col in columns)
                    {
                        try
                        {
                            context.Database.ExecuteSqlRaw($"ALTER TABLE Properties ADD COLUMN {col};");
                        }
                        catch { /* Ignore if exists */ }
                    }

                    // Ensure boolean columns are not NULL (SQLite adds new columns as NULL by default)
                    // This fixes "The data is NULL at ordinal..." error when reading into non-nullable bool properties
                    var boolColumns = new[] {
                        "HasAC", "HasLandscape", "HasPool", "HasJacuzzi",
                        "HasLanai", "HasCarpet", "HasTile", "HasWoodFlooring",
                        "HasOven", "HasRefrigerator", "HasWasherDryer"
                    };

                    foreach (var col in boolColumns)
                    {
                        try
                        {
                            context.Database.ExecuteSqlRaw($"UPDATE Properties SET {col} = 0 WHERE {col} IS NULL;");
                        }
                        catch { /* Ignore errors */ }
                    }

                    // Explicitly fix the new columns for v3.1.0/v3.1.1 to ensure no NULLs exist
                    // This is a double-check in case the loop above failed or was skipped
                    try
                    {
                        context.Database.ExecuteSqlRaw("UPDATE Properties SET HasOven = 0 WHERE HasOven IS NULL;");
                        context.Database.ExecuteSqlRaw("UPDATE Properties SET HasRefrigerator = 0 WHERE HasRefrigerator IS NULL;");
                        context.Database.ExecuteSqlRaw("UPDATE Properties SET HasWasherDryer = 0 WHERE HasWasherDryer IS NULL;");
                        context.Database.ExecuteSqlRaw("UPDATE Properties SET OtherFeatures = '' WHERE OtherFeatures IS NULL;");
                    }
                    catch { /* Ignore */ }

                    // Create Attachments table if not exists (for v1.5.0 update)
                    context.Database.ExecuteSqlRaw(@"
                        CREATE TABLE IF NOT EXISTS Attachments (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            PropertyId INTEGER NOT NULL,
                            FileName TEXT NOT NULL,
                            FilePath TEXT NOT NULL,
                            Description TEXT,
                            DateAdded TEXT NOT NULL,
                            CONSTRAINT FK_Attachments_Properties_PropertyId FOREIGN KEY (PropertyId) REFERENCES Properties (Id) ON DELETE CASCADE
                        );
                        CREATE INDEX IF NOT EXISTS IX_Attachments_PropertyId ON Attachments (PropertyId);
                    ");
                }
                catch { }
            }

            var mainWindow = _serviceProvider.GetRequiredService<MainWindow>();
            mainWindow.DataContext = _serviceProvider.GetRequiredService<MainViewModel>();
            mainWindow.Show();
        }

        private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show($"An unhandled exception occurred: {e.Exception.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            e.Handled = true;
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                MessageBox.Show($"A fatal error occurred: {ex.Message}", "Fatal Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

