using System.Collections.ObjectModel;
using System.Globalization;
using DemoApp.Harness;
using Maui.RevenueCat.InAppBilling.Models;
using Maui.RevenueCat.InAppBilling.Services;

namespace DemoApp;

public partial class MainPage : ContentPage
{
    private const string HarnessLogFileName = "harness-log.txt";

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
        await WaitForInitialization();
        await RefreshStatus();
        // HARNESS_AUTORUN=1 (e.g. via `simctl launch` child env) runs all checks without UI
        // interaction, so headless simulator/CI runs can capture a full checklist screenshot.
        if (Environment.GetEnvironmentVariable("HARNESS_AUTORUN") == "1" && !_hasAutoRunStarted)
        {
            _hasAutoRunStarted = true;
            RunAllChecks(RunAllChecksButton, EventArgs.Empty);
        }
    }

    /// <summary>
    /// App.OnStart calls Initialize after the page first appears on a cold start, so waiting
    /// here keeps the first status refresh (and autorun) from hitting the uninitialized SDK.
    /// </summary>
    private async Task WaitForInitialization()
    {
        for (var attemptIndex = 0; attemptIndex < 50; attemptIndex++)
        {
            if (_revenueCatBilling.IsInitialized())
            {
                return;
            }
            await Task.Delay(100);
        }
        _harnessLog.Add("WARNING: SDK still not initialized after 5 s, continuing anyway");
    }

    private void OnHarnessLogChanged()
    {
        Dispatcher.Dispatch(() => LogEditor.Text = _harnessLog.AsText());
        PersistLogForHeadlessCapture();
    }

    /// <summary>
    /// Mirrors the harness log to a file in the app data directory so headless
    /// runs (autorun driven over SSH or CI) can read results without UI access.
    /// </summary>
    private void PersistLogForHeadlessCapture()
    {
        try
        {
            File.WriteAllText(Path.Combine(FileSystem.AppDataDirectory, HarnessLogFileName), _harnessLog.AsText());
        }
        catch (IOException)
        {
        }
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
            var restoreResult = await _revenueCatBilling.RestoreTransactions();
            _harnessLog.Add(HarnessFormatter.FormatCustomerInfoResult(restoreResult, nameof(IRevenueCatBilling.RestoreTransactions)));
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
            var loginResult = await _revenueCatBilling.Login(appUserId);
            _harnessLog.Add(HarnessFormatter.FormatCustomerInfoResult(loginResult, $"{nameof(IRevenueCatBilling.Login)} as {appUserId}"));
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
            var logoutResult = await _revenueCatBilling.Logout();
            _harnessLog.Add(HarnessFormatter.FormatCustomerInfoResult(logoutResult, nameof(IRevenueCatBilling.Logout)));
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
            var managementUrlResult = await _revenueCatBilling.GetManagementSubscriptionUrl();
            if (managementUrlResult.IsError)
            {
                _harnessLog.Add($"FAIL {nameof(IRevenueCatBilling.GetManagementSubscriptionUrl)}: {managementUrlResult.ErrorStatus} {managementUrlResult.ErrorMessage}");
                return;
            }
            if (managementUrlResult.ManagementUrl is null)
            {
                _harnessLog.Add($"{nameof(IRevenueCatBilling.GetManagementSubscriptionUrl)}: null (no active store subscription)");
                return;
            }
            _harnessLog.Add($"Opening {managementUrlResult.ManagementUrl}");
            await Launcher.OpenAsync(managementUrlResult.ManagementUrl);
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

}
