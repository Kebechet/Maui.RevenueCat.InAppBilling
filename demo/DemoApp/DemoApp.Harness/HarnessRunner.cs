using System.Diagnostics;
using Maui.RevenueCat.InAppBilling.Models;
using Maui.RevenueCat.InAppBilling.Services;

namespace DemoApp.Harness;

/// <summary>
/// Sequentially exercises every non-interactive <see cref="IRevenueCatBilling"/> method,
/// reporting one <see cref="HarnessCheckResult"/> per method. Interactive flows
/// (purchase, restore, login, logout) are driven from the UI instead.
/// </summary>
public sealed class HarnessRunner
{
    private static readonly TimeSpan _defaultPerCallTimeout = TimeSpan.FromSeconds(15);

    private readonly IRevenueCatBilling _revenueCatBilling;
    private readonly HarnessLog _harnessLog;
    private readonly TimeSpan _perCallTimeout;

    public List<OfferingDto> LastLoadedOfferings { get; private set; } = [];

    public HarnessRunner(IRevenueCatBilling revenueCatBilling, HarnessLog harnessLog, TimeSpan? perCallTimeout = null)
    {
        _revenueCatBilling = revenueCatBilling;
        _harnessLog = harnessLog;
        _perCallTimeout = perCallTimeout ?? _defaultPerCallTimeout;
    }

    public async Task<List<HarnessCheckResult>> RunAllChecksAsync(IProgress<HarnessCheckResult>? progress = null)
    {
        var harnessCheckResults = new List<HarnessCheckResult>();

        foreach (var (checkName, checkAction) in _orderedChecks)
        {
            progress?.Report(new HarnessCheckResult { Name = checkName, Status = HarnessCheckStatus.Running });
            var harnessCheckResult = await ExecuteCheck(checkName, checkAction);
            harnessCheckResults.Add(harnessCheckResult);
            progress?.Report(harnessCheckResult);
        }

        return harnessCheckResults;
    }

    private async Task<HarnessCheckResult> ExecuteCheck(string checkName, Func<Task<string>> checkAction)
    {
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var summary = await checkAction().WaitAsync(_perCallTimeout);
            _harnessLog.Add($"PASS {checkName} ({stopwatch.ElapsedMilliseconds} ms): {summary}");
            return new HarnessCheckResult { Name = checkName, Status = HarnessCheckStatus.Passed, Summary = summary };
        }
        catch (TimeoutException)
        {
            _harnessLog.Add($"TIMEOUT {checkName} after {_perCallTimeout.TotalSeconds:0} s");
            return new HarnessCheckResult
            {
                Name = checkName,
                Status = HarnessCheckStatus.TimedOut,
                Error = $"No response within {_perCallTimeout.TotalSeconds:0} s",
            };
        }
        catch (Exception exception)
        {
            _harnessLog.Add($"FAIL {checkName} ({stopwatch.ElapsedMilliseconds} ms): {exception.Message}");
            return new HarnessCheckResult { Name = checkName, Status = HarnessCheckStatus.Failed, Error = exception.Message };
        }
    }

    private IEnumerable<(string Name, Func<Task<string>> Action)> _orderedChecks
    {
        get
        {
            yield return (nameof(IRevenueCatBilling.IsInitialized), () => Task.FromResult(_revenueCatBilling.IsInitialized().ToString()));
            yield return (nameof(IRevenueCatBilling.IsAnonymous), () => Task.FromResult(_revenueCatBilling.IsAnonymous().ToString()));
            yield return (nameof(IRevenueCatBilling.GetAppUserId), () => Task.FromResult(_revenueCatBilling.GetAppUserId()));
            yield return (nameof(IRevenueCatBilling.CanMakePayments), async () => (await _revenueCatBilling.CanMakePayments()).ToString());
            yield return (nameof(IRevenueCatBilling.GetOfferings), async () =>
            {
                LastLoadedOfferings = await _revenueCatBilling.GetOfferings(false);
                var packageCount = LastLoadedOfferings.Sum(x => x.AvailablePackages.Count);
                return $"{LastLoadedOfferings.Count} offering(s), {packageCount} package(s)";
            }
            );
            yield return ($"{nameof(IRevenueCatBilling.GetOfferings)} forceRefresh", async () =>
            {
                var refreshedOfferings = await _revenueCatBilling.GetOfferings(true);
                return $"{refreshedOfferings.Count} offering(s)";
            }
            );
            yield return (nameof(IRevenueCatBilling.CheckTrialOrIntroDiscountEligibility), async () =>
            {
                var productSkus = LastLoadedOfferings
                    .SelectMany(x => x.AvailablePackages)
                    .Select(x => x.Product.Sku)
                    .Where(x => !string.IsNullOrEmpty(x))
                    .Distinct()
                    .ToList();
                if (!productSkus.Any())
                {
                    return "skipped, no products loaded";
                }
                var eligibilityStatuses = await _revenueCatBilling.CheckTrialOrIntroDiscountEligibility(productSkus);
                return eligibilityStatuses.Any()
                    ? string.Join(", ", eligibilityStatuses.Select(x => $"{x.Key}={x.Value}"))
                    : "empty result";
            }
            );
            yield return (nameof(IRevenueCatBilling.GetCustomerInfo), async () =>
            {
                var customerInfo = await _revenueCatBilling.GetCustomerInfo();
                return customerInfo is null
                    ? "null"
                    : $"{customerInfo.ActiveSubscriptions.Count} active sub(s), {customerInfo.Entitlements.Count} entitlement(s)";
            }
            );
            yield return (nameof(IRevenueCatBilling.GetActiveSubscriptions), async () =>
            {
                var activeSubscriptions = await _revenueCatBilling.GetActiveSubscriptions();
                return activeSubscriptions.Any() ? string.Join(", ", activeSubscriptions) : "none";
            }
            );
            yield return (nameof(IRevenueCatBilling.GetAllPurchasedIdentifiers), async () =>
            {
                var purchasedIdentifiers = await _revenueCatBilling.GetAllPurchasedIdentifiers();
                return purchasedIdentifiers.Any() ? string.Join(", ", purchasedIdentifiers) : "none";
            }
            );
            yield return (nameof(IRevenueCatBilling.GetPurchaseDateForProductIdentifier), async () =>
            {
                var firstPurchasedIdentifier = (await _revenueCatBilling.GetAllPurchasedIdentifiers()).FirstOrDefault();
                if (firstPurchasedIdentifier is null)
                {
                    return "skipped, nothing purchased yet";
                }
                var purchaseDate = await _revenueCatBilling.GetPurchaseDateForProductIdentifier(firstPurchasedIdentifier);
                return $"{firstPurchasedIdentifier}: {purchaseDate?.ToString("O") ?? "null"}";
            }
            );
            yield return (nameof(IRevenueCatBilling.GetManagementSubscriptionUrl), async () =>
                await _revenueCatBilling.GetManagementSubscriptionUrl() ?? "null (no active store subscription)");
            yield return (nameof(IRevenueCatBilling.GetStorefrontCountryCode), async () =>
            {
                var storefrontCountryCode = await _revenueCatBilling.GetStorefrontCountryCode();
                return string.IsNullOrEmpty(storefrontCountryCode) ? "empty" : storefrontCountryCode;
            }
            );
            yield return (nameof(IRevenueCatBilling.SetEmail), () =>
            {
                _revenueCatBilling.SetEmail("harness@example.com");
                return Task.FromResult("sent, verify in RevenueCat dashboard");
            }
            );
            yield return (nameof(IRevenueCatBilling.SetDisplayName), () =>
            {
                _revenueCatBilling.SetDisplayName("Harness Tester");
                return Task.FromResult("sent, verify in RevenueCat dashboard");
            }
            );
            yield return (nameof(IRevenueCatBilling.SetPhoneNumber), () =>
            {
                _revenueCatBilling.SetPhoneNumber("+420123456789");
                return Task.FromResult("sent, verify in RevenueCat dashboard");
            }
            );
            yield return (nameof(IRevenueCatBilling.SetAttributes), () =>
            {
                _revenueCatBilling.SetAttributes(new Dictionary<string, string> { ["harness_run_at"] = DateTime.UtcNow.ToString("O") });
                return Task.FromResult("sent, verify in RevenueCat dashboard");
            }
            );
        }
    }
}
