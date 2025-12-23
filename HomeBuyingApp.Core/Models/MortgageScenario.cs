namespace HomeBuyingApp.Core.Models
{
    public class MortgageScenario
    {
        public int Id { get; set; }
        public int PropertyId { get; set; }
        public Property? Property { get; set; }

        public string Name { get; set; } = string.Empty; // e.g. "20% Down"
        public decimal PurchasePrice { get; set; }
        public decimal DownPaymentAmount { get; set; }
        public decimal InterestRate { get; set; } // e.g. 0.065 for 6.5%
        public int LoanTermYears { get; set; }
        
        // Inputs
        public decimal PropertyTaxAnnualAmount { get; set; }
        public decimal HomeownerInsuranceAnnualAmount { get; set; }
        public decimal PmiRate { get; set; } // e.g. 0.005 for 0.5%
        public decimal PmiMonthlyAmount { get; set; }
        public decimal HoaMonthlyAmount { get; set; }

        // Calculated properties could be methods or computed properties
        public decimal MonthlyPrincipalAndInterest { get; set; }
        public decimal MonthlyPropertyTax { get; set; }
        public decimal MonthlyHomeownerInsurance { get; set; }
        public decimal TotalMonthlyPayment { get; set; }
    }
}
