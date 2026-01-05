using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeBuyingApp.Core.Models
{
    /// <summary>
    /// Represents an attachment for a journal entry (e.g., pre-approvals, documents, images).
    /// </summary>
    public class JournalAttachment
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// The ID of the journal entry this attachment belongs to.
        /// </summary>
        public int JournalEntryId { get; set; }

        /// <summary>
        /// Navigation property for the linked journal entry.
        /// </summary>
        [ForeignKey(nameof(JournalEntryId))]
        public JournalEntry? JournalEntry { get; set; }

        /// <summary>
        /// The original file name of the attachment.
        /// </summary>
        [Required]
        [MaxLength(255)]
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// The path where the file is stored (relative or absolute).
        /// </summary>
        [Required]
        public string FilePath { get; set; } = string.Empty;

        /// <summary>
        /// Optional description of the attachment.
        /// </summary>
        [MaxLength(500)]
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// The date when the attachment was added.
        /// </summary>
        public DateTime DateAdded { get; set; } = DateTime.Now;

        /// <summary>
        /// The file size in bytes.
        /// </summary>
        public long FileSize { get; set; }

        /// <summary>
        /// The MIME type or file type (e.g., "application/pdf", "image/jpeg").
        /// </summary>
        [MaxLength(100)]
        public string FileType { get; set; } = string.Empty;
    }
}
