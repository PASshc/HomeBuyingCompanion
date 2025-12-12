using System;

namespace HomeBuyingApp.Core.Models
{
    public class PropertyAttachment
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public Property Property { get; set; }

        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty; // Relative or absolute path
        public string Description { get; set; } = string.Empty;
        public DateTime DateAdded { get; set; } = DateTime.Now;
    }
}
