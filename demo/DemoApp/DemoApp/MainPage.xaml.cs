using System.Collections.ObjectModel;
using System.Globalization;
using DemoApp.Harness;
using Maui.RevenueCat.InAppBilling.Models;
using Maui.RevenueCat.InAppBilling.Services;

namespace DemoApp;

public partial class MainPage : ContentPage
{
    private readonly IRevenueCatBilling _revenueCatBilling;
    private readonly HarnessLog _harnessLog = new();
    private readonly HarnessRunner _harnessRunner;
    private bool _hasAutoRunStarted;

    public ObservableCollection<HarnessCheckResult> HarnessCheckResults { get; } = [];
    public ObservableCollection<PackageDto> LoadedPackages { get; } = [];

    public MainPage(IRevenueCatBilling revenueCatBilling)
    {
        InitializeComponent();
        _revenueCatBilling = revenueCatBilling;
        _harnessRunner = new HarnessRunner(revenueCatBilling, _harnessLog);
        BindingContext = this;
        _harnessLog.Changed += OnHarnessLogChanged;
        StorePicker.SelectedIndex = (int)DemoStoreSelection.Current;
    }

    private async void StoreChanged(object sender, EventArgs e)
    {
        var selectedStore = (DemoStore)StorePicker.SelectedIndex;
        if (selectedStore == DemoStoreSelection.Current)
        {
            return;
        }

        DemoStoreSelection.Current = selectedStore;
        _harnessLog.Add($"Store changed to {selectedStore}");
        await DisplayAlert("Store changed", "Restart the app to apply the selected store.", "OK");
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await RefreshStatus();
        // HARNESS_AUTORUN=1 (e.g. via `simctl launch` child env) runs all checks without UI
        // interaction, so headless simulator/CI runs can capture a full checklist screenshot.
        if (Environment.GetEnvironmentVariable("HARNESS_AUTORUN") == "1" && !_hasAutoRunStarted)
        {
            _hasAutoRunStarted = true;
            RunAllChecks(RunAllChecksButton, EventArgs.Empty);
        }
    }

    private void OnHarnessLogChanged()
    {
        Dispatcher.Dispatch(() => LogEditor.Text = _harnessLog.AsText());
    }

    private async void RunAllChecks(object sender, EventArgs e)
    {
        if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
        {
            _harnessLog.Add("WARNING: no internet connection, checks will likely fail");
        }

        RunAllChecksButton.IsEnabled = false;
        HarnessCheckResults.Clear();
        try
        {
            var progress = new Progress<HarnessCheckResult>(OnCheckProgress);
            await Task.Run(() => _harnessRunner.RunAllChecksAsync(progress));
            RefreshLoadedPackages();
            await RefreshStatus();
        }
        catch (Exception exception)
        {
            _harnessLog.Add($"FAIL {nameof(RunAllChecks)}: {exception.Message}");
        }
        finally
        {
            RunAllChecksButton.IsEnabled = true;
        }
    }

    private void OnCheckProgress(HarnessCheckResult harnessCheckResult)
    {
        var existingResult = HarnessCheckResults.FirstOrDefault(x => x.Name == harnessCheckResult.Name);
        if (existingResult is null)
        {
            HarnessCheckResults.Add(harnessCheckResult);
            return;
        }
        HarnessCheckResults[HarnessCheckResults.IndexOf(existingResult)] = harnessCheckResult;
    }

    private void RefreshLoadedPackages()
    {
        LoadedPackages.Clear();
        var loadedPackages = _harnessRunner.LastLoadedOfferings
            .SelectMany(x => x.AvailablePackages)
            .ToList();
        foreach (var loadedPackage in loadedPackages)
        {
            LoadedPackages.Add(loadedPackage);
        }
        NoPackagesHintLabel.IsVisible = !LoadedPackages.Any();
    }

    private async Task RefreshStatus()
    {
        try
        {
            var isInitialized = _revenueCatBilling.IsInitialized();
            var isAnonymous = _revenueCatBilling.IsAnonymous();
            var appUserId = _revenueCatBilling.GetAppUserId();
            var storefrontCountryCode = await _revenueCatBilling.GetStorefrontCountryCode();
            var activeApiKey = DemoStoreSelection.ActiveApiKey;
            var activeApiKeyPrefix = activeApiKey.Length > 5 ? activeApiKey[..5] : activeApiKey;
            Dispatcher.Dispatch(() =>
            {
                StoreLabel.Text = $"Store: {DemoStoreSelection.Current} (key {activeApiKeyPrefix}...)";
                InitializedLabel.Text = $"Initialized: {isInitialized}";
                AnonymousLabel.Text = $"Anonymous: {isAnonymous}";
                AppUserIdLabel.Text = $"App user id: {appUserId}";
                StorefrontLabel.Text = $"Storefront: {(string.IsNullOrEmpty(storefrontCountryCode) ? "?" : storefrontCountryCode)}";
                CultureLabel.Text = $"Culture: {CultureInfo.CurrentCulture.Name} | UI: {CultureInfo.CurrentUICulture.Name}";
            });
        }
        catch (Exception exception)
        {
            _harnessLog.Add($"FAIL {nameof(RefreshStatus)}: {exception.Message}");
        }
    }

    private async void PurchasePackage(object sender, EventArgs e)
    {
        var purchaseButton = (Button)sender;
        var packageToPurchase = (PackageDto)purchaseButton.BindingContext;
        purchaseButton.IsEnabled = false;
        _harnessLog.Add($"Purchasing {packageToPurchase.Identifier} ({packageToPurchase.Product.Sku})");
        try
        {
            // Deliberately called from a background thread: the wrapper must marshal the purchase
            // to the main thread itself (the Test Store dialog crashed here before that fix),
            // so this doubles as a regression canary.
            var purchaseResult = await Task.Run(() => _revenueCatBilling.PurchaseProduct(packageToPurchase));
            _harnessLog.Add(purchaseResult.IsSuccess
                ? $"PASS Purchase {packageToPurchase.Identifier}: transaction {purchaseResult.Transaction?.TransactionIdentifier ?? "?"}"
                : $"FAIL Purchase {packageToPurchase.Identifier}: {purchaseResult.ErrorStatus} {purchaseResult.ErrorMessage}");
            await RefreshStatus();
        }
        catch (Exception exception)
        {
            _harnessLog.Add($"FAIL Purchase {packageToPurchase.Identifier}: {exception.Message}");
        }
        finally
        {
            purchaseButton.IsEnabled = true;
        }
    }

    private async void RestoreTransactions(object sender, EventArgs e)
    {
        try
        {
            var customerInfo = await _revenueCatBilling.RestoreTransactions();
            _harnessLog.Add($"PASS {nameof(IRevenueCatBilling.RestoreTransactions)}: {FormatCustomerInfo(customerInfo)}");
            await RefreshStatus();
        }
        catch (Exception exception)
        {
            _harnessLog.Add($"FAIL {nameof(IRevenueCatBilling.RestoreTransactions)}: {exception.Message}");
        }
    }

    private async void Login(object sender, EventArgs e)
    {
        var appUserId = LoginAppUserIdEntry.Text;
        if (string.IsNullOrWhiteSpace(appUserId))
        {
            _harnessLog.Add("Login skipped: enter an app user id first");
            return;
        }

        try
        {
            var customerInfo = await _revenueCatBilling.Login(appUserId);
            _harnessLog.Add($"PASS {nameof(IRevenueCatBilling.Login)} as {appUserId}: {FormatCustomerInfo(customerInfo)}");
            await RefreshStatus();
        }
        catch (Exception exception)
        {
            _harnessLog.Add($"FAIL {nameof(IRevenueCatBilling.Login)}: {exception.Message}");
        }
    }

    private async void Logout(object sender, EventArgs e)
    {
        try
        {
            var customerInfo = await _revenueCatBilling.Logout();
            _harnessLog.Add($"PASS {nameof(IRevenueCatBilling.Logout)}: {FormatCustomerInfo(customerInfo)}");
            await RefreshStatus();
        }
        catch (Exception exception)
        {
            _harnessLog.Add($"FAIL {nameof(IRevenueCatBilling.Logout)}: {exception.Message}");
        }
    }

    private async void OpenManagementUrl(object sender, EventArgs e)
    {
        try
        {
            var managementUrl = await _revenueCatBilling.GetManagementSubscriptionUrl();
            if (managementUrl is null)
            {
                _harnessLog.Add($"{nameof(IRevenueCatBilling.GetManagementSubscriptionUrl)}: null (no active store subscription)");
                return;
            }
            _harnessLog.Add($"Opening {managementUrl}");
            await Launcher.OpenAsync(managementUrl);
        }
        catch (Exception exception)
        {
            _harnessLog.Add($"FAIL {nameof(OpenManagementUrl)}: {exception.Message}");
        }
    }

    private async void CopyLog(object sender, EventArgs e)
    {
        await Clipboard.SetTextAsync(_harnessLog.AsText());
        _harnessLog.Add("Log copied to clipboard");
    }

    private static string FormatCustomerInfo(CustomerInfoDto? customerInfo)
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
}
