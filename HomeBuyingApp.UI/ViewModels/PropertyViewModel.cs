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

        public bool HasAC
        {
            get => _model.HasAC;
            set
            {
                if (_model.HasAC != value)
                {
                    _model.HasAC = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool HasLandscape
        {
            get => _model.HasLandscape;
            set
            {
                if (_model.HasLandscape != value)
                {
                    _model.HasLandscape = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool HasPool
        {
            get => _model.HasPool;
            set
            {
                if (_model.HasPool != value)
                {
                    _model.HasPool = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool HasJacuzzi
        {
            get => _model.HasJacuzzi;
            set
            {
                if (_model.HasJacuzzi != value)
                {
                    _model.HasJacuzzi = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool HasLanai
        {
            get => _model.HasLanai;
            set
            {
                if (_model.HasLanai != value)
                {
                    _model.HasLanai = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool HasCarpet
        {
            get => _model.HasCarpet;
            set
            {
                if (_model.HasCarpet != value)
                {
                    _model.HasCarpet = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool HasTile
        {
            get => _model.HasTile;
            set
            {
                if (_model.HasTile != value)
                {
                    _model.HasTile = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool HasWoodFlooring
        {
            get => _model.HasWoodFlooring;
            set
            {
                if (_model.HasWoodFlooring != value)
                {
                    _model.HasWoodFlooring = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool HasOven
        {
            get => _model.HasOven;
            set
            {
                if (_model.HasOven != value)
                {
                    _model.HasOven = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool HasRefrigerator
        {
            get => _model.HasRefrigerator;
            set
            {
                if (_model.HasRefrigerator != value)
                {
                    _model.HasRefrigerator = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool HasWasherDryer
        {
            get => _model.HasWasherDryer;
            set
            {
                if (_model.HasWasherDryer != value)
                {
                    _model.HasWasherDryer = value;
                    OnPropertyChanged();
                }
            }
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
    }
}
