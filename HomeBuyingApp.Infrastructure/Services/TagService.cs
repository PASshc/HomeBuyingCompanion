using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HomeBuyingApp.Core.Models;
using HomeBuyingApp.Core.Services;
using HomeBuyingApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HomeBuyingApp.Infrastructure.Services
{
    public class TagService : ITagService
    {
        private readonly AppDbContext _context;

        public TagService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<PropertyTag>> GetAllTagsAsync()
        {
            return await _context.PropertyTags
                .OrderBy(t => t.Type)
                .ThenBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<List<PropertyTag>> GetTagsByTypeAsync(TagType type)
        {
            return await _context.PropertyTags
                .Where(t => t.Type == type)
                .OrderBy(t => t.Name)
                .ToListAsync();
        }

        public async Task<PropertyTag> CreateTagAsync(string name, TagType type, bool isCustom = true)
        {
            // Check if tag with same name exists
            var existing = await _context.PropertyTags
                .FirstOrDefaultAsync(t => t.Name.ToLower() == name.ToLower());

            if (existing != null)
                return existing;

            var tag = new PropertyTag
            {
                Name = name.Trim(),
                Type = type,
                IsCustom = isCustom
            };

            _context.PropertyTags.Add(tag);
            await _context.SaveChangesAsync();
            return tag;
        }

        public async Task DeleteTagAsync(int tagId)
        {
            var tag = await _context.PropertyTags.FindAsync(tagId);
            if (tag != null)
            {
                _context.PropertyTags.Remove(tag);
                await _context.SaveChangesAsync();
            }
        }

        public async Task AddTagToPropertyAsync(int propertyId, int tagId)
        {
            var property = await _context.Properties
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.Id == propertyId);

            var tag = await _context.PropertyTags.FindAsync(tagId);

            if (property != null && tag != null && !property.Tags.Any(t => t.Id == tagId))
            {
                property.Tags.Add(tag);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveTagFromPropertyAsync(int propertyId, int tagId)
        {
            var property = await _context.Properties
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.Id == propertyId);

            if (property != null)
            {
                var tag = property.Tags.FirstOrDefault(t => t.Id == tagId);
                if (tag != null)
                {
                    property.Tags.Remove(tag);
                    await _context.SaveChangesAsync();
                }
            }
        }

        public async Task<List<PropertyTag>> GetTagsForPropertyAsync(int propertyId)
        {
            var property = await _context.Properties
                .Include(p => p.Tags)
                .FirstOrDefaultAsync(p => p.Id == propertyId);

            return property?.Tags?.OrderBy(t => t.Type).ThenBy(t => t.Name).ToList() 
                   ?? new List<PropertyTag>();
        }
    }
}
