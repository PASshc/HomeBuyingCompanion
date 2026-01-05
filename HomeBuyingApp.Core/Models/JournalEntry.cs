using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HomeBuyingApp.Core.Models
{
    /// <summary>
    /// Represents a journal entry for tracking the home buying journey.
    /// </summary>
    public class JournalEntry
    {
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// The date of the journal entry.
        /// </summary>
        [Required]
        public DateTime EntryDate { get; set; } = DateTime.Now;

        /// <summary>
        /// The category of the entry (Progress, Lesson Learned, Mortgage Info, Decision, Research).
        /// </summary>
        [Required]
        public JournalCategory Category { get; set; } = JournalCategory.Progress;

        /// <summary>
        /// The title or summary of the entry.
        /// </summary>
        [Required]
        [MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        /// <summary>
        /// The full content of the journal entry (supports rich text as RTF).
        /// </summary>
        public string Content { get; set; } = string.Empty;

        /// <summary>
        /// Optional: Link to a specific property this entry relates to.
        /// </summary>
        public int? PropertyId { get; set; }

        /// <summary>
        /// Navigation property for the linked property.
        /// </summary>
        [ForeignKey(nameof(PropertyId))]
        public Property? Property { get; set; }

        /// <summary>
        /// When this entry was created.
        /// </summary>
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// When this entry was last modified.
        /// </summary>
        public DateTime ModifiedAt { get; set; } = DateTime.Now;

        /// <summary>
        /// Collection of attachments for this journal entry.
        /// </summary>
        public ICollection<JournalAttachment> Attachments { get; set; } = new List<JournalAttachment>();
    }

    /// <summary>
    /// Categories for journal entries.
    /// </summary>
    public enum JournalCategory
    {
        Progress,
        LessonLearned,
        MortgageInfo,
        Decision,
        Research,
        General
    }
}
