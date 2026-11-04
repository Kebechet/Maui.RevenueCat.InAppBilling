using System.Text;
using Xunit;

namespace BumpScripts.Tests;

/// <summary>
/// The csproj edits the bump makes. They must change exactly the value they target and leave
/// the file's BOM and line endings alone: a rewrite that flips CRLF to LF turns a one-line
/// bump into a whole-file diff that hides the real change.
/// </summary>
public class CsprojEditTests
{
    private const string Pom = """
        <?xml version="1.0" encoding="UTF-8"?>
        <project xmlns="http://maven.apache.org/POM/4.0.0">
          <dependencies>
            <dependency>
              <groupId>dev.drewhamilton.poko</groupId>
              <artifactId>poko-annotations</artifactId>
              <version>0.20.2</version>
              <scope>runtime</scope>
            </dependency>
          </dependencies>
        </project>
        """;

    [Fact]
    public void SyncIgnoredJavaDependencies_PomMovedTheDependency_FollowsThePomVersion()
    {
        // Arrange
        var csproj = BumpScript.WriteTempFile("""
            <Project>
              <ItemGroup>
                <AndroidIgnoredJavaDependency Include="dev.drewhamilton.poko:poko-annotations:0.17.2" />
                <AndroidIgnoredJavaDependency Include="com.example:gone:1.0.0" />
              </ItemGroup>
            </Project>
            """, ".csproj");
        var pom = BumpScript.WriteTempFile(Pom, ".pom");

        // Act
        BumpScript.Run("sync-ignored-java-dependencies", csproj, pom);

        // Assert
        var text = File.ReadAllText(csproj);
        Assert.Contains("""Include="dev.drewhamilton.poko:poko-annotations:0.20.2" """, text);
        Assert.Contains("""Include="com.example:gone:1.0.0" """, text);
    }

    [Fact]
    public void SetReleaseNote_MultiLineNoteInCrlfFileWithBom_KeepsEncodingAndRoundTrips()
    {
        // Arrange
        var path = Path.Combine(Path.GetTempPath(), $"bump_test_{Guid.NewGuid():N}.csproj");
        var original = "<Project>\r\n  <PropertyGroup>\r\n    <Version>1.0.0.0</Version>\r\n    <PackageReleaseNotes>old\r\nnote</PackageReleaseNotes>\r\n  </PropertyGroup>\r\n</Project>\r\n";
        File.WriteAllText(path, original, new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
        const string note = "2.0.0.0: updated.\n2.0.0: Fix one thing.\nFull changelog: https://example.com";

        // Act
        BumpScript.Run("set-release-note", path, note);
        BumpScript.Run("set-version", path, "2.0.0.0");
        var readBack = BumpScript.Run("get-release-note", path);

        // Assert
        var bytes = File.ReadAllBytes(path);
        var text = Encoding.UTF8.GetString(bytes, 3, bytes.Length - 3);
        Assert.Equal(new byte[] { 0xEF, 0xBB, 0xBF }, bytes.Take(3));
        Assert.DoesNotContain("\n", text.Replace("\r\n", string.Empty));
        Assert.Equal(note, readBack.Replace("\r\n", "\n"));
        Assert.Equal("2.0.0.0", BumpScript.Run("get-version", path));
    }

    [Fact]
    public void SetReleaseNote_NoteWithXmlMarkup_IsRefused()
    {
        // Arrange
        var csproj = BumpScript.WriteTempFile("<Project><PropertyGroup><PackageReleaseNotes>old</PackageReleaseNotes></PropertyGroup></Project>", ".csproj");

        // Act
        var exception = Assert.Throws<InvalidOperationException>(() => BumpScript.Run("set-release-note", csproj, "uses List<T> now"));

        // Assert
        Assert.Contains("must not contain XML markup", exception.Message);
        Assert.Contains("<PackageReleaseNotes>old</PackageReleaseNotes>", File.ReadAllText(csproj));
    }

    [Theory]
    [InlineData("5.100.0", "5.99.2", "1")]
    [InlineData("10.30.1", "11.0.1", "-1")]
    [InlineData("11.1.0", "11.1.0.0", "0")]
    public void CompareVersions_DottedVersions_ComparesNumerically(string left, string right, string expected)
    {
        // Act
        var comparison = BumpScript.Run("compare-versions", left, right);

        // Assert
        Assert.Equal(expected, comparison);
    }
}
