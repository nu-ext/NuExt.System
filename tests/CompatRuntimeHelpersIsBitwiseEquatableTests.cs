using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

#pragma warning disable CS0649 // Field is never assigned to, and will always have its default value

namespace NuExt.System.Tests;

[TestFixture]
public class CompatRuntimeHelpersIsBitwiseEquatableTests
{
    // === Test Data: Type Definitions ===
    private struct PureBlittableStruct
    {
        public int X;
    }

    private struct StructWithReference
    {
        public int Id;
        public string Name; // Reference field, breaks bitwise equality
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct SequentialBlittableStruct
    {
        public byte A;
        public long B;
    }

    [StructLayout(LayoutKind.Explicit)]
    private struct ExplicitBlittableStruct
    {
        [FieldOffset(0)]
        public byte A;
        [FieldOffset(4)]
        public long B;
    }

    [StructLayout(LayoutKind.Auto)]
    private struct AutoBlittableStruct
    {
        public byte A;
        public long B;
    }

    private struct NestedBlittableStruct
    {
        public PureBlittableStruct Inner;
        public int Value;
    }

    private struct ComplexStructWithRef
    {
        public NestedBlittableStruct SafeData;
        public StructWithReference UnsafeData;
    }

    private enum TestEnum : byte
    {
        Zero = 0,
        Max = 255
    }

    [Flags]
    private enum Days
    {
        None = 0,
        Sunday = 1,
        Monday = 2,
        Tuesday = 4,
        Wednesday = 8,
        Thursday = 16,
        Friday = 32,
        Saturday = 64
    }

    private class SampleClass
    {
        public int Data;
    }

    private static bool IsBitwiseEquatable<T>()
    {
        return TypeInfoCache<T>.IsKnownBitwiseEquatable;
    }

    // === Core Tests ===

    [Test]
    public void IsBitwiseEquatable_OnReferenceType_ReturnsFalse()
    {
        using (Assert.EnterMultipleScope())
        {
            Assert.That(IsBitwiseEquatable<string>(), Is.False);
            Assert.That(IsBitwiseEquatable<object>(), Is.False);
            Assert.That(IsBitwiseEquatable<SampleClass>(), Is.False);
            Assert.That(IsBitwiseEquatable<int[]>(), Is.False);
        }
    }


    // === Performance & Caching Tests ===
    [Test]
    public void IsBitwiseEquatable_ResultIsCached_ReturnsConsistentValue()
    {
        bool firstCall = IsBitwiseEquatable<ComplexStructWithRef>();
        for (int i = 0; i < 5; i++)
        {
            bool subsequentCall = IsBitwiseEquatable<ComplexStructWithRef>();
            Assert.That(subsequentCall, Is.EqualTo(firstCall),
                $"Cached result mismatch on iteration {i}");
        }
    }

    // === Comparative Tests (for modern .NET where available) ===
#if NET
    private static bool RuntimeHelpersIsBitwiseEquatable<T>()
    {
        var method = Type.GetType("System.Runtime.CompilerServices.RuntimeHelpers")?
            .GetMethod("IsBitwiseEquatable", BindingFlags.Static | BindingFlags.NonPublic)
            ?.MakeGenericMethod(typeof(T));
        Assert.That(method, Is.Not.Null);
        return (bool)method.Invoke(null, null)!;
    }

    [Test]
    [TestCase(typeof(sbyte), ExpectedResult = true)]
    [TestCase(typeof(byte), ExpectedResult = true)]
    [TestCase(typeof(short), ExpectedResult = true)]
    [TestCase(typeof(ushort), ExpectedResult = true)]
    [TestCase(typeof(int), ExpectedResult = true)]
    [TestCase(typeof(uint), ExpectedResult = true)]
    [TestCase(typeof(long), ExpectedResult = true)]
    [TestCase(typeof(ulong), ExpectedResult = true)]
    [TestCase(typeof(bool), ExpectedResult = true)]
    [TestCase(typeof(char), ExpectedResult = true)]
    [TestCase(typeof(string), ExpectedResult = true)]
    [TestCase(typeof(double), ExpectedResult = true)]
    [TestCase(typeof(float), ExpectedResult = true)]
    [TestCase(typeof(decimal), ExpectedResult = true)]
    [TestCase(typeof(object), ExpectedResult = true)]
    [TestCase(typeof(SampleClass), ExpectedResult = true)]
    [TestCase(typeof(int[]), ExpectedResult = true)]
    [TestCase(typeof(Guid), ExpectedResult = true)]
    [TestCase(typeof(Int128), ExpectedResult = true)]
    [TestCase(typeof(UInt128), ExpectedResult = true)]
    [TestCase(typeof(DateTime), ExpectedResult = true)]
    [TestCase(typeof(DateTimeOffset), ExpectedResult = true)]
    [TestCase(typeof(TimeSpan), ExpectedResult = true)]
    [TestCase(typeof(IntPtr), ExpectedResult = true)]
    [TestCase(typeof(UIntPtr), ExpectedResult = true)]
    [TestCase(typeof(bool), ExpectedResult = true)]
    [TestCase(typeof(char), ExpectedResult = true)]
    [TestCase(typeof(TestEnum), ExpectedResult = true)]
    [TestCase(typeof(ConsoleColor), ExpectedResult = true)]
    [TestCase(typeof(Days), ExpectedResult = true)]
    //[TestCase(typeof(PureBlittableStruct), ExpectedResult = true)]
    [TestCase(typeof(AutoBlittableStruct), ExpectedResult = true)]
    [TestCase(typeof(SequentialBlittableStruct), ExpectedResult = true)]
    [TestCase(typeof(ExplicitBlittableStruct), ExpectedResult = true)]
    [TestCase(typeof(NestedBlittableStruct), ExpectedResult = true)]
    [TestCase(typeof(StructWithReference), ExpectedResult = true)]
    [TestCase(typeof(ComplexStructWithRef), ExpectedResult = true)]
    [TestCase(typeof(Rune), ExpectedResult = true)]
    public bool IsBitwiseEquatable_MatchesRuntimeHelpers(Type type)
    {
        var methodRuntimeHelpers = typeof(CompatRuntimeHelpersIsBitwiseEquatableTests)
            .GetMethod(nameof(RuntimeHelpersIsBitwiseEquatable), BindingFlags.Static | BindingFlags.NonPublic)
            ?.MakeGenericMethod(type);
        Assert.That(methodRuntimeHelpers, Is.Not.Null);

        var method = typeof(CompatRuntimeHelpersIsBitwiseEquatableTests)
            .GetMethod(nameof(IsBitwiseEquatable), BindingFlags.Static | BindingFlags.NonPublic)
            ?.MakeGenericMethod(type);
        Assert.That(method, Is.Not.Null);

        return (bool)methodRuntimeHelpers!.Invoke(null, null)! == (bool)method!.Invoke(null, null)!;
    }

    // === Parameterized Tests for Multiple Types With original method ===
    [Test]
    [TestCase(typeof(sbyte), ExpectedResult = true)]
    [TestCase(typeof(byte), ExpectedResult = true)]
    [TestCase(typeof(short), ExpectedResult = true)]
    [TestCase(typeof(ushort), ExpectedResult = true)]
    [TestCase(typeof(int), ExpectedResult = true)]
    [TestCase(typeof(uint), ExpectedResult = true)]
    [TestCase(typeof(long), ExpectedResult = true)]
    [TestCase(typeof(ulong), ExpectedResult = true)]
    [TestCase(typeof(bool), ExpectedResult = true)]
    [TestCase(typeof(char), ExpectedResult = true)]
    [TestCase(typeof(string), ExpectedResult = false)]
    [TestCase(typeof(double), ExpectedResult = false)]
    [TestCase(typeof(float), ExpectedResult = false)]
    [TestCase(typeof(decimal), ExpectedResult = false)]
    [TestCase(typeof(object), ExpectedResult = false)]
    [TestCase(typeof(SampleClass), ExpectedResult = false)]
    [TestCase(typeof(int[]), ExpectedResult = false)]
#if NET11_0_OR_GREATER
    [TestCase(typeof(Guid), ExpectedResult = true)]
    [TestCase(typeof(Int128), ExpectedResult = true)]
    [TestCase(typeof(UInt128), ExpectedResult = true)]
#else
    [TestCase(typeof(Guid), ExpectedResult = false)]
    [TestCase(typeof(Int128), ExpectedResult = false)]
    [TestCase(typeof(UInt128), ExpectedResult = false)]
#endif
    [TestCase(typeof(DateTime), ExpectedResult = false)]
    [TestCase(typeof(DateTimeOffset), ExpectedResult = false)]
    [TestCase(typeof(TimeSpan), ExpectedResult = false)]
    [TestCase(typeof(IntPtr), ExpectedResult = true)]
    [TestCase(typeof(UIntPtr), ExpectedResult = true)]
    [TestCase(typeof(bool), ExpectedResult = true)]
    [TestCase(typeof(char), ExpectedResult = true)]
    [TestCase(typeof(TestEnum), ExpectedResult = true)]
    [TestCase(typeof(ConsoleColor), ExpectedResult = true)]
    [TestCase(typeof(Days), ExpectedResult = true)]
    [TestCase(typeof(PureBlittableStruct), ExpectedResult = true)]
    [TestCase(typeof(AutoBlittableStruct), ExpectedResult = false)]
    [TestCase(typeof(SequentialBlittableStruct), ExpectedResult = false)]
    [TestCase(typeof(ExplicitBlittableStruct), ExpectedResult = false)]
    [TestCase(typeof(NestedBlittableStruct), ExpectedResult = false)]
    [TestCase(typeof(StructWithReference), ExpectedResult = false)]
    [TestCase(typeof(ComplexStructWithRef), ExpectedResult = false)]
    [TestCase(typeof(Rune), ExpectedResult = true)]
    public bool IsBitwiseEquatable_Original_ReturnsExpected(Type type)
    {
        // Use reflection to call the generic method with the runtime type
        var method = typeof(CompatRuntimeHelpersIsBitwiseEquatableTests)
            .GetMethod(nameof(RuntimeHelpersIsBitwiseEquatable), BindingFlags.Static | BindingFlags.NonPublic)
            ?.MakeGenericMethod(type);

        return (bool)method!.Invoke(null, null)!;
    }
#endif

    // === Parameterized Tests for Multiple Types ===
    [Test]
    [TestCase(typeof(sbyte), ExpectedResult = true)]
    [TestCase(typeof(byte), ExpectedResult = true)]
    [TestCase(typeof(short), ExpectedResult = true)]
    [TestCase(typeof(ushort), ExpectedResult = true)]
    [TestCase(typeof(int), ExpectedResult = true)]
    [TestCase(typeof(uint), ExpectedResult = true)]
    [TestCase(typeof(long), ExpectedResult = true)]
    [TestCase(typeof(ulong), ExpectedResult = true)]
    [TestCase(typeof(bool), ExpectedResult = true)]
    [TestCase(typeof(char), ExpectedResult = true)]
    [TestCase(typeof(string), ExpectedResult = false)]
    [TestCase(typeof(double), ExpectedResult = false)]
    [TestCase(typeof(float), ExpectedResult = false)]
    [TestCase(typeof(decimal), ExpectedResult = false)]
    [TestCase(typeof(object), ExpectedResult = false)]
    [TestCase(typeof(SampleClass), ExpectedResult = false)]
    [TestCase(typeof(int[]), ExpectedResult = false)]
#if NET11_0_OR_GREATER
    [TestCase(typeof(Guid), ExpectedResult = true)]
    [TestCase(typeof(Int128), ExpectedResult = true)]
    [TestCase(typeof(UInt128), ExpectedResult = true)]
#elif NET
    [TestCase(typeof(Guid), ExpectedResult = false)]
    [TestCase(typeof(Int128), ExpectedResult = false)]
    [TestCase(typeof(UInt128), ExpectedResult = false)]
#else
    [TestCase(typeof(Guid), ExpectedResult = false)]
#endif
    [TestCase(typeof(DateTime), ExpectedResult = false)]
    [TestCase(typeof(DateTimeOffset), ExpectedResult = false)]
    [TestCase(typeof(TimeSpan), ExpectedResult = false)]
    [TestCase(typeof(IntPtr), ExpectedResult = true)]
    [TestCase(typeof(UIntPtr), ExpectedResult = true)]
    [TestCase(typeof(bool), ExpectedResult = true)]
    [TestCase(typeof(char), ExpectedResult = true)]
    [TestCase(typeof(TestEnum), ExpectedResult = true)]
    [TestCase(typeof(ConsoleColor), ExpectedResult = true)]
    [TestCase(typeof(Days), ExpectedResult = true)]
    //[TestCase(typeof(PureBlittableStruct), ExpectedResult = true)]
    [TestCase(typeof(AutoBlittableStruct), ExpectedResult = false)]
    [TestCase(typeof(SequentialBlittableStruct), ExpectedResult = false)]
    [TestCase(typeof(ExplicitBlittableStruct), ExpectedResult = false)]
    [TestCase(typeof(NestedBlittableStruct), ExpectedResult = false)]
    [TestCase(typeof(StructWithReference), ExpectedResult = false)]
    [TestCase(typeof(ComplexStructWithRef), ExpectedResult = false)]
#if NET
    [TestCase(typeof(Rune), ExpectedResult = true)]
#endif
    public bool IsBitwiseEquatable_ForType_ReturnsExpected(Type type)
    {
        // Use reflection to call the generic method with the runtime type
        var method = typeof(CompatRuntimeHelpersIsBitwiseEquatableTests)
            .GetMethod(nameof(IsBitwiseEquatable), BindingFlags.Static | BindingFlags.NonPublic)
            ?.MakeGenericMethod(type);

        return (bool)method!.Invoke(null, null)!;
    }

    [Test]
    [TestCase(typeof(decimal), ExpectedResult = false)]
    [TestCase(typeof(Guid), ExpectedResult = false)]
    [TestCase(typeof(DateTime), ExpectedResult = false)]
    [TestCase(typeof(DateTimeOffset), ExpectedResult = false)]
    [TestCase(typeof(TimeSpan), ExpectedResult = false)]
    [TestCase(typeof(IntPtr), ExpectedResult = true)]
    [TestCase(typeof(UIntPtr), ExpectedResult = true)]
    [TestCase(typeof(double), ExpectedResult = true)]
    [TestCase(typeof(float), ExpectedResult = true)]
    [TestCase(typeof(char), ExpectedResult = true)]
    [TestCase(typeof(bool), ExpectedResult = true)]
    public bool Type_IsPrimitive(Type type)
    {
        return type.IsPrimitive;
    }
}
