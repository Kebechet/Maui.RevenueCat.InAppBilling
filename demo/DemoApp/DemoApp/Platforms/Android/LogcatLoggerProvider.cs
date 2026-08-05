using Microsoft.Extensions.Logging;

namespace DemoApp;

/// <summary>
/// Routes ILogger output to Android logcat (tag "DemoHarness") so wrapper errors
/// are visible via adb without an attached debugger.
/// </summary>
public sealed class LogcatLoggerProvider : ILoggerProvider
{
    public ILogger CreateLogger(string categoryName)
    {
        return new LogcatLogger(categoryName);
    }

    public void Dispose()
    {
    }
}

public sealed class LogcatLogger : ILogger
{
    private const string Tag = "DemoHarness";

    private readonly string _categoryName;

    public LogcatLogger(string categoryName)
    {
        _categoryName = categoryName;
    }

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
    {
        return null;
    }

    public bool IsEnabled(LogLevel logLevel)
    {
        return logLevel >= LogLevel.Debug;
    }

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
    {
        var message = $"{_categoryName}: {formatter(state, exception)}";
        if (exception is not null)
        {
            message = $"{message}\n{exception}";
        }
        Android.Util.Log.WriteLine(ToLogPriority(logLevel), Tag, message);
    }

    private static Android.Util.LogPriority ToLogPriority(LogLevel logLevel)
    {
        return logLevel switch
        {
            LogLevel.Trace => Android.Util.LogPriority.Verbose,
            LogLevel.Debug => Android.Util.LogPriority.Debug,
            LogLevel.Information => Android.Util.LogPriority.Info,
            LogLevel.Warning => Android.Util.LogPriority.Warn,
            LogLevel.Error => Android.Util.LogPriority.Error,
            LogLevel.Critical => Android.Util.LogPriority.Error,
            _ => Android.Util.LogPriority.Info,
        };
    }
}
