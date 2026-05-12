using System.Diagnostics;

namespace Postprocess.Tests;

/// <summary>
/// Invokes the file-based script at <c>.github/scripts/postprocess_ios_bindings.cs</c>
/// in a subprocess.
///
/// The script mutates files in place, so each test writes its fixture to a fresh
/// temp file, calls the script with that path, and reads the rewritten file back.
/// </summary>
internal static class ScriptRunner
{
    private static readonly string ScriptPath = LocateScript();

    public static string Run(string input)
    {
        var tempFile = Path.Combine(Path.GetTempPath(),
            $"postprocess_test_{Guid.NewGuid():N}.cs");
        File.WriteAllText(tempFile, input);
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "dotnet",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            psi.ArgumentList.Add("run");
            psi.ArgumentList.Add(ScriptPath);
            psi.ArgumentList.Add("--");
            psi.ArgumentList.Add(tempFile);

            using var p = Process.Start(psi)!;
            var stdout = p.StandardOutput.ReadToEnd();
            var stderr = p.StandardError.ReadToEnd();
            p.WaitForExit();
            if (p.ExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"Script exited {p.ExitCode}.\nstdout:\n{stdout}\nstderr:\n{stderr}");
            }
            return File.ReadAllText(tempFile);
        }
        finally
        {
            try { File.Delete(tempFile); } catch { /* ignore */ }
        }
    }

    private static string LocateScript()
    {
        // Walk up from the test assembly location until we find the script.
        // Layout: <repo>/tests/Postprocess.Tests/bin/<config>/<tfm>/<dll>
        //         <repo>/.github/scripts/postprocess_ios_bindings.cs
        var dir = AppContext.BaseDirectory;
        for (var i = 0; i < 8 && dir is not null; i++)
        {
            var candidate = Path.Combine(dir, ".github", "scripts", "postprocess_ios_bindings.cs");
            if (File.Exists(candidate)) return candidate;
            dir = Path.GetDirectoryName(dir);
        }
        throw new FileNotFoundException(
            "Could not locate .github/scripts/postprocess_ios_bindings.cs walking up from " +
            AppContext.BaseDirectory);
    }
}
