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
        public int Bedrooms { get; set; }
        public double Bathrooms { get; set; }
        public int SquareFeet { get; set; }
        public bool HasHoa { get; set; }
        public decimal HoaFee { get; set; }
        public decimal PropertyTaxRate { get; set; } // e.g. 0.012 for 1.2%
        public string Comments { get; set; } = string.Empty; // Peters Comments
        public string Notes { get; set; } = string.Empty;
        public int Rating { get; set; } // 1-5
        public bool LookAt { get; set; }
        public bool Interest { get; set; }
        public bool IsArchived { get; set; }
        public PropertyStatus Status { get; set; }

        // Key Dates
        public System.DateTime? ViewingDate { get; set; }
        public System.DateTime? InspectionDate { get; set; }
        public System.DateTime? ClosingDate { get; set; }

        // Features
        public bool HasAC { get; set; }
        public bool HasLandscape { get; set; }
        public bool HasPool { get; set; }
        public bool HasJacuzzi { get; set; }
        public bool HasLanai { get; set; }
        public bool HasCarpet { get; set; }
        public bool HasTile { get; set; }
        public bool HasWoodFlooring { get; set; }
        public bool HasOven { get; set; }
        public bool HasRefrigerator { get; set; }
        public bool HasWasherDryer { get; set; }
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
    }
}
