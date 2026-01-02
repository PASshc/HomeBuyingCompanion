# Script to create 1080x1080 PNG logo
param(
    [string]$InputImage = "temp_logo.png",
    [string]$OutputImage = "HomeBuyingApp.UI\Resources\app_logo_1080x1080.png"
)

Add-Type -AssemblyName System.Drawing

# Load the source image
$sourceBitmap = [System.Drawing.Image]::FromFile((Resolve-Path $InputImage))

# Create new bitmap with exact dimensions
$targetBitmap = New-Object System.Drawing.Bitmap(1080, 1080)
$graphics = [System.Drawing.Graphics]::FromImage($targetBitmap)

# Set high quality rendering
$graphics.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
$graphics.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::HighQuality
$graphics.PixelOffsetMode = [System.Drawing.Drawing2D.PixelOffsetMode]::HighQuality
$graphics.CompositingQuality = [System.Drawing.Drawing2D.CompositingQuality]::HighQuality

# Draw the image scaled to 1080x1080
$graphics.DrawImage($sourceBitmap, 0, 0, 1080, 1080)

# Save as PNG
$targetBitmap.Save($OutputImage, [System.Drawing.Imaging.ImageFormat]::Png)

# Cleanup
$graphics.Dispose()
$targetBitmap.Dispose()
$sourceBitmap.Dispose()

Write-Host "Logo created successfully at: $OutputImage"
Write-Host "Dimensions: 1080x1080 pixels"

# Verify the output
$verifyImage = [System.Drawing.Image]::FromFile((Resolve-Path $OutputImage))
Write-Host "Verified - Width: $($verifyImage.Width), Height: $($verifyImage.Height)"
$verifyImage.Dispose()
