using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using HomeBuyingApp.Core.Models;

namespace HomeBuyingApp.Core.Services
{
    /// <summary>
    /// Service interface for managing journal entries.
    /// </summary>
    public interface IJournalService
    {
        /// <summary>
        /// Gets all journal entries ordered by date descending.
        /// </summary>
        Task<IEnumerable<JournalEntry>> GetAllEntriesAsync();

        /// <summary>
        /// Gets journal entries filtered by category.
        /// </summary>
        Task<IEnumerable<JournalEntry>> GetEntriesByCategoryAsync(JournalCategory category);

        /// <summary>
        /// Gets journal entries for a specific date range.
        /// </summary>
        Task<IEnumerable<JournalEntry>> GetEntriesByDateRangeAsync(DateTime startDate, DateTime endDate);

        /// <summary>
        /// Gets journal entries linked to a specific property.
        /// </summary>
        Task<IEnumerable<JournalEntry>> GetEntriesForPropertyAsync(int propertyId);

        /// <summary>
        /// Gets a single journal entry by ID.
        /// </summary>
        Task<JournalEntry?> GetEntryByIdAsync(int id);

        /// <summary>
        /// Creates a new journal entry.
        /// </summary>
        Task<JournalEntry> CreateEntryAsync(JournalEntry entry);

        /// <summary>
        /// Updates an existing journal entry.
        /// </summary>
        Task UpdateEntryAsync(JournalEntry entry);

        /// <summary>
        /// Deletes a journal entry.
        /// </summary>
        Task DeleteEntryAsync(int id);

        /// <summary>
        /// Searches journal entries by title or content.
        /// </summary>
        Task<IEnumerable<JournalEntry>> SearchEntriesAsync(string searchTerm);

        /// <summary>
        /// Adds an attachment to a journal entry.
        /// </summary>
        Task<JournalAttachment> AddAttachmentAsync(JournalAttachment attachment);

        /// <summary>
        /// Removes an attachment from a journal entry.
        /// </summary>
        Task DeleteAttachmentAsync(int attachmentId);

        /// <summary>
        /// Gets all attachments for a journal entry.
        /// </summary>
        Task<IEnumerable<JournalAttachment>> GetAttachmentsForEntryAsync(int journalEntryId);
    }
}
