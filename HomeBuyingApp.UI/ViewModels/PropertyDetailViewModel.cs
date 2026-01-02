using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using HomeBuyingApp.Core.Models;
using HomeBuyingApp.Core.Services;

namespace HomeBuyingApp.UI.ViewModels
{
    public class PropertyDetailViewModel : ViewModelBase
    {
        private readonly IPropertyService _propertyService;
        private readonly ITagService _tagService;
        private readonly PropertyViewModel _property;
        private readonly Action _onSave;
        private readonly Action _onCancel;

        public PropertyViewModel Property => _property;
        public MortgageCalculatorViewModel MortgageCalculator { get; }
        public ObservableCollection<PropertyAttachment> Attachments { get; }

        // Quick Notes (note chips)
        public ObservableCollection<string> QuickNotesList { get; } = new ObservableCollection<string>();

        private string _newQuickNote = string.Empty;
        public string NewQuickNote
        {
            get => _newQuickNote;
            set { _newQuickNote = value; OnPropertyChanged(); }
        }

        // Other Features (feature chips)
        public ObservableCollection<string> OtherFeaturesList { get; } = new ObservableCollection<string>();

        private string _newOtherFeature = string.Empty;
        public string NewOtherFeature
        {
            get => _newOtherFeature;
            set { _newOtherFeature = value; OnPropertyChanged(); }
        }

        // Tag collections - separate for PROs and CONs
        public ObservableCollection<PropertyTag> AvailableProTags { get; } = new ObservableCollection<PropertyTag>();
        public ObservableCollection<PropertyTag> AvailableConTags { get; } = new ObservableCollection<PropertyTag>();
        public ObservableCollection<PropertyTag> PropertyProTags { get; } = new ObservableCollection<PropertyTag>();
        public ObservableCollection<PropertyTag> PropertyConTags { get; } = new ObservableCollection<PropertyTag>();

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

        public IEnumerable<PropertyStatus> Statuses => new[]
        {
            PropertyStatus.PendingVisit,
            PropertyStatus.Interested,
            PropertyStatus.PropertyInspection,
            PropertyStatus.OfferMade,
            PropertyStatus.OfferRejected,
            PropertyStatus.Closed,
            PropertyStatus.NotInterested
        };

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand PasteDetailsFromClipboardCommand { get; }
        public ICommand PasteImage1Command { get; }
        public ICommand PasteImage2Command { get; }
        public ICommand PasteImage3Command { get; }
        public ICommand PasteImage4Command { get; }
        public ICommand DeleteImage1Command { get; }
        public ICommand DeleteImage2Command { get; }
        public ICommand DeleteImage3Command { get; }
        public ICommand DeleteImage4Command { get; }
        public ICommand AddAttachmentCommand { get; }
        public ICommand DeleteAttachmentCommand { get; }
        public ICommand OpenAttachmentCommand { get; }

        // Tag commands - separate for PROs and CONs
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

        // Quick Notes commands
        public ICommand AddQuickNoteCommand { get; }
        public ICommand RemoveQuickNoteCommand { get; }

        // Other Features commands
        public ICommand AddOtherFeatureCommand { get; }
        public ICommand RemoveOtherFeatureCommand { get; }

        private BitmapImage? _image1;
        public BitmapImage? Image1
        {
            get => _image1;
            set { _image1 = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasImage1)); }
        }

        private BitmapImage? _image2;
        public BitmapImage? Image2
        {
            get => _image2;
            set { _image2 = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasImage2)); }
        }

        private BitmapImage? _image3;
        public BitmapImage? Image3
        {
            get => _image3;
            set { _image3 = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasImage3)); }
        }

        private BitmapImage? _image4;
        public BitmapImage? Image4
        {
            get => _image4;
            set { _image4 = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasImage4)); }
        }

        public bool HasImage1 => Image1 != null;
        public bool HasImage2 => Image2 != null;
        public bool HasImage3 => Image3 != null;
        public bool HasImage4 => Image4 != null;
        public bool HasAnyImage => HasImage1 || HasImage2 || HasImage3 || HasImage4;

        public PropertyDetailViewModel(
            IPropertyService propertyService,
            ITagService tagService,
            PropertyViewModel property, 
            Action onSave, 
            Action onCancel)
        {
            _propertyService = propertyService;
            _tagService = tagService;
            _property = property;
            _onSave = onSave;
            _onCancel = onCancel;

            // Initialize Mortgage Calculator with property details
            MortgageCalculator = new MortgageCalculatorViewModel();
            InitializeMortgageCalculator();

            Attachments = new ObservableCollection<PropertyAttachment>(_property.Model.Attachments ?? new List<PropertyAttachment>());

            SaveCommand = new RelayCommand(async _ => await SaveAsync());
            CancelCommand = new RelayCommand(_ => Cancel());
            PasteDetailsFromClipboardCommand = new RelayCommand(_ => PasteDetailsFromClipboard());
            PasteImage1Command = new RelayCommand(_ => PasteImageFromClipboard(1));
            PasteImage2Command = new RelayCommand(_ => PasteImageFromClipboard(2));
            PasteImage3Command = new RelayCommand(_ => PasteImageFromClipboard(3));
            PasteImage4Command = new RelayCommand(_ => PasteImageFromClipboard(4));
            DeleteImage1Command = new RelayCommand(_ => DeleteImage(1));
            DeleteImage2Command = new RelayCommand(_ => DeleteImage(2));
            DeleteImage3Command = new RelayCommand(_ => DeleteImage(3));
            DeleteImage4Command = new RelayCommand(_ => DeleteImage(4));
            AddAttachmentCommand = new RelayCommand(_ => AddAttachment());
            DeleteAttachmentCommand = new RelayCommand(param => { if (param is PropertyAttachment a) DeleteAttachment(a); });
            OpenAttachmentCommand = new RelayCommand(param => { if (param is PropertyAttachment a) OpenAttachment(a); });

            // Tag commands - separate for PROs and CONs
            AddProTagCommand = new RelayCommand(async _ => await AddTagToPropertyAsync(SelectedProTag, TagType.Pro), _ => SelectedProTag != null);
            AddConTagCommand = new RelayCommand(async _ => await AddTagToPropertyAsync(SelectedConTag, TagType.Con), _ => SelectedConTag != null);
            RemoveProTagCommand = new RelayCommand(async param => { if (param is PropertyTag t) await RemoveTagFromPropertyAsync(t, TagType.Pro); });
            RemoveConTagCommand = new RelayCommand(async param => { if (param is PropertyTag t) await RemoveTagFromPropertyAsync(t, TagType.Con); });
            DeleteProTagCommand = new RelayCommand(async param => { if (param is PropertyTag t) await DeleteTagPermanentlyAsync(t, TagType.Pro); });
            DeleteConTagCommand = new RelayCommand(async param => { if (param is PropertyTag t) await DeleteTagPermanentlyAsync(t, TagType.Con); });
            DeleteSelectedProTagCommand = new RelayCommand(async _ => { if (SelectedProTag != null) await DeleteTagPermanentlyAsync(SelectedProTag, TagType.Pro); }, _ => SelectedProTag != null);
            DeleteSelectedConTagCommand = new RelayCommand(async _ => { if (SelectedConTag != null) await DeleteTagPermanentlyAsync(SelectedConTag, TagType.Con); }, _ => SelectedConTag != null);
            CreateProTagCommand = new RelayCommand(async _ => await CreateAndAddTagAsync(NewProTagName, TagType.Pro), _ => !string.IsNullOrWhiteSpace(NewProTagName));
            CreateConTagCommand = new RelayCommand(async _ => await CreateAndAddTagAsync(NewConTagName, TagType.Con), _ => !string.IsNullOrWhiteSpace(NewConTagName));

            // Quick Notes commands
            AddQuickNoteCommand = new RelayCommand(_ => AddQuickNote(), _ => !string.IsNullOrWhiteSpace(NewQuickNote));
            RemoveQuickNoteCommand = new RelayCommand(param => { if (param is string note) RemoveQuickNote(note); });

            // Other Features commands
            AddOtherFeatureCommand = new RelayCommand(_ => AddOtherFeature(), _ => !string.IsNullOrWhiteSpace(NewOtherFeature));
            RemoveOtherFeatureCommand = new RelayCommand(param => { if (param is string feature) RemoveOtherFeature(feature); });

            // Load existing images if available
            LoadImages();

            // Load tags
            _ = LoadTagsAsync();

            // Load quick notes
            LoadQuickNotes();

            // Load other features
            LoadOtherFeatures();
        }

        private void LoadQuickNotes()
        {
            QuickNotesList.Clear();
            if (!string.IsNullOrWhiteSpace(_property.QuickNotes))
            {
                try
                {
                    var notes = JsonSerializer.Deserialize<List<string>>(_property.QuickNotes);
                    if (notes != null)
                    {
                        foreach (var note in notes)
                        {
                            QuickNotesList.Add(note);
                        }
                    }
                }
                catch { /* Ignore JSON parse errors */ }
            }
        }

        private void SaveQuickNotes()
        {
            _property.QuickNotes = JsonSerializer.Serialize(QuickNotesList.ToList());
        }

        private void AddQuickNote()
        {
            if (string.IsNullOrWhiteSpace(NewQuickNote)) return;
            
            var note = NewQuickNote.Trim();
            if (!QuickNotesList.Contains(note))
            {
                QuickNotesList.Add(note);
                SaveQuickNotes();
            }
            NewQuickNote = string.Empty;
        }

        private void RemoveQuickNote(string note)
        {
            if (QuickNotesList.Remove(note))
            {
                SaveQuickNotes();
            }
        }

        private void LoadOtherFeatures()
        {
            OtherFeaturesList.Clear();
            if (!string.IsNullOrWhiteSpace(_property.OtherFeatures))
            {
                try
                {
                    var features = JsonSerializer.Deserialize<List<string>>(_property.OtherFeatures);
                    if (features != null)
                    {
                        foreach (var feature in features)
                        {
                            OtherFeaturesList.Add(feature);
                        }
                    }
                }
                catch
                {
                    // If not valid JSON, treat existing text as a single feature (migration)
                    if (!string.IsNullOrWhiteSpace(_property.OtherFeatures))
                    {
                        OtherFeaturesList.Add(_property.OtherFeatures.Trim());
                        SaveOtherFeatures(); // Save in new JSON format
                    }
                }
            }
        }

        private void SaveOtherFeatures()
        {
            _property.OtherFeatures = JsonSerializer.Serialize(OtherFeaturesList.ToList());
        }

        private void AddOtherFeature()
        {
            if (string.IsNullOrWhiteSpace(NewOtherFeature)) return;
            
            var feature = NewOtherFeature.Trim();
            if (!OtherFeaturesList.Contains(feature))
            {
                OtherFeaturesList.Add(feature);
                SaveOtherFeatures();
            }
            NewOtherFeature = string.Empty;
        }

        private void RemoveOtherFeature(string feature)
        {
            if (OtherFeaturesList.Remove(feature))
            {
                SaveOtherFeatures();
            }
        }

        private async System.Threading.Tasks.Task LoadTagsAsync()
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
                    propertyTags = _property.Model.Tags ?? new List<PropertyTag>();
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

        private async System.Threading.Tasks.Task CreateAndAddTagAsync(string tagName, TagType type)
        {
            if (string.IsNullOrWhiteSpace(tagName)) return;

            var normalizedName = tagName.Trim();
            var availableCollection = type == TagType.Pro ? AvailableProTags : AvailableConTags;
            var propertyCollection = type == TagType.Pro ? PropertyProTags : PropertyConTags;

            // Check if tag already exists (case-insensitive)
            var existingTag = availableCollection.FirstOrDefault(t => 
                string.Equals(t.Name, normalizedName, StringComparison.OrdinalIgnoreCase));

            PropertyTag tagToAdd;
            
            if (existingTag != null)
            {
                // Use existing tag instead of creating a duplicate
                tagToAdd = existingTag;
                MessageBox.Show($"Tag '{existingTag.Name}' already exists. Using existing tag.", 
                    "Duplicate Tag", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                // Create new tag
                tagToAdd = await _tagService.CreateTagAsync(normalizedName, type, isCustom: true);
                
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

            // Clear input
            if (type == TagType.Pro)
                NewProTagName = string.Empty;
            else
                NewConTagName = string.Empty;
        }

        private void LoadImages()
        {
            Image1 = LoadImageFromPath(_property.ImagePath1);
            Image2 = LoadImageFromPath(_property.ImagePath2);
            Image3 = LoadImageFromPath(_property.ImagePath3);
            Image4 = LoadImageFromPath(_property.ImagePath4);
        }

        private BitmapImage? LoadImageFromPath(string path)
        {
            if (string.IsNullOrEmpty(path) || !File.Exists(path))
                return null;

            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(path, UriKind.Absolute);
                bitmap.EndInit();
                bitmap.Freeze();
                return bitmap;
            }
            catch
            {
                return null;
            }
        }

        private void PasteImageFromClipboard(int slot)
        {
            try
            {
                if (!Clipboard.ContainsImage())
                {
                    MessageBox.Show("Clipboard does not contain an image. Copy an image first (e.g., right-click an image and 'Copy image').", "Paste Image", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var image = Clipboard.GetImage();
                if (image == null)
                {
                    MessageBox.Show("Could not retrieve image from clipboard.", "Paste Image", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Save image to AppData - use same folder as database
                string appFolderName = "HomeBuyingApp";
#if DEBUG
                appFolderName = "HomeBuyingApp_Dev";
#endif
                var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), appFolderName, "Images");
                if (!Directory.Exists(appDataPath))
                {
                    Directory.CreateDirectory(appDataPath);
                }

                var fileName = $"property_{_property.Id}_img{slot}_{Guid.NewGuid()}.png";
                var filePath = Path.Combine(appDataPath, fileName);

                // Encode as PNG
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(image));
                    encoder.Save(fileStream);
                }

                // Delete old image if exists
                var oldPath = GetImagePath(slot);
                if (!string.IsNullOrEmpty(oldPath) && File.Exists(oldPath))
                {
                    try { File.Delete(oldPath); } catch { }
                }

                SetImagePath(slot, filePath);

                // Load and display
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(filePath, UriKind.Absolute);
                bitmap.EndInit();
                bitmap.Freeze();
                SetImage(slot, bitmap);

                MessageBox.Show($"Image {slot} pasted successfully.", "Paste Image", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to paste image: {ex.Message}", "Paste Image", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private string GetImagePath(int slot) => slot switch
        {
            1 => _property.ImagePath1,
            2 => _property.ImagePath2,
            3 => _property.ImagePath3,
            4 => _property.ImagePath4,
            _ => string.Empty
        };

        private void SetImagePath(int slot, string path)
        {
            switch (slot)
            {
                case 1: _property.ImagePath1 = path; break;
                case 2: _property.ImagePath2 = path; break;
                case 3: _property.ImagePath3 = path; break;
                case 4: _property.ImagePath4 = path; break;
            }
        }

        private void SetImage(int slot, BitmapImage? bitmap)
        {
            switch (slot)
            {
                case 1: Image1 = bitmap; break;
                case 2: Image2 = bitmap; break;
                case 3: Image3 = bitmap; break;
                case 4: Image4 = bitmap; break;
            }
        }

        private void DeleteImage(int slot)
        {
            var currentImage = slot switch { 1 => Image1, 2 => Image2, 3 => Image3, 4 => Image4, _ => null };
            if (currentImage == null) return;

            if (MessageBox.Show($"Are you sure you want to remove image {slot}?", "Delete Image", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                // Delete file
                var path = GetImagePath(slot);
                if (!string.IsNullOrEmpty(path) && File.Exists(path))
                {
                    try { File.Delete(path); } catch { }
                }

                SetImagePath(slot, string.Empty);
                SetImage(slot, null);
            }
        }

        private void PasteDetailsFromClipboard()
        {
            try
            {
                if (!Clipboard.ContainsText())
                {
                    MessageBox.Show("Clipboard has no text to parse. Copy the listing summary text (address/price/beds/baths/sqft) and try again.", "Paste Details", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var text = Clipboard.GetText();
                var parsed = ListingTextParser.Parse(text);

                var any = false;
                var applied = new List<string>();

                if (TryGetHttpUrl(text, out var clipboardUrl))
                {
                    // Helpful default: users often copy the URL first.
                    // This is not scraping; it's just storing the URL they already copied.
                    if (!string.Equals(Property.ListingUrl, clipboardUrl, StringComparison.OrdinalIgnoreCase))
                    {
                        Property.ListingUrl = clipboardUrl;
                        any = true;
                        applied.Add("listing url");
                    }
                }

                if (parsed.ListPrice.HasValue && parsed.ListPrice.Value > 0)
                {
                    Property.ListPrice = parsed.ListPrice.Value;
                    any = true;
                    applied.Add("price");
                }

                if (parsed.Bedrooms.HasValue && parsed.Bedrooms.Value > 0)
                {
                    Property.Bedrooms = (int)decimal.Truncate(parsed.Bedrooms.Value);
                    any = true;
                    applied.Add("beds");
                }

                if (parsed.Bathrooms.HasValue && parsed.Bathrooms.Value > 0)
                {
                    Property.Bathrooms = (double)parsed.Bathrooms.Value;
                    any = true;
                    applied.Add("baths");
                }

                if (parsed.SquareFeet.HasValue && parsed.SquareFeet.Value > 0)
                {
                    Property.SquareFeet = parsed.SquareFeet.Value;
                    any = true;
                    applied.Add("sqft");
                }

                if (!string.IsNullOrWhiteSpace(parsed.AddressLine))
                {
                    // If user copied a combined address line, keep it as the Address field.
                    Property.Address = parsed.AddressLine;
                    any = true;
                    applied.Add("address");
                }

                if (!string.IsNullOrWhiteSpace(parsed.City))
                {
                    Property.City = parsed.City;
                    any = true;
                    applied.Add("city");
                }

                if (!string.IsNullOrWhiteSpace(parsed.State))
                {
                    Property.State = parsed.State;
                    any = true;
                    applied.Add("state");
                }

                if (!string.IsNullOrWhiteSpace(parsed.ZipCode))
                {
                    Property.ZipCode = parsed.ZipCode;
                    any = true;
                    applied.Add("zip");
                }

                if (any)
                {
                    InitializeMortgageCalculator();
                    var appliedMsg = applied.Count > 0
                        ? $"Applied: {string.Join(", ", applied.Distinct())}."
                        : "Pasted details applied.";
                    MessageBox.Show(appliedMsg, "Paste Details", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    var preview = text ?? string.Empty;
                    preview = preview.Replace("\r\n", "\n").Replace("\r", "\n");
                    preview = preview.Length > 400 ? preview.Substring(0, 400) + "…" : preview;

                    var onlyUrlMsg = TryGetHttpUrl(text, out _)
                        ? "It looks like you copied only the URL. This button parses listing text (price, beds, baths, sqft, address), not the web page.\n\n"
                        : string.Empty;

                    MessageBox.Show(
                        "Could not recognize listing details from the clipboard text.\n\n" +
                        onlyUrlMsg +
                        "Tips:\n" +
                        "- Copy the listing header/summary area (price + beds/baths + sqft + address).\n" +
                        "- If the site uses bullets (•) or pipes (|), that's OK.\n\n" +
                        $"Clipboard preview:\n{preview}",
                        "Paste Details",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to paste details: {ex.Message}", "Paste Details", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static bool TryGetHttpUrl(string? text, out string url)
        {
            url = string.Empty;
            if (string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            var trimmed = text.Trim();

            // Common case: clipboard contains only the URL.
            if (Uri.TryCreate(trimmed, UriKind.Absolute, out var absolute)
                && (absolute.Scheme == Uri.UriSchemeHttp || absolute.Scheme == Uri.UriSchemeHttps))
            {
                url = absolute.ToString();
                return true;
            }

            // Handle URLs missing scheme (e.g., "www.zillow.com/..." copied from some UI).
            if (Uri.TryCreate("https://" + trimmed, UriKind.Absolute, out var implied)
                && (implied.Scheme == Uri.UriSchemeHttp || implied.Scheme == Uri.UriSchemeHttps)
                && string.Equals(implied.Host, "www.zillow.com", StringComparison.OrdinalIgnoreCase))
            {
                url = implied.ToString();
                return true;
            }

            return false;
        }

        private void InitializeMortgageCalculator()
        {
            if (_property.ListPrice > 0)
            {
                MortgageCalculator.PurchasePrice = _property.ListPrice;
            }
            
            if (_property.HoaFee > 0)
            {
                MortgageCalculator.HoaMonthlyAmount = _property.HoaFee;
            }

            if (_property.PropertyTaxRate > 0 && _property.ListPrice > 0)
            {
                MortgageCalculator.PropertyTaxAnnualAmount = _property.ListPrice * (_property.PropertyTaxRate / 100m);
            }

            // Load saved calculation values if they exist
            if (_property.Model.CalcInterestRate.HasValue) MortgageCalculator.InterestRate = _property.Model.CalcInterestRate.Value;
            if (_property.Model.CalcLoanTermYears.HasValue) MortgageCalculator.LoanTermYears = _property.Model.CalcLoanTermYears.Value;
            if (_property.Model.CalcPropertyTaxAnnualAmount.HasValue) MortgageCalculator.PropertyTaxAnnualAmount = _property.Model.CalcPropertyTaxAnnualAmount.Value;
            if (_property.Model.CalcHomeownerInsuranceAnnualAmount.HasValue) MortgageCalculator.HomeownerInsuranceAnnualAmount = _property.Model.CalcHomeownerInsuranceAnnualAmount.Value;
            if (_property.Model.CalcPmiRate.HasValue) MortgageCalculator.PmiRate = _property.Model.CalcPmiRate.Value;
            if (_property.Model.CalcHoaMonthlyAmount.HasValue) MortgageCalculator.HoaMonthlyAmount = _property.Model.CalcHoaMonthlyAmount.Value;
        }

        private async System.Threading.Tasks.Task SaveAsync()
        {
            // Save Mortgage Calculator values to Property Model
            _property.Model.CalcDownPaymentAmount = MortgageCalculator.DownPaymentAmount;
            _property.Model.CalcInterestRate = MortgageCalculator.InterestRate;
            _property.Model.CalcLoanTermYears = MortgageCalculator.LoanTermYears;
            _property.Model.CalcPropertyTaxAnnualAmount = MortgageCalculator.PropertyTaxAnnualAmount;
            _property.Model.CalcHomeownerInsuranceAnnualAmount = MortgageCalculator.HomeownerInsuranceAnnualAmount;
            _property.Model.CalcPmiRate = MortgageCalculator.PmiRate;
            _property.Model.CalcHoaMonthlyAmount = MortgageCalculator.HoaMonthlyAmount;

            if (_property.Model.Id == 0)
            {
                // Check for duplicates
                bool exists = await _propertyService.PropertyExistsAsync(
                    _property.Address ?? "", 
                    _property.City ?? "", 
                    _property.State ?? "", 
                    _property.ZipCode ?? "");

                if (exists)
                {
                    MessageBox.Show("A property with this address already exists.", "Duplicate Property", MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                await _propertyService.AddPropertyAsync(_property.Model);
            }
            else
            {
                await _propertyService.UpdatePropertyAsync(_property.Model);
            }

            // Save attachment updates (e.g., Description/Notes changes)
            foreach (var attachment in Attachments.Where(a => a.Id > 0))
            {
                await _propertyService.UpdateAttachmentAsync(attachment);
            }

            _onSave?.Invoke();
        }

        private void Cancel()
        {
            _onCancel?.Invoke();
        }

        private void AddAttachment()
        {
            var openFileDialog = new OpenFileDialog();
            if (openFileDialog.ShowDialog() == true)
            {
                try 
                {
                    var sourceFilePath = openFileDialog.FileName;
                    var fileName = Path.GetFileName(sourceFilePath);
                    var extension = Path.GetExtension(sourceFilePath);
                    
                    var appDataPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HomeBuyingApp", "Attachments");
                    if (!Directory.Exists(appDataPath))
                    {
                        Directory.CreateDirectory(appDataPath);
                    }
                    
                    var uniqueFileName = $"{Guid.NewGuid()}{extension}";
                    var destFilePath = Path.Combine(appDataPath, uniqueFileName);
                    
                    File.Copy(sourceFilePath, destFilePath);
                    
                    var attachment = new PropertyAttachment
                    {
                        FileName = fileName,
                        FilePath = destFilePath,
                        Description = fileName,
                        DateAdded = DateTime.Now
                    };
                    
                    if (_property.Model.Attachments == null) _property.Model.Attachments = new List<PropertyAttachment>();
                    _property.Model.Attachments.Add(attachment);
                    Attachments.Add(attachment);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to add attachment: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void DeleteAttachment(PropertyAttachment attachment)
        {
            if (attachment == null) return;
            
            if (MessageBox.Show($"Are you sure you want to delete '{attachment.FileName}'?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                Attachments.Remove(attachment);
                _property.Model.Attachments?.Remove(attachment);
            }
        }

        private void OpenAttachment(PropertyAttachment attachment)
        {
            if (attachment == null || string.IsNullOrEmpty(attachment.FilePath)) return;
            
            try
            {
                Process.Start(new ProcessStartInfo(attachment.FilePath) { UseShellExecute = true });
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Could not open file: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
