using System.Collections.Generic;

namespace HomeBuyingApp.Core.Models
{
    public class Property
    {
        public int Id { get; set; }
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string ListingUrl { get; set; } = string.Empty;
        public string MlsNumber { get; set; } = string.Empty;
        public decimal ListPrice { get; set; }
        public decimal? EstimatedOffer { get; set; }
        public decimal? WillingToPay { get; set; }
        public int? Bedrooms { get; set; }
        public double? Bathrooms { get; set; }
        public int? SquareFeet { get; set; }
        public bool HasHoa { get; set; }
        public decimal HoaFee { get; set; }
        public decimal PropertyTaxRate { get; set; } // e.g. 0.012 for 1.2%
        public string Comments { get; set; } = string.Empty; // Peters Comments
        public string Notes { get; set; } = string.Empty;
        public string QuickNotes { get; set; } = string.Empty; // JSON array of quick note chips
        public decimal Rating { get; set; } // 1-5 (allows decimals like 3.5)
        public bool LookAt { get; set; }
        public bool Interest { get; set; }
        public bool IsArchived { get; set; }
        public PropertyStatus Status { get; set; }

        // Key Dates
        public System.DateTime? ViewingDate { get; set; }
        public System.DateTime? InspectionDate { get; set; }
        public System.DateTime? ClosingDate { get; set; }

        // Interior Features - Flooring
        public bool HasCarpet { get; set; }
        public bool HasTile { get; set; }
        public bool HasWoodFlooring { get; set; }

        // Interior Features - Appliances
        public bool HasOven { get; set; }
        public bool HasRefrigerator { get; set; }
        public bool HasWasherDryer { get; set; }
        public bool HasDishwasher { get; set; }

        // Interior Features - Climate/Systems
        public bool HasAC { get; set; }
        public bool HasCentralHeat { get; set; }
        public bool HasFireplace { get; set; }
        public bool HasCeilingFans { get; set; }

        // Interior Features - Storage
        public bool HasWalkInCloset { get; set; }
        public bool HasAttic { get; set; }
        public bool HasBasement { get; set; }

        // Exterior Features - Outdoor
        public bool HasLandscape { get; set; }
        public bool HasPool { get; set; }
        public bool HasJacuzzi { get; set; }
        public bool HasLanai { get; set; }
        public bool HasCoveredPatio { get; set; }
        public bool HasDeckPatio { get; set; }
        public bool HasFencedYard { get; set; }
        public bool HasSprinklerSystem { get; set; }

        // Exterior Features - Structures
        public bool HasGarage { get; set; }
        public bool HasCarport { get; set; }
        public bool HasStorageShed { get; set; }
        public bool HasGuestHouse { get; set; }

        // Other Features
        public bool HasSolarPanels { get; set; }
        public bool HasSecuritySystem { get; set; }
        public bool HasCustomFeature1 { get; set; }
        public string CustomFeature1Name { get; set; } = string.Empty;
        public bool HasCustomFeature2 { get; set; }
        public string CustomFeature2Name { get; set; } = string.Empty;

        public string OtherFeatures { get; set; } = string.Empty;

        // Saved Mortgage Calculation
        public decimal? CalcDownPaymentAmount { get; set; }
        public decimal? CalcInterestRate { get; set; }
        public int? CalcLoanTermYears { get; set; }
        public decimal? CalcPropertyTaxAnnualAmount { get; set; }
        public decimal? CalcHomeownerInsuranceAnnualAmount { get; set; }
        public decimal? CalcPmiRate { get; set; }
        public decimal? CalcHoaMonthlyAmount { get; set; }

        public List<MortgageScenario> MortgageScenarios { get; set; } = new List<MortgageScenario>();
        public List<ViewingAppointment> Viewings { get; set; } = new List<ViewingAppointment>();
        public List<PropertyAttachment> Attachments { get; set; } = new List<PropertyAttachment>();
        public List<PropertyTag> Tags { get; set; } = new List<PropertyTag>();

        // Property images (pasted from clipboard, up to 4)
        public string ImagePath1 { get; set; } = string.Empty;
        public string ImagePath2 { get; set; } = string.Empty;
        public string ImagePath3 { get; set; } = string.Empty;
        public string ImagePath4 { get; set; } = string.Empty;

        // Keep for migration compatibility (will be migrated to ImagePath1)
        [Obsolete("Use ImagePath1-4 instead")]
        public string PrimaryImagePath { get; set; } = string.Empty;
    }
}
