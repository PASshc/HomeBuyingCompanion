using HomeBuyingApp.Core.Models;
using System.Collections.Generic;

namespace HomeBuyingApp.Core.Services
{
    public interface IMortgageCalculatorService
    {
        MortgageScenario Calculate(MortgageScenario scenario);
        List<AmortizationEntry> GenerateAmortizationSchedule(MortgageScenario scenario);
    }
}
