namespace System;

/// <summary>
/// Provides extension methods for working with enumeration values.
/// </summary>
public static class EnumExtensions
{
    extension(Enum)
    {
        /// <summary>
        /// Retrieves the enum value associated with the specified string name.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="name">The string name of the enum value to retrieve.</param>
        /// <returns>The enum value associated with the specified name.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is empty or the enum value is not found.</exception>
        public static TEnum GetValue<TEnum>(string name) where TEnum : struct, Enum =>
            EnumHelper<TEnum>.GetValue(name, ignoreCase: false);

        /// <summary>
        /// Retrieves the enum value associated with the specified string name.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="name">The string name of the enum value to retrieve.</param>
        /// <param name="ignoreCase">Whether to ignore case when matching the name.</param>
        /// <returns>The enum value associated with the specified name.</returns>
        /// <exception cref="ArgumentNullException">Thrown when <paramref name="name"/> is null.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="name"/> is empty or the enum value is not found.</exception>
        public static TEnum GetValue<TEnum>(string name, bool ignoreCase) where TEnum : struct, Enum =>
             EnumHelper<TEnum>.GetValue(name, ignoreCase);

        /// <summary>
        /// Tries to get the enum value associated with the specified string name.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="name">The string name of the enum value to retrieve.</param>
        /// <param name="value">When this method returns, contains the enum value associated with the specified name, if the name is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
        /// <returns><see langword="true"/> if the enum value was found; otherwise, <see langword="false"/>.</returns>
        public static bool TryGetValue<TEnum>(string? name, out TEnum value) where TEnum : struct, Enum =>
            EnumHelper<TEnum>.TryGetValue(name, ignoreCase: false, out value);

        /// <summary>
        /// Tries to get the enum value associated with the specified string name.
        /// </summary>
        /// <typeparam name="TEnum">The type of the enum.</typeparam>
        /// <param name="name">The string name of the enum value to retrieve.</param>
        /// <param name="ignoreCase">Whether to ignore case when matching the name.</param>
        /// <param name="value">When this method returns, contains the enum value associated with the specified name, if the name is found; otherwise, the default value for the type of the value parameter. This parameter is passed uninitialized.</param>
        /// <returns><see langword="true"/> if the enum value was found; otherwise, <see langword="false"/>.</returns>
        public static bool TryGetValue<TEnum>(string? name, bool ignoreCase, out TEnum value) where TEnum : struct, Enum =>
            EnumHelper<TEnum>.TryGetValue(name, ignoreCase, out value);
    }
}
