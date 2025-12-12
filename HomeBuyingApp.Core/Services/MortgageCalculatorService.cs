using System;
using System.Collections.Generic;
using HomeBuyingApp.Core.Models;

namespace HomeBuyingApp.Core.Services
{
    public class MortgageCalculatorService : IMortgageCalculatorService
    {
        public MortgageScenario Calculate(MortgageScenario scenario)
        {
            // M = P [ i(1 + i)^n ] / [ (1 + i)^n – 1 ]
            // P = Principal loan amount
            // i = Monthly interest rate
            // n = Number of months

            decimal principal = scenario.PurchasePrice - scenario.DownPaymentAmount;
            
            if (principal <= 0)
            {
                scenario.MonthlyPrincipalAndInterest = 0;
                scenario.MonthlyPropertyTax = scenario.PropertyTaxAnnualAmount / 12;
                scenario.MonthlyHomeownerInsurance = scenario.HomeownerInsuranceAnnualAmount / 12;
                scenario.TotalMonthlyPayment = scenario.MonthlyPropertyTax 
                                             + scenario.MonthlyHomeownerInsurance 
                                             + scenario.HoaMonthlyAmount;
                return scenario;
            }

            double monthlyRate = (double)scenario.InterestRate / 12.0;
            int totalMonths = scenario.LoanTermYears * 12;

            if (monthlyRate == 0)
            {
                scenario.MonthlyPrincipalAndInterest = principal / totalMonths;
            }
            else
            {
                double numerator = monthlyRate * Math.Pow(1 + monthlyRate, totalMonths);
                double denominator = Math.Pow(1 + monthlyRate, totalMonths) - 1;
                scenario.MonthlyPrincipalAndInterest = principal * (decimal)(numerator / denominator);
            }

            // Calculate monthly components
            scenario.MonthlyPropertyTax = scenario.PropertyTaxAnnualAmount / 12;
            scenario.MonthlyHomeownerInsurance = scenario.HomeownerInsuranceAnnualAmount / 12;

            // Auto-calculate PMI if LTV > 80%
            // Standard PMI is approx 0.5% to 1.5% of loan amount annually
            if ((scenario.DownPaymentAmount / scenario.PurchasePrice) < 0.20m)
            {
                // Use provided PMI rate or default to 0.5%
                decimal pmiRate = scenario.PmiRate > 0 ? scenario.PmiRate : 0.005m;
                scenario.PmiMonthlyAmount = (principal * pmiRate) / 12;
            }
            else
            {
                scenario.PmiMonthlyAmount = 0;
            }

            scenario.TotalMonthlyPayment = scenario.MonthlyPrincipalAndInterest 
                                         + scenario.MonthlyPropertyTax 
                                         + scenario.MonthlyHomeownerInsurance 
                                         + scenario.PmiMonthlyAmount 
                                         + scenario.HoaMonthlyAmount;

            return scenario;
        }

        public List<AmortizationEntry> GenerateAmortizationSchedule(MortgageScenario scenario)
        {
            var schedule = new List<AmortizationEntry>();
            decimal principal = scenario.PurchasePrice - scenario.DownPaymentAmount;
            
            if (principal <= 0) return schedule;

            double monthlyRate = (double)scenario.InterestRate / 12.0;
            int totalMonths = scenario.LoanTermYears * 12;
            decimal monthlyPayment = scenario.MonthlyPrincipalAndInterest;

            // If calculate hasn't been run or returned 0 for some reason, recalculate P&I locally
            if (monthlyPayment == 0 && monthlyRate > 0)
            {
                 double numerator = monthlyRate * Math.Pow(1 + monthlyRate, totalMonths);
                 double denominator = Math.Pow(1 + monthlyRate, totalMonths) - 1;
                 monthlyPayment = principal * (decimal)(numerator / denominator);
            }
            else if (monthlyPayment == 0 && monthlyRate == 0)
            {
                monthlyPayment = principal / totalMonths;
            }

            decimal currentBalance = principal;

            for (int month = 1; month <= totalMonths; month++)
            {
                decimal interestPayment = currentBalance * (decimal)monthlyRate;
                decimal principalPayment = monthlyPayment - interestPayment;

                // Handle last month rounding
                if (month == totalMonths || principalPayment > currentBalance)
                {
                    principalPayment = currentBalance;
                    monthlyPayment = principalPayment + interestPayment;
                }

                currentBalance -= principalPayment;

                schedule.Add(new AmortizationEntry
                {
                    Month = month,
                    Payment = monthlyPayment,
                    Principal = principalPayment,
                    Interest = interestPayment,
                    RemainingBalance = currentBalance
                });

                if (currentBalance <= 0) break;
            }

            return schedule;
        }
    }
}
