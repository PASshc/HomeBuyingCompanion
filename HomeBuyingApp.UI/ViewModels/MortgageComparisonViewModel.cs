namespace HomeBuyingApp.UI.ViewModels
{
    public class MortgageComparisonViewModel : ViewModelBase
    {
        public MortgageCalculatorViewModel ScenarioA { get; }
        public MortgageCalculatorViewModel ScenarioB { get; }

        public MortgageComparisonViewModel()
        {
            ScenarioA = new MortgageCalculatorViewModel();
            ScenarioB = new MortgageCalculatorViewModel();
            
            // Initialize with slightly different defaults so they are distinguishable
            ScenarioB.PurchasePrice = 350000;
            ScenarioB.DownPaymentAmount = 70000;
        }
    }
}
