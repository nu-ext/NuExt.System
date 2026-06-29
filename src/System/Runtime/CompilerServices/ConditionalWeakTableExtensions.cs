using System.Threading;

namespace System.Runtime.CompilerServices;

public static class ConditionalWeakTableExtensions
{
    extension<TKey, TValue>(ConditionalWeakTable<TKey, TValue> table)
        where TKey : class
        where TValue : class
    {
#if !NET5_0_OR_GREATER && !NETSTANDARD2_1_OR_GREATER
        /// <summary>Adds the key and value if the key doesn't exist, or updates the existing key's value if it does exist.</summary>
        /// <param name="key">key to add or update. May not be null.</param>
        /// <param name="value">value to associate with key.</param>
        /// <exception cref="ArgumentNullException"><paramref name="key"/> is <see langword="null"/>.</exception>
        public void AddOrUpdate(TKey key, TValue value)
        {
            lock (LockPool.Get(key))
            {
                table.Remove(key);
                table.Add(key, value);
            }
        }
#endif
    }
}
