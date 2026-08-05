using Maui.RevenueCat.InAppBilling.Services;

namespace DemoApp;

public partial class App : Application
{
    private readonly IRevenueCatBilling _revenueCatBilling;

    public App(IRevenueCatBilling revenueCatBilling)
    {
        InitializeComponent();

        _revenueCatBilling = revenueCatBilling;
    }

    protected override void OnStart()
    {
        _revenueCatBilling.Initialize(DemoStoreSelection.ActiveApiKey);

        base.OnStart();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}
