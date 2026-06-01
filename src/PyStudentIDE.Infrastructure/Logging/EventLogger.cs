using System.Text.Json;

namespace PyStudentIDE.Infrastructure.Logging;

public enum LogLevel
{
    Info,
    Warning,
    Error,
    Security
}

public class LogEntry
{
    public DateTime Timestamp { get; set; }
    public LogLevel Level { get; set; }
    public string Category { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public int? UserId { get; set; }
    public string? Metadata { get; set; }
}

public class EventLogger
{
    private readonly string _logPath;
    private readonly object _lock = new();

    public EventLogger(string logPath)
    {
        _logPath = logPath;
    }

    public void Log(LogEntry entry)
    {
        entry.Timestamp = DateTime.UtcNow;
        var json = JsonSerializer.Serialize(entry);
        lock (_lock)
        {
            var dir = Path.GetDirectoryName(_logPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            File.AppendAllText(_logPath, json + Environment.NewLine);
        }
    }

    public void LogPasteAttempt(int userId)
    {
        Log(new LogEntry
        {
            Level = LogLevel.Security,
            Category = "PASTE_BLOCK",
            Message = "Paste attempt blocked in editor",
            UserId = userId
        });
    }

    public void LogDelivery(int userId, int assignmentId, string firma)
    {
        Log(new LogEntry
        {
            Level = LogLevel.Info,
            Category = "DELIVERY",
            Message = $"File delivered for assignment {assignmentId}",
            UserId = userId,
            Metadata = firma
        });
    }

    public void LogSignatureError(int userId, int assignmentId, string error)
    {
        Log(new LogEntry
        {
            Level = LogLevel.Error,
            Category = "SIGNATURE",
            Message = $"Signature failed for assignment {assignmentId}: {error}",
            UserId = userId
        });
    }

    public void LogServerError(string component, string error)
    {
        Log(new LogEntry
        {
            Level = LogLevel.Error,
            Category = "SERVER",
            Message = $"[{component}] {error}"
        });
    }
}
