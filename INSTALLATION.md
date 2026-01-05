# Installation Guide

## Download

Download the latest installer from the [Releases](https://github.com/PASshc/HomeBuyingCompanion/releases) page.

## Standard Installation

1. Run `HomeBuyingAppSetup_vX.X.X.exe`
2. Follow the on-screen wizard
3. Choose installation location (default: `C:\Program Files\Home Buying App`)
4. Optionally create a desktop shortcut
5. Click Install

## Silent Installation

For automated or enterprise deployments, the installer supports silent installation.

### Command-Line Switches

| Switch | Description |
|--------|-------------|
| `/SILENT` | Shows progress bar only, no wizard dialogs |
| `/VERYSILENT` | Completely silent installation, no UI |
| `/SUPPRESSMSGBOXES` | Suppresses all message boxes |
| `/NORESTART` | Prevents automatic restart after installation |
| `/CLOSEAPPLICATIONS` | Automatically closes running app instances |
| `/DIR="path"` | Override default installation directory |
| `/LOG="file"` | Creates installation log file |

### Examples

**Basic silent install:**
```cmd
HomeBuyingAppSetup_v7.4.3.exe /VERYSILENT /SUPPRESSMSGBOXES
```

**Silent install with custom directory:**
```cmd
HomeBuyingAppSetup_v7.4.3.exe /VERYSILENT /SUPPRESSMSGBOXES /DIR="C:\MyApps\HomeBuyingApp"
```

**Silent install with logging:**
```cmd
HomeBuyingAppSetup_v7.4.3.exe /VERYSILENT /SUPPRESSMSGBOXES /LOG="C:\Temp\install.log"
```

**Enterprise/Microsoft Store deployment:**
```cmd
HomeBuyingAppSetup_v7.4.3.exe /VERYSILENT /SUPPRESSMSGBOXES /NORESTART /CLOSEAPPLICATIONS
```

## Silent Uninstallation

```cmd
"C:\Program Files\Home Buying App\unins000.exe" /VERYSILENT /SUPPRESSMSGBOXES /NORESTART
```

## Exit Codes

The installer uses standard Inno Setup exit codes.

### Installation Exit Codes

| Exit Code | Description |
|-----------|-------------|
| 0 | Installation completed successfully |
| 1 | Setup failed to initialize |
| 2 | User clicked Cancel before installation started |
| 3 | Fatal error while preparing next installation phase |
| 4 | Fatal error occurred during installation |
| 5 | User clicked Cancel during installation (partial install) |
| 6 | Setup aborted after failed prepare |
| 7 | Setup aborted by a function |
| 8 | User clicked Cancel during prepare (partial prepare) |

### Uninstallation Exit Codes

| Exit Code | Description |
|-----------|-------------|
| 0 | Uninstallation completed successfully |
| 1 | Uninstall failed to initialize |
| 2 | User clicked Cancel |
| 5 | User clicked Cancel during uninstall (partial uninstall) |

For complete exit code documentation, see: [Inno Setup Exit Codes](https://jrsoftware.org/ishelp/index.php?topic=setupexitcodes)

## System Requirements

- **OS:** Windows 10 (64-bit) or later
- **Runtime:** Self-contained (.NET runtime included)
- **Disk Space:** ~150 MB
- **Privileges:** User-level (no admin required)

## Data Storage

The application stores data locally in:
```
%LocalAppData%\HomeBuyingApp\
```

This includes:
- SQLite database (`homebuying.db`)
- Property images
- Journal attachments
- Property attachments

## Troubleshooting

### Installation Issues

1. **Installer won't start:** Ensure you have write permissions to the installation directory
2. **Silent install hangs:** Check if the app is already running; use `/CLOSEAPPLICATIONS`
3. **Missing files after install:** Run installer with `/LOG` and check the log file

### Uninstallation

To completely remove the application:
1. Uninstall via Windows Settings > Apps or run the uninstaller
2. Optionally delete user data at `%LocalAppData%\HomeBuyingApp\`

## License

This software is licensed under the MIT License. See [LICENSE](LICENSE) for details.
