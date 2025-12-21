using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using Microsoft.Win32;
using HomeBuyingApp.Core.Models;
using HomeBuyingApp.Core.Services;

namespace HomeBuyingApp.UI.ViewModels
{
    public class PropertyDetailViewModel : ViewModelBase
    {
        private readonly IPropertyService _propertyService;
        private readonly IWebScraperService _webScraperService;
        private readonly PropertyViewModel _property;
        private readonly Action _onSave;
        private readonly Action _onCancel;

        public PropertyViewModel Property => _property;
        public MortgageCalculatorViewModel MortgageCalculator { get; }
        public ObservableCollection<PropertyAttachment> Attachments { get; }

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
        public ICommand AutoFillFromUrlCommand { get; }
        public ICommand AddAttachmentCommand { get; }
        public ICommand DeleteAttachmentCommand { get; }
        public ICommand OpenAttachmentCommand { get; }

        public PropertyDetailViewModel(
            IPropertyService propertyService, 
            IWebScraperService webScraperService,
            PropertyViewModel property, 
            Action onSave, 
            Action onCancel)
        {
            _propertyService = propertyService;
            _webScraperService = webScraperService;
            _property = property;
            _onSave = onSave;
            _onCancel = onCancel;

            // Initialize Mortgage Calculator with property details
            MortgageCalculator = new MortgageCalculatorViewModel();
            InitializeMortgageCalculator();

            Attachments = new ObservableCollection<PropertyAttachment>(_property.Model.Attachments ?? new List<PropertyAttachment>());

            SaveCommand = new RelayCommand(async _ => await SaveAsync());
            CancelCommand = new RelayCommand(_ => Cancel());
            AutoFillFromUrlCommand = new RelayCommand(async _ => await AutoFillFromUrlAsync(), _ => !string.IsNullOrWhiteSpace(Property.ListingUrl));
            AddAttachmentCommand = new RelayCommand(_ => AddAttachment());
            DeleteAttachmentCommand = new RelayCommand(param => DeleteAttachment(param as PropertyAttachment));
            OpenAttachmentCommand = new RelayCommand(param => OpenAttachment(param as PropertyAttachment));
        }

        private async System.Threading.Tasks.Task AutoFillFromUrlAsync()
        {
            if (string.IsNullOrWhiteSpace(Property.ListingUrl)) return;

            try
            {
                var scrapedData = await _webScraperService.ScrapePropertyDetailsAsync(Property.ListingUrl);

                if (scrapedData != null)
                {
                    // Always copy Comments field (contains scraper status/errors)
                    if (!string.IsNullOrWhiteSpace(scrapedData.Comments))
                    {
                        Property.Comments = scrapedData.Comments;
                    }
                    
                    if (scrapedData.ListPrice > 0) Property.ListPrice = scrapedData.ListPrice;
                    if (scrapedData.Bedrooms.HasValue && scrapedData.Bedrooms.Value > 0) Property.Bedrooms = scrapedData.Bedrooms;
                    if (scrapedData.Bathrooms.HasValue && scrapedData.Bathrooms.Value > 0) Property.Bathrooms = scrapedData.Bathrooms;
                    if (scrapedData.SquareFeet.HasValue && scrapedData.SquareFeet.Value > 0) Property.SquareFeet = scrapedData.SquareFeet;
                    
                    if (!string.IsNullOrWhiteSpace(scrapedData.Address)) Property.Address = scrapedData.Address;
                    if (!string.IsNullOrWhiteSpace(scrapedData.City)) Property.City = scrapedData.City;
                    if (!string.IsNullOrWhiteSpace(scrapedData.State)) Property.State = scrapedData.State;
                    if (!string.IsNullOrWhiteSpace(scrapedData.ZipCode)) Property.ZipCode = scrapedData.ZipCode;

                    // Update Mortgage Calculator with new price
                    InitializeMortgageCalculator();
                    
                    // Only show success if we actually got data
                    if (scrapedData.ListPrice > 0 || !string.IsNullOrWhiteSpace(scrapedData.Address))
                    {
                        MessageBox.Show("Property details updated from URL.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to auto-fill details: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
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
