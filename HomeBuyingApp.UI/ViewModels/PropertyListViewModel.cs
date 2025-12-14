using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using HomeBuyingApp.Core.Services;
using HomeBuyingApp.Core.Models;
using HomeBuyingApp.Infrastructure.Data;

namespace HomeBuyingApp.UI.ViewModels
{
    public class PropertyListViewModel : ViewModelBase
    {
        private readonly IPropertyService _propertyService;
        private readonly ICsvService _csvService;
        private readonly IWebScraperService _webScraperService;
        private readonly IBackupService _backupService;
        private readonly AppDbContext _dbContext;
        private ObservableCollection<PropertyViewModel> _properties;
        private List<PropertyViewModel> _allProperties = new List<PropertyViewModel>();
        private PropertyDetailViewModel _currentDetailViewModel;
        private bool _isDetailVisible;
        private PropertyViewModel _selectedProperty;
        private bool _showArchived;

        public bool ShowArchived
        {
            get => _showArchived;
            set
            {
                if (_showArchived != value)
                {
                    _showArchived = value;
                    OnPropertyChanged();
                    ApplyFilter();
                }
            }
        }

        public ObservableCollection<PropertyViewModel> Properties
        {
            get => _properties;
            set
            {
                _properties = value;
                OnPropertyChanged();
            }
        }

        public PropertyViewModel SelectedProperty
        {
            get => _selectedProperty;
            set
            {
                _selectedProperty = value;
                OnPropertyChanged();
            }
        }

        public PropertyDetailViewModel CurrentDetailViewModel
        {
            get => _currentDetailViewModel;
            set
            {
                _currentDetailViewModel = value;
                OnPropertyChanged();
            }
        }

        public bool IsDetailVisible
        {
            get => _isDetailVisible;
            set
            {
                _isDetailVisible = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(IsListVisible));
            }
        }

        public bool IsListVisible => !IsDetailVisible;

        public ICommand LoadPropertiesCommand { get; }
        public ICommand AddPropertyCommand { get; }
        public ICommand EditPropertyCommand { get; }
        public ICommand DeletePropertyCommand { get; }
        public ICommand ExportCommand { get; }
        public ICommand ImportCommand { get; }
        public ICommand BackupCommand { get; }
        public ICommand RestoreCommand { get; }

        public PropertyListViewModel(IPropertyService propertyService, ICsvService csvService, IWebScraperService webScraperService, IBackupService backupService, AppDbContext dbContext)
        {
            _propertyService = propertyService;
            _csvService = csvService;
            _webScraperService = webScraperService;
            _backupService = backupService;
            _dbContext = dbContext;
            Properties = new ObservableCollection<PropertyViewModel>();
            LoadPropertiesCommand = new RelayCommand(async _ => await LoadPropertiesAsync());
            AddPropertyCommand = new RelayCommand(_ => AddProperty());
            EditPropertyCommand = new RelayCommand(_ => EditProperty(), _ => SelectedProperty != null);
            DeletePropertyCommand = new RelayCommand(async _ => await DeletePropertyAsync(), _ => SelectedProperty != null);
            ExportCommand = new RelayCommand(_ => ExportProperties());
            ImportCommand = new RelayCommand(async _ => await ImportPropertiesAsync());
            BackupCommand = new RelayCommand(_ => BackupData());
            RestoreCommand = new RelayCommand(_ => RestoreData());
        }

        private void ExportProperties()
        {
            try
            {
                var dialog = new Microsoft.Win32.SaveFileDialog
                {
                    FileName = "Properties",
                    DefaultExt = ".csv",
                    Filter = "CSV documents (.csv)|*.csv"
                };

                if (dialog.ShowDialog() == true)
                {
                    var properties = _allProperties.Select(p => p.Model).ToList();
                    var csvContent = _csvService.GenerateCsv(properties);
                    System.IO.File.WriteAllText(dialog.FileName, csvContent);
                    MessageBox.Show("Properties exported successfully!", "Export", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error exporting properties: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private async Task ImportPropertiesAsync()
        {
            try
            {
                var dialog = new Microsoft.Win32.OpenFileDialog
                {
                    DefaultExt = ".csv",
                    Filter = "CSV documents (.csv)|*.csv"
                };

                if (dialog.ShowDialog() == true)
                {
                    var csvContent = System.IO.File.ReadAllText(dialog.FileName);
                    var properties = _csvService.ParseCsv(csvContent);
                    int count = 0;

                    foreach (var p in properties)
                    {
                        // Check for duplicates before adding
                        if (!await _propertyService.PropertyExistsAsync(p.Address, p.City, p.State, p.ZipCode))
                        {
                            await _propertyService.AddPropertyAsync(p);
                            count++;
                        }
                    }

                    await LoadPropertiesAsync();
                    MessageBox.Show($"Imported {count} new properties.", "Import", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Error importing properties: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public async Task LoadPropertiesAsync()
        {
            var properties = await _propertyService.GetAllPropertiesAsync();
            
            // Unsubscribe from old events to be safe (though not strictly necessary if objects are GC'd)
            if (_allProperties != null)
            {
                foreach (var vm in _allProperties)
                {
                    vm.PropertyChanged -= PropertyViewModel_PropertyChanged;
                }
            }

            _allProperties = properties.Select(p => new PropertyViewModel(p)).ToList();

            // Subscribe to events for auto-save on list changes
            foreach (var vm in _allProperties)
            {
                vm.PropertyChanged += PropertyViewModel_PropertyChanged;
            }

            ApplyFilter();
        }

        private async void PropertyViewModel_PropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (sender is PropertyViewModel vm)
            {
                if (e.PropertyName == nameof(PropertyViewModel.LookAt) ||
                    e.PropertyName == nameof(PropertyViewModel.Interest) ||
                    e.PropertyName == nameof(PropertyViewModel.IsArchived))
                {
                    try
                    {
                        await _propertyService.UpdatePropertyAsync(vm.Model);
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show($"Error saving property change: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void ApplyFilter()
        {
            Properties.Clear();
            foreach (var property in _allProperties)
            {
                if (ShowArchived || !property.IsArchived)
                {
                    Properties.Add(property);
                }
            }
        }

        private void AddProperty()
        {
            var newProperty = new Property();
            var propertyVm = new PropertyViewModel(newProperty);
            OpenDetailView(propertyVm);
        }

        private void EditProperty()
        {
            if (SelectedProperty != null)
            {
                OpenDetailView(SelectedProperty);
            }
        }

        private void OpenDetailView(PropertyViewModel property)
        {
            CurrentDetailViewModel = new PropertyDetailViewModel(
                _propertyService, 
                _webScraperService,
                property, 
                OnSave, 
                OnCancel);
            IsDetailVisible = true;
        }

        private async void OnSave()
        {
            IsDetailVisible = false;
            CurrentDetailViewModel = null;
            await LoadPropertiesAsync();
        }

        private void OnCancel()
        {
            IsDetailVisible = false;
            CurrentDetailViewModel = null;
            // Reload to revert any changes made in the VM but not saved to DB
            _ = LoadPropertiesAsync();
        }

        private async Task DeletePropertyAsync()
        {
            if (SelectedProperty != null)
            {
                var result = MessageBox.Show($"Are you sure you want to delete {SelectedProperty.Address}?", "Confirm Delete", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    await _propertyService.DeletePropertyAsync(SelectedProperty.Id);
                    await LoadPropertiesAsync();
                }
            }
        }

        private async void BackupData()
        {
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                FileName = $"HomeBuyingApp_Backup_{System.DateTime.Now:yyyyMMdd}",
                DefaultExt = ".zip",
                Filter = "Zip Files (*.zip)|*.zip"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    await _backupService.CreateBackupAsync(dialog.FileName);
                    MessageBox.Show("Backup created successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (System.Exception ex)
                {
                    MessageBox.Show($"Error creating backup: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private async void RestoreData()
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                DefaultExt = ".zip",
                Filter = "Zip Files (*.zip)|*.zip"
            };

            if (dialog.ShowDialog() == true)
            {
                var result = MessageBox.Show("Restoring data will overwrite current data. Are you sure?", "Confirm Restore", MessageBoxButton.YesNo, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    try
                    {
                        // Close database connections before restore
                        Microsoft.Data.Sqlite.SqliteConnection.ClearAllPools();
                        System.GC.Collect();
                        System.GC.WaitForPendingFinalizers();
                        
                        await _backupService.RestoreBackupAsync(dialog.FileName);
                        MessageBox.Show("Data restored successfully! The application will now reload data.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        await LoadPropertiesAsync();
                    }
                    catch (System.Exception ex)
                    {
                        MessageBox.Show($"Error restoring backup: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }
    }
}
