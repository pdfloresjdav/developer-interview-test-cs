using System.Collections.Generic;
using Moq;
using Smartwyre.DeveloperTest.Data;
using Smartwyre.DeveloperTest.Services;
using Smartwyre.DeveloperTest.Services.Incentives;
using Smartwyre.DeveloperTest.Types;
using Xunit;

namespace Smartwyre.DeveloperTest.Tests;

public class RebateServiceTests
{
    private readonly Mock<IRebateDataStore> _rebateStore = new();
    private readonly Mock<IProductDataStore> _productStore = new();

    private RebateService CreateService() => new(
        _rebateStore.Object,
        _productStore.Object,
        new IIncentiveCalculator[]
        {
            new FixedCashAmountCalculator(),
            new FixedRateRebateCalculator(),
            new AmountPerUomCalculator()
        });

    private static CalculateRebateRequest Request(decimal volume = 0) => new()
    {
        RebateIdentifier = "reb-1",
        ProductIdentifier = "prod-1",
        Volume = volume
    };

    [Fact]
    public void Calculate_FixedCashAmount_Succeeds_AndStoresResult()
    {
        _rebateStore.Setup(s => s.GetRebate("reb-1"))
            .Returns(new Rebate { Incentive = IncentiveType.FixedCashAmount, Amount = 50m });
        _productStore.Setup(s => s.GetProduct("prod-1"))
            .Returns(new Product { SupportedIncentives = SupportedIncentiveType.FixedCashAmount });

        var result = CreateService().Calculate(Request());

        Assert.True(result.Success);
        Assert.Equal(50m, result.Amount);
        _rebateStore.Verify(s => s.StoreCalculationResult(It.IsAny<Rebate>(), 50m), Times.Once);
    }

    [Fact]
    public void Calculate_FixedRateRebate_Succeeds()
    {
        _rebateStore.Setup(s => s.GetRebate("reb-1"))
            .Returns(new Rebate { Incentive = IncentiveType.FixedRateRebate, Percentage = 0.1m });
        _productStore.Setup(s => s.GetProduct("prod-1"))
            .Returns(new Product { SupportedIncentives = SupportedIncentiveType.FixedRateRebate, Price = 100m });

        var result = CreateService().Calculate(Request(volume: 5m));

        Assert.True(result.Success);
        Assert.Equal(50m, result.Amount); // 100 * 0.1 * 5
    }

    [Fact]
    public void Calculate_AmountPerUom_Succeeds()
    {
        _rebateStore.Setup(s => s.GetRebate("reb-1"))
            .Returns(new Rebate { Incentive = IncentiveType.AmountPerUom, Amount = 10m });
        _productStore.Setup(s => s.GetProduct("prod-1"))
            .Returns(new Product { SupportedIncentives = SupportedIncentiveType.AmountPerUom });

        var result = CreateService().Calculate(Request(volume: 3m));

        Assert.True(result.Success);
        Assert.Equal(30m, result.Amount); // 10 * 3
    }

    [Fact]
    public void Calculate_WhenRebateNotFound_Fails_AndDoesNotStore()
    {
        _rebateStore.Setup(s => s.GetRebate(It.IsAny<string>())).Returns((Rebate)null);
        _productStore.Setup(s => s.GetProduct(It.IsAny<string>())).Returns(new Product());

        var result = CreateService().Calculate(Request());

        Assert.False(result.Success);
        _rebateStore.Verify(s => s.StoreCalculationResult(It.IsAny<Rebate>(), It.IsAny<decimal>()), Times.Never);
    }

    [Fact]
    public void Calculate_WhenProductNotFound_Fails()
    {
        _rebateStore.Setup(s => s.GetRebate(It.IsAny<string>()))
            .Returns(new Rebate { Incentive = IncentiveType.FixedCashAmount, Amount = 50m });
        _productStore.Setup(s => s.GetProduct(It.IsAny<string>())).Returns((Product)null);

        var result = CreateService().Calculate(Request());

        Assert.False(result.Success);
    }

    [Fact]
    public void Calculate_WhenProductDoesNotSupportIncentive_Fails()
    {
        _rebateStore.Setup(s => s.GetRebate(It.IsAny<string>()))
            .Returns(new Rebate { Incentive = IncentiveType.FixedCashAmount, Amount = 50m });
        _productStore.Setup(s => s.GetProduct(It.IsAny<string>()))
            .Returns(new Product { SupportedIncentives = SupportedIncentiveType.AmountPerUom });

        var result = CreateService().Calculate(Request());

        Assert.False(result.Success);
        _rebateStore.Verify(s => s.StoreCalculationResult(It.IsAny<Rebate>(), It.IsAny<decimal>()), Times.Never);
    }
}
