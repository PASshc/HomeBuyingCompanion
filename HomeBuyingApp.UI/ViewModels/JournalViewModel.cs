using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using HomeBuyingApp.Core.Models;
using HomeBuyingApp.Core.Services;
using Microsoft.Win32;

namespace HomeBuyingApp.UI.ViewModels
{
    /// <summary>
    /// ViewModel for the Journal feature - tracking the home buying journey.
    /// </summary>
    public class JournalViewModel : ViewModelBase
    {
        private readonly IJournalService _journalService;
        private readonly IPropertyService _propertyService;

        public JournalViewModel(IJournalService journalService, IPropertyService propertyService)
        {
            _journalService = journalService;
            _propertyService = propertyService;

            // Commands
            NewEntryCommand = new RelayCommand(_ => CreateNewEntry());
            SaveEntryCommand = new RelayCommand(async _ => await SaveEntryAsync(), _ => SelectedEntry != null);
            DeleteEntryCommand = new RelayCommand(async _ => await DeleteEntryAsync(), _ => SelectedEntry != null && SelectedEntry.Id > 0);
            CancelEditCommand = new RelayCommand(_ => CancelEdit());
            SearchCommand = new RelayCommand(async _ => await SearchAsync());
            ClearFilterCommand = new RelayCommand(async _ => await ClearFilterAsync());
            AddAttachmentCommand = new RelayCommand(async _ => await AddAttachmentAsync(), _ => SelectedEntry != null);
            DeleteAttachmentCommand = new RelayCommand(async param => await DeleteAttachmentAsync(param as JournalAttachment), param => param is JournalAttachment);
            OpenAttachmentCommand = new RelayCommand(param => OpenAttachment(param as JournalAttachment), param => param is JournalAttachment);

            // Load initial data
            _ = LoadEntriesAsync();
            _ = LoadPropertiesAsync();
        }

        #region Collections

        public ObservableCollection<JournalEntry> Entries { get; } = new ObservableCollection<JournalEntry>();
        public ObservableCollection<Property> AvailableProperties { get; } = new ObservableCollection<Property>();
        public ObservableCollection<JournalAttachment> Attachments { get; } = new ObservableCollection<JournalAttachment>();

        public IEnumerable<JournalCategory> Categories => Enum.GetValues<JournalCategory>();

        public bool HasEntries => Entries.Count > 0;

        public bool HasAttachments => Attachments.Count > 0;

        #endregion

        #region Properties

        private JournalEntry? _selectedEntry;
        public JournalEntry? SelectedEntry
        {
            get => _selectedEntry;
            set
            {
                _selectedEntry = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsEntrySelected));
                OnPropertyChanged(nameof(EditorTitle));
                LoadAttachmentsForSelectedEntry();
            }
        }

        public bool IsEntrySelected => SelectedEntry != null;

        public string EditorTitle => SelectedEntry == null ? "" : 
            (SelectedEntry.Id == 0 ? "New Entry" : "Edit Entry");

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { _searchText = value; OnPropertyChanged(); }
        }

        private JournalCategory? _filterCategory;
        public JournalCategory? FilterCategory
        {
            get => _filterCategory;
            set
            {
                _filterCategory = value;
                OnPropertyChanged();
                _ = ApplyFilterAsync();
            }
        }

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        #endregion

        #region Commands

        public ICommand NewEntryCommand { get; }
        public ICommand SaveEntryCommand { get; }
        public ICommand DeleteEntryCommand { get; }
        public ICommand CancelEditCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ClearFilterCommand { get; }
        public ICommand AddAttachmentCommand { get; }
        public ICommand DeleteAttachmentCommand { get; }
        public ICommand OpenAttachmentCommand { get; }

        #endregion

        #region Methods

        public async System.Threading.Tasks.Task LoadEntriesAsync()
        {
            try
            {
                IsLoading = true;
                var entries = await _journalService.GetAllEntriesAsync();
                
                Entries.Clear();
                foreach (var entry in entries)
                {
                    Entries.Add(entry);
                }
                OnPropertyChanged(nameof(HasEntries));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading journal entries: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async System.Threading.Tasks.Task LoadPropertiesAsync()
        {
            try
            {
                var properties = await _propertyService.GetAllPropertiesAsync();
                AvailableProperties.Clear();
                foreach (var property in properties)
                {
                    AvailableProperties.Add(property);
                }
            }
            catch { /* Ignore property loading errors */ }
        }

        private void CreateNewEntry()
        {
            SelectedEntry = new JournalEntry
            {
                EntryDate = DateTime.Today,
                Category = JournalCategory.Progress,
                Title = string.Empty,
                Content = string.Empty
            };
        }

        private async System.Threading.Tasks.Task SaveEntryAsync()
        {
            if (SelectedEntry == null) return;

            if (string.IsNullOrWhiteSpace(SelectedEntry.Title))
            {
                MessageBox.Show("Please enter a title for this entry.", "Validation", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                if (SelectedEntry.Id == 0)
                {
                    // Create new entry
                    var created = await _journalService.CreateEntryAsync(SelectedEntry);
                    Entries.Insert(0, created);
                    SelectedEntry = created;
                    OnPropertyChanged(nameof(HasEntries));
                }
                else
                {
                    // Update existing entry
                    var entryId = SelectedEntry.Id; // Save the ID before refresh
                    await _journalService.UpdateEntryAsync(SelectedEntry);
                    
                    // Refresh the list to show updated data
                    await LoadEntriesAsync();
                    
                    // Re-select the entry using the saved ID
                    SelectedEntry = Entries.FirstOrDefault(e => e.Id == entryId);
                }

                MessageBox.Show("Entry saved successfully.", "Success", 
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving entry: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task DeleteEntryAsync()
        {
            if (SelectedEntry == null || SelectedEntry.Id == 0) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete this entry?\n\n\"{SelectedEntry.Title}\"",
                "Delete Entry",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                await _journalService.DeleteEntryAsync(SelectedEntry.Id);
                Entries.Remove(SelectedEntry);
                SelectedEntry = null;
                OnPropertyChanged(nameof(HasEntries));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting entry: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelEdit()
        {
            SelectedEntry = null;
        }

        private async System.Threading.Tasks.Task SearchAsync()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                await LoadEntriesAsync();
                return;
            }

            try
            {
                IsLoading = true;
                var entries = await _journalService.SearchEntriesAsync(SearchText);
                
                Entries.Clear();
                foreach (var entry in entries)
                {
                    Entries.Add(entry);
                }
                OnPropertyChanged(nameof(HasEntries));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error searching entries: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async System.Threading.Tasks.Task ApplyFilterAsync()
        {
            try
            {
                IsLoading = true;
                IEnumerable<JournalEntry> entries;

                if (FilterCategory.HasValue)
                {
                    entries = await _journalService.GetEntriesByCategoryAsync(FilterCategory.Value);
                }
                else
                {
                    entries = await _journalService.GetAllEntriesAsync();
                }

                Entries.Clear();
                foreach (var entry in entries)
                {
                    Entries.Add(entry);
                }
                OnPropertyChanged(nameof(HasEntries));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error filtering entries: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async System.Threading.Tasks.Task ClearFilterAsync()
        {
            SearchText = string.Empty;
            FilterCategory = null;
            await LoadEntriesAsync();
        }

        private void LoadAttachmentsForSelectedEntry()
        {
            Attachments.Clear();
            
            if (SelectedEntry?.Attachments != null)
            {
                foreach (var attachment in SelectedEntry.Attachments.OrderByDescending(a => a.DateAdded))
                {
                    Attachments.Add(attachment);
                }
            }
            
            OnPropertyChanged(nameof(HasAttachments));
        }

        private async System.Threading.Tasks.Task AddAttachmentAsync()
        {
            if (SelectedEntry == null) return;

            // If this is a new entry, we need to save it first
            if (SelectedEntry.Id == 0)
            {
                if (string.IsNullOrWhiteSpace(SelectedEntry.Title))
                {
                    MessageBox.Show("Please enter a title and save the entry before adding attachments.", 
                        "Save Required", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                var result = MessageBox.Show(
                    "The entry must be saved before adding attachments. Save now?",
                    "Save Required",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                if (result != MessageBoxResult.Yes) return;

                await SaveEntryAsync();
                if (SelectedEntry.Id == 0) return; // Save failed
            }

            var openFileDialog = new OpenFileDialog
            {
                Title = "Select Attachment",
                Filter = "All Files (*.*)|*.*|Documents (*.pdf;*.doc;*.docx;*.xls;*.xlsx)|*.pdf;*.doc;*.docx;*.xls;*.xlsx|Images (*.png;*.jpg;*.jpeg;*.gif)|*.png;*.jpg;*.jpeg;*.gif",
                Multiselect = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    var basePath = !string.IsNullOrEmpty(App.AppDataPath) 
                        ? App.AppDataPath 
                        : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HomeBuyingApp");
                    var appDataPath = Path.Combine(basePath, "JournalAttachments");
                    
                    if (!Directory.Exists(appDataPath))
                    {
                        Directory.CreateDirectory(appDataPath);
                    }

                    foreach (var sourceFilePath in openFileDialog.FileNames)
                    {
                        var fileName = Path.GetFileName(sourceFilePath);
                        var extension = Path.GetExtension(sourceFilePath);
                        var fileInfo = new FileInfo(sourceFilePath);

                        var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                        var destFilePath = Path.Combine(appDataPath, uniqueFileName);

                        File.Copy(sourceFilePath, destFilePath);

                        var attachment = new JournalAttachment
                        {
                            JournalEntryId = SelectedEntry.Id,
                            FileName = fileName,
                            FilePath = destFilePath,
                            Description = fileName,
                            DateAdded = DateTime.Now,
                            FileSize = fileInfo.Length,
                            FileType = GetFileType(extension)
                        };

                        var savedAttachment = await _journalService.AddAttachmentAsync(attachment);
                        // Only add to the UI collection, not to SelectedEntry.Attachments
                        // to avoid EF tracking issues causing duplicates on save
                        Attachments.Insert(0, savedAttachment);
                    }

                    OnPropertyChanged(nameof(HasAttachments));
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to add attachment: {ex.Message}", "Error", 
                        MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async System.Threading.Tasks.Task DeleteAttachmentAsync(JournalAttachment? attachment)
        {
            if (attachment == null) return;

            var result = MessageBox.Show(
                $"Are you sure you want to delete '{attachment.FileName}'?",
                "Delete Attachment",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            try
            {
                // Delete from database
                await _journalService.DeleteAttachmentAsync(attachment.Id);

                // Try to delete the physical file
                if (File.Exists(attachment.FilePath))
                {
                    try { File.Delete(attachment.FilePath); } catch { /* Ignore file deletion errors */ }
                }

                // Remove from collections
                Attachments.Remove(attachment);
                SelectedEntry?.Attachments.Remove(attachment);
                OnPropertyChanged(nameof(HasAttachments));
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to delete attachment: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OpenAttachment(JournalAttachment? attachment)
        {
            if (attachment == null || string.IsNullOrEmpty(attachment.FilePath)) return;

            if (!File.Exists(attachment.FilePath))
            {
                MessageBox.Show("The attachment file could not be found.", "File Not Found", 
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            try
            {
                Process.Start(new ProcessStartInfo(attachment.FilePath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open file: {ex.Message}", "Error", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static string GetFileType(string extension)
        {
            return extension.ToLower() switch
            {
                ".pdf" => "application/pdf",
                ".doc" or ".docx" => "application/msword",
                ".xls" or ".xlsx" => "application/vnd.ms-excel",
                ".ppt" or ".pptx" => "application/vnd.ms-powerpoint",
                ".txt" => "text/plain",
                ".png" => "image/png",
                ".jpg" or ".jpeg" => "image/jpeg",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                _ => "application/octet-stream"
            };
        }

        #endregion
    }
}
