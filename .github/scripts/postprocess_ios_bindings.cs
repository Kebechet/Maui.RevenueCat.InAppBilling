#:sdk Microsoft.NET.Sdk
#:property LangVersion=preview

// Post-process Objective Sharpie output for the Maui.RevenueCat.iOS binding.
//
// Applies the deterministic transformations documented in
// src/Maui.RevenueCat.iOS/README.md. Each pass is rule-based with bounded
// scope; anything that needs human judgement (delegate naming, the
// `Name = "..."` symbol-not-found workaround, etc.) is detected and listed
// in a manual-review report instead of edited.
//
// Run as a .NET 10 file-based script:
//     dotnet run .github/scripts/postprocess_ios_bindings.cs -- \
//         ApiDefinitions.cs StructsAndEnums.cs --report manual_review.md

using System.Text;
using System.Text.RegularExpressions;

if (args.Length == 0)
{
    Console.Error.WriteLine(
        "usage: dotnet run postprocess_ios_bindings.cs -- <file>... [--report <path>]");
    return 1;
}

string? reportPath = null;
var files = new List<string>();
for (var i = 0; i < args.Length; i++)
{
    if (args[i] == "--report" && i + 1 < args.Length)
    {
        reportPath = args[++i];
    }
    else
    {
        files.Add(args[i]);
    }
}

var results = new List<FileResult>();
foreach (var file in files)
{
    if (!File.Exists(file))
    {
        Console.Error.WriteLine($"WARN: skipping {file} - not found");
        continue;
    }
    Console.WriteLine($"Processing {file}");
    var r = Postprocess.ProcessFile(file);
    results.Add(r);
    foreach (var (name, delta) in r.PassStats)
    {
        var sign = delta < 0 ? "-" : (delta > 0 ? "+" : ".");
        Console.WriteLine($"   {sign} {name}: {delta:+#;-#;0} chars");
    }
    foreach (var (label, items) in r.Findings)
    {
        Console.WriteLine($"   ! {label}: {items.Count} item(s)");
    }
}

if (reportPath is not null)
{
    Postprocess.WriteReport(results, reportPath);
    Console.WriteLine($"Report written to {reportPath}");
}

return 0;


sealed record FileResult(
    string Path,
    int OriginalChars,
    int FinalChars,
    List<(string Name, int Delta)> PassStats,
    Dictionary<string, List<string>> Findings);


static class Postprocess
{
    // Platform-availability attribute names sharpie emits from clang macros.
    // Stripped wholesale (their presence on Windows hosts causes
    // symbol-not-found warnings and they're encoded for SupportedOSPlatformVersion
    // in the csproj instead).
    private static readonly string[] PlatformAttrNames =
    {
        "iOS", "NoiOS",
        "Mac", "NoMac",
        "MacCatalyst", "NoMacCatalyst",
        "Watch", "NoWatch", "WatchOS", "NoWatchOS",
        "TV", "NoTV", "TvOS", "NoTvOS",
    };

    // Matches a sharpie-style interface block:
    //   optional leading comment line(s)
    //   one or more attribute lines (e.g. [BaseType...], [DisableDefaultCtor], [Category])
    //   interface NAME (: BASE_LIST)?
    //   { ... } with one level of brace nesting allowed inside
    private static readonly Regex InterfaceBlockRe = new(
        @"(?<lead>(?:^[ \t]*//[^\n]*\n)*" +
        @"(?:^[ \t]*\[[^\]]+\][ \t]*\n)+)" +
        @"(?<decl>[ \t]*(?:partial\s+)?interface[ \t]+" +
        @"(?<name>[A-Za-z_]\w*)" +
        @"(?<bases>(?:[ \t]*:[ \t]*[^\n{]+)?)[ \t]*\n)" +
        @"(?<body>[ \t]*\{(?<inner>(?:[^{}]|\{[^{}]*\})*)\}[ \t]*\n?)",
        RegexOptions.Multiline | RegexOptions.Compiled);

    // Symbols the README flags as causing "Undefined symbols for architecture
    // arm64" at link time. Reported, not auto-deleted, because the right
    // action depends on whether the project actually links these symbols.
    private static readonly string[] LinkerRiskSymbols =
    {
        // README's original list (RevenueCat 5.34 / earlier).
        "FakeASIdManager", "FakeAfficheClient", "FakeTrackingManager",
        "NetworkOperation", "PaymentQueueWrapper", "ProductsFetcherSK1",
        "PurchasesReceiptParser", "StoreKit1Wrapper", "StoreKitRequestFetcher",
        "TrackingManagerProxy",
        // Added in RevenueCat 5.72 — same symptoms: "Undefined symbols for
        // architecture arm64: _OBJC_CLASS_$_<Name>" at link time. Sharpie
        // binds these as if they were public framework classes, but the
        // RevenueCat dylib doesn't export them.
        "StoreKit2PromotionalOfferPurchaseOptions",
        "RedirectLoggerSessionDelegate",
    };

    // Obj-C selectors of NSObject methods bindings inherit and shouldn't
    // re-declare (README).
    private static readonly string[] InheritedNsObjectSelectors =
        { "isEqual:", "description", "debugDescription" };

    // Suffix sharpie applies to category-style Swift extensions of an Obj-C
    // type. Example: `RCAttribution_RevenueCat_Swift_3714` extends `RCAttribution`.
    private static readonly Regex SwiftSuffixRe = new(
        @"^(?<base>.+?)_RevenueCat_Swift_\d+$", RegexOptions.Compiled);

    // ---- transformation passes ----

    // Drop sharpie's [Verify(...)] markers.
    private static string StripVerifyAttributes(string text)
    {
        text = Regex.Replace(text, @"^[ \t]*\[Verify[ \t]*\([^)]*\)\][ \t]*\n", "",
            RegexOptions.Multiline);
        text = Regex.Replace(text, @"\[Verify[ \t]*\([^)]*\)\][ \t]*", "");
        return text;
    }

    // Collapse sharpie's multi-line documentation comments.
    //
    // Sharpie sometimes emits `// @property ... __attribute__((deprecated("`
    // where the deprecation message contains physical newlines. Only the
    // first line begins with `//`, so subsequent lines are parsed as code by
    // the C# compiler and break the build.
    //
    // Detection rule: a line that starts with `//` and ends with `("` (string
    // opens but doesn't close on that line) is the opener. Walk forward
    // joining everything onto that line until we see a line containing
    // `")` followed by closing parens. The result is one long `//` comment
    // on a single line.
    private static string CollapseMultilineDocComments(string text)
    {
        var openRe = new Regex(@"^[ \t]*//.*\(""[ \t]*$");
        var closerInLine = new Regex(@"""\)+;?");

        var lines = text.Split('\n');
        var keep = new List<string>(lines.Length);
        for (var i = 0; i < lines.Length; i++)
        {
            if (!openRe.IsMatch(lines[i]))
            {
                keep.Add(lines[i]);
                continue;
            }

            // Try to find the closing line within a bounded lookahead (sharpie
            // never emits massive deprecation strings; 40 lines is plenty).
            var sb = new StringBuilder(lines[i].TrimEnd());
            var j = i + 1;
            var closed = false;
            while (j < lines.Length && j - i <= 40)
            {
                sb.Append(' ').Append(lines[j].TrimStart());
                if (closerInLine.IsMatch(lines[j]))
                {
                    closed = true;
                    break;
                }
                j++;
            }

            if (closed)
            {
                keep.Add(sb.ToString());
                i = j;
            }
            else
            {
                // Couldn't find a clean close — leave the original lines alone
                // rather than mangle them. Surface this in the report instead.
                keep.Add(lines[i]);
            }
        }
        return string.Join('\n', keep);
    }

    // Comment out `(AutoGeneratedName = true)` per README.
    private static string CommentAutoGeneratedName(string text) =>
        Regex.Replace(text, @"\(AutoGeneratedName = true\)",
            "/* (AutoGeneratedName = true) */");

    // Replace `NSUrlRequest` with `NSMutableUrlRequest` (README).
    private static string ReplaceNsUrlRequest(string text) =>
        Regex.Replace(text, @"\bNSUrlRequest\b", "NSMutableUrlRequest");

    // Normalize sharpie's Objective-C all-caps acronyms to the PascalCase that
    // Xamarin / .NET for iOS exposes.
    //
    // Sharpie emits names verbatim from the Cocoa headers (e.g. NSURL, NSUUID,
    // NSHTTPURLResponse). The managed bindings publish them de-acronymized
    // (NSUrl, NSUuid, NSHttpUrlResponse). Without this rename the build fails
    // with CS0246 "type or namespace not found".
    //
    // Map covers the names we've actually observed sharpie emit for RevenueCat;
    // add more if new ones show up. Each rule is `\b<from>\b -> <to>`.
    private static readonly (string From, string To)[] CocoaCasingRules =
    {
        ("INSURLSessionTaskDelegate", "INSUrlSessionTaskDelegate"),
        ("NSURLSessionTaskDelegate",  "NSUrlSessionTaskDelegate"),
        ("NSURLSessionTask",          "NSUrlSessionTask"),
        ("NSURLSession",              "NSUrlSession"),
        ("NSURLRequest",              "NSUrlRequest"),
        ("NSHTTPURLResponse",         "NSHttpUrlResponse"),
        ("NSURL",                     "NSUrl"),
        ("NSUUID",                    "NSUuid"),
    };

    private static string NormalizeCocoaCasing(string text)
    {
        foreach (var (from, to) in CocoaCasingRules)
        {
            text = Regex.Replace(text, $@"\b{Regex.Escape(from)}\b", to);
        }
        return text;
    }

    // Strip sharpie's bogus `using RevenueCat;` at the top of ApiDefinitions.cs.
    //
    // Sharpie infers a `using <FrameworkName>;` directive from the framework
    // it bound, but `RevenueCat` is the native Swift module — not a managed
    // namespace — so the line produces a CS0246 error.
    private static string StripBogusUsingRevenueCat(string text) =>
        Regex.Replace(text, @"^using[ \t]+RevenueCat[ \t]*;[ \t]*\n", "",
            RegexOptions.Multiline);

    // Sharpie sometimes emits `*/[Attribute]` (block-comment-close immediately
    // followed by an attribute) on a single line. While that's syntactically
    // valid C#, Xamarin's binding generator (bgen) appears to miss the
    // `[Protocol]` attribute in that layout and fails to emit the auto-prefixed
    // `IFoo` interface, leaving downstream `: IFoo` references unresolved.
    // Inserting a newline between `*/` and `[` keeps bgen happy.
    private static string SplitBlockCommentAttributeAdjacency(string text) =>
        Regex.Replace(text, @"\*/(\[)", "*/\n\t$1");

    // Remove the interface blocks for symbols that sharpie binds but the iOS
    // linker rejects (README troubleshooting). These are internal RevenueCat
    // helper types whose names aren't exported by the framework — leaving the
    // bindings in produces "Undefined symbols for architecture arm64" at link.
    //
    // v5.34.0.1 of this binding (the last working release) had every one of
    // these removed manually; the README documents the same workaround.
    private static string RemoveLinkerRiskInterfaces(string text)
    {
        var names = new HashSet<string>(LinkerRiskSymbols);

        // Sharpie binds Swift extensions of these internal types as
        // `[Category] [BaseType (typeof(X))]` interfaces (e.g.
        // `PaymentQueueWrapper_RevenueCat_Swift_2020`). Once the `X` interface
        // is dropped below, those categories would dangle (CS0246) — and a
        // category body can itself reference `X` (e.g. a static `default`
        // accessor returning `X`). So drop any interface whose base type is a
        // linker-risk symbol too, alongside the symbol's own interface.
        var baseTypeRe = new Regex(
            @"\[BaseType[ \t]*\([ \t]*typeof[ \t]*\([ \t]*(?<t>[A-Za-z_]\w*)[ \t]*\)");

        var edits = new List<(int Start, int End)>();
        foreach (Match m in InterfaceBlockRe.Matches(text))
        {
            var dropByName = names.Contains(m.Groups["name"].Value);

            var dropByBase = false;
            foreach (Match b in baseTypeRe.Matches(m.Groups["lead"].Value))
            {
                if (names.Contains(b.Groups["t"].Value))
                {
                    dropByBase = true;
                    break;
                }
            }

            if (dropByName || dropByBase)
            {
                edits.Add((m.Index, m.Index + m.Length));
            }
        }
        foreach (var (start, end) in edits.OrderByDescending(e => e.Start))
        {
            text = text[..start] + text[end..];
        }
        return text;
    }

    // Remove `[Protocol]` interfaces that don't also carry `[Model]`,
    // along with every reference to their auto-generated `I<ProtocolName>`
    // in other interfaces' base lists (README — "remove Protocols that were
    // used for inheritance").
    //
    // Keep-rule: `[Protocol, Model]` (or `[Model, Protocol]`, or `[Protocol]`
    // alongside a separate `[Model]` attribute) means the protocol is meant
    // to be implemented by consumers (e.g. RCPurchasesDelegate). Those stay.
    private static string RemoveInheritanceOnlyProtocols(string text)
    {
        var inheritanceProtocols = new HashSet<string>();
        foreach (Match m in InterfaceBlockRe.Matches(text))
        {
            var lead = m.Groups["lead"].Value;
            if (!Regex.IsMatch(lead, @"\[Protocol\b")) continue;
            if (Regex.IsMatch(lead, @"\bModel\b")) continue; // keep consumer protocols
            inheritanceProtocols.Add(m.Groups["name"].Value);
        }
        if (inheritanceProtocols.Count == 0) return text;

        // Scrub the `: I<Name>` base-list references first so the standalone
        // protocol blocks are easier to detect by the empty-removal logic.
        foreach (var name in inheritanceProtocols)
        {
            var iname = Regex.Escape("I" + name);
            // Patterns to cover, on the `interface X : <BaseList>` line:
            //   `: IName`            (only base)              -> drop `: IName`
            //   `: IName, OtherBase` (first in list)          -> drop `IName, `
            //   `: OtherBase, IName` (later in list)          -> drop `, IName`
            //   `: OtherBase, IName, Another` (middle of list)-> drop `, IName`
            //
            // Run the "middle/end" pattern first so we don't strip the
            // leading colon prematurely.
            text = Regex.Replace(text, $@",[ \t]*{iname}\b", "");
            text = Regex.Replace(text, $@"{iname}[ \t]*,[ \t]*", "");
            text = Regex.Replace(text, $@"[ \t]*:[ \t]*{iname}\b", "");
        }

        // Now remove the protocol blocks themselves.
        var edits = new List<(int Start, int End)>();
        foreach (Match m in InterfaceBlockRe.Matches(text))
        {
            if (inheritanceProtocols.Contains(m.Groups["name"].Value))
            {
                edits.Add((m.Index, m.Index + m.Length));
            }
        }
        foreach (var (start, end) in edits.OrderByDescending(e => e.Start))
        {
            text = text[..start] + text[end..];
        }
        return text;
    }

    // Strip literal `\n` escape sequences inside [Obsoleted("...")] (README).
    private static string StripObsoletedNewlines(string text) =>
        Regex.Replace(text,
            @"\[Obsoleted[ \t]*\(""(?:[^""\\]|\\.)*""\)\]",
            m => m.Value.Replace(@"\n", ""));

    // Drop members marked [Obsoleted(...)] or [Deprecated(...)] (README).
    //
    // Algorithm:
    //   1. Walk line by line; on a marker line, look ahead past following
    //      attribute / comment lines.
    //   2. If the next non-attribute/comment line is an `interface NAME ...`
    //      declaration, this is an INTERFACE-level marker — leave it alone
    //      (RemoveObsoleteInterfaces handles those). Without this guard the
    //      pass would walk forward looking for `;`, miss it (interface body
    //      is `{}`), and consume the next interface's members.
    //   3. Otherwise treat it as a MEMBER marker: rewind over preceding
    //      attribute lines already emitted, then fast-forward to the line
    //      containing `;` outside parentheses. Drop that whole region.
    private static string RemoveObsoleteMembers(string text)
    {
        var lines = text.Split('\n');
        var markerRe = new Regex(@"^[ \t]*\[(?:Obsoleted|Deprecated)[ \t]*\(");
        var attrRe = new Regex(@"^[ \t]*\[[A-Za-z]");
        var interfaceRe = new Regex(@"^[ \t]*(?:partial\s+)?interface[ \t]+");
        var keep = new List<string>(lines.Length);

        for (var i = 0; i < lines.Length; i++)
        {
            if (!markerRe.IsMatch(lines[i]))
            {
                keep.Add(lines[i]);
                continue;
            }

            // (1) Lookahead: skip following attribute / comment lines and see
            // what kind of declaration the marker stack is sitting on.
            var ahead = i + 1;
            while (ahead < lines.Length &&
                   (attrRe.IsMatch(lines[ahead]) ||
                    lines[ahead].TrimStart().StartsWith("//")))
            {
                ahead++;
            }
            if (ahead < lines.Length && interfaceRe.IsMatch(lines[ahead]))
            {
                // (2) Interface-level marker — not this pass's job.
                keep.Add(lines[i]);
                continue;
            }

            // (3) Member-level removal.
            while (keep.Count > 0 && attrRe.IsMatch(keep[^1]))
            {
                keep.RemoveAt(keep.Count - 1);
            }
            var j = i;
            var depth = 0;
            while (j < lines.Length)
            {
                depth += lines[j].Count(c => c == '(') - lines[j].Count(c => c == ')');
                if (depth <= 0 && lines[j].Contains(';')) break;
                j++;
            }
            if (j + 1 < lines.Length && string.IsNullOrWhiteSpace(lines[j + 1]))
            {
                j++;
            }
            i = j;
        }
        return string.Join('\n', keep);
    }

    // Drop entire interface blocks whose lead attributes carry
    // [Obsoleted(...)] or [Deprecated(...)] (README).
    private static string RemoveObsoleteInterfaces(string text)
    {
        var markerRe = new Regex(@"\[(?:Obsoleted|Deprecated)[ \t]*\(");
        var edits = new List<(int Start, int End)>();
        foreach (Match m in InterfaceBlockRe.Matches(text))
        {
            if (markerRe.IsMatch(m.Groups["lead"].Value))
            {
                edits.Add((m.Index, m.Index + m.Length));
            }
        }
        edits.Sort((a, b) => b.Start.CompareTo(a.Start));
        foreach (var (start, end) in edits)
        {
            text = text[..start] + text[end..];
        }
        return text;
    }

    // Drop device-specific availability attributes (README).
    //
    // Targets:
    //   - classic forms: [iOS (X, Y)], [NoiOS], [Mac (...)], [NoMac], ...
    //   - PlatformName-based: [Introduced (PlatformName.iOS, ...)],
    //     [Unavailable (PlatformName.iOS)], [Deprecated (PlatformName.iOS, ...)].
    //
    // Note: this must run AFTER RemoveObsoleteMembers / RemoveObsoleteInterfaces
    // so the obsolete-removal passes still see [Deprecated(PlatformName...)]
    // markers and remove the whole member/interface, not just the attribute.
    private static string StripPlatformAvailabilityAttributes(string text)
    {
        var nameAlt = string.Join("|", PlatformAttrNames.Select(Regex.Escape));
        var platformEntry = $@"(?:{nameAlt})(?:[ \t]*\([^)]*\))?";

        // Standalone single platform attribute on its own line.
        text = Regex.Replace(
            text,
            $@"^[ \t]*\[{platformEntry}\][ \t]*\n",
            "",
            RegexOptions.Multiline);

        // Combined form: a single `[...]` whose entries are ALL platform
        // attributes, comma-separated. Sharpie emits this shape — e.g.
        // `[Watch (8,0), TV (15,0), Mac (12,0), iOS (15,0)]` — and bgen on
        // recent .NET-for-iOS rejects it because `MacAttribute`/`WatchAttribute`
        // aren't part of the net9.0-ios targeting pack.
        text = Regex.Replace(
            text,
            $@"^[ \t]*\[(?:{platformEntry}[ \t]*,[ \t]*)+{platformEntry}\][ \t]*\n",
            "",
            RegexOptions.Multiline);

        // Inline single platform attribute anywhere.
        text = Regex.Replace(
            text,
            $@"\[{platformEntry}\][ \t]*",
            "");

        foreach (var attr in new[] { "Introduced", "Unavailable", "Deprecated" })
        {
            text = Regex.Replace(
                text,
                $@"^[ \t]*\[{attr}[ \t]*\([ \t]*PlatformName\.[^)]*\)\][ \t]*\n",
                "",
                RegexOptions.Multiline);
            text = Regex.Replace(
                text,
                $@"\[{attr}[ \t]*\([ \t]*PlatformName\.[^)]*\)\][ \t]*",
                "");
        }
        return text;
    }

    // Drop members whose [Export("...")] selector matches isEqual: / description
    // / debugDescription -- inherited from NSObject (README).
    private static string RemoveInheritedNsObjectMethods(string text)
    {
        var selAlt = string.Join("|", InheritedNsObjectSelectors.Select(Regex.Escape));
        var targetRe = new Regex(
            $@"^[ \t]*\[[ \t]*Export[ \t]*\([ \t]*""(?:{selAlt})""[ \t]*\)\]");
        var attrRe = new Regex(@"^[ \t]*\[[A-Za-z]");

        var lines = text.Split('\n');
        var keep = new List<string>(lines.Length);
        for (var i = 0; i < lines.Length; i++)
        {
            if (targetRe.IsMatch(lines[i]))
            {
                while (keep.Count > 0 && attrRe.IsMatch(keep[^1]))
                {
                    keep.RemoveAt(keep.Count - 1);
                }
                var j = i;
                while (j < lines.Length && !lines[j].Contains(';')) j++;
                if (j + 1 < lines.Length && string.IsNullOrWhiteSpace(lines[j + 1])) j++;
                i = j;
                continue;
            }
            keep.Add(lines[i]);
        }
        return string.Join('\n', keep);
    }

    // Fold XXX_RevenueCat_Swift_NNNN interfaces into the canonical XXX
    // interface (README, e.g. `RCAttribution_RevenueCat_Swift_3714`).
    //
    // Canonical's attributes/base list are preserved; only the suffix
    // interface's *members* are appended. The suffix block (lead attributes
    // + body) is removed in full.
    //
    // A single canonical can have multiple suffix counterparts (sharpie emits
    // a new `_Swift_NNNN` per category). All of them must be folded into ONE
    // composite edit on the canonical, otherwise two edits at the same range
    // applied in sequence corrupt indices and produce broken output.
    private static string MergeSwiftSuffixInterfaces(string text)
    {
        var byName = new Dictionary<string, Match>();
        foreach (Match m in InterfaceBlockRe.Matches(text))
        {
            byName[m.Groups["name"].Value] = m;
        }

        // Group all suffix interfaces by their canonical target so each
        // canonical receives exactly one merge edit.
        var suffixesByCanonical = new Dictionary<string, List<Match>>();
        foreach (var (name, m) in byName)
        {
            var sm = SwiftSuffixRe.Match(name);
            if (!sm.Success) continue;
            var baseName = sm.Groups["base"].Value;
            if (!byName.ContainsKey(baseName)) continue;
            if (!suffixesByCanonical.TryGetValue(baseName, out var list))
            {
                list = new List<Match>();
                suffixesByCanonical[baseName] = list;
            }
            list.Add(m);
        }

        if (suffixesByCanonical.Count == 0) return text;

        var edits = new List<(int Start, int End, string Replacement)>();
        foreach (var (canonicalName, suffixes) in suffixesByCanonical)
        {
            var canonical = byName[canonicalName];

            // Concatenate suffix bodies in source order. Preserve the
            // pre-`}` whitespace from canonical's body (typically `\n\t`)
            // so the closing brace stays indented after we rewrite the body.
            suffixes.Sort((a, b) => a.Index.CompareTo(b.Index));
            var origInner = canonical.Groups["inner"].Value;
            var innerNoTrailingWs = origInner.TrimEnd();
            var preCloseWs = origInner[innerNoTrailingWs.Length..]; // "\n\t" etc.

            var sb = new StringBuilder(innerNoTrailingWs);
            foreach (var s in suffixes)
            {
                var si = s.Groups["inner"].Value.Trim('\n').TrimEnd();
                if (si.Length == 0) continue;
                sb.Append("\n\n").Append(si);
            }
            sb.Append(preCloseWs);
            var newInner = sb.ToString();

            // Preserve canonical's body indentation (`\t{`) and its trailing
            // whitespace/newline so the rest of the file stays formatted.
            var origBody = canonical.Groups["body"].Value;
            var openIdx = origBody.IndexOf('{');
            var leadingWs = openIdx > 0 ? origBody[..openIdx] : "";
            var trailing = origBody[origBody.TrimEnd().Length..];

            var newBody = leadingWs + "{" + newInner + "}" + trailing;
            var newCanonical =
                canonical.Groups["lead"].Value +
                canonical.Groups["decl"].Value +
                newBody;

            edits.Add((canonical.Index, canonical.Index + canonical.Length, newCanonical));
            foreach (var s in suffixes)
            {
                edits.Add((s.Index, s.Index + s.Length, ""));
            }
        }

        edits.Sort((a, b) => b.Start.CompareTo(a.Start));
        foreach (var (start, end, replacement) in edits)
        {
            text = text[..start] + replacement + text[end..];
        }
        return text;
    }

    // Drop interface blocks whose bodies contain no real members after the
    // earlier passes (README). Iterates to a fixed point.
    //
    // An interface is only dropped if its name is NOT referenced elsewhere
    // in the file. Sharpie's output contains forward-declaration-style empty
    // interfaces (e.g. `interface RCPurchaseParams { }`) that other interfaces
    // use as parameter or return types. Removing them outright would leave
    // dangling references and produce CS0246 errors.
    private static string RemoveEmptyInterfaces(string text)
    {
        while (true)
        {
            var edits = new List<(int Start, int End)>();
            foreach (Match m in InterfaceBlockRe.Matches(text))
            {
                var inner = m.Groups["inner"].Value;
                var noBlock = Regex.Replace(inner, @"/\*.*?\*/", "", RegexOptions.Singleline);
                var noLine = Regex.Replace(noBlock, @"//[^\n]*", "");
                if (!string.IsNullOrWhiteSpace(noLine)) continue;

                var name = m.Groups["name"].Value;
                // Count occurrences of the name outside this interface block.
                // The match's own `interface NAME` declaration contributes
                // one reference, so we need >1 elsewhere to call it referenced.
                var before = text[..m.Index];
                var after = text[(m.Index + m.Length)..];
                var pattern = $@"\b{Regex.Escape(name)}\b";
                if (Regex.IsMatch(before, pattern) || Regex.IsMatch(after, pattern))
                {
                    // Referenced elsewhere — keep the empty declaration.
                    continue;
                }
                edits.Add((m.Index, m.Index + m.Length));
            }
            if (edits.Count == 0) return text;
            edits.Sort((a, b) => b.Start.CompareTo(a.Start));
            foreach (var (start, end) in edits)
            {
                text = text[..start] + text[end..];
            }
        }
    }

    // Add INativeObject to every interface used as a type parameter of
    // NSDictionary<TKey, TValue> or NSArray<T> / NSSet<T> (README).
    //
    // These generics constrain their parameters to INativeObject. Without
    // the marker the build fails with
    //   "cannot be used as type parameter ... in the generic type or
    //    method 'NSArray<TKey>'" / 'NSDictionary<...>' / 'NSSet<...>'.
    private static string AddInativeObjectForDictValues(string text)
    {
        var valueTypes = new HashSet<string>();

        // NSDictionary<K, V> — V is constrained.
        foreach (Match m in Regex.Matches(text,
                     @"NSDictionary[ \t]*<[ \t]*[A-Za-z_]\w*[ \t]*,[ \t]*([A-Za-z_]\w*)[ \t]*>"))
        {
            valueTypes.Add(m.Groups[1].Value);
        }

        // NSArray<T> and NSSet<T> — T is constrained.
        foreach (Match m in Regex.Matches(text,
                     @"NS(?:Array|Set)[ \t]*<[ \t]*([A-Za-z_]\w*)[ \t]*>"))
        {
            valueTypes.Add(m.Groups[1].Value);
        }

        if (valueTypes.Count == 0) return text;

        return InterfaceBlockRe.Replace(text, m =>
        {
            var name = m.Groups["name"].Value;
            if (!valueTypes.Contains(name)) return m.Value;
            var bases = m.Groups["bases"].Value;
            if (bases.Contains("INativeObject")) return m.Value;

            var newBases = bases.TrimStart().StartsWith(':')
                ? bases.TrimEnd() + ", INativeObject"
                : " : INativeObject";

            var declWithBases = Regex.Replace(
                m.Groups["decl"].Value,
                @"interface[ \t]+" + Regex.Escape(name) + @"(?:[ \t]*:[ \t]*[^\n{]+)?",
                $"interface {name}{newBases}");

            return m.Groups["lead"].Value + declWithBases + m.Groups["body"].Value;
        });
    }

    // Rename C# methods that share a signature within the same interface but
    // bind different ObjC selectors.
    //
    // Sharpie sometimes emits two `[Export(...)]` blocks whose C# signatures
    // collide (same return type, name, and parameter types — only the
    // parameter name or the underlying selector differs). C# treats these as
    // duplicate definitions (CS0111). To preserve both bindings, rename the
    // second by appending a PascalCased token derived from its selector's
    // distinguishing tail.
    //
    // Example sharpie output:
    //     [Export ("logIn:completion:")]
    //     void LogIn (string appUserID, Action<...> completion);
    //     [Export ("logIn:completionHandler:")]
    //     void LogIn (string appUserID, Action<...> completionHandler);
    //
    // After this pass the second becomes `void LogInCompletionHandler(...)`.
    // Matches the rename pattern in the v5.34.0.1 hand-cleaned binding.
    private static string DeduplicateOverloadedMethods(string text)
    {
        // Re-evaluate per interface so signatures from one interface don't
        // collide with another's.
        return InterfaceBlockRe.Replace(text, m =>
        {
            var inner = m.Groups["inner"].Value;
            var newInner = DedupeBody(inner);
            if (ReferenceEquals(newInner, inner) || newInner == inner) return m.Value;
            return m.Groups["lead"].Value + m.Groups["decl"].Value + "{" + newInner + "}";

            static string DedupeBody(string body)
            {
                // A member block: zero+ comment lines, one+ attribute lines,
                // then a signature line ending with `;`. Capture each block.
                var memberRe = new Regex(
                    @"(?<attrs>(?:^[ \t]*//[^\n]*\n)*(?:^[ \t]*\[[^\]]+\][ \t]*\n)+)" +
                    @"(?<sigLine>^[ \t]*[^\n]*?\b(?<name>[A-Za-z_]\w*)[ \t]*\((?<params>[^;]*)\)[ \t]*;[ \t]*\n)",
                    RegexOptions.Multiline);

                var seen = new HashSet<string>();
                var edits = new List<(int Start, int End, string Replacement)>();
                foreach (Match mm in memberRe.Matches(body))
                {
                    var methodName = mm.Groups["name"].Value;
                    // Skip property declarations (no parentheses, but `[A-Za-z_]\w*` + `(` won't match them anyway).
                    var paramTypes = NormalizeParamTypes(mm.Groups["params"].Value);
                    var key = methodName + "(" + paramTypes + ")";
                    if (seen.Add(key)) continue; // first time — keep

                    // Duplicate. Extract a suffix from the [Export] selector.
                    var exportMatch = Regex.Match(mm.Groups["attrs"].Value,
                        @"\[Export[ \t]*\([ \t]*""(?<sel>[^""]+)""");
                    if (!exportMatch.Success) continue;

                    var suffix = SelectorTailToPascal(exportMatch.Groups["sel"].Value, methodName);
                    if (suffix.Length == 0) continue;

                    // Rewrite the signature line: replace the *first* occurrence
                    // of the method name (the method declarator, not types in
                    // its parameter list) with name+suffix.
                    var sigLine = mm.Groups["sigLine"].Value;
                    var renamed = new Regex(
                        $@"\b{Regex.Escape(methodName)}\b[ \t]*\(")
                        .Replace(sigLine, methodName + suffix + " (", 1);
                    if (renamed == sigLine) continue;

                    var sigStart = mm.Index + mm.Groups["attrs"].Length;
                    edits.Add((sigStart, sigStart + sigLine.Length, renamed));
                }

                if (edits.Count == 0) return body;
                edits.Sort((a, b) => b.Start.CompareTo(a.Start));
                foreach (var (start, end, replacement) in edits)
                {
                    body = body[..start] + replacement + body[end..];
                }
                return body;
            }

            static string NormalizeParamTypes(string list)
            {
                // Walk the parameter list, splitting on commas at top-level
                // (don't split inside <>, [], or ()). For each part, drop
                // leading [Attribute] decorations and the trailing parameter
                // name, leaving just the type.
                var parts = new List<string>();
                var depth = 0;
                var start = 0;
                for (var i = 0; i < list.Length; i++)
                {
                    var c = list[i];
                    if (c is '<' or '(' or '[') depth++;
                    else if (c is '>' or ')' or ']') depth--;
                    else if (c == ',' && depth == 0)
                    {
                        parts.Add(list[start..i]);
                        start = i + 1;
                    }
                }
                parts.Add(list[start..]);

                for (var i = 0; i < parts.Count; i++)
                {
                    var p = parts[i].Trim();
                    p = Regex.Replace(p, @"\[[^\]]+\][ \t]*", "");
                    var lastWs = p.LastIndexOfAny(new[] { ' ', '\t' });
                    if (lastWs > 0) p = p[..lastWs].TrimEnd();
                    parts[i] = p;
                }
                return string.Join(",", parts);
            }

            static string SelectorTailToPascal(string selector, string methodName)
            {
                // Selector like "logIn:completionHandler:" -> take the last
                // non-empty colon-separated segment that doesn't reproduce
                // the method-name prefix.
                var parts = selector.Split(':');
                for (var i = parts.Length - 1; i >= 0; i--)
                {
                    var p = parts[i];
                    if (p.Length == 0) continue;
                    if (string.Equals(p, methodName, StringComparison.OrdinalIgnoreCase)) continue;
                    if (methodName.StartsWith(p, StringComparison.OrdinalIgnoreCase)) continue;
                    return char.ToUpperInvariant(p[0]) + p[1..];
                }
                return string.Empty;
            }
        });
    }

    // Convert a single block-scoped namespace to a file-scoped declaration
    // and dedent the contained block by one level. Matches the layout of the
    // hand-cleaned v5.34.0.1 binding.
    //
    //   namespace Foo                    namespace Foo;
    //   {                       =>
    //       interface Bar { ... }        interface Bar { ... }
    //   }
    //
    // Only fires if the file contains exactly one namespace declaration whose
    // body extends to a final closing `}`. Otherwise leaves the file alone.
    private static string UseFileScopedNamespace(string text)
    {
        var nsRe = new Regex(
            @"^namespace[ \t]+(?<name>[A-Za-z_][\w.]*)[ \t]*\r?\n\{[ \t]*\r?\n",
            RegexOptions.Multiline);
        var match = nsRe.Match(text);
        if (!match.Success) return text;

        // Reject if there's a second namespace declaration — out of scope.
        if (nsRe.Match(text, match.Index + match.Length).Success) return text;

        // Find the matching closing brace: walk from end of file backward,
        // skipping trailing whitespace, expecting `}` then file end.
        var tail = text.TrimEnd();
        if (!tail.EndsWith("}")) return text;
        var closeIndex = tail.Length - 1;

        var bodyStart = match.Index + match.Length;
        var body = text[bodyStart..closeIndex];
        var trailing = text[(closeIndex + 1)..];

        // Dedent body by one level. Sharpie uses tab indentation, but accept
        // either a single leading tab or 4 leading spaces per line.
        var dedented = Regex.Replace(body, @"^(\t|    )", "", RegexOptions.Multiline);

        return text[..match.Index] +
               $"namespace {match.Groups["name"].Value};\n\n" +
               dedented.TrimStart('\n', '\r').TrimEnd() + "\n" + trailing.TrimStart();
    }

    // Drop top-level `delegate <ret> <Name>(...);` declarations whose name
    // has no other reference in the file (README -- "removed delegate that
    // was not used anywhere").
    private static string RemoveUnusedDelegates(string text)
    {
        // `[ \t]*` allows the delegate to live inside a block-namespace
        // (sharpie's default) where it would be indented one level.
        var declRe = new Regex(
            @"^[ \t]*delegate[ \t]+[^\n;]+?[ \t](?<name>[A-Za-z_]\w*)[ \t]*\([^;]*\);[ \t]*\n",
            RegexOptions.Multiline);

        var matches = declRe.Matches(text).Cast<Match>().ToList();
        if (matches.Count == 0) return text;

        var edits = new List<(int Start, int End)>();
        foreach (var m in matches)
        {
            var name = m.Groups["name"].Value;
            var refCount = Regex.Matches(text, $@"\b{Regex.Escape(name)}\b").Count;
            if (refCount <= 1)
            {
                edits.Add((m.Index, m.Index + m.Length));
            }
        }
        edits.Sort((a, b) => b.Start.CompareTo(a.Start));
        foreach (var (start, end) in edits)
        {
            text = text[..start] + text[end..];
        }
        return text;
    }

    // ---- detection-only passes ----

    private static List<string> DetectBlockCallbackActions(string text)
    {
        var pattern = new Regex(
            @"\[BlockCallback\][ \t]*(?<kind>Action|Func)<(?<args>[^>]+)>" +
            @"[ \t]+(?<param>[A-Za-z_]\w*)");
        var hits = new List<string>();
        foreach (Match m in pattern.Matches(text))
        {
            hits.Add(
                $"`[BlockCallback] {m.Groups["kind"].Value}<{m.Groups["args"].Value.Trim()}> " +
                $"{m.Groups["param"].Value}` - extract to a named delegate");
        }
        return hits;
    }

    private static List<string> DetectLinkerRiskSymbols(string text)
    {
        var hits = new List<string>();
        foreach (var sym in LinkerRiskSymbols)
        {
            if (Regex.IsMatch(text, $@"\b{Regex.Escape(sym)}\b"))
            {
                hits.Add($"`{sym}` declared - delete from ApiDefinitions.cs if linker reports it as undefined");
            }
        }
        return hits;
    }

    private static List<string> DetectRemainingVerify(string text) =>
        Regex.IsMatch(text, @"\[Verify[ \t]*\(")
            ? new List<string> { "At least one `[Verify(...)]` attribute survived stripping - inspect." }
            : new List<string>();

    private static List<string> DetectNameAttributes(string text)
    {
        var count = Regex.Matches(text, @"Name[ \t]*=[ \t]*""[^""]+""").Count;
        if (count == 0) return new List<string>();
        return new List<string>
        {
            $"{count} `Name = \"...\"` attribute argument(s) present - " +
            "if Windows build emits symbol-not-found errors, the README's fix is to drop these.",
        };
    }

    // ---- driver ----

    // Order matters:
    //   1. Strip Verify before anything else looks at attribute structure.
    //   2. Run obsolete/deprecated member+interface removal BEFORE stripping
    //      [Deprecated (PlatformName...)] as a platform attr, otherwise the
    //      marker would be gone by the time the member-removal pass ran.
    //   3. Strip remaining platform attrs.
    //   4. Merge swift-suffix interfaces, then prune any interfaces left empty.
    //   5. Cross-reference passes (INativeObject, unused delegates) last.
    private static readonly (string Name, Func<string, string> Fn)[] Passes =
    {
        ("strip [Verify(...)]",                       StripVerifyAttributes),
        ("collapse multi-line // doc comments",       CollapseMultilineDocComments),
        ("comment (AutoGeneratedName = true)",        CommentAutoGeneratedName),
        ("normalize Cocoa acronym casing",            NormalizeCocoaCasing),
        ("strip bogus `using RevenueCat;`",           StripBogusUsingRevenueCat),
        ("split `*/[Attribute]` adjacency",           SplitBlockCommentAttributeAdjacency),
        ("remove linker-risk interfaces",             RemoveLinkerRiskInterfaces),
        ("remove inheritance-only [Protocol] types",  RemoveInheritanceOnlyProtocols),
        ("NSUrlRequest -> NSMutableUrlRequest",       ReplaceNsUrlRequest),
        ("strip \\n in [Obsoleted(...)]",             StripObsoletedNewlines),
        ("remove [Obsoleted/Deprecated] members",     RemoveObsoleteMembers),
        ("remove [Obsoleted/Deprecated] interfaces",  RemoveObsoleteInterfaces),
        ("strip device-specific availability attrs",  StripPlatformAvailabilityAttributes),
        ("remove inherited NSObject methods",         RemoveInheritedNsObjectMethods),
        ("merge _RevenueCat_Swift_NNNN interfaces",   MergeSwiftSuffixInterfaces),
        ("remove empty interfaces",                   RemoveEmptyInterfaces),
        ("add INativeObject for NSDictionary values", AddInativeObjectForDictValues),
        ("rename duplicate method overloads",         DeduplicateOverloadedMethods),
        ("remove unused top-level delegates",         RemoveUnusedDelegates),
        ("convert block namespace to file-scoped",    UseFileScopedNamespace),
    };

    private static readonly (string Label, Func<string, List<string>> Fn)[] Detectors =
    {
        ("Inline `[BlockCallback] Action<>` / `Func<>` parameters", DetectBlockCallbackActions),
        ("Surviving `[Verify]` attributes",                         DetectRemainingVerify),
        ("`Name = \"...\"` attribute arguments",                    DetectNameAttributes),
    };

    public static FileResult ProcessFile(string path)
    {
        var original = File.ReadAllText(path);
        // Normalize CRLF -> LF for regex compatibility; restore on write so
        // we don't churn line endings. sharpie itself emits LF on macOS, but
        // local testing on Windows can hit CRLF.
        var hadCrLf = original.Contains("\r\n");
        var text = hadCrLf ? original.Replace("\r\n", "\n") : original;
        var stats = new List<(string Name, int Delta)>();
        foreach (var (name, fn) in Passes)
        {
            var before = text.Length;
            text = fn(text);
            stats.Add((name, text.Length - before));
        }
        var output = hadCrLf ? text.Replace("\n", "\r\n") : text;
        File.WriteAllText(path, output);

        var findings = new Dictionary<string, List<string>>();
        foreach (var (label, fn) in Detectors)
        {
            var items = fn(text);
            if (items.Count > 0) findings[label] = items;
        }
        return new FileResult(path, original.Length, text.Length, stats, findings);
    }

    public static void WriteReport(List<FileResult> results, string reportPath)
    {
        var sb = new StringBuilder();
        sb.AppendLine("# iOS Bindings Post-Processing Report");
        sb.AppendLine();
        sb.AppendLine("Automated cleanups have been applied. The items below still need a human");
        sb.AppendLine("eye before the binding will compile cleanly.");
        sb.AppendLine();

        foreach (var r in results)
        {
            sb.AppendLine($"## `{System.IO.Path.GetFileName(r.Path)}`");
            sb.AppendLine();
            var delta = r.FinalChars - r.OriginalChars;
            sb.AppendLine($"- Original size: {r.OriginalChars} chars");
            sb.AppendLine($"- Final size:    {r.FinalChars} chars ({delta:+#;-#;0})");
            sb.AppendLine();
            sb.AppendLine("### Pass results");
            sb.AppendLine();
            sb.AppendLine("| Pass | Delta chars |");
            sb.AppendLine("|------|------------:|");
            foreach (var (name, d) in r.PassStats)
            {
                sb.AppendLine($"| {name} | {d:+#;-#;0} |");
            }
            sb.AppendLine();

            if (r.Findings.Count > 0)
            {
                sb.AppendLine("### Manual follow-ups detected");
                sb.AppendLine();
                foreach (var (label, items) in r.Findings)
                {
                    sb.AppendLine($"**{label}**");
                    sb.AppendLine();
                    foreach (var item in items)
                    {
                        sb.AppendLine($"- {item}");
                    }
                    sb.AppendLine();
                }
            }
            else
            {
                sb.AppendLine("_No issues detected by the static checks. Still skim the diff before merging._");
                sb.AppendLine();
            }
        }

        File.WriteAllText(reportPath, sb.ToString());
    }
}
