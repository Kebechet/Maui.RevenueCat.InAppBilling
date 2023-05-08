using Maui.RevenueCat.InAppBilling.Services;

namespace Maui.RevenueCat.InAppBilling;

public static class RevenueCatBillingInstaller
{
    public static IServiceCollection AddRevenueCatBilling(this IServiceCollection services,
        bool? forceEnableDebugLogs = null)
    {
        if (forceEnableDebugLogs is null)
        {
            forceEnableDebugLogs = IsDebug();
        }

#if __MOBILE__
        services.AddSingleton<IRevenueCatBilling, RevenueCatBilling>();
        RevenueCatBilling.EnableDebugLogs(forceEnableDebugLogs.Value);
#endif

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
