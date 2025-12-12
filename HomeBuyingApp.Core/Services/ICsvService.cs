using System.Collections.Generic;
using HomeBuyingApp.Core.Models;

namespace HomeBuyingApp.Core.Services
{
    public interface ICsvService
    {
        string GenerateCsv(IEnumerable<Property> properties);
        IEnumerable<Property> ParseCsv(string csvContent);
    }
}
