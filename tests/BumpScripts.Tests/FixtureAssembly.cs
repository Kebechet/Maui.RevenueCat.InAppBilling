using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace BumpScripts.Tests;

/// <summary>
/// Compiles small C# sources into assemblies on disk, so <c>api-report</c> can read a binding,
/// its baseline and a wrapper the way it reads the real ones: from metadata, with the binding
/// attributes referenced from another assembly as bgen and the Android generator reference them.
/// </summary>
internal static class FixtureAssembly
{
    /// <summary>
    /// Stand-ins for the attributes that name the native member: Microsoft.iOS's
    /// <c>Foundation.ExportAttribute</c> and Mono.Android's <c>Android.Runtime.RegisterAttribute</c>.
    /// </summary>
    public const string PlatformAttributesSource = """
        namespace Foundation
        {
            public sealed class ExportAttribute : System.Attribute
            {
                public ExportAttribute(string selector) { }
                public ExportAttribute(string selector, int semantic) { }
            }
        }

        namespace Android.Runtime
        {
            public sealed class RegisterAttribute : System.Attribute
            {
                public RegisterAttribute(string name) { }
                public RegisterAttribute(string name, string signature, string connector) { }
            }
        }
        """;

    private static readonly MetadataReference[] _runtimeReferences = ((string)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES")!)
        .Split(Path.PathSeparator)
        .Select(x => MetadataReference.CreateFromFile(x))
        .ToArray();

    /// <summary>
    /// Writes the assembly into a new temporary directory under its own name (which is what
    /// <c>api-report</c> matches the wrapper's references against) and returns its path.
    /// </summary>
    public static string Compile(string assemblyName, string source, params string[] referencePaths)
    {
        var compilation = CSharpCompilation.Create(
            assemblyName,
            [CSharpSyntaxTree.ParseText(source)],
            _runtimeReferences.Concat(referencePaths.Select(x => MetadataReference.CreateFromFile(x))),
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var directory = Path.Combine(Path.GetTempPath(), $"bump_test_{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, $"{assemblyName}.dll");
        var result = compilation.Emit(path);
        if (!result.Success)
        {
            var errors = result.Diagnostics.Where(x => x.Severity == DiagnosticSeverity.Error);
            throw new InvalidOperationException($"fixture {assemblyName} does not compile:\n{string.Join("\n", errors)}");
        }
        return path;
    }
}
