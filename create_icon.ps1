Add-Type -AssemblyName System.Drawing

$pngPath = "d:\dev\home-buying-app\HomeBuyingApp.UI\Resources\icon.png"
$icoPath = "d:\dev\home-buying-app\HomeBuyingApp.UI\Resources\app.ico"

if (-not (Test-Path $pngPath)) {
    Write-Error "Could not find the image! Please save your image to: $pngPath"
    exit 1
}

try {
    # Load the PNG
    $bitmap = [System.Drawing.Bitmap]::FromFile($pngPath)
    
    # Convert to Icon
    $icon = [System.Drawing.Icon]::FromHandle($bitmap.GetHicon())
    
    # Save to file
    $fileStream = [System.IO.File]::OpenWrite($icoPath)
    $icon.Save($fileStream)
    $fileStream.Close()
    
    # Cleanup
    $icon.Dispose()
    $bitmap.Dispose()

    Write-Host "Successfully converted icon.png to app.ico"
}
catch {
    Write-Error "Failed to convert icon: $_"
    exit 1
}
