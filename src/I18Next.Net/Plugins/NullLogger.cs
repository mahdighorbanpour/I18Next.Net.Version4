using I18Next.Net.Logging;
using System;

namespace I18Next.Net.Plugins;

/// <summary>
///     Logger implementation that does not log anything.
/// </summary>
public class NullLogger : ILogger
{
    public static NullLogger Instance { get; } = new();

    private NullLogger()
    {
    }

    public LogLevel LogLevel { get; set; } = LogLevel.Warning;

    public bool IsEnabled(LogLevel logLevel)
    {
        return false;
    }

    public void Log(LogLevel logLevel, Exception exception, string message, params object[] args)
    {
    }

    public void Log(LogLevel logLevel, string message, params object[] args)
    {
    }
}
