# Smartwyre Developer Test Instructions

You have been selected to complete our candidate coding exercise. Please follow the directions in this readme.

Clone, **DO NOT FORK**, this repository to your account on the online Git resource of your choosing (GitHub, BitBucket, GitLab, etc.). Your solution should retain previous commit history and you should utilize best practices for committing your changes to the repository.

You are welcome to use whatever tools you normally would when coding — including documentation, libraries, frameworks, or AI tools (such as ChatGPT or Copilot).

However, it is important that you fully understand your solution. As part of the interview process, we will review your code with you in detail. You should be able to:

- Explain the design choices you made.
- Walk us through how your solution works.
- Make modifications or extensions to your code during the review.

Please note: if your submission appears to have been generated entirely by an AI agent or another third party, without your own understanding or contribution, it will not meet our evaluation criteria.

# The Exercise

In the 'RebateService.cs' file you will find a method for calculating a rebate. At a high level the steps for calculating a rebate are:

 1. Lookup the rebate that the request is being made against.
 2. Lookup the product that the request is being made against.
 2. Check that the rebate and request are valid to calculate the incentive type rebate.
 3. Store the rebate calculation.

What we'd like you to do is refactor the code with the following things in mind:

 - Adherence to SOLID principles
 - Testability
 - Readability
 - Currently there are 3 known incentive types. In the future the business will want to add many more incentive types. Your solution should make it easy for developers to add new incentive types in the future.

We’d also like you to 
 - Add some unit tests to the Smartwyre.DeveloperTest.Tests project to show how you would test the code that you’ve produced 
 - Run the RebateService from the Smartwyre.DeveloperTest.Runner console application accepting inputs (either via command line arguments or via prompts is fine)

The only specific "rules" are:

- The solution must build
- All tests must pass

You are free to use any frameworks/NuGet packages that you see fit. You should plan to spend around 1 hour completing the exercise.

Feel free to use code comments to describe your changes. You are also welcome to update this readme with any important details for us to consider.

Once you have completed the exercise either ensure your repository is available publicly or contact the hiring manager to set up a private share.

---

# Solution Notes

## How to run

```bash
dotnet build          # builds the solution
dotnet test           # runs the unit tests (16 tests)

# Run the console app with command-line arguments: rebateId productId volume
dotnet run --project Smartwyre.DeveloperTest.Runner -- reb-1 prod-1 5

# Or run with no arguments and it will prompt for each value
dotnet run --project Smartwyre.DeveloperTest.Runner
```

> Note: the data stores are stubs (their DB code was removed "for brevity"), so they return
> empty `Rebate`/`Product` objects. With default data the calculation reports that it could
> not be calculated, which is the correct behaviour. The unit tests supply real data via mocks.

## What I changed

The starting point was a single `Calculate` method built around one big `switch`. It was hard
to test (it `new`ed its own data stores) and every new incentive type meant editing that switch.
With the brief saying "many more incentive types" are coming, that was the main thing to fix.

**Dependencies are injected now.** `RebateService` takes `IRebateDataStore` and
`IProductDataStore` through its constructor instead of newing them up, so the tests can hand it
mocks and it never touches a real database.

**Each incentive type is its own calculator.** I pulled the per-type rules out of the switch into
an `IIncentiveCalculator` with one implementation per type (`FixedCashAmountCalculator`,
`FixedRateRebateCalculator`, `AmountPerUomCalculator`, under `Services/Incentives`). Each one
decides whether it applies (`CanCalculate`) and how much to pay (`Calculate`). `RebateService`
just looks up the rebate and product, picks the matching calculator by `IncentiveType` and
delegates.

Adding a new incentive type is then: add the enum value, write a new calculator, register it in
`Program.cs`. Nothing existing has to change.

**A few bugs came out in the wash:**
- The old null checks sat *after* `switch (rebate.Incentive)`, which had already dereferenced
  `rebate` — so they never actually ran. The null check happens up front now.
- `FixedCashAmount` never null-checked the product while the other two did. It's consistent now.
- `CalculateRebateResult` carries the calculated `Amount` so the caller can see the value, not
  just success/fail.

## Tests

`IncentiveCalculatorTests` covers each calculator on its own — the formulas and the
zero/unsupported edge cases. `RebateServiceTests` covers the orchestration with mocked stores,
including that the result is stored only on success. Mocking is done with Moq.
