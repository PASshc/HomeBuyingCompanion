using System;
using System.Collections.Generic;
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

        /// <summary>
        /// Gets the application data path (different for DEBUG vs Release).
        /// Use this for storing images, attachments, and other user data.
        /// </summary>
        public static string AppDataPath { get; private set; } = string.Empty;

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
            
            // Store the app data path for use by other components
            AppDataPath = appDataPath;
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
            services.AddScoped<IJournalService, JournalService>();
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
            services.AddTransient<JournalViewModel>();
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

                    // v6.1.0: Create JournalEntries table for home buying journey tracking
                    context.Database.ExecuteSqlRaw(@"
                        CREATE TABLE IF NOT EXISTS JournalEntries (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            EntryDate TEXT NOT NULL,
                            Category INTEGER NOT NULL DEFAULT 0,
                            Title TEXT NOT NULL,
                            Content TEXT DEFAULT '',
                            PropertyId INTEGER NULL,
                            CreatedAt TEXT NOT NULL,
                            ModifiedAt TEXT NOT NULL,
                            FOREIGN KEY (PropertyId) REFERENCES Properties (Id) ON DELETE SET NULL
                        );
                        CREATE INDEX IF NOT EXISTS IX_JournalEntries_EntryDate ON JournalEntries (EntryDate);
                        CREATE INDEX IF NOT EXISTS IX_JournalEntries_Category ON JournalEntries (Category);
                        CREATE INDEX IF NOT EXISTS IX_JournalEntries_PropertyId ON JournalEntries (PropertyId);
                    ");

                    // v6.2.0: Create JournalAttachments table for document attachments on journal entries
                    context.Database.ExecuteSqlRaw(@"
                        CREATE TABLE IF NOT EXISTS JournalAttachments (
                            Id INTEGER PRIMARY KEY AUTOINCREMENT,
                            JournalEntryId INTEGER NOT NULL,
                            FileName TEXT NOT NULL,
                            FilePath TEXT NOT NULL,
                            Description TEXT DEFAULT '',
                            DateAdded TEXT NOT NULL,
                            FileSize INTEGER NOT NULL DEFAULT 0,
                            FileType TEXT DEFAULT '',
                            FOREIGN KEY (JournalEntryId) REFERENCES JournalEntries (Id) ON DELETE CASCADE
                        );
                        CREATE INDEX IF NOT EXISTS IX_JournalAttachments_JournalEntryId ON JournalAttachments (JournalEntryId);
                    ");

                    // v7.2.0: Add Category column to PropertyTags table for tag organization
                    try
                    {
                        context.Database.ExecuteSqlRaw(@"
                            ALTER TABLE PropertyTags ADD COLUMN Category TEXT DEFAULT '';
                        ");
                    }
                    catch { /* Column may already exist */ }

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
                if (hasData)
                {
                    // Update existing tags with categories if they don't have one
                    UpdateExistingTagsWithCategories(context);
                    return;
                }

                // PRO tags with categories (Type = 0)
                var proTags = new (string Name, string Category)[]
                {
                    ("Updated", "Kitchen"),
                    ("Remodeled", "Kitchen"),
                    ("Spacious", "Kitchen"),
                    ("Updated", "Bathroom"),
                    ("Remodeled", "Bathroom"),
                    ("New", "Roof"),
                    ("Good Condition", "Roof"),
                    ("Efficient", "HVAC"),
                    ("Updated", "HVAC"),
                    ("New", "Flooring"),
                    ("Hardwood", "Flooring"),
                    ("Large", "Yard"),
                    ("Private", "Yard"),
                    ("Fenced", "Yard"),
                    ("Good Schools", "Location"),
                    ("Quiet Street", "Location"),
                    ("Cul-de-sac", "Location"),
                    ("Great View", "Location"),
                    ("Low", "HOA"),
                    ("Spacious", "Bedroom"),
                    ("Walk-in Closet", "Bedroom"),
                    ("Attached", "Garage"),
                    ("2-Car", "Garage"),
                    ("Great Layout", ""),
                    ("Move-in Ready", ""),
                    ("Natural Light", ""),
                    ("Storage Space", ""),
                    ("Open Floor Plan", ""),
                    ("Corner Lot", ""),
                    ("Energy Efficient", "")
                };

                // CON tags with categories (Type = 1)
                var conTags = new (string Name, string Category)[]
                {
                    ("Outdated", "Kitchen"),
                    ("Small", "Kitchen"),
                    ("Needs Work", "Kitchen"),
                    ("Outdated", "Bathroom"),
                    ("Small", "Bathroom"),
                    ("Old", "Roof"),
                    ("Needs Replacement", "Roof"),
                    ("Old", "HVAC"),
                    ("Needs Replacement", "HVAC"),
                    ("Worn", "Flooring"),
                    ("Needs Replacement", "Flooring"),
                    ("Small", "Yard"),
                    ("No Privacy", "Yard"),
                    ("Poor Schools", "Location"),
                    ("Busy Street", "Location"),
                    ("Flood Zone", "Location"),
                    ("Far from Work", "Location"),
                    ("High", "HOA"),
                    ("Restrictive", "HOA"),
                    ("Small", "Bedroom"),
                    ("No Closet", "Bedroom"),
                    ("No Garage", "Garage"),
                    ("Small", "Garage"),
                    ("Foundation Issues", ""),
                    ("High Taxes", ""),
                    ("Needs Work", ""),
                    ("Poor Layout", ""),
                    ("No Updates", ""),
                    ("Bad Neighbors", "")
                };

                foreach (var (name, category) in proTags)
                {
#pragma warning disable EF1002
                    context.Database.ExecuteSqlRaw($"INSERT OR IGNORE INTO PropertyTags (Name, Category, Type, IsCustom) VALUES ('{name}', '{category}', 0, 0);");
#pragma warning restore EF1002
                }

                foreach (var (name, category) in conTags)
                {
#pragma warning disable EF1002
                    context.Database.ExecuteSqlRaw($"INSERT OR IGNORE INTO PropertyTags (Name, Category, Type, IsCustom) VALUES ('{name}', '{category}', 1, 0);");
#pragma warning restore EF1002
                }
            }
            catch { /* Ignore seeding errors */ }
        }

        private static void UpdateExistingTagsWithCategories(AppDbContext context)
        {
            try
            {
                // Update tags based on keywords in the tag name
                // Kitchen-related
                context.Database.ExecuteSqlRaw("UPDATE PropertyTags SET Category = 'Kitchen' WHERE (Category IS NULL OR Category = '') AND (Name LIKE '%Kitchen%' OR Name LIKE '%Island%' OR Name LIKE '%Countertop%' OR Name LIKE '%Cabinet%' OR Name LIKE '%Appliance%');");
                
                // Bathroom-related
                context.Database.ExecuteSqlRaw("UPDATE PropertyTags SET Category = 'Bathroom' WHERE (Category IS NULL OR Category = '') AND (Name LIKE '%Bathroom%' OR Name LIKE '%Bath%' OR Name LIKE '%Shower%' OR Name LIKE '%Tub%' OR Name LIKE '%Vanity%');");
                
                // Roof-related
                context.Database.ExecuteSqlRaw("UPDATE PropertyTags SET Category = 'Roof' WHERE (Category IS NULL OR Category = '') AND Name LIKE '%Roof%';");
                
                // HVAC-related
                context.Database.ExecuteSqlRaw("UPDATE PropertyTags SET Category = 'HVAC' WHERE (Category IS NULL OR Category = '') AND (Name LIKE '%HVAC%' OR Name LIKE '%AC%' OR Name LIKE '%Heat%' OR Name LIKE '%Furnace%' OR Name LIKE '%Air Condition%');");
                
                // Flooring-related
                context.Database.ExecuteSqlRaw("UPDATE PropertyTags SET Category = 'Flooring' WHERE (Category IS NULL OR Category = '') AND (Name LIKE '%Floor%' OR Name LIKE '%Tile%' OR Name LIKE '%Carpet%' OR Name LIKE '%Hardwood%' OR Name LIKE '%Laminate%');");
                
                // Yard-related
                context.Database.ExecuteSqlRaw("UPDATE PropertyTags SET Category = 'Yard' WHERE (Category IS NULL OR Category = '') AND (Name LIKE '%Yard%' OR Name LIKE '%Backyard%' OR Name LIKE '%Landscape%' OR Name LIKE '%Garden%' OR Name LIKE '%Lawn%' OR Name LIKE '%Fence%' OR Name LIKE '%Patio%' OR Name LIKE '%Deck%' OR Name LIKE '%Pool%');");
                
                // Exterior-related
                context.Database.ExecuteSqlRaw("UPDATE PropertyTags SET Category = 'Exterior' WHERE (Category IS NULL OR Category = '') AND (Name LIKE '%Exterior%' OR Name LIKE '%Siding%' OR Name LIKE '%Paint%' OR Name LIKE '%Curb%' OR Name LIKE '%Driveway%');");
                
                // Garage-related
                context.Database.ExecuteSqlRaw("UPDATE PropertyTags SET Category = 'Garage' WHERE (Category IS NULL OR Category = '') AND (Name LIKE '%Garage%' OR Name LIKE '%Carport%' OR Name LIKE '%Parking%');");
                
                // Location-related
                context.Database.ExecuteSqlRaw("UPDATE PropertyTags SET Category = 'Location' WHERE (Category IS NULL OR Category = '') AND (Name LIKE '%View%' OR Name LIKE '%Street%' OR Name LIKE '%Location%' OR Name LIKE '%Neighbor%' OR Name LIKE '%Lake%' OR Name LIKE '%Beach%' OR Name LIKE '%Mountain%' OR Name LIKE '%Cul-de-sac%' OR Name LIKE '%Corner Lot%' OR Name LIKE '%Close to%' OR Name LIKE '%Near%');");
                
                // Schools-related
                context.Database.ExecuteSqlRaw("UPDATE PropertyTags SET Category = 'Schools' WHERE (Category IS NULL OR Category = '') AND (Name LIKE '%School%' OR Name LIKE '%District%');");
                
                // HOA-related
                context.Database.ExecuteSqlRaw("UPDATE PropertyTags SET Category = 'HOA' WHERE (Category IS NULL OR Category = '') AND Name LIKE '%HOA%';");
                
                // Bedroom-related
                context.Database.ExecuteSqlRaw("UPDATE PropertyTags SET Category = 'Bedroom' WHERE (Category IS NULL OR Category = '') AND (Name LIKE '%Bedroom%' OR Name LIKE '%Closet%' OR Name LIKE '%Master%');");
                
                // Plumbing-related
                context.Database.ExecuteSqlRaw("UPDATE PropertyTags SET Category = 'Plumbing' WHERE (Category IS NULL OR Category = '') AND (Name LIKE '%Plumbing%' OR Name LIKE '%Pipe%' OR Name LIKE '%Water Heater%' OR Name LIKE '%Sewer%');");
                
                // Electrical-related
                context.Database.ExecuteSqlRaw("UPDATE PropertyTags SET Category = 'Electrical' WHERE (Category IS NULL OR Category = '') AND (Name LIKE '%Electrical%' OR Name LIKE '%Wiring%' OR Name LIKE '%Panel%' OR Name LIKE '%Outlet%');");
                
                // Living-related
                context.Database.ExecuteSqlRaw("UPDATE PropertyTags SET Category = 'Living' WHERE (Category IS NULL OR Category = '') AND (Name LIKE '%Living%' OR Name LIKE '%Family Room%' OR Name LIKE '%Den%' OR Name LIKE '%Layout%' OR Name LIKE '%Floor Plan%' OR Name LIKE '%Open Concept%');");
                
                // Basement-related
                context.Database.ExecuteSqlRaw("UPDATE PropertyTags SET Category = 'Other' WHERE (Category IS NULL OR Category = '') AND (Name LIKE '%Basement%' OR Name LIKE '%Attic%' OR Name LIKE '%Storage%' OR Name LIKE '%Foundation%');");
            }
            catch { /* Ignore update errors */ }
        }
    }
}

