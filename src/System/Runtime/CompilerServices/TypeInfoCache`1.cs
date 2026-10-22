using System.Diagnostics;
using System.Reflection;

namespace System.Runtime.CompilerServices;

/// <summary>
/// Static cache holder for type information.
/// </summary>
internal static class TypeInfoCache<T>
{
    public static readonly bool IsReferenceOrContainsReferences = GetIsReferenceOrContainsReferences(typeof(T));
    public static readonly bool IsKnownBitwiseEquatable = GetIsKnownBitwiseEquatable(typeof(T));

    private static bool GetIsReferenceOrContainsReferences(Type type)
    {
        // Pointer types do not contain managed references.
        if (type.IsPointer)
        {
            return false;
        }

        // Reference types (classes, interfaces, arrays, strings, delegates)
        if (!type.IsValueType)
        {
            return true;
        }

        if (type.IsPrimitive || type.IsEnum)
        {
            return false;
        }

        if (type == typeof(decimal) || type == typeof(Guid) ||
            type == typeof(DateTime) || type == typeof(DateTimeOffset) || type == typeof(TimeSpan))
        {
            return false;
        }

#if NET
        if (type == typeof(System.Text.Rune)) return false;
#endif

        // For structs, perform a recursive check of all instance fields
        return ContainsReferencesInFields(type);
    }

    /// <summary>
    /// Recursively inspects all fields of a value type to determine
    /// if any reference type fields exist in its graph.
    /// </summary>
    private static bool ContainsReferencesInFields(Type type)
    {
        Debug.Assert(type.IsValueType);

        FieldInfo[] fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        foreach (FieldInfo field in fields)
        {
            Type fieldType = field.FieldType;

            if (GetIsReferenceOrContainsReferences(fieldType))
            {
                return true;
            }
        }

        // No reference type fields found in this type or any nested types.
        return false;
    }

    private static bool GetIsKnownBitwiseEquatable(Type type)
    {
        // Reference types can never be compared bitwise.
        if (!type.IsValueType)
        {
            return false;
        }

        if (type == typeof(decimal) || type == typeof(Guid) ||
            type == typeof(double) || type == typeof(float) ||
            type == typeof(DateTime) || type == typeof(DateTimeOffset) || type == typeof(TimeSpan))
        {
            return false;
        }

#if NET
        if (type == typeof(System.Text.Rune)) return true;
#endif

        if (type.IsPrimitive || type.IsEnum) return true;

        // For user structs: we cannot reliably determine if they are bitwise equatable
        // because we cannot verify blittable status without unsafe code or runtime checks.
        // Conservative approach: return false for all user-defined structs.
        return false;
    }
}