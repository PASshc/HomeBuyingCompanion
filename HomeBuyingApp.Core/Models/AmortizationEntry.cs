namespace HomeBuyingApp.Core.Models
{
    public class AmortizationEntry
    {
        public int Month { get; set; }
        public decimal Payment { get; set; }
        public decimal Principal { get; set; }
        public decimal Interest { get; set; }
        public decimal RemainingBalance { get; set; }
    }
}
