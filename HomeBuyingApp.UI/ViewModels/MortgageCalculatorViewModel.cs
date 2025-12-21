using HomeBuyingApp.Core.Models;
using HomeBuyingApp.Core.Services;
using System.Collections.Generic;
using System.Windows.Input;

namespace HomeBuyingApp.UI.ViewModels
{
    public class MortgageCalculatorViewModel : ViewModelBase
    {
        private readonly IMortgageCalculatorService _calculatorService;
        private MortgageScenario _scenario;
        private bool _isUpdatingDownPayment;

        public List<decimal> DownPaymentPercentOptions { get; } = new List<decimal>
        {
            3m, 3.5m, 5m, 10m, 20m
        };

        public MortgageCalculatorViewModel()
        {
            _calculatorService = new MortgageCalculatorService();
            _scenario = new MortgageScenario
            {
                PurchasePrice = 300000,
                DownPaymentAmount = 60000,
                InterestRate = 0.065m,
                LoanTermYears = 30,
                PropertyTaxAnnualAmount = 3600,
                HomeownerInsuranceAnnualAmount = 1200,
                HoaMonthlyAmount = 0,
                PmiMonthlyAmount = 0,
                PmiRate = 0.005m // Default 0.5%
            };
            Calculate();
        }

        public decimal PurchasePrice
        {
            get => _scenario.PurchasePrice;
            set
            {
                if (_scenario.PurchasePrice != value)
                {
                    _scenario.PurchasePrice = value;
                    OnPropertyChanged();
                    
                    // Update Down Payment Amount based on Percent when Price changes
                    if (!_isUpdatingDownPayment)
                    {
                        _isUpdatingDownPayment = true;
                        _scenario.DownPaymentAmount = _scenario.PurchasePrice * (DownPaymentPercent / 100m);
                        OnPropertyChanged(nameof(DownPaymentAmount));
                        _isUpdatingDownPayment = false;
                    }

                    Calculate();
                }
            }
        }

        public decimal DownPaymentAmount
        {
            get => _scenario.DownPaymentAmount;
        }

        private decimal _downPaymentPercent = 20;
        public decimal DownPaymentPercent
        {
            get => _downPaymentPercent;
            set
            {
                if (_downPaymentPercent != value)
                {
                    _downPaymentPercent = value;
                    OnPropertyChanged();

                    if (!_isUpdatingDownPayment)
                    {
                        _isUpdatingDownPayment = true;
                        _scenario.DownPaymentAmount = _scenario.PurchasePrice * (_downPaymentPercent / 100m);
                        OnPropertyChanged(nameof(DownPaymentAmount));
                        _isUpdatingDownPayment = false;
                    }
                    
                    Calculate();
                }
            }
        }

        public decimal InterestRate
        {
            get => _scenario.InterestRate;
            set
            {
                if (_scenario.InterestRate != value)
                {
                    _scenario.InterestRate = value;
                    OnPropertyChanged();
                    Calculate();
                }
            }
        }

        public decimal PmiRate
        {
            get => _scenario.PmiRate;
            set
            {
                if (_scenario.PmiRate != value)
                {
                    _scenario.PmiRate = value;
                    // Reset explicit PMI amount so it recalculates based on rate
                    _scenario.PmiMonthlyAmount = 0; 
                    OnPropertyChanged();
                    Calculate();
                }
            }
        }

        public int LoanTermYears
        {
            get => _scenario.LoanTermYears;
            set
            {
                if (_scenario.LoanTermYears != value)
                {
                    _scenario.LoanTermYears = value;
                    OnPropertyChanged();
                    Calculate();
                }
            }
        }

        public decimal PropertyTaxAnnualAmount
        {
            get => _scenario.PropertyTaxAnnualAmount;
            set
            {
                if (_scenario.PropertyTaxAnnualAmount != value)
                {
                    _scenario.PropertyTaxAnnualAmount = value;
                    OnPropertyChanged();
                    Calculate();
                }
            }
        }

        public decimal HomeownerInsuranceAnnualAmount
        {
            get => _scenario.HomeownerInsuranceAnnualAmount;
            set
            {
                if (_scenario.HomeownerInsuranceAnnualAmount != value)
                {
                    _scenario.HomeownerInsuranceAnnualAmount = value;
                    OnPropertyChanged();
                    Calculate();
                }
            }
        }

        public decimal HoaMonthlyAmount
        {
            get => _scenario.HoaMonthlyAmount;
            set
            {
                if (_scenario.HoaMonthlyAmount != value)
                {
                    _scenario.HoaMonthlyAmount = value;
                    OnPropertyChanged();
                    Calculate();
                }
            }
        }

        public decimal MonthlyPrincipalAndInterest => _scenario.MonthlyPrincipalAndInterest;
        public decimal MonthlyPropertyTax => _scenario.MonthlyPropertyTax;
        public decimal MonthlyHomeownerInsurance => _scenario.MonthlyHomeownerInsurance;
        public decimal PmiMonthlyAmount => _scenario.PmiMonthlyAmount;
        public decimal TotalMonthlyPayment => _scenario.TotalMonthlyPayment;

        private List<AmortizationEntry> _amortizationSchedule;
        public List<AmortizationEntry> AmortizationSchedule
        {
            get => _amortizationSchedule;
            set
            {
                _amortizationSchedule = value;
                OnPropertyChanged();
            }
        }

        private void Calculate()
        {
            _scenario = _calculatorService.Calculate(_scenario);
            AmortizationSchedule = _calculatorService.GenerateAmortizationSchedule(_scenario);

            OnPropertyChanged(nameof(MonthlyPrincipalAndInterest));
            OnPropertyChanged(nameof(MonthlyPropertyTax));
            OnPropertyChanged(nameof(MonthlyHomeownerInsurance));
            OnPropertyChanged(nameof(PmiMonthlyAmount));
            OnPropertyChanged(nameof(TotalMonthlyPayment));
        }
    }
}
