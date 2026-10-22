namespace System.Collections.Concurrent;

public static class ConcurrentDictionaryExtensions
{
    extension<TKey, TValue>(ConcurrentDictionary<TKey, TValue> dict) where TKey : notnull
    {

        /// <summary>
        /// Atomically replaces the value for the specified key if it exists; otherwise adds the key/value pair.
        /// </summary>
        /// <param name="key">The key to update or add.</param>
        /// <param name="newValue">The value to set when updating or adding.</param>
        /// <param name="oldValue">
        /// When the return value is <see langword="true"/>, contains the value that was replaced;
        /// otherwise set to <see langword="default"/> when a new entry was added.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if an existing value was replaced; otherwise <see langword="false"/> if a new entry was added.
        /// </returns>
        /// <remarks>
        /// Provides atomic update-or-add semantics using the thread-safe conditional operations of
        /// <see cref="ConcurrentDictionary{TKey,TValue}"/>.
        /// Unlike <see cref="ConcurrentDictionary{TKey,TValue}.AddOrUpdate(TKey, Func{TKey,TValue}, Func{TKey,TValue,TValue})"/>,
        /// this method does not invoke user delegates and returns the replaced value when an update occurs.
        /// </remarks>
        public bool TryUpdateOrAdd(TKey key, TValue newValue, out TValue? oldValue)
        {
            ArgumentNullException.ThrowIfNull(dict);

            // Fast path: try add
            if (dict.TryAdd(key, newValue))
            {
                oldValue = default;
                return false; // added
            }

            // Contended path: CAS-loop
            while (true)
            {
                if (dict.TryGetValue(key, out var current))
                {
                    if (dict.TryUpdate(key, newValue, current))
                    {
                        oldValue = current;
                        return true; // updated
                    }

                    // Lost race, retry
                    continue;
                }

                // Key was removed meanwhile → try add again
                if (dict.TryAdd(key, newValue))
                {
                    oldValue = default;
                    return false; // added
                }
            }
        }

        /// <summary>
        /// Atomically adds a key with the provided <paramref name="addValue"/> if the key does not exist,
        /// or replaces the value using <paramref name="updateFactory"/> if it does.
        /// </summary>
        /// <param name="key">The key to update or add.</param>
        /// <param name="addValue">The value to add when the key is absent.</param>
        /// <param name="updateFactory">
        /// A function that produces the new value based on the currently observed value for <paramref name="key"/>.
        /// It may be invoked multiple times under contention; it should be side-effect free and deterministic.
        /// </param>
        /// <param name="oldValue">
        /// When the return value is <see langword="true"/>, contains the value that was replaced;
        /// otherwise set to <see langword="default"/> when a new entry was added.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if an existing value was replaced; otherwise <see langword="false"/> if a new entry was added.
        /// </returns>
        /// <remarks>
        /// CAS-based implementation that avoids indexer lookups and reference-equality heuristics.
        /// Compared to <see cref="ConcurrentDictionary{TKey,TValue}.AddOrUpdate(TKey, Func{TKey,TValue}, Func{TKey,TValue,TValue})"/>,
        /// this overload returns the replaced value and avoids delegate allocations on the add path.
        /// </remarks>
        public bool TryUpdateOrAdd(TKey key, TValue addValue, Func<TKey, TValue, TValue> updateFactory, out TValue? oldValue)
        {
            ArgumentNullException.ThrowIfNull(dict);
            ArgumentNullException.ThrowIfNull(updateFactory);

            // Fast add path
            if (dict.TryAdd(key, addValue))
            {
                oldValue = default;
                return false; // added
            }

            // Contended path: CAS-loop
            while (true)
            {
                if (dict.TryGetValue(key, out var current))
                {
                    var candidate = updateFactory(key, current);
                    if (dict.TryUpdate(key, candidate, current))
                    {
                        oldValue = current;
                        return true; // updated
                    }

                    // Lost race, retry
                    continue;
                }

                // Key disappeared → try add again with the provided addValue
                if (dict.TryAdd(key, addValue))
                {
                    oldValue = default;
                    return false; // added
                }
            }
        }

        /// <summary>
        /// Atomically adds a key with a value produced by <paramref name="addFactory"/> if the key does not exist,
        /// or replaces the value using <paramref name="updateFactory"/> if it does.
        /// </summary>
        /// <param name="key">The key to update or add.</param>
        /// <param name="addFactory">
        /// A function that produces the value to add when the key is absent.
        /// It is evaluated lazily and at most once per method call (its result is cached across retries).
        /// </param>
        /// <param name="updateFactory">
        /// A function that produces the new value based on the currently observed value for <paramref name="key"/>.
        /// It may be invoked multiple times under contention; it should be side-effect free and deterministic.
        /// </param>
        /// <param name="oldValue">
        /// When the return value is <see langword="true"/>, contains the value that was replaced;
        /// otherwise set to <see langword="default"/> when a new entry was added.
        /// </param>
        /// <returns>
        /// <see langword="true"/> if an existing value was replaced; otherwise <see langword="false"/> if a new entry was added.
        /// </returns>
        /// <remarks>
        /// CAS-based implementation with precise factory semantics:
        /// <list type="bullet">
        ///   <item><description><paramref name="addFactory"/> is evaluated only if an add is actually attempted, and at most once per call.</description></item>
        ///   <item><description><paramref name="updateFactory"/> can be evaluated multiple times, once per observed version under contention.</description></item>
        ///   <item><description>No indexer usage; relies only on <see cref="ConcurrentDictionary{TKey,TValue}.TryGetValue"/>, <see cref="ConcurrentDictionary{TKey,TValue}.TryUpdate"/>, and <see cref="ConcurrentDictionary{TKey,TValue}.TryAdd"/>.</description></item>
        /// </list>
        /// </remarks>
        public bool TryUpdateOrAdd(TKey key, Func<TKey, TValue> addFactory, Func<TKey, TValue, TValue> updateFactory, out TValue? oldValue)
        {
            ArgumentNullException.ThrowIfNull(dict);
            ArgumentNullException.ThrowIfNull(addFactory);
            ArgumentNullException.ThrowIfNull(updateFactory);

            bool addValueCreated = false;
            TValue? addValue = default;

            // Local function to compute add value lazily and at most once
            TValue GetAddValueOnce()
            {
                if (!addValueCreated)
                {
                    addValue = addFactory(key);
                    addValueCreated = true;
                }
                return addValue!;
            }

            // CAS-loop
            while (true)
            {
                if (dict.TryGetValue(key, out var current))
                {
                    var candidate = updateFactory(key, current);
                    if (dict.TryUpdate(key, candidate, current))
                    {
                        oldValue = current;
                        return true; // updated
                    }

                    // Lost race, retry
                    continue;
                }

                // Key absent → attempt to add (compute add value lazily and cache it)
                var v = GetAddValueOnce();
                if (dict.TryAdd(key, v))
                {
                    oldValue = default;
                    return false; // added
                }

                // Lost race on add, retry without recomputing add value
            }
        }

    }
}