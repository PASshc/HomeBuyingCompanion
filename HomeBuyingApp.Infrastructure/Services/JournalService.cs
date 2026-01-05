using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using HomeBuyingApp.Core.Models;
using HomeBuyingApp.Core.Services;
using HomeBuyingApp.Infrastructure.Data;

namespace HomeBuyingApp.Infrastructure.Services
{
    /// <summary>
    /// Service implementation for managing journal entries.
    /// </summary>
    public class JournalService : IJournalService
    {
        private readonly AppDbContext _context;

        public JournalService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<JournalEntry>> GetAllEntriesAsync()
        {
            return await _context.JournalEntries
                .Include(j => j.Property)
                .Include(j => j.Attachments)
                .OrderByDescending(j => j.EntryDate)
                .ThenByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<JournalEntry>> GetEntriesByCategoryAsync(JournalCategory category)
        {
            return await _context.JournalEntries
                .Include(j => j.Property)
                .Include(j => j.Attachments)
                .Where(j => j.Category == category)
                .OrderByDescending(j => j.EntryDate)
                .ThenByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<JournalEntry>> GetEntriesByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.JournalEntries
                .Include(j => j.Property)
                .Include(j => j.Attachments)
                .Where(j => j.EntryDate >= startDate && j.EntryDate <= endDate)
                .OrderByDescending(j => j.EntryDate)
                .ThenByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<JournalEntry>> GetEntriesForPropertyAsync(int propertyId)
        {
            return await _context.JournalEntries
                .Include(j => j.Property)
                .Include(j => j.Attachments)
                .Where(j => j.PropertyId == propertyId)
                .OrderByDescending(j => j.EntryDate)
                .ThenByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task<JournalEntry?> GetEntryByIdAsync(int id)
        {
            return await _context.JournalEntries
                .Include(j => j.Property)
                .Include(j => j.Attachments)
                .FirstOrDefaultAsync(j => j.Id == id);
        }

        public async Task<JournalEntry> CreateEntryAsync(JournalEntry entry)
        {
            entry.CreatedAt = DateTime.Now;
            entry.ModifiedAt = DateTime.Now;
            
            _context.JournalEntries.Add(entry);
            await _context.SaveChangesAsync();
            
            return entry;
        }

        public async Task UpdateEntryAsync(JournalEntry entry)
        {
            var existing = await _context.JournalEntries.FindAsync(entry.Id);
            if (existing != null)
            {
                existing.EntryDate = entry.EntryDate;
                existing.Category = entry.Category;
                existing.Title = entry.Title;
                existing.Content = entry.Content;
                existing.PropertyId = entry.PropertyId;
                existing.ModifiedAt = DateTime.Now;
                
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteEntryAsync(int id)
        {
            var entry = await _context.JournalEntries.FindAsync(id);
            if (entry != null)
            {
                _context.JournalEntries.Remove(entry);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<JournalEntry>> SearchEntriesAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return await GetAllEntriesAsync();

            var term = searchTerm.ToLower();
            return await _context.JournalEntries
                .Include(j => j.Property)
                .Include(j => j.Attachments)
                .Where(j => j.Title.ToLower().Contains(term) || 
                           j.Content.ToLower().Contains(term))
                .OrderByDescending(j => j.EntryDate)
                .ThenByDescending(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task<JournalAttachment> AddAttachmentAsync(JournalAttachment attachment)
        {
            _context.JournalAttachments.Add(attachment);
            await _context.SaveChangesAsync();
            return attachment;
        }

        public async Task DeleteAttachmentAsync(int attachmentId)
        {
            var attachment = await _context.JournalAttachments.FindAsync(attachmentId);
            if (attachment != null)
            {
                _context.JournalAttachments.Remove(attachment);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<JournalAttachment>> GetAttachmentsForEntryAsync(int journalEntryId)
        {
            return await _context.JournalAttachments
                .Where(a => a.JournalEntryId == journalEntryId)
                .OrderByDescending(a => a.DateAdded)
                .ToListAsync();
        }
    }
}
