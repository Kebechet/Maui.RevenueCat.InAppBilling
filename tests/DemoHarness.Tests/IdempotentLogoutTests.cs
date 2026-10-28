using Maui.RevenueCat.InAppBilling.Models;
using Maui.RevenueCat.InAppBilling.Services;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Xunit;
using PurchaseErrorStatus = Maui.RevenueCat.InAppBilling.Enums.PurchaseErrorStatus;

namespace DemoHarness.Tests;

public class IdempotentLogoutTests
{
    private readonly ILogger _logger = Substitute.For<ILogger>();
    private readonly CustomerInfoResultDto _currentCustomerInfoResult = new() { Value = CreateCustomerInfo() };
    private readonly CustomerInfoResultDto _loggedOutCustomerInfoResult = new() { Value = CreateCustomerInfo() };

    private int _logOutCallCount;
    private int _getCurrentCustomerInfoCallCount;

    private static CustomerInfoDto CreateCustomerInfo() => new()
    {
        ActiveSubscriptions = [],
        AllPurchasedIdentifiers = [],
        NonConsumablePurchases = [],
        FirstSeen = null,
        LatestExpirationDate = null,
        ManagementUrl = null,
        Entitlements = [],
    };

    private Task<CustomerInfoResultDto> GetCurrentCustomerInfo()
    {
        _getCurrentCustomerInfoCallCount++;
        return Task.FromResult(_currentCustomerInfoResult);
    }

    private Task<CustomerInfoResultDto> LogOut()
    {
        _logOutCallCount++;
        return Task.FromResult(_loggedOutCustomerInfoResult);
    }

    [Fact]
    public async Task Run_IdentifiedUser_LogsOut()
    {
        var logoutResult = await IdempotentLogout.Run(() => false, GetCurrentCustomerInfo, LogOut, _logger);

        Assert.Equal(1, _logOutCallCount);
        Assert.Same(_loggedOutCustomerInfoResult, logoutResult);
    }

    [Fact]
    public async Task Run_AnonymousUser_SucceedsWithCurrentCustomerInfoWithoutLoggingOut()
    {
        var logoutResult = await IdempotentLogout.Run(() => true, GetCurrentCustomerInfo, LogOut, _logger);

        Assert.Equal(0, _logOutCallCount);
        Assert.Equal(1, _getCurrentCustomerInfoCallCount);
        Assert.Same(_currentCustomerInfoResult, logoutResult);
        Assert.True(logoutResult.IsSuccess);
    }

    [Fact]
    public async Task Run_AnonymousUser_LogsNoError()
    {
        await IdempotentLogout.Run(() => true, GetCurrentCustomerInfo, LogOut, _logger);

        Assert.DoesNotContain(_logger.ReceivedCalls(), x =>
            x.GetMethodInfo().Name == nameof(ILogger.Log) &&
            (LogLevel)x.GetArguments()[0]! >= LogLevel.Warning);
    }

    /// <summary>
    /// RevenueCat switches to a new anonymous ID before its logOut callback returns, so a second Logout started
    /// while the first is still in flight finds the user anonymous (Sentry SATISFIT-APP-78 in the SatisFIT app).
    /// </summary>
    [Fact]
    public async Task Run_SecondLogoutWhileFirstInFlight_LogsOutOnceAndBothSucceed()
    {
        var isAnonymous = false;
        var firstLogOut = new TaskCompletionSource<CustomerInfoResultDto>();
        Func<Task<CustomerInfoResultDto>> logOutSwitchingToAnonymousBeforeCompleting = () =>
        {
            _logOutCallCount++;
            isAnonymous = true;
            return firstLogOut.Task;
        };

        var firstLogout = IdempotentLogout.Run(() => isAnonymous, GetCurrentCustomerInfo, logOutSwitchingToAnonymousBeforeCompleting, _logger);
        var secondLogout = IdempotentLogout.Run(() => isAnonymous, GetCurrentCustomerInfo, logOutSwitchingToAnonymousBeforeCompleting, _logger);
        firstLogOut.SetResult(_loggedOutCustomerInfoResult);
        var logoutResults = await Task.WhenAll(firstLogout, secondLogout);

        Assert.Equal(1, _logOutCallCount);
        Assert.All(logoutResults, x => Assert.True(x.IsSuccess));
    }

    [Fact]
    public async Task Run_IdentifiedUserLogoutFails_ReturnsTheFailure()
    {
        var failedLogoutResult = new CustomerInfoResultDto { Error = PurchaseErrorStatus.NetworkError };

        var logoutResult = await IdempotentLogout.Run(() => false, GetCurrentCustomerInfo, () => Task.FromResult(failedLogoutResult), _logger);

        Assert.Same(failedLogoutResult, logoutResult);
        Assert.Equal(0, _getCurrentCustomerInfoCallCount);
    }
}
