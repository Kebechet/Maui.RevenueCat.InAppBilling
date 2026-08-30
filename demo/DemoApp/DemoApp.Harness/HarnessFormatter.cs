using Maui.RevenueCat.InAppBilling.Enums;
using Maui.RevenueCat.InAppBilling.Models;
using Maui.RevenueCat.InAppBilling.Services;
using Types.Result;

namespace DemoApp.Harness;

/// <summary>
/// Renders <see cref="IRevenueCatBilling"/> results as harness log lines. Lives here rather than
/// in the page so the formatting is reachable from tests - the interactive flows can't be driven
/// by <see cref="HarnessRunner"/>.
/// </summary>
public static class HarnessFormatter
{
    public static string FormatCustomerInfo(CustomerInfoDto? customerInfo)
    {
        if (customerInfo is null)
        {
            return "null";
        }

        var entitlementIdentifiers = customerInfo.Entitlements.Any()
            ? string.Join(", ", customerInfo.Entitlements.Select(x => x.Identifier))
            : "none";

        return $"{customerInfo.ActiveSubscriptions.Count} active sub(s), {customerInfo.AllPurchasedIdentifiers.Count} purchased, entitlements: {entitlementIdentifiers}";
    }

    public static string FormatCustomerInfoResult(CustomerInfoResultDto customerInfoResult, string operationName)
    {
        return customerInfoResult.IsSuccess
            ? $"PASS {operationName}: {FormatCustomerInfo(customerInfoResult.Value)}"
            : $"FAIL {operationName}: {FormatError(customerInfoResult)}";
    }

    /// <summary>
    /// Renders the failure side of any billing result: the closed <see cref="PurchaseErrorStatus"/>
    /// plus the store SDK exception message when there is one.
    /// </summary>
    public static string FormatError<TError>(Result<TError> result)
        where TError : struct
    {
        var errorMessage = result.ErrorException?.Message;

        return string.IsNullOrEmpty(errorMessage)
            ? $"{result.Error}"
            : $"{result.Error}: {errorMessage}";
    }
}
