using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace System;

/// <summary>
/// Provides compatibility implementations of exception helper methods.
/// </summary>
public static class ExceptionExtensions
{
    extension(ArgumentException)
    {
#if !NET7_0_OR_GREATER
#pragma warning disable CS8777 //Parameter must have a non-null value when exiting.
        /// <summary>Throws an exception if <paramref name="argument"/> is null or empty.</summary>
        /// <param name="argument">The string argument to validate as non-null and non-empty.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        /// <exception cref="ArgumentNullException"><paramref name="argument"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="argument"/> is empty.</exception>
        public static void ThrowIfNullOrEmpty([NotNull] string? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            if (string.IsNullOrEmpty(argument))
            {
                ThrowNullOrEmptyException(argument, paramName);
            }
        }
#pragma warning restore CS8777 //Parameter must have a non-null value when exiting.
#endif

#if !NET8_0_OR_GREATER
#pragma warning disable CS8777 //Parameter must have a non-null value when exiting.
        /// <summary>Throws an exception if <paramref name="argument"/> is null, empty, or consists only of white-space characters.</summary>
        /// <param name="argument">The string argument to validate.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        /// <exception cref="ArgumentNullException"><paramref name="argument"/> is null.</exception>
        /// <exception cref="ArgumentException"><paramref name="argument"/> is empty or consists only of white-space characters.</exception>
        public static void ThrowIfNullOrWhiteSpace([NotNull] string? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            if (string.IsNullOrWhiteSpace(argument))
            {
                ThrowNullOrWhiteSpaceException(argument, paramName);
            }
        }
#pragma warning restore CS8777 //Parameter must have a non-null value when exiting.
#endif

        [DoesNotReturn]
        private static void ThrowNullOrEmptyException(string? argument, string? paramName)
        {
            ArgumentNullException.ThrowIfNull(argument, paramName);
            throw new ArgumentException(SR.Argument_EmptyString, paramName);
        }

        [DoesNotReturn]
        private static void ThrowNullOrWhiteSpaceException(string? argument, string? paramName)
        {
            ArgumentNullException.ThrowIfNull(argument, paramName);
            throw new ArgumentException(SR.Argument_EmptyOrWhiteSpaceString, paramName);
        }
    }

    extension(ArgumentNullException)
    {
#if !NET6_0_OR_GREATER
        /// <summary>Throws an <see cref="ArgumentNullException"/> if <paramref name="argument"/> is null.</summary>
        /// <param name="argument">The reference type argument to validate as non-null.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        public static void ThrowIfNull([NotNull] object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            if (argument is null)
            {
                Throw(paramName);
            }
        }
#endif

#if !NET7_0_OR_GREATER
        /// <summary>Throws an <see cref="ArgumentNullException"/> if <paramref name="argument"/> is null.</summary>
        /// <param name="argument">The pointer argument to validate as non-null.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        public static unsafe void ThrowIfNull([NotNull] void* argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            if (argument is null)
            {
                Throw(paramName);
            }
        }
#endif
        /// <summary>Throws an <see cref="ArgumentNullException"/> if <paramref name="argument"/> is null.</summary>
        /// <param name="argument">The pointer argument to validate as non-null.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="argument"/> corresponds.</param>
        public static void ThrowIfNull(IntPtr argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null)
        {
            if (argument == IntPtr.Zero)
            {
                Throw(paramName);
            }
        }

        [DoesNotReturn]
        internal static void Throw(string? paramName) =>
            throw new ArgumentNullException(paramName);
    }

#if !NET8_0_OR_GREATER
    extension(ArgumentOutOfRangeException)
    {
        [DoesNotReturn]
        private static void ThrowZero<T>(T value, string? paramName) =>
            throw new ArgumentOutOfRangeException(paramName, value, SR.Format(SR.ArgumentOutOfRange_Generic_MustBeNonZero, paramName, value));

        [DoesNotReturn]
        private static void ThrowNegative<T>(T value, string? paramName) =>
            throw new ArgumentOutOfRangeException(paramName, value, SR.Format(SR.ArgumentOutOfRange_Generic_MustBeNonNegative, paramName, value));

        [DoesNotReturn]
        private static void ThrowNegativeOrZero<T>(T value, string? paramName) =>
            throw new ArgumentOutOfRangeException(paramName, value, SR.Format(SR.ArgumentOutOfRange_Generic_MustBeNonNegativeNonZero, paramName, value));

        [DoesNotReturn]
        private static void ThrowGreater<T>(T value, T other, string? paramName) =>
            throw new ArgumentOutOfRangeException(paramName, value, SR.Format(SR.ArgumentOutOfRange_Generic_MustBeLessOrEqual, paramName, value, other));

        [DoesNotReturn]
        private static void ThrowGreaterEqual<T>(T value, T other, string? paramName) =>
            throw new ArgumentOutOfRangeException(paramName, value, SR.Format(SR.ArgumentOutOfRange_Generic_MustBeLess, paramName, value, other));

        [DoesNotReturn]
        private static void ThrowLess<T>(T value, T other, string? paramName) =>
            throw new ArgumentOutOfRangeException(paramName, value, SR.Format(SR.ArgumentOutOfRange_Generic_MustBeGreaterOrEqual, paramName, value, other));

        [DoesNotReturn]
        private static void ThrowLessEqual<T>(T value, T other, string? paramName) =>
            throw new ArgumentOutOfRangeException(paramName, value, SR.Format(SR.ArgumentOutOfRange_Generic_MustBeGreater, paramName, value, other));

        [DoesNotReturn]
        private static void ThrowEqual<T>(T value, T other, string? paramName) =>
            throw new ArgumentOutOfRangeException(paramName, value, SR.Format(SR.ArgumentOutOfRange_Generic_MustBeNotEqual, paramName, (object?)value ?? "null", (object?)other ?? "null"));

        [DoesNotReturn]
        private static void ThrowNotEqual<T>(T value, T other, string? paramName) =>
            throw new ArgumentOutOfRangeException(paramName, value, SR.Format(SR.ArgumentOutOfRange_Generic_MustBeEqual, paramName, (object?)value ?? "null", (object?)other ?? "null"));

        /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is zero.</summary>
        /// <param name="value">The argument to validate as non-zero.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
        public static void ThrowIfZero<T>(T value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
            where T : struct, IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T>
        {
            if (value.Equals(default(T)))
                ThrowZero(value, paramName);
        }

        /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is less than zero.</summary>
        /// <param name="value">The argument to validate as non-negative.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
        public static void ThrowIfNegative<T>(T value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
            where T : struct, IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T>
        {
            if (value.CompareTo(default(T)) < 0)
                ThrowNegative(value, paramName);
        }

        /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is less than or equal to zero.</summary>
        /// <param name="value">The argument to validate as positive.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
        public static void ThrowIfNegativeOrZero<T>(T value, [CallerArgumentExpression(nameof(value))] string? paramName = null)
            where T : struct, IComparable, IFormattable, IConvertible, IComparable<T>, IEquatable<T>
        {
            if (value.CompareTo(default(T)) <= 0)
                ThrowNegativeOrZero(value, paramName);
        }

        /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is equal to <paramref name="other"/>.</summary>
        /// <param name="value">The argument to validate as not equal to <paramref name="other"/>.</param>
        /// <param name="other">The value to compare with <paramref name="value"/>.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
        public static void ThrowIfEqual<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            if (EqualityComparer<T>.Default.Equals(value, other))
                ThrowEqual(value, other, paramName);
        }

        /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is not equal to <paramref name="other"/>.</summary>
        /// <param name="value">The argument to validate as equal to <paramref name="other"/>.</param>
        /// <param name="other">The value to compare with <paramref name="value"/>.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
        public static void ThrowIfNotEqual<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(value, other))
                ThrowNotEqual(value, other, paramName);
        }

        /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is greater than <paramref name="other"/>.</summary>
        /// <param name="value">The argument to validate as less or equal than <paramref name="other"/>.</param>
        /// <param name="other">The value to compare with <paramref name="value"/>.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
        public static void ThrowIfGreaterThan<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
            where T : IComparable<T>
        {
            if (value.CompareTo(other) > 0)
                ThrowGreater(value, other, paramName);
        }

        /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is greater than or equal <paramref name="other"/>.</summary>
        /// <param name="value">The argument to validate as less than <paramref name="other"/>.</param>
        /// <param name="other">The value to compare with <paramref name="value"/>.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
        public static void ThrowIfGreaterThanOrEqual<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
            where T : IComparable<T>
        {
            if (value.CompareTo(other) >= 0)
                ThrowGreaterEqual(value, other, paramName);
        }

        /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is less than <paramref name="other"/>.</summary>
        /// <param name="value">The argument to validate as greater than or equal than <paramref name="other"/>.</param>
        /// <param name="other">The value to compare with <paramref name="value"/>.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
        public static void ThrowIfLessThan<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
            where T : IComparable<T>
        {
            if (value.CompareTo(other) < 0)
                ThrowLess(value, other, paramName);
        }

        /// <summary>Throws an <see cref="ArgumentOutOfRangeException"/> if <paramref name="value"/> is less than or equal <paramref name="other"/>.</summary>
        /// <param name="value">The argument to validate as greater than than <paramref name="other"/>.</param>
        /// <param name="other">The value to compare with <paramref name="value"/>.</param>
        /// <param name="paramName">The name of the parameter with which <paramref name="value"/> corresponds.</param>
        public static void ThrowIfLessThanOrEqual<T>(T value, T other, [CallerArgumentExpression(nameof(value))] string? paramName = null)
            where T : IComparable<T>
        {
            if (value.CompareTo(other) <= 0)
                ThrowLessEqual(value, other, paramName);
        }
    }
#endif


#if !NET7_0_OR_GREATER
    extension(ObjectDisposedException)
    {
        /// <summary>Throws an <see cref="ObjectDisposedException"/> if the specified <paramref name="condition"/> is <see langword="true"/>.</summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="instance">The object whose type's full name should be included in any resulting <see cref="ObjectDisposedException"/>.</param>
        /// <exception cref="ObjectDisposedException">The <paramref name="condition"/> is <see langword="true"/>.</exception>
        [StackTraceHidden]
        public static void ThrowIf([DoesNotReturnIf(true)] bool condition, object instance)
        {
            if (condition)
            {
                ThrowHelper.ThrowObjectDisposedException(instance);
            }
        }

        /// <summary>Throws an <see cref="ObjectDisposedException"/> if the specified <paramref name="condition"/> is <see langword="true"/>.</summary>
        /// <param name="condition">The condition to evaluate.</param>
        /// <param name="type">The type whose full name should be included in any resulting <see cref="ObjectDisposedException"/>.</param>
        /// <exception cref="ObjectDisposedException">The <paramref name="condition"/> is <see langword="true"/>.</exception>
        [StackTraceHidden]
        public static void ThrowIf([DoesNotReturnIf(true)] bool condition, Type type)
        {
            if (condition)
            {
                ThrowHelper.ThrowObjectDisposedException(type);
            }
        }
    }
#endif
}
