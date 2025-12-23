# Home Buying Companion - Application Architecture

**Version:** 5.2.0  
**Platform:** Windows Desktop (WPF)  
**Framework:** .NET 8.0  
**Database:** SQLite (Entity Framework Core)

---

## Table of Contents

1. [Overview](#overview)
2. [Solution Structure](#solution-structure)
3. [Architecture Pattern](#architecture-pattern)
4. [Core Domain Models](#core-domain-models)
5. [Service Layer](#service-layer)
6. [Data Layer](#data-layer)
7. [UI Layer](#ui-layer)
8. [Feature Breakdown](#feature-breakdown)
9. [Database Schema](#database-schema)
10. [Build & Deployment](#build--deployment)
11. [Version History](#version-history)

---

## Overview

Home Buying Companion is a comprehensive desktop application designed to assist home buyers in:

- **Tracking properties** with detailed information
- **Calculating mortgages** with multiple scenarios
- **Managing the home buying process** from research to closing
- **Organizing notes, attachments, and ratings** for each property

The application stores all data locally using SQLite, ensuring privacy and offline access without cloud dependencies.

---

## Solution Structure

```
HomeBuyingApp.sln
│
├── HomeBuyingApp.Core/              # Domain layer (Models & Interfaces)
│   ├── Models/                      # Domain entities
│   │   ├── Property.cs              # Main property entity (100+ fields)
│   │   ├── PropertyStatus.cs        # Enum: status tracking
│   │   ├── PropertyTag.cs           # PROs/CONs tag entity
│   │   ├── TagType.cs               # Enum: Pro, Con, Neutral
│   │   ├── PropertyAttachment.cs    # File attachment entity
│   │   ├── MortgageScenario.cs      # Mortgage calculation scenario
│   │   ├── AmortizationEntry.cs     # Monthly payment breakdown
│   │   └── ViewingAppointment.cs    # Viewing schedule entity
│   │
│   └── Services/                    # Service interfaces
│       ├── IPropertyService.cs      # Property CRUD operations
│       ├── ITagService.cs           # Tag management
│       ├── IMortgageCalculatorService.cs
│       ├── ICsvService.cs           # CSV import/export
│       ├── IBackupService.cs        # Backup/restore
│       └── ListingTextParser.cs     # Clipboard paste parser
│
├── HomeBuyingApp.Infrastructure/    # Data access layer
│   ├── Data/
│   │   └── AppDbContext.cs          # EF Core DbContext
│   │
│   └── Services/                    # Service implementations
│       ├── PropertyService.cs
│       ├── TagService.cs
│       ├── BackupService.cs
│       └── CsvService.cs
│
├── HomeBuyingApp.UI/                # Presentation layer (WPF)
│   ├── App.xaml                     # Application entry & DI setup
│   ├── MainWindow.xaml              # Shell with tab navigation
│   │
│   ├── Views/                       # XAML views
│   │   ├── PropertyListView.xaml    # Property grid with filters
│   │   ├── PropertyDetailView.xaml  # Full property editor
│   │   ├── MortgageCalculatorView.xaml
│   │   └── MortgageComparisonView.xaml
│   │
│   ├── ViewModels/                  # MVVM ViewModels
│   │   ├── MainViewModel.cs
│   │   ├── PropertyListViewModel.cs
│   │   ├── PropertyDetailViewModel.cs
│   │   ├── PropertyViewModel.cs     # Property wrapper
│   │   ├── MortgageCalculatorViewModel.cs
│   │   ├── MortgageComparisonViewModel.cs
│   │   ├── ViewModelBase.cs         # INotifyPropertyChanged base
│   │   └── RelayCommand.cs          # ICommand implementation
│   │
│   ├── Controls/                    # Custom controls
│   │   └── StarRatingControl.xaml   # 5-star rating control
│   │
│   ├── Converters/                  # XAML value converters
│   │   ├── BooleanToYesNoConverter.cs
│   │   ├── BooleanToVisibilityConverter.cs
│   │   ├── InverseBooleanToVisibilityConverter.cs
│   │   ├── CurrencyConverter.cs
│   │   ├── PercentageConverter.cs
│   │   ├── NumberWithCommasConverter.cs
│   │   ├── StatusToBorderBrushConverter.cs
│   │   └── TagTypeToColorConverter.cs
│   │
│   ├── Helpers/                     # Utility classes
│   │   └── RichTextBoxBinding.cs    # RichTextBox XAML binding
│   │
│   └── Resources/                   # Icons and assets
│       └── homeBuyAppIcon_v2.ico
│
├── HomeBuyingApp.Test/              # Test project
│
└── Dist/                            # Build outputs
    ├── HomeBuyingApp/               # Published executable
    └── Installer/                   # Inno Setup installer
```

---

## Architecture Pattern

### MVVM (Model-View-ViewModel)

The application follows the **MVVM pattern** for clean separation of concerns:

```
┌─────────────────────────────────────────────────────────────────┐
│                           VIEW (XAML)                           │
│  PropertyListView, PropertyDetailView, MortgageCalculatorView  │
└───────────────────────────┬─────────────────────────────────────┘
                            │ Data Binding
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                      VIEWMODEL (C#)                             │
│  PropertyListViewModel, PropertyDetailViewModel, etc.           │
│  - Exposes properties for binding                               │
│  - Implements INotifyPropertyChanged                            │
│  - Commands (RelayCommand) for user actions                     │
└───────────────────────────┬─────────────────────────────────────┘
                            │ Service calls
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                       SERVICES                                  │
│  IPropertyService, ITagService, IMortgageCalculatorService      │
└───────────────────────────┬─────────────────────────────────────┘
                            │ Data access
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                    DATA LAYER (EF Core)                         │
│  AppDbContext → SQLite Database                                 │
└─────────────────────────────────────────────────────────────────┘
```

### Dependency Injection

Services are registered in `App.xaml.cs` using `Microsoft.Extensions.DependencyInjection`:

```csharp
services.AddDbContext<AppDbContext>();
services.AddScoped<IPropertyService, PropertyService>();
services.AddScoped<ITagService, TagService>();
services.AddSingleton<IMortgageCalculatorService, MortgageCalculatorService>();
services.AddSingleton<IBackupService, BackupService>();
services.AddTransient<PropertyListViewModel>();
// ... etc
```

---

## Core Domain Models

### Property (Main Entity)

The central entity with 100+ fields organized into categories:

| Category | Fields |
|----------|--------|
| **Location** | Address, City, State, ZipCode, MlsNumber, ListingUrl |
| **Pricing** | ListPrice, EstimatedOffer, WillingToPay |
| **Basic Info** | Bedrooms, Bathrooms, SquareFeet |
| **Status** | Status (enum), LookAt, Interest, IsArchived |
| **Rating** | Rating (decimal 1-5), QuickNotes (JSON) |
| **Notes** | Comments (rich text), Notes (rich text) |
| **Dates** | ViewingDate, InspectionDate, ClosingDate |
| **HOA** | HasHoa, HoaFee, PropertyTaxRate |
| **Interior Features** | 15+ boolean flags (flooring, appliances, climate) |
| **Exterior Features** | 12+ boolean flags (pool, yard, garage, etc.) |
| **Custom Features** | 2 customizable feature checkboxes with labels |
| **Images** | ImagePath1-4 (up to 4 images) |
| **Mortgage Calc** | Saved calculation parameters |
| **Relationships** | Tags, Attachments, MortgageScenarios, Viewings |

### PropertyStatus (Enum)

```csharp
public enum PropertyStatus
{
    Interested = 0,
    PendingVisit = 1,
    OfferMade = 2,
    OfferRejected = 3,
    Closed = 4,
    PropertyInspection = 5,
    NotInterested = 6,
    Researching = 7
}
```

### PropertyTag

Many-to-many relationship with Property for PROs/CONs:

```csharp
public class PropertyTag
{
    public int Id { get; set; }
    public string Name { get; set; }
    public TagType Type { get; set; }      // Pro, Con, Neutral
    public bool IsCustom { get; set; }     // User-created vs predefined
    public List<Property> Properties { get; set; }
}
```

### MortgageScenario

Stores mortgage calculation inputs and results:

- Purchase price, down payment, interest rate, loan term
- Property tax, insurance, PMI, HOA amounts
- Calculated monthly payments

### AmortizationEntry

Monthly breakdown for amortization schedule:

```csharp
public class AmortizationEntry
{
    public int Month { get; set; }
    public decimal Payment { get; set; }
    public decimal Principal { get; set; }
    public decimal Interest { get; set; }
    public decimal RemainingBalance { get; set; }
}
```

### PropertyAttachment

File attachments with notes:

```csharp
public class PropertyAttachment
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public string FileName { get; set; }
    public string FilePath { get; set; }
    public string Description { get; set; }  // Notes for the attachment
    public DateTime DateAdded { get; set; }
}
```

---

## Service Layer

### IPropertyService

```csharp
public interface IPropertyService
{
    Task<List<Property>> GetAllPropertiesAsync();
    Task<Property?> GetPropertyByIdAsync(int id);
    Task AddPropertyAsync(Property property);
    Task UpdatePropertyAsync(Property property);
    Task DeletePropertyAsync(int id);
    Task<bool> PropertyExistsAsync(string address, string city, string state, string zipCode);
    Task AddAttachmentAsync(PropertyAttachment attachment);
    Task UpdateAttachmentAsync(PropertyAttachment attachment);
    Task DeleteAttachmentAsync(int attachmentId);
}
```

### ITagService

```csharp
public interface ITagService
{
    Task<List<PropertyTag>> GetAllTagsAsync();
    Task<List<PropertyTag>> GetTagsByTypeAsync(TagType type);
    Task<PropertyTag> CreateTagAsync(string name, TagType type, bool isCustom = true);
    Task DeleteTagAsync(int tagId);
    Task AddTagToPropertyAsync(int propertyId, int tagId);
    Task RemoveTagFromPropertyAsync(int propertyId, int tagId);
    Task<List<PropertyTag>> GetTagsForPropertyAsync(int propertyId);
}
```

### IMortgageCalculatorService

```csharp
public interface IMortgageCalculatorService
{
    MortgageScenario Calculate(MortgageScenario scenario);
    List<AmortizationEntry> GenerateAmortizationSchedule(MortgageScenario scenario);
}
```

### ICsvService

```csharp
public interface ICsvService
{
    string GenerateCsv(IEnumerable<Property> properties);
    IEnumerable<Property> ParseCsv(string csvContent);
}
```

### IBackupService

```csharp
public interface IBackupService
{
    Task CreateBackupAsync(string destinationPath);
    Task RestoreBackupAsync(string sourcePath);
}
```

---

## Data Layer

### AppDbContext

Entity Framework Core DbContext with SQLite:

```csharp
public class AppDbContext : DbContext
{
    public DbSet<Property> Properties { get; set; }
    public DbSet<MortgageScenario> MortgageScenarios { get; set; }
    public DbSet<ViewingAppointment> Viewings { get; set; }
    public DbSet<PropertyAttachment> Attachments { get; set; }
    public DbSet<PropertyTag> PropertyTags { get; set; }
}
```

### Database Location

- **Production:** `%LocalAppData%\HomeBuyingApp\homebuying.db`
- **Development:** `%LocalAppData%\HomeBuyingApp_Dev\homebuying.db`

### Schema Migrations

The app handles schema evolution in `App.xaml.cs` OnStartup:

1. Creates tables if not exist
2. Adds new columns with ALTER TABLE
3. Sets default values for NULL columns
4. Seeds predefined tags

---

## UI Layer

### Views

| View | Purpose |
|------|---------|
| **MainWindow** | Application shell with TabControl navigation |
| **PropertyListView** | DataGrid with filtering, sorting, status colors |
| **PropertyDetailView** | Full property editor with all sections |
| **MortgageCalculatorView** | Embedded calculator in property detail |
| **MortgageComparisonView** | Side-by-side scenario comparison |

### ViewModels

| ViewModel | Responsibilities |
|-----------|-----------------|
| **MainViewModel** | Tab navigation, main window state |
| **PropertyListViewModel** | Property collection, filtering, CRUD actions |
| **PropertyDetailViewModel** | Single property editing, all features |
| **PropertyViewModel** | Wrapper around Property model with change notification |
| **MortgageCalculatorViewModel** | Calculator inputs and outputs |
| **MortgageComparisonViewModel** | Multiple scenario comparison |

### Custom Controls

#### StarRatingControl

5-star clickable rating control with DependencyProperty:

```xaml
<controls:StarRatingControl Rating="{Binding Property.Rating, Mode=TwoWay}"/>
```

### Value Converters

| Converter | Purpose |
|-----------|---------|
| `BooleanToYesNoConverter` | `true` → "Yes", `false` → "No" |
| `BooleanToVisibilityConverter` | `true` → Visible, `false` → Collapsed |
| `InverseBooleanToVisibilityConverter` | Opposite of above |
| `CurrencyConverter` | Format decimals as currency |
| `PercentageConverter` | Format decimals as percentage |
| `NumberWithCommasConverter` | Add thousand separators |
| `StatusToBorderBrushConverter` | Property status → color |
| `TagTypeToColorConverter` | Pro=Green, Con=Red |

---

## Feature Breakdown

### Property Management

- ✅ Add/Edit/Delete properties
- ✅ Duplicate detection by address
- ✅ 8 status types with color coding
- ✅ Archive (soft delete) with toggle
- ✅ Quick filters (LookAt, Interest)
- ✅ Filter by city and status

### Rating System

- ✅ 5-star clickable rating control
- ✅ Supports decimal values (e.g., 3.5)
- ✅ Visual star display (★/☆)

### Quick Notes (Chips)

- ✅ Add/remove quick note chips
- ✅ Stored as JSON array
- ✅ Purple chip styling with ✕ remove button

### PROs/CONs Tags

- ✅ Separate PRO and CON sections
- ✅ Predefined tags (seeded on first run)
- ✅ Custom tag creation
- ✅ Tags apply across all properties
- ✅ Color-coded (green PROs, red CONs)

### Rich Text Notes

- ✅ Comments and Notes fields
- ✅ Bold, italic, underline formatting
- ✅ Bullet lists
- ✅ XAML-based storage

### Property Features (25+ Checkboxes)

**Interior:**
- Flooring: Carpet, Tile, Wood
- Appliances: Oven, Refrigerator, Washer/Dryer, Dishwasher
- Climate: A/C, Central Heat, Fireplace, Ceiling Fans
- Storage: Walk-in Closet, Attic, Basement

**Exterior:**
- Outdoor: Pool, Jacuzzi, Lanai, Covered Patio, Deck, Fenced Yard, Sprinklers
- Structures: Garage, Carport, Storage Shed, Guest House

**Other:**
- Solar Panels, Security System
- 2 Custom features with editable labels

### Images

- ✅ Up to 4 images per property
- ✅ Paste from clipboard
- ✅ Thumbnail preview
- ✅ Click to view full size

### Attachments

- ✅ Attach any file type
- ✅ Notes column for each attachment
- ✅ Open in default application
- ✅ Delete with confirmation

### Mortgage Calculator

- ✅ Calculate monthly payments
- ✅ P&I, taxes, insurance, PMI, HOA breakdown
- ✅ Save parameters per property
- ✅ Generate amortization schedule
- ✅ Compare multiple scenarios

### Data Management

- ✅ CSV import/export
- ✅ Clipboard paste from listings (Zillow, Redfin)
- ✅ Full backup/restore (.zip)
- ✅ Local SQLite storage

---

## Database Schema

### Tables

```sql
Properties (
    Id INTEGER PRIMARY KEY,
    Address TEXT,
    City TEXT,
    State TEXT,
    ZipCode TEXT,
    ListingUrl TEXT,
    MlsNumber TEXT,
    ListPrice REAL,
    EstimatedOffer REAL,
    WillingToPay REAL,
    Bedrooms INTEGER,
    Bathrooms REAL,
    SquareFeet INTEGER,
    HasHoa INTEGER,
    HoaFee REAL,
    PropertyTaxRate REAL,
    Comments TEXT,
    Notes TEXT,
    QuickNotes TEXT,
    Rating REAL,
    LookAt INTEGER,
    Interest INTEGER,
    IsArchived INTEGER,
    Status INTEGER,
    ViewingDate TEXT,
    InspectionDate TEXT,
    ClosingDate TEXT,
    -- 25+ feature boolean columns
    -- 4 image path columns
    -- Mortgage calc columns
)

PropertyTags (
    Id INTEGER PRIMARY KEY,
    Name TEXT NOT NULL,
    Type INTEGER NOT NULL,       -- 0=Pro, 1=Con, 2=Neutral
    IsCustom INTEGER NOT NULL
)

PropertyPropertyTag (
    PropertiesId INTEGER,
    TagsId INTEGER,
    PRIMARY KEY (PropertiesId, TagsId)
)

Attachments (
    Id INTEGER PRIMARY KEY,
    PropertyId INTEGER NOT NULL,
    FileName TEXT NOT NULL,
    FilePath TEXT NOT NULL,
    Description TEXT,
    DateAdded TEXT NOT NULL,
    FOREIGN KEY (PropertyId) REFERENCES Properties(Id) ON DELETE CASCADE
)

MortgageScenarios (
    Id INTEGER PRIMARY KEY,
    PropertyId INTEGER NOT NULL,
    Name TEXT,
    PurchasePrice REAL,
    DownPaymentAmount REAL,
    InterestRate REAL,
    LoanTermYears INTEGER,
    -- ... additional fields
)
```

---

## Build & Deployment

### Prerequisites

- .NET 8.0 SDK
- Visual Studio 2022 or VS Code
- Inno Setup 6 (for installer)

### Development Build

```bash
dotnet build HomeBuyingApp.UI
dotnet run --project HomeBuyingApp.UI
```

### Release Build

```bash
dotnet publish HomeBuyingApp.UI -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

### Create Installer

```powershell
./publish_app.ps1
```

This script:
1. Publishes self-contained single-file executable
2. Compiles Inno Setup installer
3. Outputs to `Dist/Installer/HomeBuyingAppSetup_v5.2.0.exe`

### Distribution Files

| File | Location | Description |
|------|----------|-------------|
| Standalone EXE | `Dist/HomeBuyingApp/HomeBuyingApp.UI.exe` | Portable, no install needed |
| Installer | `Dist/Installer/HomeBuyingAppSetup_v5.2.0.exe` | Windows installer |

---

## Version History

| Version | Date | Changes |
|---------|------|---------|
| **5.2.0** | Dec 2024 | Star rating control, quick notes chips, PROs/CONs tags, attachment notes, colorful UI icons |
| **5.1.0** | Dec 2024 | Decimal ratings (e.g., 3.5), codebase cleanup |
| **5.0.0** | Dec 2024 | Rich text Notes/Comments, multi-image support (4 per property) |
| **4.2.0** | Dec 2024 | Expanded features (20+ checkboxes) |
| **4.1.0** | Dec 2024 | Attachments, viewing/inspection/closing dates |
| **4.0.0** | Dec 2024 | Mortgage comparison, per-property calculations |
| **3.0.0** | Dec 2024 | CSV import/export, backup/restore |
| **2.0.0** | Dec 2024 | Mortgage calculator, status tracking |
| **1.0.0** | Dec 2024 | Initial release with property tracking |

---

## License

MIT License

---

*Generated: December 22, 2025*
