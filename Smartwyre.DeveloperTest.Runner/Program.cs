using System;
using System.Collections.Generic;
using Smartwyre.DeveloperTest.Data;
using Smartwyre.DeveloperTest.Services;
using Smartwyre.DeveloperTest.Services.Incentives;
using Smartwyre.DeveloperTest.Types;

namespace Smartwyre.DeveloperTest.Runner;

class Program
{
    static void Main(string[] args)
    {
        // Wire everything up by hand - no DI container for a console app this small.
        IRebateService rebateService = new RebateService(
            new RebateDataStore(),
            new ProductDataStore(),
            new IIncentiveCalculator[]
            {
                new FixedCashAmountCalculator(),
                new FixedRateRebateCalculator(),
                new AmountPerUomCalculator()
            });

        // Args: rebateId productId volume. Anything missing is asked for at the prompt.
        var request = new CalculateRebateRequest
        {
            RebateIdentifier = ReadArgOrPrompt(args, 0, "Rebate identifier"),
            ProductIdentifier = ReadArgOrPrompt(args, 1, "Product identifier"),
            Volume = ReadVolume(args)
        };

        var result = rebateService.Calculate(request);

        Console.WriteLine();
        if (result.Success)
        {
            Console.WriteLine($"Rebate calculated successfully. Amount: {result.Amount}");
        }
        else
        {
            Console.WriteLine("Rebate could not be calculated for the supplied request.");
        }
    }

    private static string ReadArgOrPrompt(IReadOnlyList<string> args, int index, string label)
    {
        if (index < args.Count && !string.IsNullOrWhiteSpace(args[index]))
        {
            return args[index];
        }

        Console.Write($"{label}: ");
        return Console.ReadLine() ?? string.Empty;
    }

    private static decimal ReadVolume(IReadOnlyList<string> args)
    {
        var raw = ReadArgOrPrompt(args, 2, "Volume");
        decimal volume;
        while (!decimal.TryParse(raw, out volume))
        {
            Console.Write("Please enter a valid numeric volume: ");
            raw = Console.ReadLine() ?? string.Empty;
        }

        return volume;
    }
}
