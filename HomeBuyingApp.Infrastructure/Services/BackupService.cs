using HomeBuyingApp.Core.Services;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System;
using System.Data.Common;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace HomeBuyingApp.Infrastructure.Services
{
    public class BackupService : IBackupService
    {
        private readonly string _dbPath;
        private readonly string _attachmentsPath;
        private readonly string _imagesPath;

        public BackupService(string dbPath, string attachmentsPath, string imagesPath)
        {
            _dbPath = dbPath;
            _attachmentsPath = attachmentsPath;
            _imagesPath = imagesPath;
        }

        public async Task CreateBackupAsync(string destinationPath)
        {
            await Task.Run(() =>
            {
                var tempDir = Path.Combine(Path.GetTempPath(), "HomeBuyingAppBackup_" + Guid.NewGuid());
                Directory.CreateDirectory(tempDir);

                try
                {
                    // 1. Copy Database
                    // Use SqliteConnection to backup safely if possible, or just file copy (WAL mode might have .shm/.wal files)
                    // For simplicity, we'll try file copy. If it fails, we might need to vacuum into a new file.
                    // But standard file copy usually works for SQLite if not in middle of write.
                    
                    if (File.Exists(_dbPath))
                    {
                        // Check for WAL files
                        File.Copy(_dbPath, Path.Combine(tempDir, Path.GetFileName(_dbPath)));
                        if (File.Exists(_dbPath + "-wal")) File.Copy(_dbPath + "-wal", Path.Combine(tempDir, Path.GetFileName(_dbPath) + "-wal"));
                        if (File.Exists(_dbPath + "-shm")) File.Copy(_dbPath + "-shm", Path.Combine(tempDir, Path.GetFileName(_dbPath) + "-shm"));
                    }

                    // 2. Copy Attachments
                    var destAttachmentsPath = Path.Combine(tempDir, "Attachments");
                    
                    if (Directory.Exists(_attachmentsPath))
                    {
                        CopyDirectory(_attachmentsPath, destAttachmentsPath, true);
                    }

                    // 3. Copy Images
                    var destImagesPath = Path.Combine(tempDir, "Images");
                    
                    if (Directory.Exists(_imagesPath))
                    {
                        CopyDirectory(_imagesPath, destImagesPath, true);
                    }

                    // 4. Zip it all
                    if (File.Exists(destinationPath)) File.Delete(destinationPath);
                    ZipFile.CreateFromDirectory(tempDir, destinationPath);
                }
                finally
                {
                    if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
                }
            });
        }

        public async Task RestoreBackupAsync(string sourcePath)
        {
            await Task.Run(() =>
            {
                var tempDir = Path.Combine(Path.GetTempPath(), "HomeBuyingAppRestore_" + Guid.NewGuid());
                Directory.CreateDirectory(tempDir);

                try
                {
                    ZipFile.ExtractToDirectory(sourcePath, tempDir);

                    // 1. Restore Database
                    // We assume the zip contains the db file at root
                    var dbName = Path.GetFileName(_dbPath);
                    var dbSource = Path.Combine(tempDir, dbName);
                    
                    if (File.Exists(dbSource))
                    {
                        // Close all SQLite connections for this database
                        SqliteConnection.ClearAllPools();
                        
                        // Force garbage collection to release file handles
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        GC.Collect();
                        
                        // Small delay to ensure file locks are released
                        System.Threading.Thread.Sleep(100);
                        
                        // Backup current database
                        var backup = _dbPath + ".bak";
                        if (File.Exists(backup)) File.Delete(backup);
                        if (File.Exists(_dbPath)) File.Move(_dbPath, backup);

                        // Delete WAL/SHM files if they exist
                        var walPath = _dbPath + "-wal";
                        var shmPath = _dbPath + "-shm";
                        if (File.Exists(walPath)) File.Delete(walPath);
                        if (File.Exists(shmPath)) File.Delete(shmPath);

                        // Copy restored database
                        File.Copy(dbSource, _dbPath);
                        
                        // Restore WAL/SHM if they exist in backup
                        var walSource = dbSource + "-wal";
                        var shmSource = dbSource + "-shm";
                        if (File.Exists(walSource)) File.Copy(walSource, walPath, true);
                        if (File.Exists(shmSource)) File.Copy(shmSource, shmPath, true);
                    }
                    else
                    {
                        throw new FileNotFoundException($"Database file not found in backup: {dbName}");
                    }

                    // 2. Restore Attachments
                    var attachmentsSource = Path.Combine(tempDir, "Attachments");
                    
                    if (Directory.Exists(attachmentsSource))
                    {
                        if (!Directory.Exists(_attachmentsPath)) Directory.CreateDirectory(_attachmentsPath);
                        
                        CopyDirectory(attachmentsSource, _attachmentsPath, true);
                    }

                    // 3. Restore Images
                    var imagesSource = Path.Combine(tempDir, "Images");
                    
                    if (Directory.Exists(imagesSource))
                    {
                        if (!Directory.Exists(_imagesPath)) Directory.CreateDirectory(_imagesPath);
                        
                        CopyDirectory(imagesSource, _imagesPath, true);
                    }
                }
                finally
                {
                    if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
                }
            });
        }

        private static void CopyDirectory(string sourceDir, string destinationDir, bool recursive)
        {
            var dir = new DirectoryInfo(sourceDir);
            if (!dir.Exists) throw new DirectoryNotFoundException($"Source directory not found: {dir.FullName}");

            DirectoryInfo[] dirs = dir.GetDirectories();
            Directory.CreateDirectory(destinationDir);

            foreach (FileInfo file in dir.GetFiles())
            {
                string targetFilePath = Path.Combine(destinationDir, file.Name);
                file.CopyTo(targetFilePath, true);
            }

            if (recursive)
            {
                foreach (DirectoryInfo subDir in dirs)
                {
                    string newDestinationDir = Path.Combine(destinationDir, subDir.Name);
                    CopyDirectory(subDir.FullName, newDestinationDir, true);
                }
            }
        }
    }
}
