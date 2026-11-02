using Smartwyre.DeveloperTest.Services.Incentives;
using Smartwyre.DeveloperTest.Types;
using Xunit;

namespace Smartwyre.DeveloperTest.Tests;

public class IncentiveCalculatorTests
{
    // ---- FixedCashAmount ----

    [Fact]
    public void FixedCashAmount_Calculates_RebateAmount()
    {
        var calc = new FixedCashAmountCalculator();
        var rebate = new Rebate { Amount = 75m };
        var product = new Product { SupportedIncentives = SupportedIncentiveType.FixedCashAmount };

        Assert.True(calc.CanCalculate(rebate, product, new CalculateRebateRequest()));
        Assert.Equal(75m, calc.Calculate(rebate, product, new CalculateRebateRequest()));
    }

    [Fact]
    public void FixedCashAmount_CannotCalculate_WhenAmountIsZero()
    {
        var calc = new FixedCashAmountCalculator();
        var rebate = new Rebate { Amount = 0m };
        var product = new Product { SupportedIncentives = SupportedIncentiveType.FixedCashAmount };

        Assert.False(calc.CanCalculate(rebate, product, new CalculateRebateRequest()));
    }

    [Fact]
    public void FixedCashAmount_CannotCalculate_WhenNotSupported()
    {
        var calc = new FixedCashAmountCalculator();
        var rebate = new Rebate { Amount = 75m };
        var product = new Product { SupportedIncentives = SupportedIncentiveType.AmountPerUom };

        Assert.False(calc.CanCalculate(rebate, product, new CalculateRebateRequest()));
    }

    // ---- FixedRateRebate ----

    [Fact]
    public void FixedRateRebate_Calculates_PriceTimesPercentageTimesVolume()
    {
        var calc = new FixedRateRebateCalculator();
        var rebate = new Rebate { Percentage = 0.2m };
        var product = new Product { SupportedIncentives = SupportedIncentiveType.FixedRateRebate, Price = 50m };
        var request = new CalculateRebateRequest { Volume = 4m };

        Assert.True(calc.CanCalculate(rebate, product, request));
        Assert.Equal(40m, calc.Calculate(rebate, product, request)); // 50 * 0.2 * 4
    }

    [Theory]
    [InlineData(0, 50, 4)]   // percentage zero
    [InlineData(0.2, 0, 4)]  // price zero
    [InlineData(0.2, 50, 0)] // volume zero
    public void FixedRateRebate_CannotCalculate_WhenAnyFactorIsZero(decimal percentage, decimal price, decimal volume)
    {
        var calc = new FixedRateRebateCalculator();
        var rebate = new Rebate { Percentage = percentage };
        var product = new Product { SupportedIncentives = SupportedIncentiveType.FixedRateRebate, Price = price };
        var request = new CalculateRebateRequest { Volume = volume };

        Assert.False(calc.CanCalculate(rebate, product, request));
    }

    // ---- AmountPerUom ----

    [Fact]
    public void AmountPerUom_Calculates_AmountTimesVolume()
    {
        var calc = new AmountPerUomCalculator();
        var rebate = new Rebate { Amount = 10m };
        var product = new Product { SupportedIncentives = SupportedIncentiveType.AmountPerUom };
        var request = new CalculateRebateRequest { Volume = 6m };

        Assert.True(calc.CanCalculate(rebate, product, request));
        Assert.Equal(60m, calc.Calculate(rebate, product, request)); // 10 * 6
    }

    [Theory]
    [InlineData(0, 6)] // amount zero
    [InlineData(10, 0)] // volume zero
    public void AmountPerUom_CannotCalculate_WhenAmountOrVolumeIsZero(decimal amount, decimal volume)
    {
        var calc = new AmountPerUomCalculator();
        var rebate = new Rebate { Amount = amount };
        var product = new Product { SupportedIncentives = SupportedIncentiveType.AmountPerUom };
        var request = new CalculateRebateRequest { Volume = volume };

        Assert.False(calc.CanCalculate(rebate, product, request));
    }
}
