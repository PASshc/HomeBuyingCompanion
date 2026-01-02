# Verify Backup Contents Script
param(
    [Parameter(Mandatory=$true)]
    [string]$BackupPath
)

if (!(Test-Path $BackupPath)) {
    Write-Host "ERROR: Backup file not found: $BackupPath" -ForegroundColor Red
    exit 1
}

Write-Host "`nAnalyzing backup: $BackupPath" -ForegroundColor Cyan
Write-Host "=" * 60

try {
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $zip = [System.IO.Compression.ZipFile]::OpenRead($BackupPath)
    
    $dbFiles = $zip.Entries | Where-Object { $_.FullName -like "*.db*" }
    $imageFiles = $zip.Entries | Where-Object { $_.FullName -like "Images/*" }
    $attachmentFiles = $zip.Entries | Where-Object { $_.FullName -like "Attachments/*" }
    
    Write-Host "`nDatabase Files:" -ForegroundColor Yellow
    if ($dbFiles) {
        $dbFiles | ForEach-Object { Write-Host "  - $($_.FullName) ($([math]::Round($_.Length/1KB, 2)) KB)" }
    } else {
        Write-Host "  NONE FOUND!" -ForegroundColor Red
    }
    
    Write-Host "`nImage Files:" -ForegroundColor Yellow
    if ($imageFiles) {
        Write-Host "  Total: $($imageFiles.Count) images"
        $imageFiles | Select-Object -First 10 | ForEach-Object { 
            Write-Host "  - $($_.FullName) ($([math]::Round($_.Length/1KB, 2)) KB)" 
        }
        if ($imageFiles.Count > 10) {
            Write-Host "  ... and $($imageFiles.Count - 10) more" -ForegroundColor Gray
        }
    } else {
        Write-Host "  NONE FOUND!" -ForegroundColor Red
    }
    
    Write-Host "`nAttachment Files:" -ForegroundColor Yellow
    if ($attachmentFiles) {
        Write-Host "  Total: $($attachmentFiles.Count) attachments"
        $attachmentFiles | ForEach-Object { 
            Write-Host "  - $($_.FullName) ($([math]::Round($_.Length/1KB, 2)) KB)" 
        }
    } else {
        Write-Host "  None found (this is OK if you don't have attachments)" -ForegroundColor Gray
    }
    
    Write-Host "`nAll entries in backup:" -ForegroundColor Yellow
    $zip.Entries | ForEach-Object { Write-Host "  - $($_.FullName)" }
    
    $zip.Dispose()
    
    Write-Host "`n" + ("=" * 60)
    Write-Host "SUMMARY:" -ForegroundColor Cyan
    Write-Host "  Database: $(if ($dbFiles) { 'YES' } else { 'NO' }) " -ForegroundColor $(if ($dbFiles) { 'Green' } else { 'Red' })
    Write-Host "  Images: $(if ($imageFiles) { "$($imageFiles.Count) files" } else { 'NONE' })" -ForegroundColor $(if ($imageFiles) { 'Green' } else { 'Red' })
    Write-Host "  Attachments: $(if ($attachmentFiles) { "$($attachmentFiles.Count) files" } else { 'None' })" -ForegroundColor $(if ($attachmentFiles) { 'Gray' } else { 'Gray' })
    
} catch {
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
}
