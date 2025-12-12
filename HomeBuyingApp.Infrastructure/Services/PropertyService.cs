using System.Collections.Generic;
using System.Threading.Tasks;
using HomeBuyingApp.Core.Models;
using HomeBuyingApp.Core.Services;
using HomeBuyingApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HomeBuyingApp.Infrastructure.Services
{
    public class PropertyService : IPropertyService
    {
        private readonly AppDbContext _context;

        public PropertyService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<Property>> GetAllPropertiesAsync()
        {
            return await _context.Properties.ToListAsync();
        }

        public async Task<Property?> GetPropertyByIdAsync(int id)
        {
            return await _context.Properties
                .Include(p => p.Attachments)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddPropertyAsync(Property property)
        {
            _context.Properties.Add(property);
            await _context.SaveChangesAsync();
        }

        public async Task UpdatePropertyAsync(Property property)
        {
            _context.Properties.Update(property);
            await _context.SaveChangesAsync();
        }

        public async Task DeletePropertyAsync(int id)
        {
            var property = await _context.Properties.FindAsync(id);
            if (property != null)
            {
                _context.Properties.Remove(property);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> PropertyExistsAsync(string address, string city, string state, string zipCode)
        {
            return await _context.Properties.AnyAsync(p => 
                p.Address.ToLower() == address.ToLower() &&
                p.City.ToLower() == city.ToLower() &&
                p.State.ToLower() == state.ToLower() &&
                p.ZipCode == zipCode);
        }

        public async Task AddAttachmentAsync(PropertyAttachment attachment)
        {
            _context.Attachments.Add(attachment);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAttachmentAsync(int attachmentId)
        {
            var attachment = await _context.Attachments.FindAsync(attachmentId);
            if (attachment != null)
            {
                _context.Attachments.Remove(attachment);
                await _context.SaveChangesAsync();
            }
        }
    }
}
