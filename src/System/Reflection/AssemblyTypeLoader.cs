using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace System.Reflection;

/// <summary>
/// Provides methods for loading assemblies and finding types
/// that are assignable to a specified base type or interface.
/// </summary>
public static class AssemblyTypeLoader
{
    /// <summary>
    /// Loads an assembly from the specified path and finds the first type
    /// that is assignable to the specified base type or interface.
    /// </summary>
    /// <typeparam name="T">The base type or interface that the found type must be assignable to.</typeparam>
    /// <param name="assemblyPath">The path to the assembly from which the type will be loaded.</param>
    /// <param name="match">A predicate to apply additional filtering to the found types. If null, the first suitable type is returned.</param>
    /// <returns>The type that is assignable to <typeparamref name="T"/>, or null if no such type is found.</returns>
    /// <exception cref="ArgumentException"><paramref name="assemblyPath"/> is <see langword="null"/> or empty.</exception>
    public static Type? FindAssignableType<T>(string assemblyPath, Predicate<Type>? match = null)
    {
        return FindAssignableType(assemblyPath, typeof(T), match);
    }

    /// <summary>
    /// Loads an assembly from the specified path and finds the first type
    /// that is assignable to the specified base type or interface.
    /// </summary>
    /// <param name="assemblyPath">The path to the assembly from which the type will be loaded.</param>
    /// <param name="type">The base type or interface that the found type must be assignable to.</param>
    /// <param name="match">A predicate to apply additional filtering to the found types. If null, the first suitable type is returned.</param>
    /// <returns>The type that is assignable to <paramref name="type"/>, or null if no such type is found.</returns>
    /// <exception cref="ArgumentException"><paramref name="assemblyPath"/> is <see langword="null"/> or empty.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
    public static Type? FindAssignableType(string assemblyPath, Type type, Predicate<Type>? match = null)
    {
        ArgumentException.ThrowIfNullOrEmpty(assemblyPath);
        ArgumentNullException.ThrowIfNull(type);
#if DEBUG
        var assembliesBeforeLoad = AppDomain.CurrentDomain.GetAssemblies();
#endif
        Assembly? assembly;
        try
        {
            assembly = Assembly.LoadFrom(assemblyPath);
        }
        catch (Exception ex)
        {
            Debug.Fail($"Failed to load assembly from path {assemblyPath}. Exception: {ex}");
            throw;
        }
#if DEBUG
        var assembliesAfterLoad = AppDomain.CurrentDomain.GetAssemblies();
        Debug.WriteLine($"Loaded assembly {assembly.FullName} ({assembly.GetHashCode()})");
        var newAssemblies = assembliesAfterLoad.Except(assembliesBeforeLoad);
        foreach (var asm in newAssemblies)
        {
            Debug.WriteLine($"Newly loaded assembly: {asm.FullName} ({asm.GetHashCode()})");
        }
#endif
        IEnumerable<Type> types;
        try
        {
            types = assembly.ExportedTypes;
        }
        catch (ReflectionTypeLoadException ex)
        {
            types = ex.Types.Where(t => t != null)!;
        }
        foreach (var t in types)
        {
            if (type.IsAssignableFrom(t) && (match?.Invoke(t) ?? true))
            {
                return t;
            }
        }

        Debug.Assert(false, $"Can't find type assignable from '{type}' in assembly '{assembly.FullName}'");
        return null;
    }
}
