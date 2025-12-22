# Home Buying Companion

A comprehensive desktop application designed to assist home buyers in tracking properties, calculating mortgages, and managing the home buying process.

## Features

- **Property Tracking**: Store details about properties including address, price, bedrooms, bathrooms, and square footage.
- **Mortgage Calculator**: Built-in calculator to estimate monthly payments, including PMI, HOA, and taxes.
- **Viewing Management**: Track viewing appointments, agent details, and notes.
- **Attachments**: Attach files (images, documents) to specific properties.
- **Comparison**: Compare multiple properties side-by-side.
- **Status Tracking**: Mark properties as "Interested", "Offer Made", "Under Contract", etc.

## Getting Started

### Prerequisites

- Windows 10 or later.
- .NET 8.0 Runtime (if not using the self-contained executable).

### Installation

1. Download the latest installer from the [Releases](https://github.com/PASshc/HomeBuyingCompanion/releases) page.
2. Run the installer (`HomeBuyingAppSetup_vX.X.X.exe`).
3. Launch the application from the Start Menu or Desktop shortcut.

## Development

### Prerequisites

- Visual Studio 2022 or VS Code.
- .NET 8.0 SDK.

### Building the Project

1. Clone the repository:
   ```bash
   git clone https://github.com/PASshc/HomeBuyingCompanion.git
   ```
2. Open the solution `HomeBuyingApp.sln`.
3. Build the solution.

### Publishing

Use the provided PowerShell script to publish a self-contained executable:

```powershell
./publish_app.ps1
```

To create the installer (requires Inno Setup):

```bash
ISCC setup.iss
```

## Technologies Used

- **WPF (Windows Presentation Foundation)**: UI Framework.
- **.NET 8**: Core platform.
- **Entity Framework Core**: Data access.
- **SQLite**: Local database.
- **Inno Setup**: Installer creation.

## License

[MIT License](LICENSE)
