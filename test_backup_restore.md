# Backup/Restore Test Guide

## Changes Made (v5.5.1)

### BackupService.cs
1. Added `SqliteConnection.ClearAllPools()` before restore to properly close all database connections
2. Added multiple GC cycles with delays to ensure file locks are released
3. Added better error handling with `FileNotFoundException` for missing database in backup
4. Added explicit delays to ensure file system has time to release locks

### MainWindow.xaml.cs
1. Properly dispose DbContext before restore operation
2. Clear SQLite connection pools at UI level
3. Auto-restart application after restore (instead of requiring manual restart)
4. Enhanced error messages with inner exception details

### App.xaml.cs
1. Exposed `ServiceProvider` property for accessing services from MainWindow

## Testing Steps

### Test 1: Backup Creation
1. Open the app (Debug or Release)
2. Add a few test properties with data
3. Go to File → Backup Database
4. Save backup file (note the location)
5. Verify the ZIP file was created

### Test 2: Restore on Same Machine
1. Open the app
2. Delete or modify some properties
3. Go to File → Restore Database
4. Select the backup file
5. App should restart automatically
6. Verify all original data is restored

### Test 3: Restore on Different Machine (YOUR ISSUE)
1. Copy the backup ZIP file to a USB drive or cloud storage
2. Install the app on another PC
3. Launch the app (it will create empty database)
4. Go to File → Restore Database
5. Select the backup ZIP file
6. App should restart automatically
7. **All your data should now be visible**

## What Was Wrong

The original issue was:
- SQLite connection pools weren't being properly cleared
- Database file locks weren't released before restore
- The app wasn't forcing a proper restart with fresh DB connections

## Verification

After restore, check:
- [ ] All properties are visible
- [ ] Images are loaded correctly
- [ ] Attachments are accessible
- [ ] Mortgage calculations are preserved
- [ ] Tags and ratings are correct

## Troubleshooting

If restore still fails:
1. Check the error message details
2. Verify the backup ZIP contains:
   - `homebuying.db` file
   - `Attachments` folder (if you had attachments)
   - `Images` folder (if you had images)
3. Check Windows Event Viewer for file access errors
4. Ensure you have write permissions to `%LocalApplicationData%\HomeBuyingApp`
