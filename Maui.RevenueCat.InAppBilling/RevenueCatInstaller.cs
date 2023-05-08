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

        RevenueCatBilling.EnableDebugLogs(forceEnableDebugLogs.Value);

        services.AddSingleton<IRevenueCatBilling, RevenueCatBilling>();

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
