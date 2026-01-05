namespace HomeBuyingApp.UI.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        public MortgageComparisonViewModel MortgageComparisonViewModel { get; }
        public PropertyListViewModel PropertyListViewModel { get; }
        public JournalViewModel JournalViewModel { get; }

        public MainViewModel(
            MortgageComparisonViewModel mortgageComparisonViewModel,
            PropertyListViewModel propertyListViewModel,
            JournalViewModel journalViewModel)
        {
            MortgageComparisonViewModel = mortgageComparisonViewModel;
            PropertyListViewModel = propertyListViewModel;
            JournalViewModel = journalViewModel;
        }
    }
}
