using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

/*
 * https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/attributes/nullable-analysis
 * https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/proposals/csharp-10.0/caller-argument-expression
 */

namespace System;

/// <summary>
/// Provides utility methods to throw various exceptions based on specified conditions.
/// </summary>
[DebuggerStepThrough]
[StackTraceHidden]
public static class Throw
{
    /// <summary>
    /// Throws an <see cref="System.ArgumentException"/> with a specified message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    [DoesNotReturn]
    public static void ArgumentException(string? message)
    {
        throw new ArgumentException(message: message);
    }

    /// <summary>
    /// Throws an <see cref="System.ArgumentException"/> with a specified message.
    /// </summary>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="paramName">The name of the parameter that caused the current exception.</param>
    [DoesNotReturn]
    public static void ArgumentException(string message, string paramName)
    {
        throw new ArgumentException(message: message, paramName: paramName);
    }

    /// <summary>
    /// Throws an <see cref="System.ArgumentException"/> with a specified message if the given condition is true.
    /// </summary>
    /// <param name="condition">The condition that determines whether to throw the exception.</param>
    /// <param name="message">The message that describes the error.</param>
    /// <param name="conditionExpression">The expression of the condition, automatically captured by the compiler.</param>
    public static void ArgumentExceptionIf([DoesNotReturnIf(true)] bool condition, string message, [CallerArgumentExpression(nameof(condition))] string? conditionExpression = null)
    {
        if (condition) throw new ArgumentException(message: message, paramName: conditionExpression);
    }

    /// <summary>
    /// Throws an <see cref="System.ArgumentOutOfRangeException"/> with a specified parameter name and optional message if the given condition is true.
    /// </summary>
    /// <param name="condition">The condition that determines whether to throw the exception.</param>
    /// <param name="paramName">The name of the parameter that caused the exception.</param>
    /// <param name="message">The optional message that describes the error.</param>
    public static void ArgumentOutOfRangeExceptionIf([DoesNotReturnIf(true)] bool condition, string? paramName, string? message = null)
    {
        if (condition)
        {
            throw !string.IsNullOrEmpty(message) ? new ArgumentOutOfRangeException(paramName: paramName, message: message) : new ArgumentOutOfRangeException(paramName: paramName);
        }
    }

    /// <summary>
    /// Throws an <see cref="System.ArgumentNullException"/> with the name of the parameter that causes this exception.
    /// </summary>
    /// <param name="paramName">The name of the parameter that caused the exception.</param>
    /// <param name="message">A message that describes the error.</param>
    /// <exception cref="System.ArgumentNullException"></exception>
    [DoesNotReturn]
    public static void Null(string? paramName, string? message = null)
    {
        throw !string.IsNullOrEmpty(message) ? new ArgumentNullException(paramName: paramName, message: message) : new ArgumentNullException(paramName: paramName);
    }

    /// <summary>
    /// Throws an <see cref="System.ArgumentNullException"/> if the given argument is null.
    /// </summary>
    /// <param name="argument">The argument to check for null.</param>
    /// <param name="paramName">The name of the parameter that caused the exception.</param>
    /// <param name="message">A message that describes the error.</param>
    public static void IfNull([NotNull] object? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null, string? message = null)
    {
        if (argument == null)
        {
            throw !string.IsNullOrEmpty(message) ? new ArgumentNullException(paramName: paramName, message: message) : new ArgumentNullException(paramName: paramName);
        }
    }

    /// <summary>
    /// Throws an <see cref="System.ArgumentNullException"/> if the given string argument is null or an <see cref="System.ArgumentException"/> if it is empty.
    /// </summary>
    /// <param name="argument">The string argument to check.</param>
    /// <param name="paramName">The name of the parameter that caused the exception.</param>
    /// <param name="message">A message that describes the error.</param>
    public static void IfNullOrEmpty([NotNull] string? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null, string? message = null)
    {
        if (argument == null)
        {
            throw !string.IsNullOrEmpty(message) ? new ArgumentNullException(paramName: paramName, message: message) : new ArgumentNullException(paramName: paramName);
        }
        if (0u >= (uint)argument.Length)
        {
            throw new ArgumentException(message: !string.IsNullOrEmpty(message) ? message : SR.Argument_EmptyString, paramName: paramName);
        }
    }

    /// <summary>
    /// Throws an <see cref="System.ArgumentNullException"/> if the given string argument is null 
    /// or an <see cref="System.ArgumentException"/> if it is empty or consists only of white-space characters.
    /// </summary>
    /// <param name="argument">The string argument to check.</param>
    /// <param name="paramName">The name of the parameter that caused the exception.</param>
    /// <param name="message">A message that describes the error.</param>
    public static void IfNullOrWhiteSpace([NotNull] string? argument, [CallerArgumentExpression(nameof(argument))] string? paramName = null, string? message = null)
    {
        if (argument == null)
        {
            throw !string.IsNullOrEmpty(message) ? new ArgumentNullException(paramName: paramName, message: message) : new ArgumentNullException(paramName: paramName);
        }
        if (0u >= (uint)argument.Length)
        {
            throw new ArgumentException(message: !string.IsNullOrEmpty(message) ? message : SR.Argument_EmptyString, paramName: paramName);
        }
        for (uint i = 0; i < argument.Length; i++)
        {
            if (!char.IsWhiteSpace(argument[(int)i]))
            {
                return;
            }
        }
        throw new ArgumentException(message: !string.IsNullOrEmpty(message) ? message : SR.Argument_WhiteSpaceString, paramName: paramName);
    }

    /// <summary>
    /// Throws an <see cref="System.InvalidOperationException"/> with a specified message.
    /// </summary>
    /// <param name="message">The message that describes the error.</param>
    [DoesNotReturn]
    public static void InvalidOperationException(string? message)
    {
        throw !string.IsNullOrEmpty(message) ? new InvalidOperationException(message: message) : new InvalidOperationException();
    }

    /// <summary>
    /// Throws an <see cref="System.InvalidOperationException"/> with a specified message if the given condition is true.
    /// </summary>
    /// <param name="condition">The condition that determines whether to throw the exception.</param>
    /// <param name="message">The message that describes the error.</param>
    public static void InvalidOperationExceptionIf([DoesNotReturnIf(true)] bool condition, string? message)
    {
        if (condition)
        {
            throw !string.IsNullOrEmpty(message) ? new InvalidOperationException(message: message) : new InvalidOperationException();
        }
    }

    /// <summary>Throws an <see cref="System.ObjectDisposedException"/>.</summary>
    /// <param name="instance">The object whose type's full name should be included in any resulting <see cref="System.ObjectDisposedException"/>.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    [DoesNotReturn]
    public static void ObjectDisposedException(object? instance, string? message = null)
    {
        throw !string.IsNullOrEmpty(message) ? new ObjectDisposedException(objectName: instance?.GetType().FullName, message: message) : new ObjectDisposedException(objectName: instance?.GetType().FullName);
    }

    /// <summary>Throws an <see cref="System.ObjectDisposedException"/>.</summary>
    /// <param name="type">The type whose full name should be included in any resulting <see cref="System.ObjectDisposedException"/>.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    [DoesNotReturn]
     public static void ObjectDisposedException(Type? type, string? message = null)
    {
        throw !string.IsNullOrEmpty(message) ? new ObjectDisposedException(objectName: type?.FullName, message: message) : new ObjectDisposedException(objectName: type?.FullName);
    }

    /// <summary>Throws an <see cref="System.ObjectDisposedException"/> if the specified <paramref name="condition"/> is <see langword="true"/>.</summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="instance">The object whose type's full name should be included in any resulting <see cref="System.ObjectDisposedException"/>.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <exception cref="System.ObjectDisposedException">The <paramref name="condition"/> is <see langword="true"/>.</exception>
    public static void ObjectDisposedExceptionIf([DoesNotReturnIf(true)] bool condition, object? instance, string? message = null)
    {
        if (condition)
        {
            throw !string.IsNullOrEmpty(message) ? new ObjectDisposedException(objectName: instance?.GetType().FullName, message: message) : new ObjectDisposedException(objectName: instance?.GetType().FullName);
        }
    }

    /// <summary>Throws an <see cref="System.ObjectDisposedException"/> if the specified <paramref name="condition"/> is <see langword="true"/>.</summary>
    /// <param name="condition">The condition to evaluate.</param>
    /// <param name="type">The type whose full name should be included in any resulting <see cref="System.ObjectDisposedException"/>.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <exception cref="System.ObjectDisposedException">The <paramref name="condition"/> is <see langword="true"/>.</exception>
    public static void ObjectDisposedExceptionIf([DoesNotReturnIf(true)] bool condition, Type? type, string? message = null)
    {
        if (condition)
        {
            throw !string.IsNullOrEmpty(message) ? new ObjectDisposedException(objectName: type?.FullName, message: message) : new ObjectDisposedException(objectName: type?.FullName);
        }
    }
}