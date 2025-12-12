$ErrorActionPreference = "Stop"

Write-Host "Cleaning previous builds..."
if (Test-Path "./Dist/HomeBuyingApp") {
    Remove-Item "./Dist/HomeBuyingApp" -Recurse -Force
}

Write-Host "Publishing HomeBuyingApp as a self-contained single file executable..."
# -c Release: Build in Release mode
# -r win-x64: Target Windows 64-bit
# --self-contained true: Include the .NET runtime so the user doesn't need to install it
# -p:PublishSingleFile=true: Bundle everything into one .exe
# -p:IncludeNativeLibrariesForSelfExtract=true: Include native libs in the bundle
dotnet publish "HomeBuyingApp.UI/HomeBuyingApp.UI.csproj" -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishReadyToRun=true -o "./Dist/HomeBuyingApp"

Write-Host "----------------------------------------------------------------"
Write-Host "Packaging complete!"
if (Test-Path "./Dist/HomeBuyingApp/HomeBuyingApp.UI.exe") {
    Write-Host "You can find the executable here: $(Resolve-Path ./Dist/HomeBuyingApp/HomeBuyingApp.UI.exe)"
} else {
    Write-Host "Executable not found in expected location."
}
Write-Host "You can zip the contents of './Dist/HomeBuyingApp' and share it."
Write-Host "----------------------------------------------------------------"
