using System.Runtime.CompilerServices;

namespace System.Diagnostics;

/// <summary>
/// Defines a mechanism for capturing debug information for an object.
/// </summary>
public interface IDebuggable
{
    /// <summary>
    /// Gets or sets the debug information for this instance.
    /// </summary>
    DebugInfo? DebugInfo { get; set; }
}

/// <summary>
/// Represents captured debug information.
/// </summary>
public record DebugInfo(int ThreadId, string MemberName, string FilePath, int LineNumber, string? StackTrace);

/// <summary>Provides extension methods for types implementing <see cref="IDebuggable" />.</summary>
public static class DebuggableExtensions
{
    /// <summary>Captures the current execution context as debug information.</summary>
    public static T SetDebugInfo<T>(this T debuggable, 
        bool captureStackTrace = false,
        [CallerMemberName] string member = "",
        [CallerFilePath] string file = "",
        [CallerLineNumber] int line = 0) where T : IDebuggable
    {
        string? stackTrace = null;
        if (captureStackTrace)
        {
            stackTrace = new StackTrace(1, true).ToString();
        }
        debuggable.DebugInfo = new DebugInfo(Environment.CurrentManagedThreadId, member, file, line, stackTrace);
        return debuggable;
    }
}

internal static class DebugInfoStorage
{
    private static readonly ConditionalWeakTable<object, DebugInfo> s_table = new();

    public static DebugInfo? GetDebugInfo(object obj)
    {
        s_table.TryGetValue(obj, out var info);
        return info;
    }

    public static void SetDebugInfo(object obj, DebugInfo? info)
    {
        if (info is null)
        {
            s_table.Remove(obj);
            return;
        }
        s_table.AddOrUpdate(obj, info);
    }
}