using System;
using System.Collections.Generic;

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides extension methods for the <see cref="IReadOnlyDictionary{TKey,TValue}"/> interface.
    /// </summary>
    public static class ReadOnlyDictionaryConfigurationExtensions
    {
        /// <summary>
        /// Gets a string value.
        /// </summary>
        /// <param name="keyValues">Key-values.</param>
        /// <param name="key">The key.</param>
        /// <returns>The value.</returns>
        public static string GetStringValue(this IReadOnlyDictionary<string, string> keyValues, string key)
        {
            if (!keyValues.TryGetValue(key, out var arg))
                throw new InvalidOperationException($"Failed to get a string value for key '{key}'.");

            return arg;
        }

        /// <summary>
        /// Tries to get a string value.
        /// </summary>
        /// <param name="keyValues">Key-values.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>true if a value was found; otherwise false.</returns>
        public static bool TryGetStringValue(this IReadOnlyDictionary<string, string> keyValues, string key, out string value)
        {
            value = default;
            
            if (!keyValues.TryGetValue(key, out var arg))
                return false;

            value = arg;
            return true;
        }
        
        /// <summary>
        /// Gets a Guid value.
        /// </summary>
        /// <param name="keyValues">Key-values.</param>
        /// <param name="key">The key.</param>
        /// <returns>The value.</returns>
        public static Guid GetGuidValue(this IReadOnlyDictionary<string, string> keyValues, string key)
        {
            if (!keyValues.TryGetValue(key, out var arg) || !Guid.TryParse(arg, out var value))
                throw new InvalidOperationException($"Failed to get a Guid value for key '{key}'.");

            return value;
        }

        /// <summary>
        /// Tries to get a Guid value.
        /// </summary>
        /// <param name="keyValues">Key-values.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>true if a value was found; otherwise false.</returns>
        public static bool TryGetGuidValue(this IReadOnlyDictionary<string, string> keyValues, string key, out Guid value)
        {
            value = default;
            
            return keyValues.TryGetValue(key, out var arg) && Guid.TryParse(arg, out value);
        }
        
        /// <summary>
        /// Gets an integer value.
        /// </summary>
        /// <param name="keyValues">Key-values.</param>
        /// <param name="key">The key.</param>
        /// <returns>The value.</returns>
        public static int GetIntValue(this IReadOnlyDictionary<string, string> keyValues, string key)
        {
            if (!keyValues.TryGetValue(key, out var arg) || !int.TryParse(arg, out var value))
                throw new InvalidOperationException($"Failed to get an integer value for key '{key}'.");

            return value;
        }

        /// <summary>
        /// Tries to get an integer value.
        /// </summary>
        /// <param name="keyValues">Key-values.</param>
        /// <param name="key">The key.</param>
        /// <param name="value">The value.</param>
        /// <returns>true if a value was found; otherwise false.</returns>
        public static bool TryGetIntValue(this IReadOnlyDictionary<string, string> keyValues, string key, out int value)
        {
            value = default;

            return keyValues.TryGetValue(key, out var arg) && int.TryParse(arg, out value);
        }
    }
}