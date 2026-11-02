using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Services.Incentives;

// One calculator per incentive type. New incentives plug in by implementing this.
public interface IIncentiveCalculator
{
    IncentiveType IncentiveType { get; }

    bool CanCalculate(Rebate rebate, Product product, CalculateRebateRequest request);

    decimal Calculate(Rebate rebate, Product product, CalculateRebateRequest request);
}
