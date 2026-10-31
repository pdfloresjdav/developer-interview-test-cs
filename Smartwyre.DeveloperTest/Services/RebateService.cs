using System.Collections.Generic;
using System.Linq;
using Smartwyre.DeveloperTest.Data;
using Smartwyre.DeveloperTest.Services.Incentives;
using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Services;

public class RebateService : IRebateService
{
    private readonly IRebateDataStore _rebateDataStore;
    private readonly IProductDataStore _productDataStore;
    private readonly IReadOnlyDictionary<IncentiveType, IIncentiveCalculator> _calculators;

    public RebateService(
        IRebateDataStore rebateDataStore,
        IProductDataStore productDataStore,
        IEnumerable<IIncentiveCalculator> calculators)
    {
        _rebateDataStore = rebateDataStore;
        _productDataStore = productDataStore;
        _calculators = calculators.ToDictionary(c => c.IncentiveType);
    }

    public CalculateRebateResult Calculate(CalculateRebateRequest request)
    {
        var result = new CalculateRebateResult { Success = false };

        Rebate rebate = _rebateDataStore.GetRebate(request.RebateIdentifier);
        Product product = _productDataStore.GetProduct(request.ProductIdentifier);

        if (rebate == null || product == null)
        {
            return result;
        }

        if (!_calculators.TryGetValue(rebate.Incentive, out var calculator))
        {
            return result;
        }

        if (!calculator.CanCalculate(rebate, product, request))
        {
            return result;
        }

        var rebateAmount = calculator.Calculate(rebate, product, request);

        _rebateDataStore.StoreCalculationResult(rebate, rebateAmount);

        result.Success = true;
        result.Amount = rebateAmount;
        return result;
    }
}
