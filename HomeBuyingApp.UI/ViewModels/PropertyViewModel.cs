using HomeBuyingApp.Core.Models;
using HomeBuyingApp.UI.Helpers;

namespace HomeBuyingApp.UI.ViewModels
{
    public class PropertyViewModel : ViewModelBase
    {
        private readonly Property _model;

        public Property Model => _model;

        public PropertyViewModel(Property model)
        {
            _model = model;
        }

        public int Id => _model.Id;

        public string Address
        {
            get => _model.Address;
            set
            {
                if (_model.Address != value)
                {
                    _model.Address = value;
                    OnPropertyChanged();
                }
            }
        }

        public string City
        {
            get => _model.City;
            set
            {
                if (_model.City != value)
                {
                    _model.City = value;
                    OnPropertyChanged();
                }
            }
        }

        public string State
        {
            get => _model.State;
            set
            {
                if (_model.State != value)
                {
                    _model.State = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal ListPrice
        {
            get => _model.ListPrice;
            set
            {
                if (_model.ListPrice != value)
                {
                    _model.ListPrice = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal? EstimatedOffer
        {
            get => _model.EstimatedOffer;
            set
            {
                if (_model.EstimatedOffer != value)
                {
                    _model.EstimatedOffer = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal? WillingToPay
        {
            get => _model.WillingToPay;
            set
            {
                if (_model.WillingToPay != value)
                {
                    _model.WillingToPay = value;
                    OnPropertyChanged();
                }
            }
        }

        public int? Bedrooms
        {
            get => _model.Bedrooms;
            set
            {
                if (_model.Bedrooms != value)
                {
                    _model.Bedrooms = value;
                    OnPropertyChanged();
                }
            }
        }

        public double? Bathrooms
        {
            get => _model.Bathrooms;
            set
            {
                if (_model.Bathrooms != value)
                {
                    _model.Bathrooms = value;
                    OnPropertyChanged();
                }
            }
        }

        public PropertyStatus Status
        {
            get => _model.Status;
            set
            {
                if (_model.Status != value)
                {
                    _model.Status = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ZipCode
        {
            get => _model.ZipCode;
            set
            {
                if (_model.ZipCode != value)
                {
                    _model.ZipCode = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ListingUrl
        {
            get => _model.ListingUrl;
            set
            {
                if (_model.ListingUrl != value)
                {
                    _model.ListingUrl = value;
                    OnPropertyChanged();
                }
            }
        }

        public string MlsNumber
        {
            get => _model.MlsNumber;
            set
            {
                if (_model.MlsNumber != value)
                {
                    _model.MlsNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        public int? SquareFeet
        {
            get => _model.SquareFeet;
            set
            {
                if (_model.SquareFeet != value)
                {
                    _model.SquareFeet = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal HoaFee
        {
            get => _model.HoaFee;
            set
            {
                if (_model.HoaFee != value)
                {
                    _model.HoaFee = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool HasHoa
        {
            get => _model.HasHoa;
            set
            {
                if (_model.HasHoa != value)
                {
                    _model.HasHoa = value;
                    OnPropertyChanged();
                }
            }
        }

        public decimal PropertyTaxRate
        {
            get => _model.PropertyTaxRate;
            set
            {
                if (_model.PropertyTaxRate != value)
                {
                    _model.PropertyTaxRate = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Notes
        {
            get => _model.Notes;
            set
            {
                if (_model.Notes != value)
                {
                    _model.Notes = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(NotesPreview));
                }
            }
        }

        public string NotesPreview => RichTextBoxBinding.ToPlainText(_model.Notes);

        public string Comments
        {
            get => _model.Comments;
            set
            {
                if (_model.Comments != value)
                {
                    _model.Comments = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(CommentsPreview));
                }
            }
        }

        public string CommentsPreview => RichTextBoxBinding.ToPlainText(_model.Comments);

        public bool LookAt
        {
            get => _model.LookAt;
            set
            {
                if (_model.LookAt != value)
                {
                    _model.LookAt = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool Interest
        {
            get => _model.Interest;
            set
            {
                if (_model.Interest != value)
                {
                    _model.Interest = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsArchived
        {
            get => _model.IsArchived;
            set
            {
                if (_model.IsArchived != value)
                {
                    _model.IsArchived = value;
                    OnPropertyChanged();
                }
            }
        }

        public int Rating
        {
            get => _model.Rating;
            set
            {
                if (_model.Rating != value)
                {
                    _model.Rating = value;
                    OnPropertyChanged();
                }
            }
        }

        public System.DateTime? ViewingDate
        {
            get => _model.ViewingDate;
            set
            {
                if (_model.ViewingDate != value)
                {
                    _model.ViewingDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public System.DateTime? InspectionDate
        {
            get => _model.InspectionDate;
            set
            {
                if (_model.InspectionDate != value)
                {
                    _model.InspectionDate = value;
                    OnPropertyChanged();
                }
            }
        }

        public System.DateTime? ClosingDate
        {
            get => _model.ClosingDate;
            set
            {
                if (_model.ClosingDate != value)
                {
                    _model.ClosingDate = value;
                    OnPropertyChanged();
                }
            }
        }

        // Interior Features - Flooring
        public bool HasCarpet
        {
            get => _model.HasCarpet;
            set { if (_model.HasCarpet != value) { _model.HasCarpet = value; OnPropertyChanged(); } }
        }

        public bool HasTile
        {
            get => _model.HasTile;
            set { if (_model.HasTile != value) { _model.HasTile = value; OnPropertyChanged(); } }
        }

        public bool HasWoodFlooring
        {
            get => _model.HasWoodFlooring;
            set { if (_model.HasWoodFlooring != value) { _model.HasWoodFlooring = value; OnPropertyChanged(); } }
        }

        // Interior Features - Appliances
        public bool HasOven
        {
            get => _model.HasOven;
            set { if (_model.HasOven != value) { _model.HasOven = value; OnPropertyChanged(); } }
        }

        public bool HasRefrigerator
        {
            get => _model.HasRefrigerator;
            set { if (_model.HasRefrigerator != value) { _model.HasRefrigerator = value; OnPropertyChanged(); } }
        }

        public bool HasWasherDryer
        {
            get => _model.HasWasherDryer;
            set { if (_model.HasWasherDryer != value) { _model.HasWasherDryer = value; OnPropertyChanged(); } }
        }

        public bool HasDishwasher
        {
            get => _model.HasDishwasher;
            set { if (_model.HasDishwasher != value) { _model.HasDishwasher = value; OnPropertyChanged(); } }
        }

        // Interior Features - Climate/Systems
        public bool HasAC
        {
            get => _model.HasAC;
            set { if (_model.HasAC != value) { _model.HasAC = value; OnPropertyChanged(); } }
        }

        public bool HasCentralHeat
        {
            get => _model.HasCentralHeat;
            set { if (_model.HasCentralHeat != value) { _model.HasCentralHeat = value; OnPropertyChanged(); } }
        }

        public bool HasFireplace
        {
            get => _model.HasFireplace;
            set { if (_model.HasFireplace != value) { _model.HasFireplace = value; OnPropertyChanged(); } }
        }

        public bool HasCeilingFans
        {
            get => _model.HasCeilingFans;
            set { if (_model.HasCeilingFans != value) { _model.HasCeilingFans = value; OnPropertyChanged(); } }
        }

        // Interior Features - Storage
        public bool HasWalkInCloset
        {
            get => _model.HasWalkInCloset;
            set { if (_model.HasWalkInCloset != value) { _model.HasWalkInCloset = value; OnPropertyChanged(); } }
        }

        public bool HasAttic
        {
            get => _model.HasAttic;
            set { if (_model.HasAttic != value) { _model.HasAttic = value; OnPropertyChanged(); } }
        }

        public bool HasBasement
        {
            get => _model.HasBasement;
            set { if (_model.HasBasement != value) { _model.HasBasement = value; OnPropertyChanged(); } }
        }

        // Exterior Features - Outdoor
        public bool HasLandscape
        {
            get => _model.HasLandscape;
            set { if (_model.HasLandscape != value) { _model.HasLandscape = value; OnPropertyChanged(); } }
        }

        public bool HasPool
        {
            get => _model.HasPool;
            set { if (_model.HasPool != value) { _model.HasPool = value; OnPropertyChanged(); } }
        }

        public bool HasJacuzzi
        {
            get => _model.HasJacuzzi;
            set { if (_model.HasJacuzzi != value) { _model.HasJacuzzi = value; OnPropertyChanged(); } }
        }

        public bool HasLanai
        {
            get => _model.HasLanai;
            set { if (_model.HasLanai != value) { _model.HasLanai = value; OnPropertyChanged(); } }
        }

        public bool HasCoveredPatio
        {
            get => _model.HasCoveredPatio;
            set { if (_model.HasCoveredPatio != value) { _model.HasCoveredPatio = value; OnPropertyChanged(); } }
        }

        public bool HasDeckPatio
        {
            get => _model.HasDeckPatio;
            set { if (_model.HasDeckPatio != value) { _model.HasDeckPatio = value; OnPropertyChanged(); } }
        }

        public bool HasFencedYard
        {
            get => _model.HasFencedYard;
            set { if (_model.HasFencedYard != value) { _model.HasFencedYard = value; OnPropertyChanged(); } }
        }

        public bool HasSprinklerSystem
        {
            get => _model.HasSprinklerSystem;
            set { if (_model.HasSprinklerSystem != value) { _model.HasSprinklerSystem = value; OnPropertyChanged(); } }
        }

        // Exterior Features - Structures
        public bool HasGarage
        {
            get => _model.HasGarage;
            set { if (_model.HasGarage != value) { _model.HasGarage = value; OnPropertyChanged(); } }
        }

        public bool HasCarport
        {
            get => _model.HasCarport;
            set { if (_model.HasCarport != value) { _model.HasCarport = value; OnPropertyChanged(); } }
        }

        public bool HasStorageShed
        {
            get => _model.HasStorageShed;
            set { if (_model.HasStorageShed != value) { _model.HasStorageShed = value; OnPropertyChanged(); } }
        }

        public bool HasGuestHouse
        {
            get => _model.HasGuestHouse;
            set { if (_model.HasGuestHouse != value) { _model.HasGuestHouse = value; OnPropertyChanged(); } }
        }

        // Other Features
        public bool HasSolarPanels
        {
            get => _model.HasSolarPanels;
            set { if (_model.HasSolarPanels != value) { _model.HasSolarPanels = value; OnPropertyChanged(); } }
        }

        public bool HasSecuritySystem
        {
            get => _model.HasSecuritySystem;
            set { if (_model.HasSecuritySystem != value) { _model.HasSecuritySystem = value; OnPropertyChanged(); } }
        }

        public bool HasCustomFeature1
        {
            get => _model.HasCustomFeature1;
            set { if (_model.HasCustomFeature1 != value) { _model.HasCustomFeature1 = value; OnPropertyChanged(); } }
        }

        public string CustomFeature1Name
        {
            get => _model.CustomFeature1Name;
            set { if (_model.CustomFeature1Name != value) { _model.CustomFeature1Name = value; OnPropertyChanged(); } }
        }

        public bool HasCustomFeature2
        {
            get => _model.HasCustomFeature2;
            set { if (_model.HasCustomFeature2 != value) { _model.HasCustomFeature2 = value; OnPropertyChanged(); } }
        }

        public string CustomFeature2Name
        {
            get => _model.CustomFeature2Name;
            set { if (_model.CustomFeature2Name != value) { _model.CustomFeature2Name = value; OnPropertyChanged(); } }
        }

        public string OtherFeatures
        {
            get => _model.OtherFeatures;
            set
            {
                if (_model.OtherFeatures != value)
                {
                    _model.OtherFeatures = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ImagePath1
        {
            get => _model.ImagePath1;
            set
            {
                if (_model.ImagePath1 != value)
                {
                    _model.ImagePath1 = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ImagePath2
        {
            get => _model.ImagePath2;
            set
            {
                if (_model.ImagePath2 != value)
                {
                    _model.ImagePath2 = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ImagePath3
        {
            get => _model.ImagePath3;
            set
            {
                if (_model.ImagePath3 != value)
                {
                    _model.ImagePath3 = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ImagePath4
        {
            get => _model.ImagePath4;
            set
            {
                if (_model.ImagePath4 != value)
                {
                    _model.ImagePath4 = value;
                    OnPropertyChanged();
                }
            }
        }
    }
}
