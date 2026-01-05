using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using HomeBuyingApp.Core.Models;
using HomeBuyingApp.Core.Services;

namespace HomeBuyingApp.UI.ViewModels
{
    /// <summary>
    /// ViewModel responsible for managing property PRO and CON tags.
    /// Extracted from PropertyDetailViewModel for better separation of concerns.
    /// </summary>
    public class PropertyTagsViewModel : ViewModelBase
    {
        private readonly ITagService _tagService;
        private readonly PropertyViewModel _property;

        public PropertyTagsViewModel(ITagService tagService, PropertyViewModel property)
        {
            _tagService = tagService;
            _property = property;

            // Initialize category collections
            foreach (var category in TagCategories.All)
            {
                Categories.Add(category);
            }

            // Tag commands - separate for PROs and CONs
            AddProTagCommand = new RelayCommand(async _ => await AddTagToPropertyAsync(SelectedProTag, TagType.Pro), _ => SelectedProTag != null);
            AddConTagCommand = new RelayCommand(async _ => await AddTagToPropertyAsync(SelectedConTag, TagType.Con), _ => SelectedConTag != null);
            RemoveProTagCommand = new RelayCommand(async param => { if (param is PropertyTag t) await RemoveTagFromPropertyAsync(t, TagType.Pro); });
            RemoveConTagCommand = new RelayCommand(async param => { if (param is PropertyTag t) await RemoveTagFromPropertyAsync(t, TagType.Con); });
            DeleteProTagCommand = new RelayCommand(async param => { if (param is PropertyTag t) await DeleteTagPermanentlyAsync(t, TagType.Pro); });
            DeleteConTagCommand = new RelayCommand(async param => { if (param is PropertyTag t) await DeleteTagPermanentlyAsync(t, TagType.Con); });
            DeleteSelectedProTagCommand = new RelayCommand(async _ => { if (SelectedProTag != null) await DeleteTagPermanentlyAsync(SelectedProTag, TagType.Pro); }, _ => SelectedProTag != null);
            DeleteSelectedConTagCommand = new RelayCommand(async _ => { if (SelectedConTag != null) await DeleteTagPermanentlyAsync(SelectedConTag, TagType.Con); }, _ => SelectedConTag != null);
            CreateProTagCommand = new RelayCommand(async _ => await CreateAndAddTagAsync(NewProTagName, TagType.Pro, SelectedProCategory), _ => !string.IsNullOrWhiteSpace(NewProTagName));
            CreateConTagCommand = new RelayCommand(async _ => await CreateAndAddTagAsync(NewConTagName, TagType.Con, SelectedConCategory), _ => !string.IsNullOrWhiteSpace(NewConTagName));

            // Load tags
            _ = LoadTagsAsync();
        }

        #region Collections

        public ObservableCollection<PropertyTag> AvailableProTags { get; } = new ObservableCollection<PropertyTag>();
        public ObservableCollection<PropertyTag> AvailableConTags { get; } = new ObservableCollection<PropertyTag>();
        public ObservableCollection<PropertyTag> PropertyProTags { get; } = new ObservableCollection<PropertyTag>();
        public ObservableCollection<PropertyTag> PropertyConTags { get; } = new ObservableCollection<PropertyTag>();
        public ObservableCollection<string> Categories { get; } = new ObservableCollection<string>();

        #endregion

        #region Properties

        private PropertyTag? _selectedProTag;
        public PropertyTag? SelectedProTag
        {
            get => _selectedProTag;
            set { _selectedProTag = value; OnPropertyChanged(); }
        }

        private PropertyTag? _selectedConTag;
        public PropertyTag? SelectedConTag
        {
            get => _selectedConTag;
            set { _selectedConTag = value; OnPropertyChanged(); }
        }

        private string _newProTagName = string.Empty;
        public string NewProTagName
        {
            get => _newProTagName;
            set { _newProTagName = value; OnPropertyChanged(); }
        }

        private string _newConTagName = string.Empty;
        public string NewConTagName
        {
            get => _newConTagName;
            set { _newConTagName = value; OnPropertyChanged(); }
        }

        private string _selectedProCategory = string.Empty;
        public string SelectedProCategory
        {
            get => _selectedProCategory;
            set { _selectedProCategory = value; OnPropertyChanged(); }
        }

        private string _selectedConCategory = string.Empty;
        public string SelectedConCategory
        {
            get => _selectedConCategory;
            set { _selectedConCategory = value; OnPropertyChanged(); }
        }

        #endregion

        #region Commands

        public ICommand AddProTagCommand { get; }
        public ICommand AddConTagCommand { get; }
        public ICommand RemoveProTagCommand { get; }
        public ICommand RemoveConTagCommand { get; }
        public ICommand DeleteProTagCommand { get; }
        public ICommand DeleteConTagCommand { get; }
        public ICommand DeleteSelectedProTagCommand { get; }
        public ICommand DeleteSelectedConTagCommand { get; }
        public ICommand CreateProTagCommand { get; }
        public ICommand CreateConTagCommand { get; }

        #endregion

        #region Methods

        public async System.Threading.Tasks.Task LoadTagsAsync()
        {
            try
            {
                // Load available PRO tags
                var proTags = await _tagService.GetTagsByTypeAsync(TagType.Pro);
                AvailableProTags.Clear();
                foreach (var tag in proTags)
                {
                    AvailableProTags.Add(tag);
                }

                // Load available CON tags
                var conTags = await _tagService.GetTagsByTypeAsync(TagType.Con);
                AvailableConTags.Clear();
                foreach (var tag in conTags)
                {
                    AvailableConTags.Add(tag);
                }

                // Load property's current tags (separated by type)
                PropertyProTags.Clear();
                PropertyConTags.Clear();
                IEnumerable<PropertyTag> propertyTags;
                
                if (_property.Model.Id > 0)
                {
                    propertyTags = await _tagService.GetTagsForPropertyAsync(_property.Model.Id);
                }
                else
                {
                    propertyTags = _property.Model.Tags ?? new System.Collections.Generic.List<PropertyTag>();
                }

                foreach (var tag in propertyTags)
                {
                    if (tag.Type == TagType.Pro)
                        PropertyProTags.Add(tag);
                    else if (tag.Type == TagType.Con)
                        PropertyConTags.Add(tag);
                }
            }
            catch { /* Ignore tag loading errors */ }
        }

        private async System.Threading.Tasks.Task AddTagToPropertyAsync(PropertyTag? tag, TagType type)
        {
            if (tag == null) return;

            var targetCollection = type == TagType.Pro ? PropertyProTags : PropertyConTags;

            // Check if already added
            if (targetCollection.Any(t => t.Id == tag.Id)) return;

            if (_property.Model.Id > 0)
            {
                await _tagService.AddTagToPropertyAsync(_property.Model.Id, tag.Id);
            }
            else
            {
                // For new properties, add to in-memory collection
                _property.Model.Tags.Add(tag);
            }

            targetCollection.Add(tag);
            _property.RefreshTags();

            // Clear selection
            if (type == TagType.Pro)
                SelectedProTag = null;
            else
                SelectedConTag = null;
        }

        private async System.Threading.Tasks.Task RemoveTagFromPropertyAsync(PropertyTag tag, TagType type)
        {
            if (_property.Model.Id > 0)
            {
                await _tagService.RemoveTagFromPropertyAsync(_property.Model.Id, tag.Id);
            }
            else
            {
                _property.Model.Tags.Remove(tag);
            }

            var targetCollection = type == TagType.Pro ? PropertyProTags : PropertyConTags;
            var availableCollection = type == TagType.Pro ? AvailableProTags : AvailableConTags;
            
            targetCollection.Remove(tag);
            
            // Add tag back to available list if not already there
            if (!availableCollection.Any(t => t.Id == tag.Id))
            {
                availableCollection.Add(tag);
            }
            
            _property.RefreshTags();
        }

        private async System.Threading.Tasks.Task DeleteTagPermanentlyAsync(PropertyTag tag, TagType type)
        {
            var result = MessageBox.Show(
                $"Are you sure you want to permanently delete the tag \"{tag.Name}\"?\n\nThis will remove it from all properties.",
                "Delete Tag",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return;

            try
            {
                await _tagService.DeleteTagAsync(tag.Id);

                // Remove from all collections by finding items with matching ID
                var proPropTag = PropertyProTags.FirstOrDefault(t => t.Id == tag.Id);
                if (proPropTag != null) PropertyProTags.Remove(proPropTag);
                
                var conPropTag = PropertyConTags.FirstOrDefault(t => t.Id == tag.Id);
                if (conPropTag != null) PropertyConTags.Remove(conPropTag);
                
                var availableProTag = AvailableProTags.FirstOrDefault(t => t.Id == tag.Id);
                if (availableProTag != null) AvailableProTags.Remove(availableProTag);
                
                var availableConTag = AvailableConTags.FirstOrDefault(t => t.Id == tag.Id);
                if (availableConTag != null) AvailableConTags.Remove(availableConTag);

                // Clear selection if the deleted tag was selected
                if (SelectedProTag?.Id == tag.Id) SelectedProTag = null;
                if (SelectedConTag?.Id == tag.Id) SelectedConTag = null;

                _property.RefreshTags();

                MessageBox.Show($"Tag \"{tag.Name}\" has been deleted.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error deleting tag: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async System.Threading.Tasks.Task CreateAndAddTagAsync(string tagName, TagType type, string category)
        {
            if (string.IsNullOrWhiteSpace(tagName)) return;

            var normalizedName = tagName.Trim();
            var normalizedCategory = category?.Trim() ?? string.Empty;
            var availableCollection = type == TagType.Pro ? AvailableProTags : AvailableConTags;
            var propertyCollection = type == TagType.Pro ? PropertyProTags : PropertyConTags;

            // Check if tag already exists (case-insensitive, same category)
            var existingTag = availableCollection.FirstOrDefault(t => 
                string.Equals(t.Name, normalizedName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(t.Category, normalizedCategory, StringComparison.OrdinalIgnoreCase));

            PropertyTag tagToAdd;
            
            if (existingTag != null)
            {
                // Use existing tag instead of creating a duplicate
                tagToAdd = existingTag;
                MessageBox.Show($"Tag '{existingTag.DisplayName}' already exists. Using existing tag.", 
                    "Duplicate Tag", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Create new tag with category
                tagToAdd = await _tagService.CreateTagAsync(normalizedName, type, normalizedCategory, isCustom: true);
                
                // Add to available tags
                if (!availableCollection.Any(t => t.Id == tagToAdd.Id))
                {
                    availableCollection.Add(tagToAdd);
                }
            }

            // Add to property if not already added
            if (!propertyCollection.Any(t => t.Id == tagToAdd.Id))
            {
                if (_property.Model.Id > 0)
                {
                    await _tagService.AddTagToPropertyAsync(_property.Model.Id, tagToAdd.Id);
                }
                else
                {
                    _property.Model.Tags.Add(tagToAdd);
                }
                propertyCollection.Add(tagToAdd);
                _property.RefreshTags();
            }

            // Clear input and reset category
            if (type == TagType.Pro)
            {
                NewProTagName = string.Empty;
                SelectedProCategory = string.Empty;
            }
            else
            {
                NewConTagName = string.Empty;
                SelectedConCategory = string.Empty;
            }
        }

        #endregion
    }
}
