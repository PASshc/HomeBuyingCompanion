using System.Collections.Generic;
using System.Threading.Tasks;
using HomeBuyingApp.Core.Models;

namespace HomeBuyingApp.Core.Services
{
    public interface ITagService
    {
        Task<List<PropertyTag>> GetAllTagsAsync();
        Task<List<PropertyTag>> GetTagsByTypeAsync(TagType type);
        Task<PropertyTag> CreateTagAsync(string name, TagType type, string category = "", bool isCustom = true);
        Task DeleteTagAsync(int tagId);
        Task AddTagToPropertyAsync(int propertyId, int tagId);
        Task RemoveTagFromPropertyAsync(int propertyId, int tagId);
        Task<List<PropertyTag>> GetTagsForPropertyAsync(int propertyId);
    }
}
