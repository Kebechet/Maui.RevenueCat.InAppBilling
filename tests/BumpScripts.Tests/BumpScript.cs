using System.Diagnostics;

namespace BumpScripts.Tests;

/// <summary>
/// Runs the file-based script at <c>.github/scripts/bump.cs</c> in a subprocess, the way
/// the bump workflows call it, and returns its standard output.
/// </summary>
internal static class BumpScript
{
    private static readonly string _scriptPath = LocateScript();

    public static string Run(params string[] arguments)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = "dotnet",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };
        startInfo.ArgumentList.Add("run");
        startInfo.ArgumentList.Add(_scriptPath);
        startInfo.ArgumentList.Add("--");
        foreach (var argument in arguments)
        {
            startInfo.ArgumentList.Add(argument);
        }

        using var process = Process.Start(startInfo)!;
        var stdout = process.StandardOutput.ReadToEnd();
        var stderr = process.StandardError.ReadToEnd();
        process.WaitForExit();
        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"bump.cs exited {process.ExitCode}.\nstdout:\n{stdout}\nstderr:\n{stderr}");
        }
        return stdout.TrimEnd('\r', '\n');
    }

    public static string WriteTempFile(string content, string extension)
    {
        var path = Path.Combine(Path.GetTempPath(), $"bump_test_{Guid.NewGuid():N}{extension}");
        File.WriteAllText(path, content);
        return path;
    }

    // Layout: <repo>/tests/BumpScripts.Tests/bin/<config>/<tfm>/<dll> and
    //         <repo>/.github/scripts/bump.cs
    private static string LocateScript()
    {
        var directory = AppContext.BaseDirectory;
        for (var i = 0; i < 8 && directory is not null; i++)
        {
            var candidate = Path.Combine(directory, ".github", "scripts", "bump.cs");
            if (File.Exists(candidate))
            {
                return candidate;
            }
            directory = Path.GetDirectoryName(directory);
        }
        throw new FileNotFoundException($"Could not locate .github/scripts/bump.cs walking up from {AppContext.BaseDirectory}");
    }
}
