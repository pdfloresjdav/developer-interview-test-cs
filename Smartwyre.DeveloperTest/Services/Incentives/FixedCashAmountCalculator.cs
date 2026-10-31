using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Services.Incentives;

public class FixedCashAmountCalculator : IIncentiveCalculator
{
    public IncentiveType IncentiveType => IncentiveType.FixedCashAmount;

    public bool CanCalculate(Rebate rebate, Product product, CalculateRebateRequest request)
        => product.SupportedIncentives.HasFlag(SupportedIncentiveType.FixedCashAmount)
           && rebate.Amount != 0;

    public decimal Calculate(Rebate rebate, Product product, CalculateRebateRequest request)
        => rebate.Amount;
}
