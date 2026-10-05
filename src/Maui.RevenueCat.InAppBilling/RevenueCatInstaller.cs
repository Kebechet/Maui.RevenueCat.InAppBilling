using Maui.RevenueCat.InAppBilling.Services;

namespace Maui.RevenueCat.InAppBilling;

public static class RevenueCatBillingInstaller
{
    /// <param name="services">The app's service collection.</param>
    /// <param name="forceEnableDebugLogs">
    /// Enables RevenueCat's debug logs. When null, the default follows this library's own build
    /// configuration, not the app's, so it is off in the NuGet package and on only when the library
    /// is referenced as a Debug project.
    /// </param>
    public static IServiceCollection AddRevenueCatBilling(this IServiceCollection services,
        bool? forceEnableDebugLogs = null)
    {
        if (forceEnableDebugLogs is null)
        {
            forceEnableDebugLogs = IsDebug();
        }

        RevenueCatBilling.EnableDebugLogs(forceEnableDebugLogs.Value);

        services.AddSingleton<IRevenueCatBilling, RevenueCatBilling>();

        services.AddLogging();

        return services;
    }

    private static bool IsDebug()
    {
#if DEBUG
        return true;
#else
        return false;
#endif
    }
}
