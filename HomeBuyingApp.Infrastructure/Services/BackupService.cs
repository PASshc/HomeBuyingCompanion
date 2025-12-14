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

        public BackupService(string dbPath, string attachmentsPath)
        {
            _dbPath = dbPath;
            _attachmentsPath = attachmentsPath;
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

                    // 3. Zip it all
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
                        // Close connections and clear pools
                        GC.Collect();
                        GC.WaitForPendingFinalizers();
                        
                        // Backup current
                        var backup = _dbPath + ".bak";
                        if (File.Exists(backup)) File.Delete(backup);
                        if (File.Exists(_dbPath)) File.Move(_dbPath, backup);

                        // Delete WAL/SHM files if they exist
                        if (File.Exists(_dbPath + "-wal")) File.Delete(_dbPath + "-wal");
                        if (File.Exists(_dbPath + "-shm")) File.Delete(_dbPath + "-shm");

                        File.Copy(dbSource, _dbPath);
                        
                        // Restore WAL/SHM if they exist in backup
                        if (File.Exists(dbSource + "-wal")) File.Copy(dbSource + "-wal", _dbPath + "-wal", true);
                        if (File.Exists(dbSource + "-shm")) File.Copy(dbSource + "-shm", _dbPath + "-shm", true);
                    }

                    // 2. Restore Attachments
                    var attachmentsSource = Path.Combine(tempDir, "Attachments");
                    
                    if (Directory.Exists(attachmentsSource))
                    {
                        if (!Directory.Exists(_attachmentsPath)) Directory.CreateDirectory(_attachmentsPath);
                        
                        CopyDirectory(attachmentsSource, _attachmentsPath, true);
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
