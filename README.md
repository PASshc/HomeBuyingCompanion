# Home Buying Companion

A comprehensive desktop application designed to assist home buyers in tracking properties, calculating mortgages, and managing the home buying process.

**Current Version: 6.0.0**

## Features

### Property Management
- **Property Tracking**: Store details about properties including address, price, bedrooms, bathrooms, and square footage
- **Status Tracking**: Mark properties as "Researching", "Interested", "Offer Made", "Under Contract", "Closed", or "Passed"
- **Rating System**: Rate properties from 1-5 (supports decimal values like 3.5)
- **Quick Filters**: Filter by Street #, City, and State for quick property searching
- **Archive**: Hide properties you're no longer considering without deleting them

### Property Details
- **Interior Features**: Track flooring types (carpet, tile, wood), appliances, and climate systems
- **Exterior Features**: Pool, jacuzzi, lanai, landscape, fenced yard, patio, and more
- **Custom Features**: Two customizable feature checkboxes with editable labels
- **Other Features**: Tag-based chips for additional property features (teal colored)
- **Images**: Attach up to 4 images per property with thumbnail preview and click-to-enlarge
- **Attachments**: Attach files (PDFs, documents) to specific properties
- **Rich Text Comments**: Full formatting toolbar with bold, italic, underline, bullet/numbered lists, and highlighting

### Mortgage Calculator
- **Built-in Calculator**: Estimate monthly payments including principal, interest, PMI, HOA, taxes, and insurance
- **Scenario Comparison**: Compare multiple mortgage scenarios side-by-side
- **Per-Property Calculations**: Save mortgage settings specific to each property
- **Amortization Schedule**: View complete payment breakdown over the loan term

### Data Management
- **CSV Import/Export**: Import properties from spreadsheets or export for backup
- **Quick Add from Clipboard**: Paste listing text (from Zillow, Redfin, etc.) to auto-populate fields
- **Backup/Restore**: Full database backup and restore functionality
- **Local Storage**: All data stored locally in SQLite - no cloud dependency

## Getting Started

### Prerequisites

- Windows 10 or later
- .NET 8.0 Runtime (included in self-contained installer)

### Installation

1. Download the latest installer from the [Releases](https://github.com/PASshc/HomeBuyingCompanion/releases) page
2. Run the installer (`HomeBuyingAppSetup_v5.2.0.exe`)
3. Launch the application from the Start Menu or Desktop shortcut

### Data Location

User data is stored in:
- **Production**: `%LocalAppData%\HomeBuyingApp\`
- **Development**: `%LocalAppData%\HomeBuyingApp_Dev\`

## Development

### Prerequisites

- Visual Studio 2022 or VS Code
- .NET 8.0 SDK

### Project Structure

```
HomeBuyingApp.sln
├── HomeBuyingApp.Core/           # Domain models and service interfaces
│   ├── Models/                   # Property, Attachment, MortgageScenario, etc.
│   └── Services/                 # IMortgageCalculatorService, IPropertyService, etc.
├── HomeBuyingApp.Infrastructure/ # Data access and service implementations
│   ├── Data/                     # Entity Framework DbContext
│   └── Services/                 # PropertyService, CsvService, BackupService, etc.
├── HomeBuyingApp.UI/             # WPF application
│   ├── Converters/               # Value converters for XAML bindings
│   ├── Helpers/                  # RichTextBox binding helper
│   ├── Resources/                # Icons and assets
│   ├── ViewModels/               # MVVM ViewModels
│   └── Views/                    # XAML views
└── Dist/                         # Published builds and installers
```

### Building the Project

1. Clone the repository:
   ```bash
   git clone https://github.com/PASshc/HomeBuyingCompanion.git
   ```
2. Open the solution `HomeBuyingApp.sln`
3. Build the solution (`Ctrl+Shift+B`)

### Running in Development

```bash
dotnet run --project HomeBuyingApp.UI
```

### Publishing

Use the provided PowerShell script to publish and create an installer:

```powershell
./publish_app.ps1
```

This script:
1. Publishes a self-contained single-file executable to `Dist/HomeBuyingApp/`
2. Compiles the Inno Setup installer to `Dist/Installer/`

To manually create just the installer (requires [Inno Setup 6](https://jrsoftware.org/isdl.php)):

```bash
"C:\Program Files (x86)\Inno Setup 6\ISCC.exe" setup.iss
```

## Technologies Used

- **WPF (Windows Presentation Foundation)**: UI Framework with MVVM pattern
- **.NET 8**: Core platform
- **Entity Framework Core 8**: Data access with code-first approach
- **SQLite**: Local database storage
- **Inno Setup 6**: Installer creation

## Version History

- **v5.2.0**: Star rating control, quick notes chips, PROs/CONs tag system, attachment notes, colorful UI icons
- **v5.1.0**: Rating supports decimals (e.g., 3.5), codebase cleanup
- **v5.0.0**: Rich text Notes/Comments, multi-image support (4 per property)
- **v4.2.0**: Expanded property features (20+ checkboxes)
- **v4.1.0**: Attachments feature, viewing/inspection/closing dates
- **v4.0.0**: Mortgage comparison view, per-property calculations
- **v3.0.0**: CSV import/export, backup/restore
- **v2.0.0**: Mortgage calculator, property status tracking
- **v1.0.0**: Initial release with property tracking

## License

[MIT License](LICENSE)
