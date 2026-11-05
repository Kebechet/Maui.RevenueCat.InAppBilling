using Xunit;

namespace BumpScripts.Tests;

/// <summary>
/// <c>nuget-versions</c>, the only way the Android binding repair reaches nuget.org. These run
/// against nuget.org itself: a published version's metadata never changes, and a fixture
/// would not catch the registration format moving under the command.
/// </summary>
public class NuGetVersionsTests
{
    [Fact]
    public void NuGetVersions_BillingClient_ShowsTheJavaArtifactAndTheNet9Group()
    {
        // Act
        var lines = BumpScript.Run("nuget-versions", "Xamarin.Android.Google.BillingClient", "net9.0-android").Split('\n');

        // Assert
        var line = Assert.Single(lines, x => x.StartsWith("9.1.0.2 |", StringComparison.Ordinal));
        Assert.StartsWith("9.1.0.2 | com.android.billingclient:billing:9.1.0 | net9.0-android35.0: ", line);
        Assert.Contains("Xamarin.AndroidX.Core >= 1.19.0.1", line);
    }

    [Fact]
    public void NuGetVersions_VersionShippedOnlyForNet10_IsMarkedNotUsable()
    {
        // Act
        var lines = BumpScript.Run("nuget-versions", "Xamarin.AndroidX.Core", "net9.0-android").Split('\n');

        // Assert
        var line = Assert.Single(lines, x => x.StartsWith("1.19.1 |", StringComparison.Ordinal));
        Assert.Equal("1.19.1 | androidx.core:core:1.19.1 | NOT usable from net9.0-android (groups: net10.0-android36.0)", line);
    }

    [Fact]
    public void NuGetVersions_OlderAndroidGroupOnly_ResolvesToThatGroup()
    {
        // Act
        var lines = BumpScript.Run("nuget-versions", "Xamarin.AndroidX.Startup.StartupRuntime", "net9.0-android").Split('\n');

        // Assert
        var line = Assert.Single(lines, x => x.StartsWith("1.1.1 |", StringComparison.Ordinal));
        Assert.StartsWith("1.1.1 | androidx.startup:startup-runtime:1.1.1 | net6.0-android31.0: ", line);
    }

    [Theory]
    [InlineData("../registration", "net9.0-android", "'../registration' is not a NuGet package id")]
    [InlineData("https://example.com/x", "net9.0-android", "'https://example.com/x' is not a NuGet package id")]
    [InlineData("Xamarin.AndroidX.Core", "net9.0-ios", "'net9.0-ios' is not a .NET Android target framework")]
    public void NuGetVersions_ArgumentThatIsNotAPackageIdOrAndroidFramework_IsRejectedBeforeAnyRequest(string packageId, string targetFramework, string expectedError)
    {
        // Act
        var (exitCode, _, stderr) = BumpScript.RunForExitCode("nuget-versions", packageId, targetFramework);

        // Assert
        Assert.Equal(2, exitCode);
        Assert.Contains(expectedError, stderr);
    }
}
