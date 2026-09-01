using System.Diagnostics;
using Maui.RevenueCat.InAppBilling.Enums;
using Maui.RevenueCat.InAppBilling.Models;
using Maui.RevenueCat.InAppBilling.Services;
using Types.Result;

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

    /// <summary>
    /// Unwraps a billing result, turning a reported store failure into the exception that
    /// <see cref="ExecuteCheck"/> already knows how to report - so an error result shows as FAILED
    /// rather than as a check that passed while carrying an error message.
    /// </summary>
    private static TValue? Unwrap<TValue>(DataResult<TValue, PurchaseErrorStatus> result)
    {
        if (!result.IsError)
        {
            return result.Value;
        }

        // The platform implementations catch OperationCanceledException and report it as a
        // cancelled result, so the per-call timeout no longer reaches ExecuteCheck as an
        // exception. Rethrow it as one, or the timeout is misreported as a plain failure.
        if (result.Error == PurchaseErrorStatus.PurchaseCancelledError)
        {
            throw new OperationCanceledException(HarnessFormatter.FormatError(result), result.ErrorException);
        }

        throw new InvalidOperationException(HarnessFormatter.FormatError(result), result.ErrorException);
    }

    private async Task<HarnessCheckResult> ExecuteCheck(string checkName, Func<CancellationToken, Task<string>> checkAction)
    {
        var stopwatch = Stopwatch.StartNew();
        // The token cancels checks that honor it; WaitAsync is the backstop for native calls
        // that ignore cancellation and would otherwise hang the whole sweep.
        using var timeoutCancellationTokenSource = new CancellationTokenSource(_perCallTimeout);
        try
        {
            var summary = await checkAction(timeoutCancellationTokenSource.Token).WaitAsync(_perCallTimeout);
            _harnessLog.Add($"PASS {checkName} ({stopwatch.ElapsedMilliseconds} ms): {summary}");
            return new HarnessCheckResult { Name = checkName, Status = HarnessCheckStatus.Passed, Summary = summary };
        }
        catch (NotImplementedException exception)
        {
            _harnessLog.Add($"SKIP {checkName}: {exception.Message}");
            return new HarnessCheckResult
            {
                Name = checkName,
                Status = HarnessCheckStatus.Skipped,
                Summary = $"not supported on this platform ({exception.Message})",
            };
        }
        catch (Exception exception) when (exception is TimeoutException or OperationCanceledException)
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

    private IEnumerable<(string Name, Func<CancellationToken, Task<string>> Action)> _orderedChecks
    {
        get
        {
            yield return (nameof(IRevenueCatBilling.IsInitialized), _ => Task.FromResult(_revenueCatBilling.IsInitialized().ToString()));
            yield return (nameof(IRevenueCatBilling.IsAnonymous), _ => Task.FromResult(_revenueCatBilling.IsAnonymous().ToString()));
            yield return (nameof(IRevenueCatBilling.GetAppUserId), _ => Task.FromResult(_revenueCatBilling.GetAppUserId()));
            yield return (nameof(IRevenueCatBilling.CanMakePayments), async cancellationToken =>
                Unwrap(await _revenueCatBilling.CanMakePayments(cancellationToken)).ToString());
            yield return (nameof(IRevenueCatBilling.GetOfferings), async cancellationToken =>
            {
                LastLoadedOfferings = Unwrap(await _revenueCatBilling.GetOfferings(false, cancellationToken)) ?? [];
                var packageCount = LastLoadedOfferings.Sum(x => x.AvailablePackages.Count);
                return $"{LastLoadedOfferings.Count} offering(s), {packageCount} package(s)";
            }
            );
            yield return ($"{nameof(IRevenueCatBilling.GetOfferings)} forceRefresh", async cancellationToken =>
            {
                var refreshedOfferings = Unwrap(await _revenueCatBilling.GetOfferings(true, cancellationToken)) ?? [];
                return $"{refreshedOfferings.Count} offering(s)";
            }
            );
            yield return (nameof(IRevenueCatBilling.CheckTrialOrIntroDiscountEligibility), async cancellationToken =>
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
                var eligibilityStatuses = Unwrap(await _revenueCatBilling.CheckTrialOrIntroDiscountEligibility(productSkus, cancellationToken));
                return eligibilityStatuses is not null && eligibilityStatuses.Any()
                    ? string.Join(", ", eligibilityStatuses.Select(x => $"{x.Key}={x.Value}"))
                    : "empty result";
            }
            );
            yield return (nameof(IRevenueCatBilling.GetCustomerInfo), async cancellationToken =>
            {
                var customerInfo = Unwrap(await _revenueCatBilling.GetCustomerInfo(cancellationToken));
                return customerInfo is null
                    ? "null"
                    : $"{customerInfo.ActiveSubscriptions.Count} active sub(s), {customerInfo.Entitlements.Count} entitlement(s)";
            }
            );
            yield return (nameof(IRevenueCatBilling.GetActiveSubscriptions), async cancellationToken =>
            {
                var activeSubscriptions = Unwrap(await _revenueCatBilling.GetActiveSubscriptions(cancellationToken)) ?? [];
                return activeSubscriptions.Any() ? string.Join(", ", activeSubscriptions) : "none";
            }
            );
            yield return (nameof(IRevenueCatBilling.GetAllPurchasedIdentifiers), async cancellationToken =>
            {
                var purchasedIdentifiers = Unwrap(await _revenueCatBilling.GetAllPurchasedIdentifiers(cancellationToken)) ?? [];
                return purchasedIdentifiers.Any() ? string.Join(", ", purchasedIdentifiers) : "none";
            }
            );
            yield return (nameof(IRevenueCatBilling.GetPurchaseDateForProductIdentifier), async cancellationToken =>
            {
                var purchasedIdentifiers = Unwrap(await _revenueCatBilling.GetAllPurchasedIdentifiers(cancellationToken)) ?? [];
                var firstPurchasedIdentifier = purchasedIdentifiers.FirstOrDefault();
                if (firstPurchasedIdentifier is null)
                {
                    return "skipped, nothing purchased yet";
                }
                var purchaseDate = Unwrap(await _revenueCatBilling.GetPurchaseDateForProductIdentifier(firstPurchasedIdentifier, cancellationToken));
                return $"{firstPurchasedIdentifier}: {purchaseDate?.ToString("O") ?? "null"}";
            }
            );
            yield return (nameof(IRevenueCatBilling.GetManagementSubscriptionUrl), async cancellationToken =>
                Unwrap(await _revenueCatBilling.GetManagementSubscriptionUrl(cancellationToken)) ?? "null (no active store subscription)");
            yield return (nameof(IRevenueCatBilling.GetStorefrontCountryCode), async cancellationToken =>
            {
                var storefrontCountryCode = Unwrap(await _revenueCatBilling.GetStorefrontCountryCode(cancellationToken));
                return string.IsNullOrEmpty(storefrontCountryCode) ? "empty" : storefrontCountryCode;
            }
            );
            yield return (nameof(IRevenueCatBilling.SetEmail), _ =>
            {
                _revenueCatBilling.SetEmail("harness@example.com");
                return Task.FromResult("sent, verify in RevenueCat dashboard");
            }
            );
            yield return (nameof(IRevenueCatBilling.SetDisplayName), _ =>
            {
                _revenueCatBilling.SetDisplayName("Harness Tester");
                return Task.FromResult("sent, verify in RevenueCat dashboard");
            }
            );
            yield return (nameof(IRevenueCatBilling.SetPhoneNumber), _ =>
            {
                _revenueCatBilling.SetPhoneNumber("+420123456789");
                return Task.FromResult("sent, verify in RevenueCat dashboard");
            }
            );
            yield return (nameof(IRevenueCatBilling.SetAttributes), _ =>
            {
                _revenueCatBilling.SetAttributes(new Dictionary<string, string> { ["harness_run_at"] = DateTime.UtcNow.ToString("O") });
                return Task.FromResult("sent, verify in RevenueCat dashboard");
            }
            );
        }
    }
}
