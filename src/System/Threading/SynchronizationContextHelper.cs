using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace System.Threading;

public sealed class SynchronizationContextHelper(SynchronizationContext synchronizationContext)
{
    private sealed class CheckAccessInvoker(Func<SynchronizationContext, Func<bool>> factory)
    {
        private Func<SynchronizationContext, Func<bool>> _factory = factory ?? throw new ArgumentNullException(nameof(factory));

        public Func<SynchronizationContext, Func<bool>> GetFactory()
            => Interlocked.CompareExchange(ref _factory!, null, null)!;

        public Func<SynchronizationContext, Func<bool>> Switch(Func<SynchronizationContext, Func<bool>>? replacement)
        {
            var newValue = replacement ?? (_ => s_falseFunc);
            Interlocked.Exchange(ref _factory, newValue);
            return newValue;
        }
    }

    private readonly SynchronizationContext _synchronizationContext = synchronizationContext ?? throw new ArgumentNullException(nameof(synchronizationContext));
    private static readonly ConditionalWeakTable<Type, CheckAccessInvoker> s_checkAccessFactories = new();
    private static readonly ConditionalWeakTable<Type, Func<SynchronizationContext, bool>> s_checkAccessDelegates = new();
    private static readonly Func<bool> s_falseFunc = () => false;

    private Func<bool>? CheckAccessDelegate
    {
        get => Interlocked.CompareExchange(ref field, null, null);
        set => Interlocked.Exchange(ref field, value);
    }

    public bool IsThreadAffineKnown { get; } = synchronizationContext.IsThreadAffineKnown;

    public bool UseReferenceEquals { get; init; } = true;

    public bool CheckAccess()
    {
        if (UseReferenceEquals && IsThreadAffineKnown && ReferenceEquals(SynchronizationContext.Current, _synchronizationContext))
        {
            return true;
        }

        var checkAccess = CheckAccessDelegate ??= GetCheckAccessDelegate();
        return checkAccess();
    }

    private Func<bool> GetCheckAccessDelegate()
    {
        if (_synchronizationContext is IThreadAffineSynchronizationContext context)
        {
            return context.CheckAccess;
        }

        var type = _synchronizationContext.GetType();

        if (s_checkAccessDelegates.TryGetValue(type, out var checkAccess))
        {
            return () => checkAccess(_synchronizationContext);
        }

        Func<SynchronizationContext, Func<bool>> factory;
        if (s_checkAccessFactories.TryGetValue(type, out var holder))
        {
            factory = holder.GetFactory();
            return factory(_synchronizationContext);
        }

        if (!IsThreadAffineKnown)
        {
            return s_falseFunc;
        }

        holder = s_checkAccessFactories.GetValue(type, static t => CreateCheckAccessInvoker(t));
        factory = holder.GetFactory();
        return factory(_synchronizationContext);
    }

    internal static void RegisterCheckAccessDelegate(Type type, Func<SynchronizationContext, bool> checkAccess)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(type);
        ArgumentNullException.ThrowIfNull(checkAccess);
#else
        _ = type ?? throw new ArgumentNullException(nameof(type));
        _ = checkAccess ?? throw new ArgumentNullException(nameof(checkAccess));
#endif
        if (!typeof(SynchronizationContext).IsAssignableFrom(type))
        {
            throw new ArgumentException($"The type must derive from {typeof(SynchronizationContext).FullName}.", nameof(type));
        }
        s_checkAccessDelegates.AddOrUpdate(type, checkAccess);
        s_checkAccessFactories.Remove(type);
    }

    internal static bool UnregisterCheckAccessDelegate(Type type)
    {
#if NET6_0_OR_GREATER
        ArgumentNullException.ThrowIfNull(type);
#else
        _ = type ?? throw new ArgumentNullException(nameof(type));
#endif
        return s_checkAccessDelegates.Remove(type);
    }

    private static CheckAccessInvoker CreateCheckAccessInvoker(Type type)
    {
        var fullName = type.FullName;
        string message;
        switch (fullName)
        {
            case "Avalonia.Threading.AvaloniaSynchronizationContext":
            case "System.Windows.Threading.DispatcherSynchronizationContext":
                {
                    var dispatcherTypeFullName = fullName.Substring(0, fullName.LastIndexOf('.') + 1) + "Dispatcher";
                    Debug.Assert(dispatcherTypeFullName is "System.Windows.Threading.Dispatcher" or "Avalonia.Threading.Dispatcher");
                    var dispatcherFields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                    FieldInfo? dispatcherField = null;
                    foreach (var field in dispatcherFields)
                    {
                        if (field.FieldType.FullName != dispatcherTypeFullName) continue;
                        dispatcherField = field;
                        break;
                    }
                    Debug.Assert(dispatcherField != null);
                    if (dispatcherField == null)
                    {
                        return new CheckAccessInvoker(_ => s_falseFunc);
                    }
                    var checkAccessMethod = dispatcherField.FieldType.GetMethod("CheckAccess",
                        BindingFlags.Instance | BindingFlags.Public,
                        binder: null, types: Type.EmptyTypes, modifiers: null);
                    Debug.Assert(checkAccessMethod != null);
                    if (checkAccessMethod == null)
                    {
                        return new CheckAccessInvoker(_ => s_falseFunc);
                    }
#if NET5_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
                    bool supportCompile = RuntimeFeature.IsDynamicCodeSupported && RuntimeFeature.IsDynamicCodeCompiled;
#else
                    bool supportCompile = true;
#endif
                    Debug.Assert(supportCompile);
                    if (supportCompile)
                    {
                        try
                        {
                            var ctxParam = Expression.Parameter(typeof(SynchronizationContext), "context");
                            var castContext = Expression.Convert(ctxParam, type);
                            var fieldAccess = Expression.Field(castContext, dispatcherField);
                            var methodCall = Expression.Call(fieldAccess, checkAccessMethod);

                            var nullCheck = Expression.NotEqual(fieldAccess, Expression.Constant(null, dispatcherField.FieldType));
                            var conditional = Expression.Condition(nullCheck, methodCall, Expression.Constant(false));
                            var lambda = Expression.Lambda<Func<SynchronizationContext, bool>>(conditional, ctxParam);
                            var compiledFunc = lambda.Compile();

                            CheckAccessInvoker invoker = null!;
                            invoker = new CheckAccessInvoker(GetCompiledFactory);
                            return invoker;

                            Func<bool> GetCompiledFactory(SynchronizationContext ctx)
                            {
                                var current = () => compiledFunc(ctx);
                                return InvokeCompiled;

                                bool InvokeCompiled()
                                {
                                    try
                                    {
                                        return current();
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.Assert(ex is MethodAccessException or FieldAccessException or PlatformNotSupportedException or MemberAccessException);
                                        string msg = $"Expression compiled function call failed for {fullName}, using reflection fallback: {ex.Message}";
                                        Trace.WriteLine(msg);
                                        Debug.Fail(msg);
                                    }

                                    try
                                    {
                                        var newFactory = invoker.Switch(GetReflectiveFactory);
                                        current = newFactory(ctx);
                                        return current();
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.Assert(ex is MethodAccessException or FieldAccessException or PlatformNotSupportedException or MemberAccessException);
                                        string msg = $"Reflected function call failed for {fullName}, using false fallback: {ex.Message}";
                                        Trace.WriteLine(msg);
                                        Debug.Fail(msg);
                                    }

                                    current = invoker.Switch(_ => s_falseFunc)(ctx);
                                    return current();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            message = $"Expression compilation failed for {fullName}, using reflection fallback: {ex.Message}";
                            Trace.WriteLine(message);
                            Debug.Fail(message);
                        }
                    }

                    return new CheckAccessInvoker(GetReflectiveFactory);

                    Func<bool> GetReflectiveFactory(SynchronizationContext ctx)
                    {
                        var dispatcher = dispatcherField.GetValue(ctx);
                        Debug.Assert(dispatcher != null);
                        if (dispatcher == null) return s_falseFunc;
                        var checkAccess = Delegate.CreateDelegate(typeof(Func<bool>), dispatcher, checkAccessMethod, throwOnBindFailure: false) as Func<bool>;
                        Debug.Assert(checkAccess != null);
                        return checkAccess ?? s_falseFunc;
                    }
                }
            case "System.Windows.Forms.WindowsFormsSynchronizationContext":
                {
                    var controlFields = type.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.DeclaredOnly);
                    FieldInfo? controlField = null;
                    foreach (var field in controlFields)
                    {
                        if (field.FieldType.FullName != "System.Windows.Forms.Control") continue;
                        controlField = field;
                        break;
                    }
                    Debug.Assert(controlField != null);
                    if (controlField == null)
                    {
                        return new CheckAccessInvoker(_ => s_falseFunc);
                    }
                    var invokeRequiredProperty = controlField.FieldType.GetProperty("InvokeRequired");
                    Debug.Assert(invokeRequiredProperty != null);
                    if (invokeRequiredProperty == null)
                    {
                        return new CheckAccessInvoker(_ => s_falseFunc);
                    }
#if NET5_0_OR_GREATER || NETSTANDARD2_1_OR_GREATER
                    bool supportCompile = RuntimeFeature.IsDynamicCodeSupported && RuntimeFeature.IsDynamicCodeCompiled;
#else
                    bool supportCompile = true;
#endif
                    Debug.Assert(supportCompile);
                    if (supportCompile)
                    {
                        try
                        {
                            var ctxParam = Expression.Parameter(typeof(SynchronizationContext), "context");
                            var castContext = Expression.Convert(ctxParam, type);
                            var fieldAccess = Expression.Field(castContext, controlField);
                            var getProperty = Expression.Property(fieldAccess, invokeRequiredProperty);
                            var invert = Expression.Not(getProperty);

                            var nullCheck = Expression.NotEqual(fieldAccess, Expression.Constant(null, controlField.FieldType));
                            var conditional = Expression.Condition(nullCheck, invert, Expression.Constant(false));
                            var lambda = Expression.Lambda<Func<SynchronizationContext, bool>>(conditional, ctxParam);
#if !NETFRAMEWORK || NET471_OR_GREATER
                            var compiledFunc = lambda.Compile(true);
#else
                            var compiledFunc = lambda.Compile();
#endif
                            CheckAccessInvoker invoker = null!;
                            invoker = new CheckAccessInvoker(GetCompiledFactory);
                            return invoker;

                            Func<bool> GetCompiledFactory(SynchronizationContext ctx)
                            {
                                var current = () => compiledFunc(ctx);
                                return InvokeCompiled;

                                bool InvokeCompiled()
                                {
                                    try
                                    {
                                        return current();
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.Assert(ex is MethodAccessException or FieldAccessException or PlatformNotSupportedException or MemberAccessException);
                                        string msg = $"Expression compiled function call failed for {fullName}, using reflection fallback: {ex.Message}";
                                        Trace.WriteLine(msg);
                                        Debug.Fail(msg);
                                    }

                                    try
                                    {
                                        var newFactory = invoker.Switch(GetReflectiveFactory);
                                        current = newFactory(ctx);
                                        return current();
                                    }
                                    catch (Exception ex)
                                    {
                                        Debug.Assert(ex is MethodAccessException or FieldAccessException or PlatformNotSupportedException or MemberAccessException);
                                        string msg = $"Reflected function call failed for {fullName}, using false fallback: {ex.Message}";
                                        Trace.WriteLine(msg);
                                        Debug.Fail(msg);
                                    }

                                    current = invoker.Switch(_ => s_falseFunc)(ctx);
                                    return current();
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            message = $"Expression compilation failed for {fullName}, using reflection fallback: {ex.Message}";
                            Trace.WriteLine(message);
                            Debug.Fail(message);
                        }
                    }

                    return new CheckAccessInvoker(GetReflectiveFactory);

                    Func<bool> GetReflectiveFactory(SynchronizationContext ctx)
                    {
                        var control = controlField.GetValue(ctx);
                        Debug.Assert(control != null);
                        if (control == null) return s_falseFunc;
                        var getter = invokeRequiredProperty.GetMethod;
                        Debug.Assert(getter != null);
                        var getInvokeRequired = getter != null
                            ? Delegate.CreateDelegate(typeof(Func<bool>), control, getter, throwOnBindFailure: false) as Func<bool>
                            : null;
                        Debug.Assert(getInvokeRequired != null);
                        return getInvokeRequired != null ? () => !getInvokeRequired() : s_falseFunc;
                    }
                }
        }

        message = $"Unexpected delegate resolution for {fullName}";
        Trace.WriteLine(message);
        Debug.Fail(message);
        return new CheckAccessInvoker(_ => s_falseFunc); // Best safe default for unknown contexts: deny direct access.
    }
}
