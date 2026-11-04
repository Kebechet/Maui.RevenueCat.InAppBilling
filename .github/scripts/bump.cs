// Every read, edit and parse the binding bump pipeline needs, as one .NET 10 file-based app:
//
//   dotnet run .github/scripts/bump.cs -- <command> [args]
//
// The shell scripts next to it stay thin glue (curl, gh, git, dotnet, sharpie). XML, JSON,
// release-note parsing and assembly metadata live here, so the repository keeps one
// scripting language.
//
// Every write preserves the file's BOM and line endings byte-for-byte: Git Bash's sed -i
// strips CRLF, which would rewrite every line of a csproj and bury the real change.
//
// Commands
//   get-version <csproj>                                print <Version>
//   get-maven-pin <csproj>                              print the com.revenuecat.purchases:purchases pin
//   get-min-os <csproj>                                 print <SupportedOSPlatformVersion>
//   get-release-note <csproj>                           print <PackageReleaseNotes>
//   set-version <csproj> <version>                      rewrite <Version>
//   set-maven-pin <csproj> <version>                    rewrite the AndroidMavenLibrary pin
//   set-release-note <csproj> <note>                    replace <PackageReleaseNotes>, newlines allowed
//   set-package-version <csproj> <pkgId> <version>      rewrite one <PackageReference> version
//   sync-ignored-java-dependencies <csproj> <pom>       move every AndroidIgnoredJavaDependency onto the POM's version
//   check-min-os <csproj> <native-min>                  raise the floor when the native lib needs more
//   compare-versions <a> <b>                            print -1, 0 or 1 (dotted numeric)
//   latest-native <android|ios>                         newest stable RevenueCat SDK version
//   last-published-native <pkgId>                       native version behind the newest nuget.org release
//   changelog-range <android|ios> <after> <through>     RevenueCat's notes for (after, through], one line per version
//   release-note-items <body.md>                        the items changelog-range keeps from one release body
//   api-report <android|ios> <new-dll> <wrapper-dll|-> <out-md> <out-full-md>
//                                                       native API added/removed/obsoleted since the published
//                                                       binding, and which of it the wrapper touches

using System.Collections.Immutable;
using System.Globalization;
using System.IO.Compression;
using System.Net;
using System.Net.Http.Headers;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Xml.Linq;

if (args.Length == 0)
{
    Console.Error.WriteLine("usage: dotnet run .github/scripts/bump.cs -- <command> [args]");
    return 2;
}

try
{
    return args[0] switch
    {
        "get-version" => Print(Csproj.ReadElement(Arg(1), "Version")),
        "get-maven-pin" => Print(Csproj.ReadMavenPin(Arg(1))),
        "get-min-os" => Print(Csproj.ReadElement(Arg(1), "SupportedOSPlatformVersion")),
        "get-release-note" => Print(Csproj.ReadReleaseNote(Arg(1))),
        "set-version" => Csproj.SetElement(Arg(1), "Version", Arg(2)),
        "set-maven-pin" => Csproj.SetMavenPin(Arg(1), Arg(2)),
        "set-release-note" => Csproj.SetReleaseNote(Arg(1), Arg(2)),
        "set-package-version" => Csproj.SetPackageVersion(Arg(1), Arg(2), Arg(3)),
        "sync-ignored-java-dependencies" => Csproj.SyncIgnoredJavaDependencies(Arg(1), Arg(2)),
        "check-min-os" => Csproj.CheckMinOs(Arg(1), Arg(2)),
        "compare-versions" => Print(Versions.Compare(Arg(1), Arg(2)).ToString(CultureInfo.InvariantCulture)),
        "latest-native" => Print(await Upstream.LatestNative(Arg(1))),
        "last-published-native" => Print(await NuGetFeed.LastPublishedNative(Arg(1))),
        "changelog-range" => Print(await Upstream.ChangelogRange(Arg(1), Arg(2), Arg(3))),
        "release-note-items" => Print(string.Join("\n", ReleaseNotes.CoreSdkItems(File.ReadAllText(Arg(1))))),
        "api-report" => await ApiReport.Run(Arg(1), Arg(2), Arg(3), Arg(4), Arg(5)),
        var other => Cli.Fail($"unknown command '{other}'"),
    };
}
catch (UsageException ex)
{
    Console.Error.WriteLine($"ERROR: {ex.Message}");
    return 2;
}
// Exit 3 so a caller can tell "upstream is down" from "the arguments were wrong". A bump
// that cannot read a source must stop: the note it would otherwise invent is published
// once and then cannot be corrected.
catch (SourceUnavailableException ex)
{
    Console.Error.WriteLine($"ERROR: {ex.Message}");
    return 3;
}

string Arg(int index)
{
    return index < args.Length
        ? args[index]
        : throw new UsageException($"'{args[0]}' is missing argument {index}");
}

static int Print(string value)
{
    Console.WriteLine(value);
    return 0;
}

static class Cli
{
    public static int Fail(string message)
    {
        Console.Error.WriteLine($"ERROR: {message}");
        return 1;
    }
}

static class TextFile
{
    public static (bool Bom, string Eol, string Text) Read(string path)
    {
        var bytes = File.ReadAllBytes(path);
        var bom = bytes.Length >= 3 && bytes[0] == 0xEF && bytes[1] == 0xBB && bytes[2] == 0xBF;
        var text = new UTF8Encoding(false).GetString(bytes, bom ? 3 : 0, bytes.Length - (bom ? 3 : 0));
        return (bom, text.Contains("\r\n") ? "\r\n" : "\n", text);
    }

    public static void Write(string path, bool bom, string text)
    {
        var body = new UTF8Encoding(false).GetBytes(text);
        using var stream = File.Create(path);
        if (bom)
        {
            stream.Write([0xEF, 0xBB, 0xBF]);
        }
        stream.Write(body);
    }
}

static class Csproj
{
    private const string MavenArtifact = "com.revenuecat.purchases:purchases";

    public static string ReadElement(string csproj, string element)
    {
        var match = Regex.Match(TextFile.Read(csproj).Text, $"<{element}>([^<]+)</{element}>");
        return match.Success
            ? match.Groups[1].Value.Trim()
            : throw new UsageException($"no <{element}> found in {csproj}");
    }

    public static string ReadMavenPin(string csproj)
    {
        var match = Regex.Match(
            TextFile.Read(csproj).Text,
            $"""<AndroidMavenLibrary\s+Include="{Regex.Escape(MavenArtifact)}"\s+Version="([^"]+)""");
        return match.Success
            ? match.Groups[1].Value
            : throw new UsageException($"no <AndroidMavenLibrary Include=\"{MavenArtifact}\" .../> in {csproj}");
    }

    public static string ReadReleaseNote(string csproj)
    {
        var match = Regex.Match(TextFile.Read(csproj).Text, "<PackageReleaseNotes>(.*?)</PackageReleaseNotes>", RegexOptions.Singleline);
        return match.Success
            ? match.Groups[1].Value
            : throw new UsageException($"no <PackageReleaseNotes> found in {csproj}");
    }

    public static int SetElement(string csproj, string element, string value)
    {
        return Replace(csproj, $"<{element}>[^<]+</{element}>", $"<{element}>{value}</{element}>", $"<{element}>");
    }

    public static int SetMavenPin(string csproj, string version)
    {
        return Replace(
            csproj,
            $"""(<AndroidMavenLibrary\s+Include="{Regex.Escape(MavenArtifact)}"\s+Version=")[^"]+(")""",
            $"${{1}}{version}${{2}}",
            "the AndroidMavenLibrary pin");
    }

    public static int SetPackageVersion(string csproj, string packageId, string version)
    {
        return Replace(
            csproj,
            $"""(<PackageReference\s+Include="{Regex.Escape(packageId)}"\s+Version=")[^"]+(")""",
            $"${{1}}{version}${{2}}",
            $"a <PackageReference> for {packageId}");
    }

    // The note replaces the previous one instead of growing on top of it: the previous
    // binding's note is already on its own nuget.org version page, and an ever-growing note
    // would bury what changed in this release.
    //
    // Singleline matters: the note is multi-line, and without it the *next* bump cannot match
    // across the newlines and dies claiming <PackageReleaseNotes> is missing.
    public static int SetReleaseNote(string csproj, string note)
    {
        if (note.AsSpan().IndexOfAny('<', '>', '&') >= 0)
        {
            return Cli.Fail($"the note must not contain XML markup: {note}");
        }

        // The note's own newlines take the file's line endings. LF inside a CRLF csproj is
        // what makes an editor "fix" the whole file later and bury the real change.
        var eol = TextFile.Read(csproj).Eol;
        var normalized = note.Replace("\r\n", "\n").Replace("\n", eol);

        return Replace(
            csproj,
            "(<PackageReleaseNotes>).*?(</PackageReleaseNotes>)",
            $"${{1}}{normalized.Replace("$", "$$")}${{2}}",
            "<PackageReleaseNotes>",
            RegexOptions.Singleline);
    }

    // An ignored Java dependency is pinned as group:artifact:version (a separate Version
    // attribute is XA4249), so every RevenueCat release that moves the dependency would fail
    // verification on a version number alone. Why a dependency is ignored does not depend on
    // its version, so the pin follows the POM. A dependency the POM no longer lists is left
    // alone and reported: whether to drop the ignore is a decision, not bookkeeping.
    public static int SyncIgnoredJavaDependencies(string csproj, string pomPath)
    {
        XNamespace ns = "http://maven.apache.org/POM/4.0.0";
        var pom = XDocument.Load(pomPath);
        var pomVersions = pom.Descendants(ns + "dependency")
            .Select(x => (
                Key: $"{x.Element(ns + "groupId")?.Value.Trim()}:{x.Element(ns + "artifactId")?.Value.Trim()}",
                Version: x.Element(ns + "version")?.Value.Trim() ?? string.Empty))
            .Where(x => x.Version.Length > 0)
            .GroupBy(x => x.Key)
            .ToDictionary(x => x.Key, x => x.First().Version);

        var (bom, _, text) = TextFile.Read(csproj);
        var updated = Regex.Replace(
            text,
            """(<AndroidIgnoredJavaDependency\s+Include=")([^":]+):([^":]+):([^"]+)(")""",
            match =>
            {
                var key = $"{match.Groups[2].Value}:{match.Groups[3].Value}";
                var pinned = match.Groups[4].Value;
                if (!pomVersions.TryGetValue(key, out var pomVersion))
                {
                    Console.Error.WriteLine($"==> ignored Java dependency {key} is no longer in the POM; left at {pinned}");
                    return match.Value;
                }
                if (pomVersion == pinned)
                {
                    return match.Value;
                }

                Console.Error.WriteLine($"==> ignored Java dependency {key}: {pinned} -> {pomVersion}");
                return $"{match.Groups[1].Value}{key}:{pomVersion}{match.Groups[5].Value}";
            });

        if (updated != text)
        {
            TextFile.Write(csproj, bom, updated);
        }
        return 0;
    }

    // A native SDK that raises its floor while the binding still claims a lower one compiles,
    // packs and passes every build check - it only breaks at deployment. So the floor is read
    // from the artifact and the binding is raised to match. Whether the WRAPPER follows is
    // breaking for consumers and stays a human decision, which is why the caller turns a
    // raise into a draft PR.
    public static int CheckMinOs(string csproj, string nativeMin)
    {
        var current = ReadElement(csproj, "SupportedOSPlatformVersion");
        var isRaised = Versions.Compare(nativeMin, current) > 0;
        var updated = isRaised
            ? nativeMin
            : current;

        if (isRaised)
        {
            var result = SetElement(csproj, "SupportedOSPlatformVersion", nativeMin);
            if (result != 0)
            {
                return result;
            }
            Console.Error.WriteLine($"==> minimum OS raised: {current} -> {nativeMin} (required by the native library)");
        }
        else
        {
            Console.Error.WriteLine($"==> minimum OS unchanged: csproj {current}, native library {nativeMin}");
        }

        GitHubOutput.Emit("min_os_native", nativeMin);
        GitHubOutput.Emit("min_os_previous", current);
        GitHubOutput.Emit("min_os_current", updated);
        GitHubOutput.Emit("min_os_raised", isRaised ? "true" : "false");
        return 0;
    }

    private static int Replace(string path, string pattern, string replacement, string what, RegexOptions options = RegexOptions.None)
    {
        var (bom, _, text) = TextFile.Read(path);
        if (!Regex.IsMatch(text, pattern, options))
        {
            return Cli.Fail($"{what} not found in {path}");
        }

        TextFile.Write(path, bom, Regex.Replace(text, pattern, replacement, options));
        return 0;
    }
}

static class GitHubOutput
{
    public static void Emit(string name, string value)
    {
        Console.WriteLine($"{name}={value}");
        var output = Environment.GetEnvironmentVariable("GITHUB_OUTPUT");
        if (!string.IsNullOrEmpty(output))
        {
            File.AppendAllText(output, $"{name}={value}{Environment.NewLine}");
        }
    }
}

static class Versions
{
    // Dotted numeric comparison: "16.0" > "14.2", "21" > "19", "5.100.0" > "5.99.2".
    public static int Compare(string left, string right)
    {
        var a = left.Split('.');
        var b = right.Split('.');
        for (var i = 0; i < Math.Max(a.Length, b.Length); i++)
        {
            var x = i < a.Length && int.TryParse(a[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out var pa) ? pa : 0;
            var y = i < b.Length && int.TryParse(b[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out var pb) ? pb : 0;
            if (x != y)
            {
                return x < y ? -1 : 1;
            }
        }
        return 0;
    }

    public static bool IsStable(string version)
    {
        return Regex.IsMatch(version, @"^\d+(\.\d+){1,3}$");
    }

    public static IComparer<string> Comparer { get; } = Comparer<string>.Create(Compare);
}

static class Http
{
    public static HttpClient Create()
    {
        var http = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };
        http.DefaultRequestHeaders.UserAgent.ParseAdd("Kebechet.Maui.RevenueCat-bump/1.0");
        return http;
    }

    public static async Task<HttpResponseMessage> Send(HttpClient http, HttpRequestMessage request, string source)
    {
        try
        {
            return await http.SendAsync(request);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            throw new SourceUnavailableException($"could not reach {source}: {ex.Message}");
        }
    }

    public static Task<HttpResponseMessage> Get(HttpClient http, string url, string source)
    {
        return Send(http, new HttpRequestMessage(HttpMethod.Get, url), source);
    }
}

static class NuGetFeed
{
    // nuget.org, not the working tree, is what a consumer upgrades from, and the two diverge
    // the moment a bump merges without being published. A 404 is the one legitimate "nothing
    // published yet"; every other failure throws, because falling back to the csproj pin
    // would silently anchor the release note to the wrong version.
    public static async Task<string?> LastPublishedVersion(string packageId)
    {
        using var http = Http.Create();
        var url = $"https://api.nuget.org/v3-flatcontainer/{packageId.ToLowerInvariant()}/index.json";
        using var response = await Http.Get(http, url, "nuget.org");
        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            return null;
        }
        if (!response.IsSuccessStatusCode)
        {
            throw new SourceUnavailableException($"nuget.org returned {(int)response.StatusCode} for {packageId}");
        }

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.GetProperty("versions")
            .EnumerateArray()
            .Select(x => x.GetString() ?? string.Empty)
            .Where(Versions.IsStable)
            .Order(Versions.Comparer)
            .LastOrDefault();
    }

    // Binding versions are <native>.<revision>, and nuget.org normalises a trailing ".0"
    // away: 10.16.1.0 comes back as 10.16.1 while 10.1.2.2 stays 10.1.2.2. Dropping only a
    // fourth component reads both correctly.
    public static async Task<string> LastPublishedNative(string packageId)
    {
        var newest = await LastPublishedVersion(packageId);
        if (newest is null)
        {
            return string.Empty;
        }

        var parts = newest.Split('.');
        return parts.Length >= 4
            ? string.Join('.', parts.Take(3))
            : newest;
    }

    public static async Task<byte[]> DownloadAssembly(string packageId, string version, string tfmPrefix, string assemblyFileName)
    {
        using var http = Http.Create();
        var id = packageId.ToLowerInvariant();
        var url = $"https://api.nuget.org/v3-flatcontainer/{id}/{version.ToLowerInvariant()}/{id}.{version.ToLowerInvariant()}.nupkg";
        using var response = await Http.Get(http, url, "nuget.org");
        if (!response.IsSuccessStatusCode)
        {
            throw new SourceUnavailableException($"nuget.org returned {(int)response.StatusCode} for {packageId} {version}");
        }

        using var package = new ZipArchive(new MemoryStream(await response.Content.ReadAsByteArrayAsync()));
        var entry = package.Entries
            .Where(x => x.FullName.StartsWith($"lib/{tfmPrefix}", StringComparison.OrdinalIgnoreCase) &&
                x.Name.Equals(assemblyFileName, StringComparison.OrdinalIgnoreCase))
            .OrderBy(x => x.FullName, StringComparer.Ordinal)
            .FirstOrDefault()
            ?? throw new SourceUnavailableException($"{packageId} {version} has no lib/{tfmPrefix}*/{assemblyFileName}");

        using var stream = entry.Open();
        using var buffer = new MemoryStream();
        await stream.CopyToAsync(buffer);
        return buffer.ToArray();
    }
}

static class Upstream
{
    public static async Task<string> LatestNative(string platform)
    {
        var releases = await Releases(platform);
        return releases.Keys
            .Order(Versions.Comparer)
            .LastOrDefault()
            ?? throw new SourceUnavailableException($"RevenueCat/purchases-{platform} lists no stable release");
    }

    // The range is (after, through], never just `through`: binding versions get skipped -
    // merged, superseded, published never - and a skipped version has no package of its
    // own, so its upstream notes would otherwise exist nowhere on nuget.org.
    //
    // Maintenance releases of an older major line (Android 10.30.1 shipped after 11.0.1)
    // fall inside a cross-major range too. They are kept: the binding jumps over them, and
    // their fixes are part of what the consumer receives.
    //
    // Every fetch either answers or throws. "Unreachable" and "nothing published" are
    // different facts, and only one of them is safe to write into a package version that can
    // never be edited again.
    public static async Task<string> ChangelogRange(string platform, string after, string through)
    {
        var releases = await Releases(platform);
        var inRange = releases.Keys
            .Where(x => Versions.Compare(x, after) > 0 && Versions.Compare(x, through) <= 0)
            .Order(Versions.Comparer)
            .ToList();

        if (!inRange.Any())
        {
            throw new SourceUnavailableException(
                $"RevenueCat/purchases-{platform} lists no release in ({after}, {through}] - the release list is incomplete, so the range cannot be described");
        }

        var lines = inRange.Select(version =>
        {
            var items = ReleaseNotes.CoreSdkItems(releases[version]);
            return items.Any()
                ? $"{version}: {string.Join(" ", items)}"
                : $"{version}: no RevenueCat SDK changes listed (RevenueCatUI or internal changes only).";
        });

        return string.Join("\n", lines);
    }

    private static async Task<Dictionary<string, string>> Releases(string platform)
    {
        if (platform is not ("android" or "ios"))
        {
            throw new UsageException($"unknown platform '{platform}' (expected android or ios)");
        }

        using var http = Http.Create();
        var token = Environment.GetEnvironmentVariable("GH_TOKEN") ?? Environment.GetEnvironmentVariable("GITHUB_TOKEN");
        var bodiesByVersion = new Dictionary<string, string>();

        // RevenueCat releases several times a week on both platforms; 500 releases reach back
        // well over a year, far beyond any range a bump would describe.
        for (var page = 1; page <= 5; page++)
        {
            using var request = new HttpRequestMessage(
                HttpMethod.Get,
                $"https://api.github.com/repos/RevenueCat/purchases-{platform}/releases?per_page=100&page={page}");
            request.Headers.Accept.ParseAdd("application/vnd.github+json");
            request.Headers.Add("X-GitHub-Api-Version", "2022-11-28");
            if (!string.IsNullOrEmpty(token))
            {
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }

            using var response = await Http.Send(http, request, $"the RevenueCat/purchases-{platform} releases API");
            if (!response.IsSuccessStatusCode)
            {
                throw new SourceUnavailableException($"the RevenueCat/purchases-{platform} releases API returned {(int)response.StatusCode}");
            }

            using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
            var pageReleases = json.RootElement.EnumerateArray().ToList();
            foreach (var release in pageReleases)
            {
                var isDraftOrPrerelease = release.GetProperty("draft").GetBoolean() || release.GetProperty("prerelease").GetBoolean();
                var tag = release.GetProperty("tag_name").GetString() ?? string.Empty;
                if (isDraftOrPrerelease || !Versions.IsStable(tag))
                {
                    continue;
                }
                bodiesByVersion.TryAdd(tag, release.GetProperty("body").GetString() ?? string.Empty);
            }

            if (pageReleases.Count < 100)
            {
                break;
            }
        }

        return bodiesByVersion;
    }
}

static class ReleaseNotes
{
    private static readonly string[] _includedKinds = ["breaking", "new feature", "bugfix", "bug fix", "performance"];

    // RevenueCat's release bodies have one "## RevenueCat SDK" section and one
    // "## RevenueCatUI SDK" section. Only the first is shipped by these bindings (RevenueCatUI
    // is out of scope, see CONTRIBUTING.md). Inside it, "### 🔄 Other Changes" and
    // "### 📦 Dependency Updates" are CI, sample-app and tooling noise for a consumer, so only
    // breaking changes, features, fixes and performance work are kept. A feature-area heading
    // ("### Paywallsv2" with "#### 🐞 Bugfixes" under it) prefixes its items with the area.
    // GitHub "> [!WARNING]" callouts are kept: Android 11.0.0 used one to say apps must now
    // compile against API 35.
    public static List<string> CoreSdkItems(string body)
    {
        var lines = body.Replace("\r\n", "\n").Split('\n');
        var start = Array.FindIndex(lines, x => Regex.IsMatch(x, @"^##\s+RevenueCat SDK\s*$", RegexOptions.IgnoreCase));
        var items = new List<string>();
        var area = string.Empty;
        var kind = string.Empty;

        for (var i = start < 0 ? 0 : start + 1; i < lines.Length; i++)
        {
            var line = lines[i].Trim();
            if (Regex.IsMatch(line, @"^##\s") && !line.StartsWith("###"))
            {
                break;
            }

            var heading = Regex.Match(line, @"^(#{3,})\s+(.*)$");
            if (heading.Success)
            {
                var text = Regex.Replace(heading.Groups[2].Value, @"[^\p{L}\p{N} ]", string.Empty).Trim();
                var isKindHeading = _includedKinds.Any(x => text.Contains(x, StringComparison.OrdinalIgnoreCase)) ||
                    text.Contains("other change", StringComparison.OrdinalIgnoreCase) ||
                    text.Contains("dependency update", StringComparison.OrdinalIgnoreCase);
                if (heading.Groups[1].Value.Length == 3)
                {
                    area = isKindHeading
                        ? string.Empty
                        : text;
                    kind = isKindHeading
                        ? text
                        : string.Empty;
                }
                else
                {
                    kind = text;
                }
                continue;
            }

            if (line.StartsWith('>'))
            {
                var callout = line.TrimStart('>').Trim();
                if (callout.Length > 0 && !callout.StartsWith("[!"))
                {
                    items.Add($"WARNING: {Clean(callout)}");
                }
                continue;
            }

            if (!(line.StartsWith("* ") || line.StartsWith("- ")))
            {
                continue;
            }
            if (!_includedKinds.Any(x => kind.Contains(x, StringComparison.OrdinalIgnoreCase)))
            {
                continue;
            }

            var item = Clean(line[2..]);
            if (item.Length == 0)
            {
                continue;
            }
            if (area.Length > 0)
            {
                item = $"{area}: {item}";
            }
            if (kind.Contains("breaking", StringComparison.OrdinalIgnoreCase))
            {
                item = $"BREAKING: {item}";
            }
            items.Add(item);
        }

        return items;
    }

    // The result is written into <PackageReleaseNotes>, which set-release-note refuses to
    // fill with markup rather than corrupt the csproj.
    //
    // The same text goes into the bump PR body, where GitHub notifies every @handle it
    // contains - so no handle survives, not even an inline "by @someone".
    private static string Clean(string value)
    {
        // RevenueCat ends every item with "via Full Name (@handle)", and an external
        // contribution carries "via @handle" or "by @handle" in the middle as well.
        var withoutTrailingAuthor = Regex.Replace(value, @"\s+via\s+[^()@]*\(@[\w-]+\)\s*$", string.Empty);
        var withoutAttribution = Regex.Replace(withoutTrailingAuthor, @"\s+(via|by)\s+@[\w-]+", string.Empty);
        var withoutHandles = Regex.Replace(withoutAttribution, @"@([\w-]+)", "$1");
        var withoutTrackerTags = Regex.Replace(withoutHandles, @"^\s*(\[[A-Z][A-Z0-9-]*\]\s*)+", string.Empty);
        var withoutPullRequests = Regex.Replace(withoutTrackerTags, @"\s*\(#\d+\)", string.Empty);
        var withoutMarkdown = withoutPullRequests.Replace("**", string.Empty).Replace("`", string.Empty);
        var withoutLinks = Regex.Replace(withoutMarkdown, @"\[([^\]]*)\]\([^)]*\)", "$1");
        var plain = withoutLinks.Replace("&", "and").Replace("<", string.Empty).Replace(">", string.Empty);
        var collapsed = Regex.Replace(plain, @"\s+", " ").Trim();
        return collapsed.Length == 0 || collapsed.EndsWith('.')
            ? collapsed
            : $"{collapsed}.";
    }
}

static class ApiReport
{
    private const int SectionLineLimit = 250;

    // The question a bump PR has to answer beyond "does it build": what did RevenueCat add
    // that the wrapper does not expose yet, and what that the wrapper calls did it remove or
    // deprecate. Both bindings are generated, so the generated assembly IS the native API
    // surface: diffing it against the published binding answers the first half, and the
    // wrapper's own member references answer the second.
    public static async Task<int> Run(string platform, string newDllPath, string wrapperDllPath, string outPath, string fullOutPath)
    {
        var (packageId, tfmPrefix, assemblyFileName) = platform switch
        {
            "android" => ("Kebechet.Maui.RevenueCat.Android", "net9.0-android", "Maui.RevenueCat.Android.dll"),
            "ios" => ("Kebechet.Maui.RevenueCat.iOS", "net9.0-ios", "Maui.RevenueCat.iOS.dll"),
            _ => throw new UsageException($"unknown platform '{platform}' (expected android or ios)"),
        };

        var newSurface = ApiSurface.Read(File.ReadAllBytes(newDllPath));
        var publishedVersion = await NuGetFeed.LastPublishedVersion(packageId);
        var oldSurface = publishedVersion is null
            ? ApiSurface.Empty
            : ApiSurface.Read(await NuGetFeed.DownloadAssembly(packageId, publishedVersion, tfmPrefix, assemblyFileName));

        var wrapperSource = "the wrapper built against this binding";
        byte[] wrapperBytes;
        if (wrapperDllPath != "-" && File.Exists(wrapperDllPath))
        {
            wrapperBytes = File.ReadAllBytes(wrapperDllPath);
        }
        else
        {
            // The wrapper did not compile against the new binding, so the published wrapper
            // stands in. It can lag behind main, which is why the report says which one it read.
            var wrapperVersion = await NuGetFeed.LastPublishedVersion("Kebechet.Maui.RevenueCat.InAppBilling")
                ?? throw new SourceUnavailableException("the wrapper did not compile and no published wrapper exists to read usage from");
            wrapperBytes = await NuGetFeed.DownloadAssembly("Kebechet.Maui.RevenueCat.InAppBilling", wrapperVersion, tfmPrefix, "Maui.RevenueCat.dll");
            wrapperSource = $"the published wrapper `{wrapperVersion}` (the wrapper did not compile against this binding)";
        }
        var usage = WrapperUsage.Read(wrapperBytes, Path.GetFileNameWithoutExtension(assemblyFileName));

        var diff = ApiDiff.Compute(oldSurface, newSurface, usage);
        var baseline = publishedVersion is null
            ? "nothing published yet"
            : $"`{publishedVersion}` on nuget.org";

        File.WriteAllText(outPath, Render(diff, baseline, wrapperSource, SectionLineLimit));
        File.WriteAllText(fullOutPath, Render(diff, baseline, wrapperSource, int.MaxValue));

        GitHubOutput.Emit("api_added", diff.Added.Count.ToString(CultureInfo.InvariantCulture));
        GitHubOutput.Emit("api_removed", diff.Removed.Count.ToString(CultureInfo.InvariantCulture));
        GitHubOutput.Emit("api_wrapper_affected", diff.WrapperAffected.Count.ToString(CultureInfo.InvariantCulture));
        return 0;
    }

    private static string Render(ApiDiff diff, string baseline, string wrapperSource, int lineLimit)
    {
        var markdown = new StringBuilder();
        markdown.AppendLine($"Baseline: {baseline}. Wrapper usage read from {wrapperSource}.");
        markdown.AppendLine();
        markdown.AppendLine("| | Types | Members |");
        markdown.AppendLine("|---|---|---|");
        markdown.AppendLine($"| Added | {diff.Added.Count(x => x.Kind == 'T')} | {diff.Added.Count(x => x.Kind != 'T')} |");
        markdown.AppendLine($"| Removed | {diff.Removed.Count(x => x.Kind == 'T')} | {diff.Removed.Count(x => x.Kind != 'T')} |");
        markdown.AppendLine($"| Newly obsolete | {diff.NewlyObsolete.Count(x => x.Kind == 'T')} | {diff.NewlyObsolete.Count(x => x.Kind != 'T')} |");
        markdown.AppendLine();

        if (diff.WrapperAffected.Any())
        {
            markdown.AppendLine($"#### ⚠️ The wrapper uses {diff.WrapperAffected.Count} API(s) this bump removes or deprecates");
            AppendGrouped(markdown, diff.WrapperAffected, lineLimit, x => x.Note);
        }
        else
        {
            markdown.AppendLine("#### ✅ Nothing the wrapper uses was removed or deprecated");
        }
        markdown.AppendLine();

        if (diff.AddedOnWrapperTypes.Any())
        {
            markdown.AppendLine($"#### 🆕 {diff.AddedOnWrapperTypes.Count} new member(s) on native types the wrapper already uses - not exposed by the wrapper yet");
            AppendGrouped(markdown, diff.AddedOnWrapperTypes, lineLimit, _ => null);
        }
        else
        {
            markdown.AppendLine("#### No new members on native types the wrapper uses");
        }
        markdown.AppendLine();

        var otherAdded = diff.Added.Except(diff.AddedOnWrapperTypes).ToList();
        AppendDetails(markdown, $"Other added API ({otherAdded.Count})", otherAdded, lineLimit);
        AppendDetails(markdown, $"Removed API ({diff.Removed.Count})", diff.Removed, lineLimit);
        AppendDetails(markdown, $"Newly obsolete API ({diff.NewlyObsolete.Count})", diff.NewlyObsolete, lineLimit);
        return markdown.ToString();
    }

    private static void AppendDetails(StringBuilder markdown, string summary, List<ApiMember> members, int lineLimit)
    {
        if (!members.Any())
        {
            return;
        }

        markdown.AppendLine($"<details><summary>{summary}</summary>");
        markdown.AppendLine();
        AppendGrouped(markdown, members, lineLimit, _ => null);
        markdown.AppendLine();
        markdown.AppendLine("</details>");
        markdown.AppendLine();
    }

    // Grouped by declaring type. A type added in this bump is listed once with its member
    // count: its generated constructors, JNI plumbing and accessors are not individually news.
    private static void AppendGrouped(StringBuilder markdown, List<ApiMember> members, int lineLimit, Func<ApiMember, string?> note)
    {
        var addedTypes = members
            .Where(x => x.Kind == 'T')
            .Select(x => x.DeclaringType)
            .ToHashSet();
        var lineCount = 0;
        var groups = members
            .GroupBy(x => x.DeclaringType)
            .OrderBy(x => x.Key, StringComparer.Ordinal);

        foreach (var group in groups)
        {
            if (lineCount >= lineLimit)
            {
                var remaining = members.Count(x => string.CompareOrdinal(x.DeclaringType, group.Key) >= 0);
                markdown.AppendLine($"- _...and {remaining} more, see the `api-report` artifact on the run_");
                return;
            }

            var typeMember = group.FirstOrDefault(x => x.Kind == 'T');
            if (addedTypes.Contains(group.Key) && typeMember is not null)
            {
                var memberCount = group.Count() - 1;
                var suffix = note(typeMember) is { } typeNote ? $" - {typeNote}" : string.Empty;
                markdown.AppendLine($"- `{group.Key}` (type, {memberCount} member(s)){suffix}");
                lineCount++;
                continue;
            }

            markdown.AppendLine($"- `{group.Key}`");
            lineCount++;
            foreach (var member in group.OrderBy(x => x.Display, StringComparer.Ordinal))
            {
                var suffix = note(member) is { } memberNote ? $" - {memberNote}" : string.Empty;
                markdown.AppendLine($"  - `{member.Display}`{suffix}");
                lineCount++;
            }
        }
    }
}

sealed record ApiMember(char Kind, string Key, string DeclaringType, string Display, bool IsObsolete, ImmutableArray<string> AccessorKeys)
{
    public string? Note { get; init; }
}

sealed record ApiSurface(Dictionary<string, ApiMember> MembersByKey)
{
    private const string KotlinEnumEntriesProperty = "Entries";
    private const string KotlinEnumEntriesType = "Kotlin.Enums.IEnumEntries";

    public static ApiSurface Empty { get; } = new(new Dictionary<string, ApiMember>());

    public static ApiSurface Read(byte[] assembly)
    {
        using var pe = new PEReader(ImmutableArray.Create(assembly));
        var reader = pe.GetMetadataReader();
        var fullNames = new TypeNameProvider(isShort: false);
        var shortNames = new TypeNameProvider(isShort: true);
        var membersByKey = new Dictionary<string, ApiMember>();

        foreach (var typeHandle in reader.TypeDefinitions)
        {
            var type = reader.GetTypeDefinition(typeHandle);
            if (!IsVisible(reader, type))
            {
                continue;
            }

            var typeName = TypeNameProvider.DefinitionName(reader, typeHandle);
            var isTypeObsolete = IsObsolete(reader, type.GetCustomAttributes());
            Add(new ApiMember('T', $"T:{typeName}", typeName, typeName, isTypeObsolete, []));

            var accessors = new HashSet<MethodDefinitionHandle>();
            foreach (var propertyHandle in type.GetProperties())
            {
                var property = reader.GetPropertyDefinition(propertyHandle);
                var propertyAccessors = property.GetAccessors();
                var methods = new[] { propertyAccessors.Getter, propertyAccessors.Setter }
                    .Where(x => !x.IsNil)
                    .ToList();
                accessors.UnionWith(methods);
                var visibleMethods = methods
                    .Select(reader.GetMethodDefinition)
                    .Where(IsVisible)
                    .ToList();
                if (!visibleMethods.Any())
                {
                    continue;
                }

                var name = reader.GetString(property.Name);
                var signature = property.DecodeSignature(fullNames, null);
                // Kotlin 1.9+ gives every enum a synthetic `entries` list. Once the Kotlin
                // version behind a release emits it, every bound enum "gains" a member that
                // carries no RevenueCat API at all.
                if (name == KotlinEnumEntriesProperty && signature.ReturnType == KotlinEnumEntriesType)
                {
                    continue;
                }
                var shortSignature = property.DecodeSignature(shortNames, null);
                var indexer = signature.ParameterTypes.Any() ? $"[{string.Join(",", signature.ParameterTypes)}]" : string.Empty;
                var shortIndexer = shortSignature.ParameterTypes.Any() ? $"[{string.Join(", ", shortSignature.ParameterTypes)}]" : string.Empty;
                var isObsolete = isTypeObsolete ||
                    IsObsolete(reader, property.GetCustomAttributes()) ||
                    visibleMethods.Any(x => IsObsolete(reader, x.GetCustomAttributes()));
                var accessorKeys = methods.Select(x => MethodKey(reader, typeName, x, fullNames)).ToImmutableArray();
                Add(new ApiMember('P', $"P:{typeName}.{name}{indexer}", typeName, $"{name}{shortIndexer} : {shortSignature.ReturnType}", isObsolete, accessorKeys));
            }

            foreach (var eventHandle in type.GetEvents())
            {
                var definition = reader.GetEventDefinition(eventHandle);
                var eventAccessors = definition.GetAccessors();
                var methods = new[] { eventAccessors.Adder, eventAccessors.Remover, eventAccessors.Raiser }
                    .Where(x => !x.IsNil)
                    .ToList();
                accessors.UnionWith(methods);
                if (!methods.Select(reader.GetMethodDefinition).Any(IsVisible))
                {
                    continue;
                }

                var name = reader.GetString(definition.Name);
                var isObsolete = isTypeObsolete || IsObsolete(reader, definition.GetCustomAttributes());
                var accessorKeys = methods.Select(x => MethodKey(reader, typeName, x, fullNames)).ToImmutableArray();
                Add(new ApiMember('E', $"E:{typeName}.{name}", typeName, $"event {name}", isObsolete, accessorKeys));
            }

            foreach (var methodHandle in type.GetMethods())
            {
                var method = reader.GetMethodDefinition(methodHandle);
                var name = reader.GetString(method.Name);
                if (accessors.Contains(methodHandle) || !IsVisible(method) || name == ".cctor")
                {
                    continue;
                }

                var shortSignature = method.DecodeSignature(shortNames, null);
                var display = $"{name}({string.Join(", ", shortSignature.ParameterTypes)})";
                if (name != ".ctor")
                {
                    display = $"{display} : {shortSignature.ReturnType}";
                }
                var isObsolete = isTypeObsolete || IsObsolete(reader, method.GetCustomAttributes());
                var key = MethodKey(reader, typeName, methodHandle, fullNames);
                Add(new ApiMember('M', key, typeName, display, isObsolete, [key]));
            }

            foreach (var fieldHandle in type.GetFields())
            {
                var field = reader.GetFieldDefinition(fieldHandle);
                var visibility = field.Attributes & FieldAttributes.FieldAccessMask;
                var isFieldVisible = visibility is FieldAttributes.Public or FieldAttributes.Family or FieldAttributes.FamORAssem;
                if (!isFieldVisible || (field.Attributes & FieldAttributes.RTSpecialName) != 0)
                {
                    continue;
                }

                var name = reader.GetString(field.Name);
                var isObsolete = isTypeObsolete || IsObsolete(reader, field.GetCustomAttributes());
                var key = $"F:{typeName}.{name}";
                Add(new ApiMember('F', key, typeName, $"{name} : {field.DecodeSignature(shortNames, null)}", isObsolete, [key]));
            }
        }

        return new ApiSurface(membersByKey);

        void Add(ApiMember member)
        {
            membersByKey.TryAdd(member.Key, member);
        }
    }

    public static string MethodKey(MetadataReader reader, string typeName, MethodDefinitionHandle handle, TypeNameProvider provider)
    {
        var method = reader.GetMethodDefinition(handle);
        var signature = method.DecodeSignature(provider, null);
        return MethodKey(typeName, reader.GetString(method.Name), signature.GenericParameterCount, signature.ParameterTypes);
    }

    public static string MethodKey(string typeName, string name, int genericParameterCount, ImmutableArray<string> parameterTypes)
    {
        var arity = genericParameterCount > 0 ? $"``{genericParameterCount}" : string.Empty;
        return $"M:{typeName}.{name}{arity}({string.Join(",", parameterTypes)})";
    }

    private static bool IsVisible(MetadataReader reader, TypeDefinition type)
    {
        var visibility = type.Attributes & TypeAttributes.VisibilityMask;
        if (visibility == TypeAttributes.Public)
        {
            return true;
        }

        var isNestedVisible = visibility is TypeAttributes.NestedPublic or TypeAttributes.NestedFamily or TypeAttributes.NestedFamORAssem;
        return isNestedVisible && IsVisible(reader, reader.GetTypeDefinition(type.GetDeclaringType()));
    }

    private static bool IsVisible(MethodDefinition method)
    {
        var visibility = method.Attributes & MethodAttributes.MemberAccessMask;
        return visibility is MethodAttributes.Public or MethodAttributes.Family or MethodAttributes.FamORAssem;
    }

    private static bool IsObsolete(MetadataReader reader, CustomAttributeHandleCollection attributes)
    {
        foreach (var attributeHandle in attributes)
        {
            var attribute = reader.GetCustomAttribute(attributeHandle);
            var typeName = attribute.Constructor.Kind switch
            {
                HandleKind.MemberReference => reader.GetMemberReference((MemberReferenceHandle)attribute.Constructor).Parent is { Kind: HandleKind.TypeReference } parent
                    ? TypeNameProvider.ReferenceName(reader, (TypeReferenceHandle)parent)
                    : string.Empty,
                HandleKind.MethodDefinition => TypeNameProvider.DefinitionName(reader, reader.GetMethodDefinition((MethodDefinitionHandle)attribute.Constructor).GetDeclaringType()),
                _ => string.Empty,
            };
            if (typeName == "System.ObsoleteAttribute")
            {
                return true;
            }
        }
        return false;
    }
}

// The binding types and members the wrapper's compiled IL refers to. Property reads show up as
// references to their get_ accessor, which is why ApiMember carries its accessor keys.
sealed record WrapperUsage(HashSet<string> TypeNames, HashSet<string> MemberKeys)
{
    public static WrapperUsage Read(byte[] wrapperAssembly, string bindingAssemblyName)
    {
        using var pe = new PEReader(ImmutableArray.Create(wrapperAssembly));
        var reader = pe.GetMetadataReader();
        var provider = new TypeNameProvider(isShort: false);
        var typeNames = new HashSet<string>(StringComparer.Ordinal);
        var memberKeys = new HashSet<string>(StringComparer.Ordinal);

        foreach (var typeReferenceHandle in reader.TypeReferences)
        {
            if (IsFromBinding(reader, typeReferenceHandle, bindingAssemblyName))
            {
                typeNames.Add(TypeNameProvider.ReferenceName(reader, typeReferenceHandle));
            }
        }

        foreach (var memberReferenceHandle in reader.MemberReferences)
        {
            var member = reader.GetMemberReference(memberReferenceHandle);
            var parentType = member.Parent.Kind switch
            {
                HandleKind.TypeReference when IsFromBinding(reader, (TypeReferenceHandle)member.Parent, bindingAssemblyName) =>
                    TypeNameProvider.ReferenceName(reader, (TypeReferenceHandle)member.Parent),
                HandleKind.TypeSpecification => GenericDefinitionName(reader, (TypeSpecificationHandle)member.Parent, bindingAssemblyName),
                _ => null,
            };
            if (parentType is null)
            {
                continue;
            }

            var name = reader.GetString(member.Name);
            if (member.GetKind() == MemberReferenceKind.Method)
            {
                var signature = member.DecodeMethodSignature(provider, null);
                memberKeys.Add(ApiSurface.MethodKey(parentType, name, signature.GenericParameterCount, signature.ParameterTypes));
            }
            else
            {
                memberKeys.Add($"F:{parentType}.{name}");
            }
        }

        return new WrapperUsage(typeNames, memberKeys);
    }

    private static string? GenericDefinitionName(MetadataReader reader, TypeSpecificationHandle handle, string bindingAssemblyName)
    {
        var blob = reader.GetBlobReader(reader.GetTypeSpecification(handle).Signature);
        if (blob.ReadSignatureTypeCode() != SignatureTypeCode.GenericTypeInstance)
        {
            return null;
        }
        blob.ReadSignatureTypeCode();
        var genericType = blob.ReadTypeHandle();
        return genericType.Kind == HandleKind.TypeReference && IsFromBinding(reader, (TypeReferenceHandle)genericType, bindingAssemblyName)
            ? TypeNameProvider.ReferenceName(reader, (TypeReferenceHandle)genericType)
            : null;
    }

    private static bool IsFromBinding(MetadataReader reader, TypeReferenceHandle handle, string bindingAssemblyName)
    {
        var scope = reader.GetTypeReference(handle).ResolutionScope;
        while (scope.Kind == HandleKind.TypeReference)
        {
            scope = reader.GetTypeReference((TypeReferenceHandle)scope).ResolutionScope;
        }

        return scope.Kind == HandleKind.AssemblyReference &&
            reader.GetString(reader.GetAssemblyReference((AssemblyReferenceHandle)scope).Name) == bindingAssemblyName;
    }
}

sealed record ApiDiff(List<ApiMember> Added, List<ApiMember> Removed, List<ApiMember> NewlyObsolete, List<ApiMember> AddedOnWrapperTypes, List<ApiMember> WrapperAffected)
{
    public static ApiDiff Compute(ApiSurface oldSurface, ApiSurface newSurface, WrapperUsage usage)
    {
        var added = newSurface.MembersByKey.Values
            .Where(x => !oldSurface.MembersByKey.ContainsKey(x.Key))
            .ToList();
        var removed = oldSurface.MembersByKey.Values
            .Where(x => !newSurface.MembersByKey.ContainsKey(x.Key))
            .ToList();
        var newlyObsolete = newSurface.MembersByKey.Values
            .Where(x => x.IsObsolete && oldSurface.MembersByKey.TryGetValue(x.Key, out var old) && !old.IsObsolete)
            .ToList();
        var addedTypes = added
            .Where(x => x.Kind == 'T')
            .Select(x => x.DeclaringType)
            .ToHashSet();
        var addedOnWrapperTypes = added
            .Where(x => usage.TypeNames.Contains(x.DeclaringType) && !addedTypes.Contains(x.DeclaringType))
            .ToList();

        var wrapperAffected = removed
            .Where(x => IsUsed(x, usage))
            .Select(x => x with { Note = "removed" })
            .Concat(newlyObsolete
                .Where(x => IsUsed(x, usage))
                .Select(x => x with { Note = "now [Obsolete]" }))
            .ToList();

        return new ApiDiff(added, removed, newlyObsolete, addedOnWrapperTypes, wrapperAffected);
    }

    private static bool IsUsed(ApiMember member, WrapperUsage usage)
    {
        return member.Kind == 'T'
            ? usage.TypeNames.Contains(member.DeclaringType)
            : member.AccessorKeys.Any(usage.MemberKeys.Contains);
    }
}

// Renders signature types as strings that are identical whether a type is a definition in the
// binding or a reference to it from the wrapper - that equality is what matches the wrapper's
// member references against the binding's members.
sealed class TypeNameProvider(bool isShort) : ISignatureTypeProvider<string, object?>
{
    public static string DefinitionName(MetadataReader reader, TypeDefinitionHandle handle)
    {
        var type = reader.GetTypeDefinition(handle);
        var name = reader.GetString(type.Name);
        var declaringType = type.GetDeclaringType();
        if (!declaringType.IsNil)
        {
            return $"{DefinitionName(reader, declaringType)}+{name}";
        }

        var ns = reader.GetString(type.Namespace);
        return ns.Length > 0 ? $"{ns}.{name}" : name;
    }

    public static string ReferenceName(MetadataReader reader, TypeReferenceHandle handle)
    {
        var type = reader.GetTypeReference(handle);
        var name = reader.GetString(type.Name);
        if (type.ResolutionScope.Kind == HandleKind.TypeReference)
        {
            return $"{ReferenceName(reader, (TypeReferenceHandle)type.ResolutionScope)}+{name}";
        }

        var ns = reader.GetString(type.Namespace);
        return ns.Length > 0 ? $"{ns}.{name}" : name;
    }

    public string GetPrimitiveType(PrimitiveTypeCode typeCode)
    {
        return Shorten($"System.{typeCode switch
        {
            PrimitiveTypeCode.IntPtr => "IntPtr",
            PrimitiveTypeCode.UIntPtr => "UIntPtr",
            PrimitiveTypeCode.TypedReference => "TypedReference",
            _ => typeCode.ToString(),
        }}");
    }

    public string GetTypeFromDefinition(MetadataReader reader, TypeDefinitionHandle handle, byte rawTypeKind)
    {
        return Shorten(DefinitionName(reader, handle));
    }

    public string GetTypeFromReference(MetadataReader reader, TypeReferenceHandle handle, byte rawTypeKind)
    {
        return Shorten(ReferenceName(reader, handle));
    }

    public string GetTypeFromSpecification(MetadataReader reader, object? genericContext, TypeSpecificationHandle handle, byte rawTypeKind)
    {
        return reader.GetTypeSpecification(handle).DecodeSignature(this, genericContext);
    }

    public string GetSZArrayType(string elementType)
    {
        return $"{elementType}[]";
    }

    public string GetArrayType(string elementType, ArrayShape shape)
    {
        return $"{elementType}[{new string(',', shape.Rank - 1)}]";
    }

    public string GetByReferenceType(string elementType)
    {
        return $"{elementType}&";
    }

    public string GetPointerType(string elementType)
    {
        return $"{elementType}*";
    }

    public string GetPinnedType(string elementType)
    {
        return elementType;
    }

    public string GetModifiedType(string modifier, string unmodifiedType, bool isRequired)
    {
        return unmodifiedType;
    }

    public string GetGenericInstantiation(string genericType, ImmutableArray<string> typeArguments)
    {
        return $"{genericType}<{string.Join(",", typeArguments)}>";
    }

    public string GetGenericTypeParameter(object? genericContext, int index)
    {
        return $"!{index}";
    }

    public string GetGenericMethodParameter(object? genericContext, int index)
    {
        return $"!!{index}";
    }

    public string GetFunctionPointerType(MethodSignature<string> signature)
    {
        return $"delegate*({string.Join(",", signature.ParameterTypes)})";
    }

    private string Shorten(string fullName)
    {
        if (!isShort)
        {
            return fullName;
        }

        var lastDot = fullName.LastIndexOf('.');
        var shortName = lastDot < 0
            ? fullName
            : fullName[(lastDot + 1)..];
        return Regex.Replace(shortName, @"`\d+", string.Empty);
    }
}

file sealed class UsageException(string message) : Exception(message);

// An upstream source that cannot be read, as opposed to one that answers "nothing here".
// Only the second is safe to describe in a package version that can never be edited again.
file sealed class SourceUnavailableException(string message) : Exception(message);
