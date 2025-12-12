# HomeBuyingApp Distribution

This project is set up to be published as a self-contained single-file executable for Windows.

## How to Build the Installer/Executable

1. Open a terminal in the solution root (`d:\dev\home-buying-app`).
2. Run the `publish_app.ps1` script:
   ```powershell
   .\publish_app.ps1
   ```

## Output

The build artifacts will be placed in the `Dist/HomeBuyingApp` folder.

- **HomeBuyingApp.UI.exe**: This is the main application. You can copy this file (and any other files in that folder) to any Windows computer and run it. No .NET installation is required on the target machine.

## Creating a Setup Installer (Optional)

If you want a traditional `setup.exe` installer:

1. Download and install [Inno Setup](https://jrsoftware.org/isdl.php).
2. Run `publish.bat` to generate the application files.
3. Open `setup.iss` (located in the root folder) with Inno Setup.
4. Click **Build > Compile**.
5. The installer will be created in `Dist/Installer/HomeBuyingAppSetup.exe`.

## Troubleshooting

If the script fails, ensure you have the .NET SDK installed and that you can run `dotnet build` successfully.
