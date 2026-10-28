# NuExt.System

`NuExt.System` is a **cross-platform application foundation for high-performance .NET desktop and hybrid frameworks**, providing cohesive primitives for **asynchrony**, **lifetime management**, **thread affinity**, **synchronization**, **observable state**, **cross-platform path processing**, and **high-performance memory operations** across modern .NET, .NET Standard, and .NET Framework without heavy dependencies.

[![NuGet](https://img.shields.io/nuget/v/NuExt.System.svg)](https://www.nuget.org/packages/NuExt.System)
[![Ask DeepWiki](https://img.shields.io/badge/Ask-DeepWiki-blue)](https://deepwiki.com/nu-ext/NuExt.System)
[![Build](https://github.com/nu-ext/NuExt.System/actions/workflows/ci.yml/badge.svg)](https://github.com/nu-ext/NuExt.System/actions/workflows/ci.yml)
[![License](https://img.shields.io/github/license/nu-ext/NuExt.System?label=license)](https://github.com/nu-ext/NuExt.System/blob/main/LICENSE)
[![Downloads](https://img.shields.io/nuget/dt/NuExt.System.svg)](https://www.nuget.org/packages/NuExt.System)

## Why NuExt.System?

- **Application-oriented** — infrastructure for desktop and hybrid application frameworks, not just isolated helpers
- **Framework-agnostic** — works across WPF, WinForms, Avalonia, hybrid UI, services, and libraries
- **Cross-platform** — platform-independent primitives and explicit Windows and Unix path semantics
- **Thread-aware** — thread affinity, synchronization contexts, and sync/async coordination
- **Lifetime-driven** — deterministic cleanup for disposables, asynchronous resources, subscriptions, and services
- **High-performance** — span-based APIs, pooled buffers, and allocation-conscious builders
- **Consistent across runtimes** — a stable API surface across modern .NET, .NET Standard, and .NET Framework
- **Dependency-light** — no required hosting model, container, UI framework, or heavy external infrastructure; modern .NET targets have no package dependencies

## Designed for

NuExt.System is intended for application and framework authors who need:

- deterministic synchronous and asynchronous lifetime management;
- safe interaction with thread-affine objects and synchronization contexts;
- asynchronous events with ordered invocation, cancellation, and exception aggregation;
- observable collections and property notifications for application state;
- synchronous and asynchronous coordination primitives;
- Windows and Unix path processing independent of the host platform;
- compatibility across modern .NET and legacy .NET Framework targets.

## Core infrastructure

### Application object foundations

- `Disposable` and `AsyncDisposable` provide consistent, thread-safe base implementations for deterministic synchronous and asynchronous cleanup.
- `PropertyChangeNotifier` provides synchronization-context-aware property notifications and reusable support for observable, thread-affine application state.

### Lifetime management

- `Lifetime` and `AsyncLifetime` coordinate cleanup actions, subscriptions, disposables, and asynchronous resources as a single lifetime scope.
- Cleanup actions execute in reverse registration order, with asynchronous cleanup and exception aggregation supported where applicable.

### Threading and synchronization

- `AsyncLock` and `ReentrantAsyncLock` provide synchronous and asynchronous coordination, cancellation, timeout handling, and explicit reentrancy semantics.
- `AsyncWaitHandle` integrates wait handles with task-based asynchronous code.
- `AsyncLazy<T>` provides thread-safe, one-time asynchronous initialization with a cached, directly awaitable task.
- `LockPool` associates a stable lock with an object identity without relying on equality or hash codes, while allowing the lock lifetime to follow the key through weak references.
- `SynchronizationContextHelper`, `SynchronizationContextExtensions`, and `ThreadAffineSynchronizationContext` provide thread-affinity checks, context marshaling, synchronous and asynchronous invocation, result propagation, and exception handling.
- `IDispatcherObject`, `ISynchronizeInvoker`, and related contracts make thread-affine application infrastructure reusable across UI frameworks and hosting models.

### Deterministic asynchronous events

NuExt.System treats an asynchronous event as an **ordered asynchronous operation**, not as an implicit parallel broadcast, making execution order, cancellation, and exception handling predictable.

```csharp
public delegate ValueTask AsyncEventHandler(
    object? sender,
    EventArgs e,
    CancellationToken cancellationToken);

public delegate ValueTask AsyncEventHandler<TEventArgs>(
    object? sender,
    TEventArgs e,
    CancellationToken cancellationToken);
```

The design follows several principles:

- **Cancellation is part of the operation contract** — `CancellationToken` is a dedicated argument rather than application data embedded into a custom `EventArgs` type. Handlers can inspect it directly and propagate it to downstream asynchronous APIs.
- **Synchronous completion remains inexpensive** — `ValueTask` avoids requiring every handler to allocate a `Task` when the work completes synchronously.
- **Invocation order is deterministic** — handlers are awaited sequentially in subscription order. No hidden parallelism is introduced through `Task.WhenAll`, so handlers can safely depend on ordering and observe preceding changes.
- **Exception behavior is explicit** — invocation can stop on the first exception or continue through the remaining handlers and throw an `AggregateException` afterward.
- **Context flow is controlled by the caller** — invocation overloads make continuation on the captured synchronization context an explicit choice.

Sequential invocation is the default because event handlers commonly interact with shared application state, thread-affine objects, and ordered lifecycle transitions. Applications that require parallel fan-out can still implement it explicitly at the point where independent execution is known to be safe.

### Observable collections

- `ObservableDictionary<TKey, TValue>` is a full observable dictionary rather than a thin wrapper around `Dictionary<TKey, TValue>`. It implements dictionary and list-oriented contracts, preserves stable insertion order, provides index-aware collection notifications for individual changes, uses reset notifications for batch operations, and keeps `Count` and indexer property notifications consistent with each mutation.
- The dictionary can participate directly in data-binding pipelines built around `INotifyCollectionChanged` and `INotifyPropertyChanged`, including WPF, Avalonia, WinUI, .NET MAUI, and other UI or application frameworks that consume these standard interfaces.
- `OrderedObservableCollection<T>` maintains application-defined item ordering across inserts and moves while allowing explicit normalization when complete renumbering is required.
- `ArrayEqualityComparer<T>` provides structural array equality using the default or a custom element comparer, with optimized comparison paths where supported; `ReferenceEqualityComparer` backports BCL reference-identity comparison to older target frameworks.

### Performance-oriented primitives

- `ValueListBuilder<T>` uses caller-provided span storage first and grows through `ArrayPool<T>` only when necessary, making it suitable for short-lived internal lists without unconditional heap allocation.
- `ValueStringBuilder` follows the same model for text construction: stack-backed or caller-provided storage for the common case, pooled growth for larger values, and a single resulting string allocation.
- `ValuePathBuilder` applies the same allocation-conscious design to host-independent path construction and normalization.
- Span-based compatibility APIs preserve newer BCL-style operations across older targets while using optimized implementation paths where the runtime permits them.

### Host-independent path semantics

`PathBuilder`, `ValuePathBuilder`, and `PathUtilities` can process Windows and Unix paths independently of the host operating system. Path rules are selected explicitly through `DirectorySeparatorChar` or `isUnixLike`, rather than inherited implicitly from the host.

This is useful for build tools, archives, deployment systems, remote file systems, IDEs, and services that exchange paths between Windows and Unix environments. It avoids treating paths as unstructured strings or relying on separator replacement, which is insufficient for drive-relative, UNC, device, rooted, and relative path forms.

### Span / MemoryExtensions Polyfills

This package includes **polyfills (API backports)** for selected `Span` / `MemoryExtensions`-style APIs from newer .NET versions. On modern runtimes, it transparently uses the inbox implementations; on older runtimes, it provides compatible behavior with the same semantics.

**What you get (highlights)**

- Search & comparison: `Contains`, `SequenceEqual` (+ `IEqualityComparer<T>`)
- Indexing: `IndexOf`, `LastIndexOf` (element / sequence)
- Set-based search: `IndexOfAny`, `LastIndexOfAny`, `IndexOfAnyExcept`, `LastIndexOfAnyExcept`
- Range-based: `IndexOfAnyInRange`, `IndexOfAnyExceptInRange`, `LastIndexOfAnyInRange`, `LastIndexOfAnyExceptInRange`
- Utilities: `StartsWith`, `EndsWith`, `Replace` (in-place / copy), `Count`, `CountAny`

**Notes**

- Allocation-free, performance-oriented; value types use bitwise fast paths where applicable.
- Semantics mirror the .NET runtime APIs; custom comparers are honored when provided.

### Additional utilities

NuExt.System also includes focused helpers for delegates and asynchronous multicast invocation, enum metadata and formatting, exception throwing and validation, reflection, diagnostics and process monitoring, hexadecimal conversion, collection operations, string compatibility APIs, runtime attributes, and selected BCL polyfills.

## Compatibility

- **.NET Standard 2.0+, .NET 8/9/10, and .NET Framework 4.6.2+**
- Modern .NET targets have no package dependencies; older targets use only focused BCL compatibility packages
- Designed primarily for **desktop and hybrid application infrastructure**
- Usable with **WPF, WinForms, Avalonia, WinUI, Blazor Hybrid, console applications, services, and shared libraries**
- Platform-independent APIs support Windows and Unix path semantics on any host OS

## Quick Examples

### 1) Reentrant async lock

```csharp
var asyncLock = new ReentrantAsyncLock();

asyncLock.Acquire(() =>
{
    // Synchronous work with reentry allowed on the same flow.
});

await asyncLock.AcquireAsync(async () =>
{
    await DoAsyncWork();
});
```

### 2) Async lifetime and aggregated disposal

```csharp
var lifetime = new AsyncLifetime { ContinueOnCapturedContext = true };
lifetime.AddDisposable(new FileStream(path, FileMode.Open));
lifetime.AddAsync(FlushBuffersAsync);

await lifetime.DisposeAsync(); // Executes cleanup actions in reverse order
```

### 3) Allocation-conscious string building with `ValueStringBuilder`

```csharp
Span<char> initial = stackalloc char[128];
var sb = new ValueStringBuilder(initial);

sb.Append("User: ");
sb.Append(userName);
sb.Append(", Items: ");
sb.Append(itemCount);

string result = sb.ToString(); // Allocates the resulting string
```

### 4) Host-independent path processing

```csharp
using var windowsPath = new PathBuilder(@"C:\repo\src\..\artifacts")
{
    DirectorySeparatorChar = '\\'
};

string normalizedWindowsPath = windowsPath.GetFullPath().ToString();
// C:\repo\artifacts, regardless of the host operating system

using var unixPath = new PathBuilder("/srv/app/../data")
{
    DirectorySeparatorChar = '/'
};

string normalizedUnixPath = unixPath.GetFullPath().ToString();
// /srv/data, regardless of the host operating system
```

### 5) Ordered asynchronous events

```csharp
AsyncEventHandler? changed = null;

changed += FirstHandlerAsync;
changed += SecondHandlerAsync;

await changed.InvokeAsync(this, EventArgs.Empty, cancellationToken);
```

## Installation

Via [NuGet](https://www.nuget.org/):

```shell
dotnet add package NuExt.System
```

Or via Visual Studio:

1. Go to `Tools -> NuGet Package Manager -> Manage NuGet Packages for Solution...`.
2. Search for `NuExt.System`.
3. Click "Install".

## Ecosystem

- [NuExt.System.Data](https://github.com/nu-ext/NuExt.System.Data)
- [NuExt.System.Data.SQLite](https://github.com/nu-ext/NuExt.System.Data.SQLite)
- [NuExt.Minimal.Mvvm](https://github.com/nu-ext/NuExt.Minimal.Mvvm)
- [NuExt.Minimal.Mvvm.SourceGenerator](https://github.com/nu-ext/NuExt.Minimal.Mvvm.SourceGenerator)
- [NuExt.Minimal.Behaviors.Wpf](https://github.com/nu-ext/NuExt.Minimal.Behaviors.Wpf)
- [NuExt.Presentation.Wpf](https://github.com/nu-ext/NuExt.Presentation.Wpf)
- [NuExt.Minimal.Mvvm.Wpf](https://github.com/nu-ext/NuExt.Minimal.Mvvm.Wpf)
- [NuExt.Minimal.Mvvm.MahApps.Metro](https://github.com/nu-ext/NuExt.Minimal.Mvvm.MahApps.Metro)

## Notes & acknowledgements

Some implementations are derived from the .NET Runtime (MIT). See LICENSE and source comments for attributions.

## Contributing

Issues and PRs are welcome. Keep changes minimal and performance-conscious.

## License

MIT. See LICENSE.