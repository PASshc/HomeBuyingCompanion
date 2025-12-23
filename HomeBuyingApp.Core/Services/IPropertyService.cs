using System.Collections.Generic;
using System.Threading.Tasks;
using HomeBuyingApp.Core.Models;

namespace HomeBuyingApp.Core.Services
{
    public interface IPropertyService
    {
        Task<List<Property>> GetAllPropertiesAsync();
        Task<Property?> GetPropertyByIdAsync(int id);
        Task AddPropertyAsync(Property property);
        Task UpdatePropertyAsync(Property property);
        Task DeletePropertyAsync(int id);
        Task<bool> PropertyExistsAsync(string address, string city, string state, string zipCode);
        Task AddAttachmentAsync(PropertyAttachment attachment);
        Task UpdateAttachmentAsync(PropertyAttachment attachment);
        Task DeleteAttachmentAsync(int attachmentId);
    }
}
