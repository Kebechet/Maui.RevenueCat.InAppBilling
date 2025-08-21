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
        var revenueCatApiKey = string.Empty;

#if __ANDROID__
    revenueCatApiKey = "goog_tHAVxtQQbsAWBRGLrEwezuavnmI";
#elif __IOS__
        revenueCatApiKey = "appl_IbYjYDwWLoqUZXxFXrCCyJKLPZc";
#endif

        _revenueCatBilling.Initialize(revenueCatApiKey);

        base.OnStart();
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }
}
