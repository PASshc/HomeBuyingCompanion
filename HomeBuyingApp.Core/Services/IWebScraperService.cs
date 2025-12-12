using System.Threading.Tasks;
using HomeBuyingApp.Core.Models;

namespace HomeBuyingApp.Core.Services
{
    public interface IWebScraperService
    {
        Task<Property> ScrapePropertyDetailsAsync(string url);
    }
}
