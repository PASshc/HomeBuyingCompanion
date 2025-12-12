namespace HomeBuyingApp.UI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public MortgageComparisonViewModel MortgageComparisonViewModel { get; }
        public PropertyListViewModel PropertyListViewModel { get; }

        public MainViewModel(
            MortgageComparisonViewModel mortgageComparisonViewModel,
            PropertyListViewModel propertyListViewModel)
        {
            MortgageComparisonViewModel = mortgageComparisonViewModel;
            PropertyListViewModel = propertyListViewModel;
        }
    }
}
