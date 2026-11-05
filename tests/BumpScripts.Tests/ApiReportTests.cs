using Xunit;

namespace BumpScripts.Tests;

/// <summary>
/// <c>api-report</c>'s rebound check: a binding member whose C# signature is unchanged but whose
/// native target moved. Code written against the published binding keeps compiling and silently
/// calls the other method, so nothing else in the report would show it.
/// </summary>
public class ApiReportTests
{
    private const string InitWithCustomEntitlementComputation = "initWithAutoSyncPurchases:customEntitlementComputation:";
    private const string InitWithForceAllowTestStore = "initWithAutoSyncPurchases:forceAllowTestStoreInReleaseBuilds:";

    private static readonly string _platformAttributesPath = FixtureAssembly.Compile("PlatformAttributes", FixtureAssembly.PlatformAttributesSource);

    // RevenueCat iOS 5.102.1: a second (bool, bool) initializer took over
    // new RCDangerousSettings(bool, bool), and the report listed nothing for it.
    [Fact]
    public void ApiReport_ConstructorBindsAnotherSelector_IsListedAsReboundAndWrapperAffected()
    {
        // Arrange
        var baselinePath = CompileIosBinding(InitWithCustomEntitlementComputation);
        var newBindingPath = CompileIosBinding(InitWithForceAllowTestStore);
        var wrapperPath = FixtureAssembly.Compile("Maui.RevenueCat", """
            public static class Wrapper
            {
                public static object CreateDangerousSettings() => new Maui.RevenueCat.iOS.RCDangerousSettings(true, true);
            }
            """, baselinePath);

        // Act
        var (stdout, report) = RunApiReport("ios", newBindingPath, wrapperPath, baselinePath);

        // Assert
        Assert.Contains("api_rebound=1", stdout);
        Assert.Contains("api_wrapper_affected=1", stdout);
        Assert.Contains("#### 🛑 1 member(s) keep their C# signature but now call a different native method", report);
        Assert.Contains($"  - `.ctor(Boolean, Boolean)` - was `{InitWithCustomEntitlementComputation}`, now `{InitWithForceAllowTestStore}`", report);
    }

    [Fact]
    public void ApiReport_UnchangedSelectors_ReportsNoRebound()
    {
        // Arrange
        var baselinePath = CompileIosBinding(InitWithCustomEntitlementComputation);
        var newBindingPath = CompileIosBinding(InitWithCustomEntitlementComputation);
        var wrapperPath = FixtureAssembly.Compile("Maui.RevenueCat", "public static class Wrapper { }");

        // Act
        var (stdout, report) = RunApiReport("ios", newBindingPath, wrapperPath, baselinePath);

        // Assert
        Assert.Contains("api_rebound=0", stdout);
        Assert.DoesNotContain("🛑", report);
    }

    [Fact]
    public void ApiReport_PropertyGetterCallsAnotherJavaMethod_IsListedAsRebound()
    {
        // Arrange
        var baselinePath = CompileAndroidBinding("getAppUserID");
        var newBindingPath = CompileAndroidBinding("getAppUserId");
        var wrapperPath = FixtureAssembly.Compile("Maui.RevenueCat", "public static class Wrapper { }");

        // Act
        var (stdout, report) = RunApiReport("android", newBindingPath, wrapperPath, baselinePath);

        // Assert
        Assert.Contains("api_rebound=1", stdout);
        Assert.Contains("  - `AppUserID : String` - was `getAppUserID ()Ljava/lang/String;`, now `getAppUserId ()Ljava/lang/String;`", report);
    }

    private static string CompileIosBinding(string twoBoolInitializerSelector)
    {
        return FixtureAssembly.Compile("Maui.RevenueCat.iOS", $$"""
            namespace Maui.RevenueCat.iOS
            {
                public class RCDangerousSettings
                {
                    [Foundation.Export("{{twoBoolInitializerSelector}}")]
                    public RCDangerousSettings(bool autoSyncPurchases, bool option) { }

                    public bool AutoSyncPurchases { [Foundation.Export("autoSyncPurchases")] get => false; }
                }
            }
            """, _platformAttributesPath);
    }

    private static string CompileAndroidBinding(string appUserIdGetterName)
    {
        return FixtureAssembly.Compile("Maui.RevenueCat.Android", $$"""
            namespace Com.Revenuecat.Purchases
            {
                public class Purchases
                {
                    public string AppUserID
                    {
                        [Android.Runtime.Register("{{appUserIdGetterName}}", "()Ljava/lang/String;", "GetGetAppUserIDHandler")]
                        get => "";
                    }
                }
            }
            """, _platformAttributesPath);
    }

    private static (string Stdout, string Report) RunApiReport(string platform, string newBindingPath, string wrapperPath, string baselinePath)
    {
        var reportPath = BumpScript.WriteTempFile(string.Empty, ".md");
        var fullReportPath = BumpScript.WriteTempFile(string.Empty, ".md");
        var stdout = BumpScript.Run("api-report", platform, newBindingPath, wrapperPath, reportPath, fullReportPath, baselinePath);
        return (stdout, File.ReadAllText(reportPath));
    }
}
