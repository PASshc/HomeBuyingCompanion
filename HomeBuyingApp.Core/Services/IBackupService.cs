using System.Threading.Tasks;

namespace HomeBuyingApp.Core.Services
{
    public interface IBackupService
    {
        Task CreateBackupAsync(string destinationPath);
        Task RestoreBackupAsync(string sourcePath);
    }
}
