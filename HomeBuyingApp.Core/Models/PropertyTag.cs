using System.Collections.Generic;

namespace HomeBuyingApp.Core.Models
{
    public class PropertyTag
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public TagType Type { get; set; }
        public bool IsCustom { get; set; }

        // Many-to-many relationship
        public List<Property> Properties { get; set; } = new List<Property>();
    }
}
