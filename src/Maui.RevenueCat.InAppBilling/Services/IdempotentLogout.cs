using Maui.RevenueCat.InAppBilling.Models;
using Microsoft.Extensions.Logging;

namespace Maui.RevenueCat.InAppBilling.Services;

/// <summary>
/// The platform-independent part of <see cref="IRevenueCatBilling.Logout"/>, shared by the Android and iOS
/// implementations so it can be tested without a native SDK.
/// </summary>
internal static class IdempotentLogout
{
    /// <summary>
    /// Logs the identified user out. When the user is already anonymous, it returns the current customer info as a
    /// success instead, without calling the native logOut.
    /// </summary>
    /// <remarks>
    /// Both native SDKs refuse logOut for an anonymous user with LogOutAnonymousUserError, yet an anonymous user is the
    /// state a logout is meant to reach. Callers get there without a mistake of their own: RevenueCat switches to a new
    /// anonymous ID before its logOut callback returns (after syncing subscriber attributes, before fetching customer
    /// info), so a second logout started while the first is in flight finds the user anonymous.
    /// </remarks>
    internal static async Task<CustomerInfoResultDto> Run(
        Func<bool> isAnonymous,
        Func<Task<CustomerInfoResultDto>> getCurrentCustomerInfo,
        Func<Task<CustomerInfoResultDto>> logOutIdentifiedUser,
        ILogger logger)
    {
        if (isAnonymous())
        {
            logger.LogDebug("Logout skipped: the RevenueCat user is already anonymous.");
            return await getCurrentCustomerInfo();
        }

        return await logOutIdentifiedUser();
    }
}
