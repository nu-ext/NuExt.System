# NuExt.System

`NuExt.System` is a **lightweight, production-ready foundation** for everyday .NET development. It brings together high-quality utilities for **asynchrony**, **lifetime management**, **threading primitives**, **high‑performance spans/memory helpers**, **diagnostics**, and **collection helpers** — all with a strong focus on **performance**, **correctness**, and **developer ergonomics**. Use it to reduce boilerplate, standardize common patterns across projects, and keep code fast and clean across modern .NET and .NET Framework.

[![NuGet](https://img.shields.io/nuget/v/NuExt.System.svg)](https://www.nuget.org/packages/NuExt.System)
[![Ask DeepWiki](https://img.shields.io/badge/Ask-DeepWiki-blue)](https://deepwiki.com/nu-ext/NuExt.System)
[![Build](https://github.com/nu-ext/NuExt.System/actions/workflows/ci.yml/badge.svg)](https://github.com/nu-ext/NuExt.System/actions/workflows/ci.yml)
[![License](https://img.shields.io/github/license/nu-ext/NuExt.System?label=license)](https://github.com/nu-ext/NuExt.System/blob/main/LICENSE)
[![Downloads](https://img.shields.io/nuget/dt/NuExt.System.svg)](https://www.nuget.org/packages/NuExt.System)

## Features

- **Asynchrony & lifetime**
  - Async locks and wait handles: `AsyncLock`, `ReentrantAsyncLock`, `AsyncWaitHandle`
  - Disposable tooling: `Disposable`, `AggregateDisposable`, `AsyncDisposable`, `AggregateAsyncDisposable`, `AsyncLifetime`
- **Threading & synchronization**
  - Reentrant async lock (context‑aware), synchronization‑context helpers, thread‑affine flows
- **High‑performance spans & memory**
  - **Backports/polyfills for `Span` / `MemoryExtensions` APIs** (see below)
  - `ValueStringBuilder` / `ValueListBuilder<T>` for zero‑allocation building
- **Collections & equality**
  - `ObservableDictionary<TKey, TValue>`, `ReferenceEqualityComparer` *(polyfill)*, `ArrayEqualityComparer<T>`, and **many useful extension helpers**
- **I/O & paths (cross‑platform)**
  - `PathBuilder` (class, `IDisposable`) and `ValuePathBuilder` (ref struct) — platform‑independent path builders
  - `PathUtilities` — static helpers for common path operations
- **Diagnostics & utilities**
  - `ProcessMonitor`, `PerformanceMonitor`, `EnumHelper<T>`, `TypeExtensions`, etc.

### Span / MemoryExtensions Polyfills

This package includes **polyfills (API backports)** for selected `Span` / `MemoryExtensions`‑style APIs from newer .NET versions. On modern runtimes, it transparently uses the inbox implementations; on older runtimes, it provides compatible behavior with the same semantics.

**What you get (highlights)**
- Search & comparison: `Contains`, `SequenceEqual` (+ `IEqualityComparer<T>`)
- Indexing: `IndexOf`, `LastIndexOf` (element / sequence)
- Set‑based search: `IndexOfAny`, `LastIndexOfAny`, `IndexOfAnyExcept`, `LastIndexOfAnyExcept`
- Range‑based: `IndexOfAnyInRange`, `IndexOfAnyExceptInRange`, `LastIndexOfAnyInRange`, `LastIndexOfAnyExceptInRange`
- Utilities: `StartsWith`, `EndsWith`, `Replace` (in‑place / copy), `Count`, `CountAny`

**Notes**
- Allocation‑free, performance‑oriented; value types use bitwise fast paths where applicable.
- Semantics mirror the .NET runtime APIs; custom comparers are honored when provided.

## Why NuExt.System?

- **Practical** — battle‑tested building blocks you use every day
- **Fast** — zero‑alloc paths, tight loops, careful branching profiles
- **Consistent** — same behavior across modern .NET and .NET Framework
- **Focused** — no heavy external dependencies or configuration

### Compatibility

- **.NET Standard 2.0+, .NET 8/9/10 and .NET Framework 4.6.2+**
- **Works across desktop, web, and services** (Console, ASP.NET Core, Avalonia, WinUI, WPF, WinForms)

## Commonly Used Types

- **Asynchrony & lifetime**
  - `System.Threading.AsyncLock`, `System.Threading.ReentrantAsyncLock`, `System.Threading.AsyncWaitHandle`
  - `System.ComponentModel.Disposable`, `AggregateDisposable`, `AsyncDisposable`, `AggregateAsyncDisposable`, `AsyncLifetime`
- **Collections & equality**
  - `System.Collections.ObjectModel.ObservableDictionary<TKey, TValue>`
  - `System.Collections.Generic.ValueListBuilder<T>`
  - `System.Collections.Generic.ReferenceEqualityComparer` *(polyfill)*
  - `System.Collections.Generic.ArrayEqualityComparer<T>`
  - **Various extension helpers** (collections, delegates, enums, strings, exceptions)
- **Strings, spans, and memory**
  - `System.Text.ValueStringBuilder`
  - `System.CompatMemoryExtensions` *(polyfills/backports)*
- **I/O & paths (cross‑platform)**
  - `System.IO.PathBuilder` *(class, `IDisposable` — mutable path builder)*
  - `System.IO.ValuePathBuilder` *(ref struct — high‑performance mutable path builder)*
  - `System.IO.PathUtilities` *(static common path operations)*
- **Diagnostics & helpers**
  - `System.Diagnostics.ProcessMonitor`, `PerformanceMonitor`
  - `System.EnumHelper<T>`, `System.FormatUtils`, `System.HexConverter`

## Quick Examples

### 1) Reentrant async lock (with ExecutionContext best practices)
```csharp
var asyncLock = new System.Threading.ReentrantAsyncLock();
var cts = new CancellationTokenSource();

// Synchronous section
asyncLock.Acquire(() =>
{
    // do work safely, reentry allowed on the same flow
});

// Asynchronous section
await asyncLock.AcquireAsync(async () =>
{
    await DoAsyncWork();
});

// Avoid unintentionally flowing AsyncLocal into CT callbacks:
using (ExecutionContext.SuppressFlow())
{
    cts.Token.Register(() => asyncLock.Acquire(() =>
    {
        // safe callback body
    }));
}
```

### 2) Async lifetime + aggregated disposal

```csharp
var lifetime = new System.AsyncLifetime() { ContinueOnCapturedContext = true };
lifetime.AddDisposable(new FileStream(path, FileMode.Open));
lifetime.AddAsync(async () => await FlushBuffersAsync());

await lifetime.DisposeAsync(); // disposes in the right order, async‑aware
```

### 3) Zero‑alloc string building with `ValueStringBuilder`

```csharp
Span<char> initial = stackalloc char[128];
var sb = new System.Text.ValueStringBuilder(initial);

sb.Append("User: ");
sb.Append(userName);
sb.Append(", Items: ");
sb.Append(itemCount);

string result = sb.ToString(); // minimal allocations
```

### 4) `ValueListBuilder<T>` and `ValueTask.WhenAll`

```csharp
public class Example
{
    public static async Task Main()
    {
        int failed = 0;
        String[] urls = [ "www.adatum.com", "www.cohovineyard.com",
                        "www.cohowinery.com", "www.northwindtraders.com",
                        "www.contoso.com" ];
        var tasks = new ValueListBuilder<ValueTask>(urls.Length);

        foreach (var value in urls)
        {
            var url = value;
            tasks.Append(new ValueTask(Task.Run(() =>
            {
                var png = new Ping();
                try
                {
                    var reply = png.Send(url);
                    if (reply.Status != IPStatus.Success)
                    {
                        Interlocked.Increment(ref failed);
                        throw new TimeoutException("Unable to reach " + url + ".");
                    }
                }
                catch (PingException)
                {
                    Interlocked.Increment(ref failed);
                    throw;
                }
            })));
        }
        ValueTask t = ValueTask.WhenAll(tasks.ToArray());
        try
        {
            await t;
        }
        catch { }

        if (t.IsCompletedSuccessfully)
            Console.WriteLine("All ping attempts succeeded.");
        else if (t.IsFaulted)
            Console.WriteLine("{0} ping attempts failed", failed);
    }
}
```

### 5) `ValueListBuilder<T>` and `ValueTask.WhenAll<TResult>`

```csharp
public class Example
{
    public static async Task Main()
    {
        int failed = 0;
        String[] urls = [ "www.adatum.com", "www.cohovineyard.com",
                        "www.cohowinery.com", "www.northwindtraders.com",
                        "www.contoso.com" ];
        var tasks = new ValueListBuilder<ValueTask<PingReply>>(urls.Length);

        foreach (var value in urls)
        {
            var url = value;
            tasks.Append(new ValueTask<PingReply>(Task.Run(() =>
            {
                var png = new Ping();
                try
                {
                    var reply = png.Send(url);
                    if (reply.Status != IPStatus.Success)
                    {
                        Interlocked.Increment(ref failed);
                        throw new TimeoutException("Unable to reach " + url + ".");
                    }
                    return reply;
                }
                catch (PingException)
                {
                    Interlocked.Increment(ref failed);
                    throw;
                }
            })));
        }
        try
        {
            PingReply[] replies = await ValueTask.WhenAll(tasks.ToArray());
            Console.WriteLine("{0} ping attempts succeeded:", replies.Length);
            for (int i = 0; i < replies.Length; i++)
            {
                var reply = replies[i];
                Console.WriteLine($"Reply from {reply.Address}: bytes={reply.Buffer.Length} time={reply.RoundtripTime}ms TTL={reply.Options?.Ttl} [{urls[i]}]");
            }
        }
        catch (AggregateException)
        {
            Console.WriteLine("{0} ping attempts failed", failed);
        }
    }
}
```

## Installation

Via [NuGet](https://www.nuget.org/):

```sh
dotnet add package NuExt.System
```

Or via Visual Studio:

1. Go to `Tools -> NuGet Package Manager -> Manage NuGet Packages for Solution...`.
2. Search for `NuExt.System`.
3. Click "Install".

## Ecosystem

- [NuExt.Minimal.Mvvm](https://github.com/nu-ext/NuExt.Minimal.Mvvm)
- [NuExt.Minimal.Mvvm.SourceGenerator](https://github.com/nu-ext/NuExt.Minimal.Mvvm.SourceGenerator)
- [NuExt.Minimal.Behaviors.Wpf](https://github.com/nu-ext/NuExt.Minimal.Behaviors.Wpf)
- [NuExt.Minimal.Mvvm.Wpf](https://github.com/nu-ext/NuExt.Minimal.Mvvm.Wpf)
- [NuExt.Minimal.Mvvm.MahApps.Metro](https://github.com/nu-ext/NuExt.Minimal.Mvvm.MahApps.Metro)
- [NuExt.System.Data](https://github.com/nu-ext/NuExt.System.Data)
- [NuExt.System.Data.SQLite](https://github.com/nu-ext/NuExt.System.Data.SQLite)

## Notes & acknowledgements

Some implementations are derived from the .NET Runtime (MIT). See LICENSE and source comments for attributions.

## Contributing

Issues and PRs are welcome. Keep changes minimal and performance-conscious.

## License

MIT. See LICENSE.