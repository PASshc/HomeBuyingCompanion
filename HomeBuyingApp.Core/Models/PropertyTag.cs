using System.Collections.Generic;

namespace HomeBuyingApp.Core.Models
{
    public class PropertyTag
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Category { get; set; } = string.Empty;
        public TagType Type { get; set; }
        public bool IsCustom { get; set; }

        // Many-to-many relationship
        public List<Property> Properties { get; set; } = new List<Property>();

        /// <summary>
        /// Returns the display name with category prefix if category is set
        /// </summary>
        public string DisplayName => string.IsNullOrEmpty(Category) ? Name : $"{Category}: {Name}";
    }

    /// <summary>
    /// Predefined categories for organizing tags
    /// </summary>
    public static class TagCategories
    {
        public static readonly string[] All = new[]
        {
            "",           // No category (for backward compatibility)
            "Kitchen",
            "Bathroom",
            "Bedroom",
            "Living",
            "Exterior",
            "Yard",
            "Garage",
            "Roof",
            "HVAC",
            "Plumbing",
            "Electrical",
            "Flooring",
            "Location",
            "HOA",
            "Schools",
            "Other"
        };
    }
}
