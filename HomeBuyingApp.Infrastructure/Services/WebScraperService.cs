using System;
using System.Threading.Tasks;
using HomeBuyingApp.Core.Models;
using HomeBuyingApp.Core.Services;

namespace HomeBuyingApp.Infrastructure.Services
{
    public class WebScraperService : IWebScraperService
    {
        public async Task<Property> ScrapePropertyDetailsAsync(string url)
        {
            await Task.CompletedTask;
            
            return new Property 
            { 
                ListingUrl = url,
                Comments = "Auto-fill is not available. Please enter property details manually."
            };
        }
    }
}
