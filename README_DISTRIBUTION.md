# HomeBuyingApp Distribution Guide

This document describes how to build and distribute the Home Buying Companion application.

## Quick Start

Run the all-in-one publish script:

```powershell
.\publish_app.ps1
```

This will:
1. Clean previous builds
2. Publish a self-contained executable
3. Create the installer (if Inno Setup is installed)

## Build Outputs

| Output | Location | Description |
|--------|----------|-------------|
| Executable | `Dist/HomeBuyingApp/HomeBuyingApp.UI.exe` | Self-contained, no .NET required |
| Installer | `Dist/Installer/HomeBuyingAppSetup_v7.4.3.exe` | Windows installer with shortcuts |

## Manual Build Steps

### 1. Publish the Application

```powershell
dotnet publish "HomeBuyingApp.UI/HomeBuyingApp.UI.csproj" `
    -c Release `
    -r win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    -p:PublishReadyToRun=true `
    -o "./Dist/HomeBuyingApp"
```

### 2. Create the Installer (Optional)

Requires [Inno Setup 6](https://jrsoftware.org/isdl.php).

```powershell
& "C:\Program Files (x86)\Inno Setup 6\ISCC.exe" setup.iss
```

## Distributing Without Installer

You can distribute the application without the installer:

1. Run `publish_app.ps1` or the manual publish command above
2. Zip the contents of `Dist/HomeBuyingApp/`
3. Share the zip file

Users can extract and run `HomeBuyingApp.UI.exe` directly - no installation required.

## Version Updates

When releasing a new version, update these files:

1. **HomeBuyingApp.UI/HomeBuyingApp.UI.csproj**:
   - `<Version>X.X.X</Version>`
   - `<AssemblyVersion>X.X.X.0</AssemblyVersion>`
   - `<FileVersion>X.X.X.0</FileVersion>`

2. **setup.iss**:
   - `#define MyAppVersion "X.X.X"`
   - `OutputBaseFilename=HomeBuyingAppSetup_vX.X.X`

3. **README.md**:
   - Update current version
   - Add to version history

## Troubleshooting

### Build Fails
- Ensure .NET 8 SDK is installed: `dotnet --version`
- Try cleaning: `dotnet clean` then `dotnet build`

### Installer Not Created
- Install [Inno Setup 6](https://jrsoftware.org/isdl.php)
- Ensure it's installed at `C:\Program Files (x86)\Inno Setup 6\`

### Application Won't Start
- Check Windows Event Viewer for .NET errors
- Try running from command line to see error output
