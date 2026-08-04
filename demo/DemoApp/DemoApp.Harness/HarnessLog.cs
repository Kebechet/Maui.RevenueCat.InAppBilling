using System.Text;

namespace DemoApp.Harness;

/// <summary>
/// Thread-safe append-only log of harness activity, copyable from the device for pasting into reviews.
/// </summary>
public sealed class HarnessLog
{
    private readonly StringBuilder _logBuilder = new();
    private readonly object _lock = new();

    public event Action? Changed;

    public void Add(string message)
    {
        lock (_lock)
        {
            _logBuilder.AppendLine($"{DateTime.Now:HH:mm:ss.fff} {message}");
        }
        Changed?.Invoke();
    }

    public string AsText()
    {
        lock (_lock)
        {
            return _logBuilder.ToString();
        }
    }
}
