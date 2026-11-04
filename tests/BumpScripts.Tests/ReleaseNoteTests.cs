using Xunit;

namespace BumpScripts.Tests;

/// <summary>
/// The release-note extraction behind <c>changelog-range</c>. Its output is written into a
/// package's release notes, which nuget.org never lets anyone edit, and into the bump PR body,
/// where every <c>@handle</c> notifies its owner.
/// </summary>
public class ReleaseNoteTests
{
    [Fact]
    public void ReleaseNoteItems_BodyWithUiSection_KeepsOnlyTheRevenueCatSdkSection()
    {
        // Arrange
        var body = BumpScript.WriteTempFile("""
            ## RevenueCat SDK
            ### 🐞 Bugfixes
            * Fix the core thing (#1) via Jane Doe (@jane)

            ## RevenueCatUI SDK
            ### 🐞 Bugfixes
            * Fix a paywall (#2) via Jane Doe (@jane)
            """, ".md");

        // Act
        var items = BumpScript.Run("release-note-items", body);

        // Assert
        Assert.Equal("Fix the core thing.", items);
    }

    [Fact]
    public void ReleaseNoteItems_OtherChangesAndDependencyUpdates_AreDropped()
    {
        // Arrange
        var body = BumpScript.WriteTempFile("""
            ## RevenueCat SDK
            ### ✨ New Features
            * Add a feature (#1) via Jane Doe (@jane)
            ### 📦 Dependency Updates
            * Update kotlinx-coroutines to 1.9.0 (#2) via Jane Doe (@jane)
            ### 🔄 Other Changes
            * ci: run actionlint (#3) via Jane Doe (@jane)
            """, ".md");

        // Act
        var items = BumpScript.Run("release-note-items", body);

        // Assert
        Assert.Equal("Add a feature.", items);
    }

    [Fact]
    public void ReleaseNoteItems_ExternalContribution_LeavesNoHandleAuthorOrPullRequestNumber()
    {
        // Arrange
        var body = BumpScript.WriteTempFile("""
            ## RevenueCat SDK
            ### 🐞 Bugfixes
            * [EXTERNAL] Fix backend prices losing a micro by @someone (#4207) via Álvaro Brey (@AlvaroBrey)
            * Update to Play Billing Library 9.1.0 (#4118) via @contributor (#4192) via Álvaro Brey (@AlvaroBrey)
            """, ".md");

        // Act
        var items = BumpScript.Run("release-note-items", body);

        // Assert
        Assert.Equal("Fix backend prices losing a micro.\nUpdate to Play Billing Library 9.1.0.", items.Replace("\r\n", "\n"));
        Assert.DoesNotContain("@", items);
        Assert.DoesNotContain("#", items);
    }

    [Fact]
    public void ReleaseNoteItems_BreakingSectionAndWarningCallout_AreMarked()
    {
        // Arrange
        var body = BumpScript.WriteTempFile("""
            ## RevenueCat SDK

            > [!WARNING]
            > Your app must now compile against API 35 or later.

            ### 💥 Breaking Changes
            * Update to Play Billing Library 9.1.0 (#1) via Jane Doe (@jane)
            """, ".md");

        // Act
        var items = BumpScript.Run("release-note-items", body);

        // Assert
        Assert.Equal("WARNING: Your app must now compile against API 35 or later.\nBREAKING: Update to Play Billing Library 9.1.0.", items.Replace("\r\n", "\n"));
    }

    [Fact]
    public void ReleaseNoteItems_FeatureAreaHeading_PrefixesItsItems()
    {
        // Arrange
        var body = BumpScript.WriteTempFile("""
            ## RevenueCat SDK
            ### Paywallsv2
            #### 🐞 Bugfixes
            * fix: Reduce CPU usage when downloading large assets (#8177) via Rick (@rickvdl)
            """, ".md");

        // Act
        var items = BumpScript.Run("release-note-items", body);

        // Assert
        Assert.Equal("Paywallsv2: fix: Reduce CPU usage when downloading large assets.", items);
    }

    [Fact]
    public void ReleaseNoteItems_ItemContainingByOrVia_KeepsItsOwnWords()
    {
        // Arrange
        var body = BumpScript.WriteTempFile("""
            ## RevenueCat SDK
            ### 🐞 Bugfixes
            * Send attribution via the backend instead of the device (#1) via Jane Doe (@jane)
            """, ".md");

        // Act
        var items = BumpScript.Run("release-note-items", body);

        // Assert
        Assert.Equal("Send attribution via the backend instead of the device.", items);
    }
}
