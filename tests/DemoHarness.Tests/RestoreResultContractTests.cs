using Maui.RevenueCat.InAppBilling.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace DemoHarness.Tests;

public sealed class RestoreResultContractTests
{
    [Fact]
    public async Task RestoreTransactions_DetailedAndLegacyContractsReturnCustomerInformation()
    {
        var revenueCatBilling = new RevenueCatBilling(NullLogger<RevenueCatBilling>.Instance);

        var detailedResult = await revenueCatBilling.RestoreTransactionsWithResult(CancellationToken.None);
        var legacyCustomerInfo = await revenueCatBilling.RestoreTransactions(CancellationToken.None);

        Assert.True(detailedResult.IsSuccess);
        Assert.False(detailedResult.IsError);
        Assert.NotNull(detailedResult.CustomerInfo);
        Assert.NotNull(legacyCustomerInfo);
    }
}
