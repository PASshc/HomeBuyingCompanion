using System;
using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace HomeBuyingApp.UI.ViewModels
{
    /// <summary>
    /// ViewModel responsible for managing property images (paste, delete, display).
    /// Extracted from PropertyDetailViewModel for better separation of concerns.
    /// </summary>
    public class PropertyImagesViewModel : ViewModelBase
    {
        private readonly PropertyViewModel _property;

        public PropertyImagesViewModel(PropertyViewModel property)
        {
            _property = property;

            PasteImage1Command = new RelayCommand(_ => PasteImageFromClipboard(1));
            PasteImage2Command = new RelayCommand(_ => PasteImageFromClipboard(2));
            PasteImage3Command = new RelayCommand(_ => PasteImageFromClipboard(3));
            PasteImage4Command = new RelayCommand(_ => PasteImageFromClipboard(4));
            DeleteImage1Command = new RelayCommand(_ => DeleteImage(1));
            DeleteImage2Command = new RelayCommand(_ => DeleteImage(2));
            DeleteImage3Command = new RelayCommand(_ => DeleteImage(3));
            DeleteImage4Command = new RelayCommand(_ => DeleteImage(4));

            LoadImages();
        }

        #region Commands

        public ICommand PasteImage1Command { get; }
        public ICommand PasteImage2Command { get; }
        public ICommand PasteImage3Command { get; }
        public ICommand PasteImage4Command { get; }
        public ICommand DeleteImage1Command { get; }
        public ICommand DeleteImage2Command { get; }
        public ICommand DeleteImage3Command { get; }
        public ICommand DeleteImage4Command { get; }

        #endregion

        #region Image Properties

        private BitmapImage? _image1;
        public BitmapImage? Image1
        {
            get => _image1;
            set { _image1 = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasImage1)); OnPropertyChanged(nameof(HasAnyImage)); }
        }

        private BitmapImage? _image2;
        public BitmapImage? Image2
        {
            get => _image2;
            set { _image2 = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasImage2)); OnPropertyChanged(nameof(HasAnyImage)); }
        }

        private BitmapImage? _image3;
        public BitmapImage? Image3
        {
            get => _image3;
            set { _image3 = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasImage3)); OnPropertyChanged(nameof(HasAnyImage)); }
        }

        private BitmapImage? _image4;
        public BitmapImage? Image4
        {
            get => _image4;
            set { _image4 = value; OnPropertyChanged(); OnPropertyChanged(nameof(HasImage4)); OnPropertyChanged(nameof(HasAnyImage)); }
        }

        public bool HasImage1 => Image1 != null;
        public bool HasImage2 => Image2 != null;
        public bool HasImage3 => Image3 != null;
        public bool HasImage4 => Image4 != null;
        public bool HasAnyImage => HasImage1 || HasImage2 || HasImage3 || HasImage4;

        #endregion

        #region Image Methods

        public void LoadImages()
        {
            for (int i = 1; i <= 4; i++)
            {
                var path = GetImagePath(i);
                if (!string.IsNullOrEmpty(path))
                {
                    LoadImageFromPath(i, path);
                }
            }
        }

        private void LoadImageFromPath(int slot, string path)
        {
            try
            {
                if (!File.Exists(path)) return;

                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(path, UriKind.Absolute);
                bitmap.EndInit();
                bitmap.Freeze();
                SetImage(slot, bitmap);
            }
            catch { /* Ignore load errors */ }
        }

        private void PasteImageFromClipboard(int slot)
        {
            try
            {
                if (!Clipboard.ContainsImage())
                {
                    MessageBox.Show("Clipboard does not contain an image.", "Paste Image", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                var image = Clipboard.GetImage();
                if (image == null) return;

                // Create directory for images if it doesn't exist
                var propertyId = _property.Model.Id;
                var basePath = !string.IsNullOrEmpty(App.AppDataPath) 
                    ? App.AppDataPath 
                    : Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "HomeBuyingApp");
                var appDataPath = Path.Combine(basePath, "Images", propertyId.ToString());
                Directory.CreateDirectory(appDataPath);

                // Save as PNG
                var fileName = $"image{slot}_{DateTime.Now:yyyyMMddHHmmss}.png";
                var filePath = Path.Combine(appDataPath, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    var encoder = new PngBitmapEncoder();
                    encoder.Frames.Add(BitmapFrame.Create(image));
                    encoder.Save(stream);
                }

                // Store path in property
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

        #endregion
    }
}
