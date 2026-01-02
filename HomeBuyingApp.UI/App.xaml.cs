using System;
using System.IO;
using System.Linq;
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

        public IServiceProvider ServiceProvider => _serviceProvider;

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
            services.AddScoped<ITagService, TagService>();
            services.AddScoped<ICsvService, CsvService>();
            services.AddSingleton<IBackupService>(provider => new BackupService(
                dbPath, 
                Path.Combine(appDataPath, "Attachments"),
                Path.Combine(appDataPath, "Images")));

            // Update Service
            services.AddSingleton<HomeBuyingApp.UI.Services.IUpdateService, HomeBuyingApp.UI.Services.UpdateService>();

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
                        "HasDishwasher INTEGER DEFAULT 0",
                        // v4.2.0 - Additional features
                        "HasCentralHeat INTEGER DEFAULT 0",
                        "HasFireplace INTEGER DEFAULT 0",
                        "HasCeilingFans INTEGER DEFAULT 0",
                        "HasWalkInCloset INTEGER DEFAULT 0",
                        "HasAttic INTEGER DEFAULT 0",
                        "HasBasement INTEGER DEFAULT 0",
                        "HasCoveredPatio INTEGER DEFAULT 0",
                        "HasDeckPatio INTEGER DEFAULT 0",
                        "HasFencedYard INTEGER DEFAULT 0",
                        "HasSprinklerSystem INTEGER DEFAULT 0",
                        "HasGarage INTEGER DEFAULT 0",
                        "HasCarport INTEGER DEFAULT 0",
                        "HasStorageShed INTEGER DEFAULT 0",
                        "HasGuestHouse INTEGER DEFAULT 0",
                        "HasSolarPanels INTEGER DEFAULT 0",
                        "HasSecuritySystem INTEGER DEFAULT 0",
                        "HasCustomFeature1 INTEGER DEFAULT 0",
                        "CustomFeature1Name TEXT DEFAULT ''",
                        "HasCustomFeature2 INTEGER DEFAULT 0",
                        "CustomFeature2Name TEXT DEFAULT ''",
                        "OtherFeatures TEXT DEFAULT ''",
                        "PrimaryImagePath TEXT DEFAULT ''",
                        "ImagePath1 TEXT DEFAULT ''",
                        "ImagePath2 TEXT DEFAULT ''",
                        "ImagePath3 TEXT DEFAULT ''",
                        "ImagePath4 TEXT DEFAULT ''",
                        // Note: Pros/Cons columns kept for DB backwards compatibility but UI feature removed in v5.1.0
                        "Pros TEXT DEFAULT ''",
                        "Cons TEXT DEFAULT ''",
                        // v6.0.0 - Quick Notes (JSON array of note chips)
                        "QuickNotes TEXT DEFAULT ''"
                    };

                    foreach (var col in columns)
                    {
                        try
                        {
                            // Suppressing EF1002: column names are hardcoded, not user input
#pragma warning disable EF1002
                            context.Database.ExecuteSqlRaw($"ALTER TABLE Properties ADD COLUMN {col};");
#pragma warning restore EF1002
                        }
                        catch { /* Ignore if exists */ }
                    }

                    // Ensure boolean columns are not NULL (SQLite adds new columns as NULL by default)
                    // This fixes "The data is NULL at ordinal..." error when reading into non-nullable bool properties
                    var boolColumns = new[] {
                        "HasAC", "HasLandscape", "HasPool", "HasJacuzzi",
                        "HasLanai", "HasCarpet", "HasTile", "HasWoodFlooring",
                        "HasOven", "HasRefrigerator", "HasWasherDryer", "HasDishwasher",
                        // v4.2.0 - Additional features
                        "HasCentralHeat", "HasFireplace", "HasCeilingFans", "HasWalkInCloset",
                        "HasAttic", "HasBasement", "HasCoveredPatio", "HasDeckPatio", "HasFencedYard",
                        "HasSprinklerSystem", "HasGarage", "HasCarport", "HasStorageShed",
                        "HasGuestHouse", "HasSolarPanels", "HasSecuritySystem", "HasCustomFeature1", "HasCustomFeature2"
                    };

                    foreach (var col in boolColumns)
                    {
                        try
                        {
                            // Suppressing EF1002: column names are hardcoded, not user input
#pragma warning disable EF1002
                            context.Database.ExecuteSqlRaw($"UPDATE Properties SET {col} = 0 WHERE {col} IS NULL;");
#pragma warning restore EF1002
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

                    // Migrate PrimaryImagePath to ImagePath1 (v4.1.0 migration)
                    try
                    {
                        context.Database.ExecuteSqlRaw("UPDATE Properties SET ImagePath1 = PrimaryImagePath WHERE ImagePath1 = '' AND PrimaryImagePath != '' AND PrimaryImagePath IS NOT NULL;");
                        context.Database.ExecuteSqlRaw("UPDATE Properties SET ImagePath1 = '' WHERE ImagePath1 IS NULL;");
                        context.Database.ExecuteSqlRaw("UPDATE Properties SET ImagePath2 = '' WHERE ImagePath2 IS NULL;");
                        context.Database.ExecuteSqlRaw("UPDATE Properties SET ImagePath3 = '' WHERE ImagePath3 IS NULL;");
                        context.Database.ExecuteSqlRaw("UPDATE Properties SET ImagePath4 = '' WHERE ImagePath4 IS NULL;");
                    }
                    catch { /* Ignore */ }

                    // v5.1.0: Ensure Rating column allows decimal values (SQLite handles this automatically)
                    // Just ensure no NULL values exist
                    try
                    {
                        context.Database.ExecuteSqlRaw("UPDATE Properties SET Rating = 0 WHERE Rating IS NULL;");
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

                    // v6.0.0: Create PropertyTags and junction table for PROs/CONs tag system
                    context.Database.ExecuteSqlRaw(@"
                        CREATE TABLE IF NOT EXISTS PropertyTags (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            Name TEXT NOT NULL,
                            Type INTEGER NOT NULL DEFAULT 0,
                            IsCustom INTEGER NOT NULL DEFAULT 0
                        );
                        CREATE TABLE IF NOT EXISTS PropertyPropertyTag (
                            PropertiesId INTEGER NOT NULL,
                            TagsId INTEGER NOT NULL,
                            PRIMARY KEY (PropertiesId, TagsId),
                            FOREIGN KEY (PropertiesId) REFERENCES Properties (Id) ON DELETE CASCADE,
                            FOREIGN KEY (TagsId) REFERENCES PropertyTags (Id) ON DELETE CASCADE
                        );
                        CREATE INDEX IF NOT EXISTS IX_PropertyPropertyTag_TagsId ON PropertyPropertyTag (TagsId);
                    ");

                    // Seed predefined tags if table is empty
                    SeedPredefinedTags(context);
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

        private static void SeedPredefinedTags(AppDbContext context)
        {
            try
            {
                // Check if any tags exist
                var count = context.Database.ExecuteSqlRaw("SELECT COUNT(*) FROM PropertyTags");
                var hasData = context.PropertyTags.Any();
                if (hasData) return;

                // PRO tags (Type = 0)
                var proTags = new[]
                {
                    "Great Layout", "Updated Kitchen", "Large Backyard", "Quiet Street",
                    "Good Schools", "Move-in Ready", "Natural Light", "Storage Space",
                    "New Roof", "Updated Bathrooms", "Open Floor Plan", "Corner Lot",
                    "Low HOA", "Energy Efficient", "Great View", "Cul-de-sac"
                };

                // CON tags (Type = 1)
                var conTags = new[]
                {
                    "Needs Work", "Small Yard", "Busy Street", "Outdated Kitchen",
                    "HOA Restrictions", "Poor Layout", "No Garage", "Flood Zone",
                    "Old Roof", "Small Rooms", "No Updates", "High Taxes",
                    "High HOA", "Foundation Issues", "Bad Neighbors", "Far from Work"
                };

                foreach (var name in proTags)
                {
#pragma warning disable EF1002
                    context.Database.ExecuteSqlRaw($"INSERT OR IGNORE INTO PropertyTags (Name, Type, IsCustom) VALUES ('{name}', 0, 0);");
#pragma warning restore EF1002
                }

                foreach (var name in conTags)
                {
#pragma warning disable EF1002
                    context.Database.ExecuteSqlRaw($"INSERT OR IGNORE INTO PropertyTags (Name, Type, IsCustom) VALUES ('{name}', 1, 0);");
#pragma warning restore EF1002
                }
            }
            catch { /* Ignore seeding errors */ }
        }
    }
}

