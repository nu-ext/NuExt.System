using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Threading.Tasks;

namespace System;

/// <summary>
/// Provides extension methods for invoking and managing delegates.
/// </summary>
public static class DelegateExtensions
{
    extension(Delegate callback)
    {
#if !NET9_0_OR_GREATER
        /// <summary>
        /// Gets a value that indicates whether the <see cref="Delegate"/> has a single invocation target.
        /// </summary>
        /// <value><see langword="true"/> if the <see cref="Delegate"/> has a single invocation target.</value>
        public bool HasSingleTarget => callback?.GetInvocationList()?.Length == 1;
#endif

        /// <summary>
        /// Dynamically invokes the specified delegate with the provided arguments.
        /// Uses direct invocation for common delegate types (Action, Func, EventHandler, etc.)
        /// falling back to <see cref="Delegate.DynamicInvoke(object[])"/> only when needed.
        /// <para>
        /// This method is optimized for dynamic scenarios where only a <see cref="Delegate"/>
        /// reference is available.
        /// </para>
        /// </summary>
        /// <param name="args">An array of arguments to pass to the delegate or null, if the delegate does not require arguments.</param>
        /// <returns>The return value from the delegate, or null if the delegate returns void.</returns>
        /// <exception cref="ArgumentNullException">Thrown when callback is null.</exception>
        public object? Call(params object?[]? args)
        {
            ArgumentNullException.ThrowIfNull(callback);

            args ??= [];

            try
            {
                return args.Length switch
                {
                    0 => callback.InvokeParameterless(),
                    1 => callback.InvokeWithOneArg(args),
                    2 => callback.InvokeWithTwoArgs(args),
                    _ => callback.DynamicInvoke(args)
                };
            }
            catch (TargetInvocationException ex) when (ex.InnerException is not null)
            {
                ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
                // The above line will always throw, but the compiler requires we throw explicitly.
                throw;
            }
        }

        private object? InvokeParameterless()
        {
            switch (callback)
            {
                case Action action: action(); return null;
                case Func<Task> taskFunc: return taskFunc();
                case Func<ValueTask> taskFunc: return taskFunc();
                case Func<string?> stringFunc: return stringFunc();
                case Func<object?> func: return func();
                case Func<int> intFunc: return intFunc();
                case Func<bool> boolFunc: return boolFunc();
                case Func<long> longFunc: return longFunc();
                case Func<double> doubleFunc: return doubleFunc();
                default: return callback.DynamicInvoke([]);
            }
        }

        private object? InvokeWithOneArg(object?[] args)
        {
            var arg0 = args[0];
            switch (callback)
            {
                case SendOrPostCallback sendOrPost: sendOrPost(arg0); return null;
                case Action<object?> actionObj: actionObj(arg0); return null;
                case Action<string?> actionStr when arg0 is string str: actionStr(str); return null;
                case Action<int> actionInt when arg0 is int i: actionInt(i); return null;
                case Action<bool> actionBool when arg0 is bool b: actionBool(b); return null;
                case Func<object?, object?> funcObj: return funcObj(arg0);
                default: return callback.DynamicInvoke(args);
            }
        }

        private object? InvokeWithTwoArgs(object?[] args)
        {
            var arg0 = args[0];
            var arg1 = args[1];
            switch (callback)
            {
                case EventHandler eventHandler when arg1 is EventArgs e: eventHandler(arg0, e); return null;
                case not null when callback.GetType().IsGenericType
                                   && callback.GetType().GetGenericTypeDefinition() == typeof(EventHandler<>)
                                   && arg1 != null
                                   && callback.GetType().GetGenericArguments()[0].IsInstanceOfType(arg1):
                    callback.DynamicInvoke(args); return null;
                case PropertyChangedEventHandler eventHandler when arg1 is PropertyChangedEventArgs e: eventHandler(arg0, e); return null;
                case Action<object?, object?> actionObj: actionObj(arg0, arg1); return null;
                case Action<string, string> actionStr when arg0 is string s1 && arg1 is string s2: actionStr(s1, s2); return null;
                case Func<object?, object?, object?> funcObj: return funcObj(arg0, arg1);
                default: return callback!.DynamicInvoke(args);
            }
        }

        /// <summary>
        /// Safely attempts to dynamically invoke the delegate without throwing exceptions. 
        /// Returns false if the delegate is null or if invocation fails.
        /// <para>
        /// This method is designed for dynamic scenarios where exception safety is needed
        /// and only a <see cref="Delegate"/> reference is available.
        /// </para>
        /// </summary>
        /// <param name="result">The return value from the delegate, or null if the delegate returns void or invocation failed.</param>
        /// <param name="args">An array of arguments to pass to the delegate or null, if the delegate does not require arguments.</param>
        /// <returns><see langword="true"/> if the delegate was invoked successfully; otherwise, <see langword="false"/>.</returns>
        public bool TryCall(out object? result, params object?[]? args)
        {
            try
            {
                if (callback != null)
                {
                    result = callback.Call(args);
                    return true;
                }
            }
            catch
            {
            }
            result = null;
            return false;
        }

    }

    extension(Delegate)
    {
        /// <summary>
        /// Combines two delegates in a type-safe manner.
        /// </summary>
        /// <typeparam name="T">The type of delegate.</typeparam>
        /// <param name="a">The first delegate. May be <see langword="null"/>.</param>
        /// <param name="b">The second delegate. May be <see langword="null"/>.</param>
        /// <returns>
        /// A new delegate that invokes both <paramref name="a"/> and <paramref name="b"/>, 
        /// or <see langword="null"/> if both parameters are <see langword="null"/>.
        /// </returns>
        /// <remarks>
        /// If either delegate is <see langword="null"/>, the other delegate is returned unchanged.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? Combine<T>(T? a, T? b) where T : Delegate
        {
            return (T?)Delegate.Combine(a, b);
        }

        /// <summary>
        /// Removes the last occurrence of a delegate from another delegate.
        /// </summary>
        /// <typeparam name="T">The type of delegate.</typeparam>
        /// <param name="a">The delegate to remove from. May be <see langword="null"/>.</param>
        /// <param name="b">The delegate to remove. May be <see langword="null"/>.</param>
        /// <returns>
        /// A new delegate with the last occurrence of <paramref name="b"/> removed from 
        /// <paramref name="a"/>, or <see langword="null"/> if the result is an empty delegate list.
        /// </returns>
        /// <remarks>
        /// If <paramref name="b"/> is <see langword="null"/> or not found in <paramref name="a"/>, 
        /// <paramref name="a"/> is returned unchanged.
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T? Remove<T>(T? a, T? b) where T : Delegate
        {
            return (T?)Delegate.Remove(a, b);
        }

        /// <summary>
        /// Atomically combines a delegate with a field delegate using compare-and-swap semantics.
        /// </summary>
        /// <typeparam name="T">The type of delegate.</typeparam>
        /// <param name="field">A reference to the field delegate to update.</param>
        /// <param name="value">The delegate to add. May be <see langword="null"/>.</param>
        /// <returns>
        /// The result of combining the field's original value with <paramref name="value"/>.
        /// If the operation fails due to contention, the returned value may differ from 
        /// the field's current value.
        /// </returns>
        /// <remarks>
        /// <para>This method implements a lock-free algorithm to update the delegate field.
        /// It may spin multiple times under contention but guarantees forward progress.</para>
        /// <para>Similar to <see cref="Interlocked.Increment(ref int)"/>, this method returns 
        /// the result of the operation, not the original value.</para>
        /// </remarks>
        public static T? Combine<T>(ref T? field, T? value) where T : Delegate
        {
            if (value == null)
                return field;

            T? original = field;
            while (true)
            {
                T? current = original;
                T? proposed = Delegate.Combine<T>(current, value);
                T? previous = Interlocked.CompareExchange(ref field, proposed, current);
                if (ReferenceEquals(previous, original))
                {
                    return proposed;
                }
                original = previous;
            }
        }

        /// <summary>
        /// Atomically removes a delegate from a field delegate using compare-and-swap semantics.
        /// </summary>
        /// <typeparam name="T">The type of delegate.</typeparam>
        /// <param name="field">A reference to the field delegate to update.</param>
        /// <param name="value">The delegate to remove. May be <see langword="null"/>.</param>
        /// <returns>
        /// The result of removing <paramref name="value"/> from the field's original value.
        /// If the operation fails due to contention, the returned value may differ from 
        /// the field's current value.
        /// </returns>
        /// <remarks>
        /// <para>This method implements a lock-free algorithm to update the delegate field.
        /// If <paramref name="value"/> is <see langword="null"/>, no operation is performed.</para>
        /// <para>Similar to <see cref="Interlocked.Decrement(ref int)"/>, this method returns 
        /// the result of the operation, not the original value.</para>
        /// </remarks>
        public static T? Remove<T>(ref T? field, T? value) where T : Delegate
        {
            if (value == null)
                return field;

            T? original = field;
            while (true)
            {
                T? current = original;
                T? proposed = Delegate.Remove<T>(current, value);
                T? previous = Interlocked.CompareExchange(ref field, proposed, current);
                if (ReferenceEquals(previous, original))
                {
                    return proposed;
                }
                original = previous;
            }
        }
    }
}
