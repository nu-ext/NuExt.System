using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace System.Diagnostics;

/// <summary>
/// Monitors performance metrics for a process and provides formatted statistics.
/// </summary>
public sealed class PerformanceMonitor: PropertyChangeNotifier
{
    private readonly ProcessMonitor _processMonitor;
    private readonly IFormatProvider? _formatProvider;
    private long _peakGcMemory;

    public PerformanceMonitor(Process process, IFormatProvider? provider, SynchronizationContext? synchronizationContext) : base(synchronizationContext)
    {
        Process = process ?? throw new ArgumentNullException(nameof(process));
        _formatProvider = provider;
        _processMonitor = new ProcessMonitor(Process);
    }

    public PerformanceMonitor(Process process, IFormatProvider? provider) : this(process, provider, null)
    {

    }

    public PerformanceMonitor(Process process) : this(process, null)
    {

    }

    /// <summary>
    /// Gets the formatted usage string. Updated at each monitoring interval.
    /// </summary>
    public string? FormattedUsage
    {
        get;
        private set => SetProperty(ref field, value);
    }

    /// <summary>
    /// Gets or sets the CPU usage format string.
    /// </summary>
    public string FormatCpuUsage { get; set; } = "CPU: {0:N1} %";

    /// <summary>
    /// Gets or sets the memory usage format string.
    /// </summary>
    public string FormatMemoryUsage { get; set; } = "MEM: {0}";

    /// <summary>
    /// Gets or sets the peak memory usage format string.
    /// </summary>
    public string FormatPeakMemoryUsage { get; set; } = "PMEM: {0}";

    /// <summary>
    /// Gets or sets the managed memory usage format string.
    /// </summary>
    public string FormatManagedMemory { get; set; } = "GC: {0}";

    /// <summary>
    /// Gets or sets the peak managed memory usage format string.
    /// </summary>
    public string FormatPeakManagedMemory { get; set; } = "PGC: {0}";

    /// <summary>
    /// Gets or sets the thread info format string.
    /// </summary>
    public string FormatThreads { get; set; } = "TRD: {0}";

    /// <summary>
    /// Gets or sets the worker thread info format string.
    /// </summary>
    public string FormatWorkerThreads { get; set; } = "WRK: {0}/{1}";

    /// <summary>
    /// Gets the process being monitored.
    /// </summary>
    public Process Process { get; }

    private bool IsCurrentProcess => Process.Id ==
#if NET5_0_OR_GREATER
                                     Environment.ProcessId;
#else
                                     s_currentProcessId;

    private static readonly int s_currentProcessId = Process.GetCurrentProcess().Id;
#endif

    /// <summary>
    /// Gets or sets a value indicating whether to show CPU usage.
    /// </summary>
    public bool ShowCpuUsage { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to show Memory usage.
    /// </summary>
    public bool ShowMemoryUsage { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to show peak memory usage.
    /// </summary>
    public bool ShowPeakMemoryUsage { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to show managed memory usage.
    /// </summary>
    public bool ShowManagedMemory { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to show peak managed memory usage.
    /// </summary>
    public bool ShowPeakManagedMemory { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to show thread info.
    /// </summary>
    public bool ShowThreads { get; set; }

    /// <summary>
    /// Gets or sets the monitoring update interval.
    /// Default is 1 second. Value must be positive.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">Value is less than or equal to zero.</exception>
    public TimeSpan UpdateInterval
    {
        get;
        set
        {
            if (value <= TimeSpan.Zero)
                throw new ArgumentOutOfRangeException(nameof(value), "Interval must be positive.");
            field = value;
        }
    } = TimeSpan.FromSeconds(1);

    /// <summary>
    /// Monitors the process performance and updates <see cref="FormattedUsage"/> asynchronously.
    /// </summary>
    /// <param name="cancellationToken">Cancellation token to stop monitoring.</param>
    /// <returns>Task representing the monitoring operation.</returns>
    public async Task RunAsync(CancellationToken cancellationToken)
    {
        try
        {
            var sb = new StringBuilder(256);
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(UpdateInterval, cancellationToken).ConfigureAwait(false);

                _processMonitor.Refresh();
                if (Process.HasExited)
                {
                    FormattedUsage = "Process terminated";
                    break;
                }

                sb.Clear();
                string delimiter = string.Empty;
                if (ShowCpuUsage)
                {
                    AppendFormat(sb, ref delimiter, FormatCpuUsage, _processMonitor.GetCpuUsage());
                }
                if (ShowMemoryUsage)
                {
                    AppendFormat(sb, ref delimiter, FormatMemoryUsage, FormatUtils.FormatSize(_formatProvider, _processMonitor.GetCurrentMemoryUsage()));
                }
                if (ShowPeakMemoryUsage)
                {
                    AppendFormat(sb, ref delimiter, FormatPeakMemoryUsage, FormatUtils.FormatSize(_formatProvider, _processMonitor.GetPeakMemoryUsage()));
                }
                if (IsCurrentProcess && (ShowManagedMemory || ShowPeakManagedMemory))
                {
                    var gcTotalMemory = GC.GetTotalMemory(false);
                    if (gcTotalMemory > _peakGcMemory)
                    {
                        _peakGcMemory = gcTotalMemory;
                    }
                    if (ShowManagedMemory)
                    {
                        AppendFormat(sb, ref delimiter, FormatManagedMemory, FormatUtils.FormatSize(_formatProvider, gcTotalMemory));
                    }
                    if (ShowPeakManagedMemory)
                    {
                        AppendFormat(sb, ref delimiter, FormatPeakManagedMemory, FormatUtils.FormatSize(_formatProvider, _peakGcMemory));
                    }
                }
                if (ShowThreads)
                {
                    var threadCount = _processMonitor.GetThreadCount();
                    if (IsCurrentProcess)
                    {
                        var workerCount = GetBusyWorkerThreads();
                        AppendFormat(sb, ref delimiter, FormatWorkerThreads, workerCount, threadCount);
                    }
                    else
                    {
                        AppendFormat(sb, ref delimiter, FormatThreads, threadCount);
                    }
                }
                FormattedUsage = sb.ToString();
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Performance monitoring error: {ex.Message}");
            Debug.Assert(false, ex.Message);
        }
    }

    /// <summary>
    /// Gets the number of busy worker threads in the ThreadPool.
    /// </summary>
    /// <returns>The number of busy worker threads.</returns>
    public int GetBusyWorkerThreads()
    {
        _ = this;
        try
        {

            ThreadPool.GetAvailableThreads(out int availableWorkerThreads, out _);
            ThreadPool.GetMaxThreads(out int maxWorkerThreads, out _);
            int busyWorkerThreads = maxWorkerThreads - availableWorkerThreads;
            return busyWorkerThreads;
        }
        catch (Exception ex)
        {
            Trace.WriteLine($"Error retrieving busy worker threads: {ex.Message}");
            return 0;
        }
    }

    private void AppendFormat(StringBuilder sb, ref string delimiter, string format, object arg0)
    {
        sb.Append(delimiter);
        sb.AppendFormat(_formatProvider, format, arg0);
        delimiter = ", ";
    }

    private void AppendFormat(StringBuilder sb, ref string delimiter, string format, object arg0, object arg1)
    {
        sb.Append(delimiter);
        sb.AppendFormat(_formatProvider, format, arg0, arg1);
        delimiter = ", ";
    }
}