using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading;

namespace System;

/// <summary>
/// Provides a base implementation of <see cref="INotifyPropertyChanged"/> and <see cref="IDispatcherObject"/>
/// by utilizing a <see cref="System.Threading.SynchronizationContext"/> for thread affinity and notification marshaling.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="IDispatcherObject.CheckAccess"/> and <see cref="IDispatcherObject.VerifyAccess"/> methods
/// delegate to the associated <see cref="SynchronizationContext"/> if one is provided during construction.
/// When no context is associated, access is always granted.
/// </para>
/// <para>
/// This design enables consistent thread-affinity behavior across UI frameworks (WPF, WinForms, Avalonia)
/// while remaining usable in non-UI contexts.
/// </para>
/// </remarks>
[DebuggerStepThrough]
public abstract partial class PropertyChangeNotifier : INotifyPropertyChanged, IDispatcherObject, IDebuggable
{
    private sealed record PropertyChangedNotificationState(PropertyChangeNotifier Sender, PropertyChangedEventHandler Handler,
        PropertyChangedEventArgs E1, PropertyChangedEventArgs E2);

    private static readonly SendOrPostCallback s_propertiesChangedCallback = static state =>
    {
        var notification = (PropertyChangedNotificationState)state!;
        notification.Handler(notification.Sender, notification.E1);
        notification.Handler(notification.Sender, notification.E2);
    };

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyChangeNotifier"/> class 
    /// with the specified synchronization context.
    /// </summary>
    /// <param name="synchronizationContext">
    /// An optional synchronization context to use for property change notifications. 
    /// If null, no synchronization context will be used.
    /// </param>
    protected PropertyChangeNotifier(SynchronizationContext? synchronizationContext)
    {
        SynchronizationContext = synchronizationContext;
        if (!HasSynchronizationContext) return;
        _state = new State(new SynchronizationContextHelper(synchronizationContext!), OnPropertyChanged);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PropertyChangeNotifier"/> class 
    /// without a synchronization context.
    /// </summary>
    protected PropertyChangeNotifier() : this(null)
    {
    }

    #region Properties

    /// <inheritdoc/>
    DebugInfo? IDebuggable.DebugInfo
    {
        get => DebugInfoStorage.GetDebugInfo(this);
        set => DebugInfoStorage.SetDebugInfo(this, value);
    }

    /// <summary>
    /// Gets a value indicating whether there are any subscribers to the PropertyChanged event.
    /// </summary>
    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public bool HasPropertyChangedSubscribers
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        get => PropertyChanged is not null;
    }

    #endregion

    #region Events

    /// <summary>
    /// Occurs when a property value changes.
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    #endregion

    #region Methods

    /// <summary>
    /// Gets the current subscribers of the PropertyChanged event.
    /// </summary>
    /// <returns>
    /// An array of <see cref="PropertyChangedEventHandler"/> delegates. If there are no subscribers, returns an empty array.
    /// </returns>
    protected PropertyChangedEventHandler[] GetPropertyChangedSubscribers()
    {
        // eventDelegate will be null if no listeners are attached to the event
        var eventDelegate = PropertyChanged;
        if (eventDelegate is null)
        {
            return [];
        }

        var subscribers = Array.ConvertAll(eventDelegate.GetInvocationList(), del => (PropertyChangedEventHandler)del);
        return subscribers;
    }

    private void OnPropertyChanged(object? state)
    {
        PropertyChanged?.Invoke(this, (PropertyChangedEventArgs)state!);
    }

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event (per <see cref="INotifyPropertyChanged" />).
    /// </summary>
    /// <param name="e">The event data containing the name of the property that changed.</param>
    protected void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        var handler = PropertyChanged;
        if (handler == null)
        {
            return;
        }
        if (HasSynchronizationContext)
        {
            var s = _state!;
            if (!s.Helper.CheckAccess())
            {
                SynchronizationContext!.Post(s.PropertyChangedCallback, e);
                return;
            }
        }
        handler(this, e);
    }

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event for a specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        var handler = PropertyChanged;
        if (handler == null)
        {
            return;
        }
        var args = EventArgsCache.Get(propertyName);
        if (HasSynchronizationContext)
        {
            var s = _state!;
            if (!s.Helper.CheckAccess())
            {
                SynchronizationContext!.Post(s.PropertyChangedCallback, args);
                return;
            }
        }
        handler(this, args);
    }

    [DebuggerHidden]
    [EditorBrowsable(EditorBrowsableState.Never)]
    private protected void RaisePropertiesChangedAndClearHandlers(PropertyChangedEventArgs e1, PropertyChangedEventArgs e2)
    {
        var handler = PropertyChanged;
        PropertyChanged = null;

        if (handler == null)
        {
            return;
        }

        if (HasSynchronizationContext && !CheckAccess())
        {
            SynchronizationContext!.Post(s_propertiesChangedCallback, new PropertyChangedNotificationState(this, handler, e1, e2));
            return;
        }

        handler(this, e1);
        handler(this, e2);
    }

    /// <summary>
    /// Determines whether the property can be set. Can be overridden in derived classes for custom logic.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="oldValue">The current value of the property.</param>
    /// <param name="newValue">The new value to set.</param>
    /// <param name="propertyName">The name of the property being updated. This is optional and can be automatically provided by the compiler.</param>
    /// <returns><see langword="true"/> if the property can be set; otherwise, <see langword="false"/>.</returns>
    protected virtual bool CanSetProperty<T>(T oldValue, T newValue, [CallerMemberName] string? propertyName = null)
    {
        return true;
    }

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event for the specified property.
    /// </summary>
    /// <param name="e">The event data containing the name of the property that changed.</param>
    public void RaisePropertyChanged(PropertyChangedEventArgs e)
    {
        OnPropertyChanged(e);
    }

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event for the specified property.
    /// </summary>
    /// <param name="propertyName">The name of the property that changed.</param>
    public void RaisePropertyChanged(string propertyName)
    {
        OnPropertyChanged(propertyName);
    }

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event for multiple properties.
    /// </summary>
    /// <param name="args">An enumerable collection of <see cref="PropertyChangedEventArgs"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="args"/> is <see langword="null"/>.</exception>
    public void RaisePropertiesChanged(IEnumerable<PropertyChangedEventArgs> args)
    {
        _ = args ?? throw new ArgumentNullException(nameof(args));
        foreach (var e in args)
        {
            OnPropertyChanged(e);
        }
    }

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event for multiple properties.
    /// </summary>
    /// <param name="propertyNames">An enumerable collection of property names.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="propertyNames"/> is <see langword="null"/>.</exception>
    public void RaisePropertiesChanged(IEnumerable<string> propertyNames)
    {
        _ = propertyNames ?? throw new ArgumentNullException(nameof(propertyNames));
        foreach (var propertyName in propertyNames)
        {
            OnPropertyChanged(propertyName);
        }
    }

#if NETSTANDARD2_1_OR_GREATER || NET5_0_OR_GREATER
    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event for multiple properties.
    /// </summary>
    /// <param name="args">A read-only span of <see cref="PropertyChangedEventArgs"/> containing event data for the properties that changed.</param>
    public void RaisePropertiesChanged(params ReadOnlySpan<PropertyChangedEventArgs> args)
    {
        foreach (var e in args)
        {
            OnPropertyChanged(e);
        }
    }

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event for multiple properties.
    /// </summary>
    /// <param name="propertyNames">A read-only span of property names.</param>
    public void RaisePropertiesChanged(params ReadOnlySpan<string> propertyNames)
    {
        foreach (var propertyName in propertyNames)
        {
            OnPropertyChanged(propertyName);
        }
    }
#else
    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event for two specified properties.
    /// </summary>
    /// <param name="e1">The event data containing the name of the first property that changed.</param>
    /// <param name="e2">The event data containing the name of the second property that changed.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RaisePropertiesChanged(PropertyChangedEventArgs e1, PropertyChangedEventArgs e2)
    {
        OnPropertyChanged(e1);
        OnPropertyChanged(e2);
    }

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event for two specified properties.
    /// </summary>
    /// <param name="propertyName1">The name of the first property that changed.</param>
    /// <param name="propertyName2">The name of the second property that changed.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RaisePropertiesChanged(string propertyName1, string propertyName2)
    {
        OnPropertyChanged(propertyName1);
        OnPropertyChanged(propertyName2);
    }

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event for three specified properties.
    /// </summary>
    /// <param name="e1">The event data containing the name of the first property that changed.</param>
    /// <param name="e2">The event data containing the name of the second property that changed.</param>
    /// <param name="e3">The event data containing the name of the third property that changed.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RaisePropertiesChanged(PropertyChangedEventArgs e1, PropertyChangedEventArgs e2, PropertyChangedEventArgs e3)
    {
        OnPropertyChanged(e1);
        OnPropertyChanged(e2);
        OnPropertyChanged(e3);
    }

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event for three specified properties.
    /// </summary>
    /// <param name="propertyName1">The name of the first property that changed.</param>
    /// <param name="propertyName2">The name of the second property that changed.</param>
    /// <param name="propertyName3">The name of the third property that changed.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void RaisePropertiesChanged(string propertyName1, string propertyName2, string propertyName3)
    {
        OnPropertyChanged(propertyName1);
        OnPropertyChanged(propertyName2);
        OnPropertyChanged(propertyName3);
    }

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event for multiple properties.
    /// </summary>
    /// <param name="args">An array of <see cref="PropertyChangedEventArgs"/>.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="args"/> is <see langword="null"/>.</exception>
    public void RaisePropertiesChanged(params PropertyChangedEventArgs[] args)
    {
        _ = args ?? throw new ArgumentNullException(nameof(args));
        for (int i = 0; i < args.Length; i++)
        {
            OnPropertyChanged(args[i]);
        }
    }

    /// <summary>
    /// Raises the <see cref="PropertyChanged"/> event for multiple properties.
    /// </summary>
    /// <param name="propertyNames">An array of property names.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="propertyNames"/> is <see langword="null"/>.</exception>
    public void RaisePropertiesChanged(params string[] propertyNames)
    {
        _ = propertyNames ?? throw new ArgumentNullException(nameof(propertyNames));
        for (int i = 0; i < propertyNames.Length; i++)
        {
            OnPropertyChanged(propertyNames[i]);
        }
    }
#endif

    /// <summary>
    /// Sets the property and raises the <see cref="PropertyChanged"/> event if the value has changed.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="storage">The backing field for the property.</param>
    /// <param name="value">The new value for the property.</param>
    /// <param name="e">The event data containing the name of the property that changed.</param>
    /// <returns><see langword="true"/> if the value was changed; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool SetProperty<T>(ref T storage, T value, PropertyChangedEventArgs e)
    {
        return SetProperty(ref storage, value, e, out _);
    }

    /// <summary>
    /// Sets the property and raises the <see cref="PropertyChanged"/> event if the value has changed.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="storage">The backing field for the property.</param>
    /// <param name="value">The new value for the property.</param>
    /// <param name="propertyName">The name of the property. This optional parameter can be omitted because of CallerMemberName attribute.</param>
    /// <returns><see langword="true"/> if the value was changed; otherwise, <see langword="false"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string? propertyName = null)
    {
        return SetProperty(ref storage, value, out _, propertyName);
    }

    /// <summary>
    /// Sets the property and raises the <see cref="PropertyChanged"/> event if the value has changed, and invokes a callback after the property is set.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="storage">The backing field for the property.</param>
    /// <param name="value">The new value for the property.</param>
    /// <param name="changedCallback">Callback to invoke after the value has been changed.</param>
    /// <param name="e">The event data containing the name of the property that changed.</param>
    /// <returns><see langword="true"/> if the value was changed; otherwise, <see langword="false"/>.</returns>
    protected bool SetProperty<T>(ref T storage, T value, Action? changedCallback, PropertyChangedEventArgs e)
    {
        if (!SetProperty(ref storage, value, e, out _)) return false;
        changedCallback?.Invoke();
        return true;
    }

    /// <summary>
    /// Sets the property and raises the <see cref="PropertyChanged"/> event if the value has changed, and invokes a callback after the property is set.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="storage">The backing field for the property.</param>
    /// <param name="value">The new value for the property.</param>
    /// <param name="changedCallback">Callback to invoke after the value has been changed.</param>
    /// <param name="propertyName">The name of the property. This optional parameter can be omitted because of CallerMemberName attribute.</param>
    /// <returns><see langword="true"/> if the value was changed; otherwise, <see langword="false"/>.</returns>
    protected bool SetProperty<T>(ref T storage, T value, Action? changedCallback, [CallerMemberName] string? propertyName = null)
    {
        if (!SetProperty(ref storage, value, out _, propertyName)) return false;
        changedCallback?.Invoke();
        return true;
    }

    /// <summary>
    /// Sets the property and raises the <see cref="PropertyChanged"/> event if the value has changed, and invokes a callback after the property is set.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="storage">The backing field for the property.</param>
    /// <param name="value">The new value for the property.</param>
    /// <param name="changedCallback">Callback to invoke after the value has been changed.</param>
    /// <param name="e">The event data containing the name of the property that changed.</param>
    /// <param name="oldValue">Outputs the old value of the property before it was changed.</param>
    /// <returns><see langword="true"/> if the value was changed; otherwise, <see langword="false"/>.</returns>
    protected bool SetProperty<T>(ref T storage, T value, Action? changedCallback, PropertyChangedEventArgs e, out T oldValue)
    {
        if (!SetProperty(ref storage, value, e, out oldValue)) return false;
        changedCallback?.Invoke();
        return true;
    }

    /// <summary>
    /// Sets the property and raises the <see cref="PropertyChanged"/> event if the value has changed, and invokes a callback after the property is set.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="storage">The backing field for the property.</param>
    /// <param name="value">The new value for the property.</param>
    /// <param name="changedCallback">Callback to invoke after the value has been changed.</param>
    /// <param name="oldValue">Outputs the old value of the property before it was changed.</param>
    /// <param name="propertyName">The name of the property. This optional parameter can be omitted because of CallerMemberName attribute.</param>
    /// <returns><see langword="true"/> if the value was changed; otherwise, <see langword="false"/>.</returns>
    protected bool SetProperty<T>(ref T storage, T value, Action? changedCallback, out T oldValue, [CallerMemberName] string? propertyName = null)
    {
        if (!SetProperty(ref storage, value, out oldValue, propertyName)) return false;
        changedCallback?.Invoke();
        return true;
    }

    /// <summary>
    /// Sets the property and raises the <see cref="PropertyChanged"/> event if the value has changed, and invokes a callback with the old value after the property is set.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="storage">The backing field for the property.</param>
    /// <param name="value">The new value for the property.</param>
    /// <param name="changedCallback">Callback to invoke with the old value after the value has been changed.</param>
    /// <param name="e">The event data containing the name of the property that changed.</param>
    /// <returns><see langword="true"/> if the value was changed; otherwise, <see langword="false"/>.</returns>
    protected bool SetProperty<T>(ref T storage, T value, Action<T>? changedCallback, PropertyChangedEventArgs e)
    {
        if (!SetProperty(ref storage, value, e, out var oldValue)) return false;
        changedCallback?.Invoke(oldValue);
        return true;
    }

    /// <summary>
    /// Sets the property and raises the <see cref="PropertyChanged"/> event if the value has changed, and invokes a callback with the old value after the property is set.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="storage">The backing field for the property.</param>
    /// <param name="value">The new value for the property.</param>
    /// <param name="changedCallback">Callback to invoke with the old value after the value has been changed.</param>
    /// <param name="propertyName">The name of the property. This optional parameter can be omitted because of CallerMemberName attribute.</param>
    /// <returns><see langword="true"/> if the value was changed; otherwise, <see langword="false"/>.</returns>
    protected bool SetProperty<T>(ref T storage, T value, Action<T>? changedCallback, [CallerMemberName] string? propertyName = null)
    {
        if (!SetProperty(ref storage, value, out var oldValue, propertyName)) return false;
        changedCallback?.Invoke(oldValue);
        return true;
    }

    /// <summary>
    /// Sets the property and raises the <see cref="PropertyChanged"/> event if the value has changed, and invokes a callback with the name of the property and the old value after the property is set.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="storage">The backing field for the property.</param>
    /// <param name="value">The new value for the property.</param>
    /// <param name="changedCallback">Callback to invoke with the name of the property and the old value after the value has been changed.</param>
    /// <param name="e">The event data containing the name of the property that changed.</param>
    /// <returns><see langword="true"/> if the value was changed; otherwise, <see langword="false"/>.</returns>
    protected bool SetProperty<T>(ref T storage, T value, Action<string?, T>? changedCallback, PropertyChangedEventArgs e)
    {
        if (!SetProperty(ref storage, value, e, out var oldValue)) return false;
        changedCallback?.Invoke(e.PropertyName, oldValue);
        return true;
    }

    /// <summary>
    /// Sets the property and raises the <see cref="PropertyChanged"/> event if the value has changed, and invokes a callback with the name of the property and the old value after the property is set.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="storage">The backing field for the property.</param>
    /// <param name="value">The new value for the property.</param>
    /// <param name="changedCallback">Callback to invoke with the name of the property and the old value after the value has been changed.</param>
    /// <param name="propertyName">The name of the property. This optional parameter can be omitted because of CallerMemberName attribute.</param>
    /// <returns><see langword="true"/> if the value was changed; otherwise, <see langword="false"/>.</returns>
    protected bool SetProperty<T>(ref T storage, T value, Action<string?, T>? changedCallback, [CallerMemberName] string? propertyName = null)
    {
        if (!SetProperty(ref storage, value, out var oldValue, propertyName)) return false;
        changedCallback?.Invoke(propertyName, oldValue);
        return true;
    }

    /// <summary>
    /// Sets the property and raises the <see cref="PropertyChanged"/> event if the value has changed, and invokes a callback with the old value after the property is set.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="storage">The backing field for the property.</param>
    /// <param name="value">The new value for the property.</param>
    /// <param name="changedCallback">Callback to invoke with the old value after the value has been changed.</param>
    /// <param name="e">The event data containing the name of the property that changed.</param>
    /// <param name="oldValue">Outputs the old value of the property before it was changed.</param>
    /// <returns><see langword="true"/> if the value was changed; otherwise, <see langword="false"/>.</returns>
    protected bool SetProperty<T>(ref T storage, T value, Action<T>? changedCallback, PropertyChangedEventArgs e, out T oldValue)
    {
        if (!SetProperty(ref storage, value, e, out oldValue)) return false;
        changedCallback?.Invoke(oldValue);
        return true;
    }

    /// <summary>
    /// Sets the property and raises the <see cref="PropertyChanged"/> event if the value has changed, and invokes a callback with the old value after the property is set.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="storage">The backing field for the property.</param>
    /// <param name="value">The new value for the property.</param>
    /// <param name="changedCallback">Callback to invoke with the old value after the value has been changed.</param>
    /// <param name="oldValue">Outputs the old value of the property before it was changed.</param>
    /// <param name="propertyName">The name of the property. This optional parameter can be omitted because of CallerMemberName attribute.</param>
    /// <returns><see langword="true"/> if the value was changed; otherwise, <see langword="false"/>.</returns>
    protected bool SetProperty<T>(ref T storage, T value, Action<T>? changedCallback, out T oldValue, [CallerMemberName] string? propertyName = null)
    {
        if (!SetProperty(ref storage, value, out oldValue, propertyName)) return false;
        changedCallback?.Invoke(oldValue);
        return true;
    }

    /// <summary>
    /// Sets the property and raises the <see cref="PropertyChanged"/> event if the value has changed, and invokes a callback with the name of the property and the old value after the property is set.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="storage">The backing field for the property.</param>
    /// <param name="value">The new value for the property.</param>
    /// <param name="changedCallback">Callback to invoke with the name of the property and the old value after the value has been changed.</param>
    /// <param name="e">The event data containing the name of the property that changed.</param>
    /// <param name="oldValue">Outputs the old value of the property before it was changed.</param>
    /// <returns><see langword="true"/> if the value was changed; otherwise, <see langword="false"/>.</returns>
    protected bool SetProperty<T>(ref T storage, T value, Action<string?, T>? changedCallback, PropertyChangedEventArgs e, out T oldValue)
    {
        if (!SetProperty(ref storage, value, e, out oldValue)) return false;
        changedCallback?.Invoke(e.PropertyName, oldValue);
        return true;
    }

    /// <summary>
    /// Sets the property and raises the <see cref="PropertyChanged"/> event if the value has changed, and invokes a callback with the name of the property and the old value after the property is set.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="storage">The backing field for the property.</param>
    /// <param name="value">The new value for the property.</param>
    /// <param name="changedCallback">Callback to invoke with the name of the property and the old value after the value has been changed.</param>
    /// <param name="oldValue">Outputs the old value of the property before it was changed.</param>
    /// <param name="propertyName">The name of the property. This optional parameter can be omitted because of CallerMemberName attribute.</param>
    /// <returns><see langword="true"/> if the value was changed; otherwise, <see langword="false"/>.</returns>
    protected bool SetProperty<T>(ref T storage, T value, Action<string?, T>? changedCallback, out T oldValue, [CallerMemberName] string? propertyName = null)
    {
        if (!SetProperty(ref storage, value, out oldValue, propertyName)) return false;
        changedCallback?.Invoke(propertyName, oldValue);
        return true;
    }

    /// <summary>
    /// Updates the specified property if the new value is different from the current value,
    /// raises the <see cref="PropertyChanged"/> event, and allows cancellation through the <see cref="CanSetProperty{T}"/> method.
    /// Outputs the old value of the property before it was changed.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="storage">The backing field for the property.</param>
    /// <param name="value">The new value for the property.</param>
    /// <param name="e">The event data containing the name of the property that changed.</param>
    /// <param name="oldValue">Outputs the old value of the property before it was changed.</param>
    /// <returns><see langword="true"/> if the property value was changed; otherwise, <see langword="false"/>.</returns>
    protected bool SetProperty<T>(ref T storage, T value, PropertyChangedEventArgs e, out T oldValue)
    {
        oldValue = storage;
        if (EqualityComparer<T>.Default.Equals(storage, value) || !CanSetProperty(oldValue, value, e.PropertyName))
        {
            return false;
        }
        storage = value;
        OnPropertyChanged(e);
        return true;
    }

    /// <summary>
    /// Updates the specified property if the new value is different from the current value,
    /// raises the <see cref="PropertyChanged"/> event, and allows cancellation through the <see cref="CanSetProperty{T}"/> method.
    /// Outputs the old value of the property before it was changed.
    /// </summary>
    /// <typeparam name="T">The type of the property.</typeparam>
    /// <param name="storage">The backing field for the property.</param>
    /// <param name="value">The new value for the property.</param>
    /// <param name="oldValue">Outputs the old value of the property before it was changed.</param>
    /// <param name="propertyName">The name of the property. This optional parameter can be omitted because of CallerMemberName attribute.</param>
    /// <returns><see langword="true"/> if the property value was changed; otherwise, <see langword="false"/>.</returns>
    protected bool SetProperty<T>(ref T storage, T value, out T oldValue, [CallerMemberName] string? propertyName = null)
    {
        oldValue = storage;
        if (EqualityComparer<T>.Default.Equals(storage, value) || !CanSetProperty(oldValue, value, propertyName))
        {
            return false;
        }
        storage = value;
        OnPropertyChanged(propertyName);
        return true;
    }

    #endregion
}

internal static partial class EventArgsCache
{
    private static readonly ConcurrentDictionary<string, PropertyChangedEventArgs> s_cache =
        new(StringComparer.Ordinal);

    private static readonly PropertyChangedEventArgs s_allPropsChanged = new(null);

    public static PropertyChangedEventArgs Get(string? propertyName) => propertyName is null ? s_allPropsChanged 
        : s_cache.GetOrAdd(propertyName, static name => new PropertyChangedEventArgs(name));
}
