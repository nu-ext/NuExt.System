using System.Runtime.InteropServices;

namespace System;

/// <summary>
/// Provides platform detection properties.
/// </summary>
public static class PlatformInformation
{
    static PlatformInformation()
    {
        IsWindows = InitializeIsWindows();
        IsLinux = InitializeIsLinux();
        IsMacOS = InitializeIsMacOS();
        IsUnix = IsLinux || IsMacOS;

        Is64BitProcess = Environment.Is64BitProcess;
        OSDescription = GetOSDescription();
    }

    /// <summary>
    /// Gets a value indicating whether the current operating system is Windows.
    /// </summary>
    public static bool IsWindows { get; }

    /// <summary>
    /// Gets a value indicating whether the current operating system is Linux.
    /// </summary>
    public static bool IsLinux { get; }

    /// <summary>
    /// Gets a value indicating whether the current operating system is macOS.
    /// </summary>
    public static bool IsMacOS { get; }

    /// <summary>
    /// Gets a value indicating whether the current operating system is Unix-based.
    /// </summary>
    public static bool IsUnix { get; }

    /// <summary>
    /// Gets a value indicating whether the current process is 64-bit.
    /// </summary>
    public static bool Is64BitProcess { get; }

    /// <summary>
    /// Gets a string describing the operating system.
    /// </summary>
    public static string OSDescription { get; }

    private static bool InitializeIsWindows()
    {
#if NETFRAMEWORK
        var platform = Environment.OSVersion.Platform;
        return platform == PlatformID.Win32NT ||
               platform == PlatformID.Win32S ||
               platform == PlatformID.Win32Windows ||
               platform == PlatformID.WinCE;
#else
#if NET5_0_OR_GREATER
        return OperatingSystem.IsWindows();
#else
        return RuntimeInformation.IsOSPlatform(OSPlatform.Windows);
#endif
#endif
    }

    private static bool InitializeIsLinux()
    {
#if NETFRAMEWORK
        var platform = Environment.OSVersion.Platform;
        if (platform == PlatformID.Unix || (int)platform == 128)
        {
            var versionString = Environment.OSVersion.VersionString;
            return versionString.IndexOf("Linux", StringComparison.OrdinalIgnoreCase) >= 0;
        }
        return false;
#elif NET5_0_OR_GREATER
        return OperatingSystem.IsLinux();
#else
        return RuntimeInformation.IsOSPlatform(OSPlatform.Linux);
#endif
    }

    private static bool InitializeIsMacOS()
    {
#if NETFRAMEWORK
        var platform = Environment.OSVersion.Platform;
        if (platform == PlatformID.Unix || (int)platform == 128)
        {
            var versionString = Environment.OSVersion.VersionString;
            return versionString.IndexOf("Darwin", StringComparison.OrdinalIgnoreCase) >= 0;
        }
        return false;
#elif NET5_0_OR_GREATER
        return OperatingSystem.IsMacOS();
#else
        return RuntimeInformation.IsOSPlatform(OSPlatform.OSX);
#endif
    }

    private static string GetOSDescription()
    {
#if NETFRAMEWORK
        return Environment.OSVersion.VersionString;
#else
        return RuntimeInformation.OSDescription;
#endif
    }
}
