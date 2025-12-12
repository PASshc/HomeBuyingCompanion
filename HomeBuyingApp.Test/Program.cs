using System;
using System.Threading.Tasks;
using HomeBuyingApp.Core.Models;
using HomeBuyingApp.Infrastructure.Services;

namespace HomeBuyingApp.Test
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("Testing WebScraperService...");
            var scraper = new WebScraperService();
            
            // Use command line argument if provided, otherwise default
            string url = args.Length > 0 ? args[0] : "https://www.zillow.com/homedetails/1101-Abagail-Dr-Deltona-FL-32725/47967255_zpid/";
            
            Console.WriteLine($"Scraping URL: {url}");
            var property = await scraper.ScrapePropertyDetailsAsync(url);
            
            Console.WriteLine("Scraping Result:");
            Console.WriteLine($"Address: {property.Address}");
            Console.WriteLine($"City: {property.City}");
            Console.WriteLine($"State: {property.State}");
            Console.WriteLine($"Zip: {property.ZipCode}");
            Console.WriteLine($"Price: {property.ListPrice:C}");
            Console.WriteLine($"Beds: {property.Bedrooms}");
            Console.WriteLine($"Baths: {property.Bathrooms}");
            Console.WriteLine($"SqFt: {property.SquareFeet}");
        }
    }
}
