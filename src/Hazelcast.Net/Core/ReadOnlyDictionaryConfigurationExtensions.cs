// Copyright (c) 2008-2024, Hazelcast, Inc. All Rights Reserved.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
// http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using System;
using System.Collections.Generic;

namespace Hazelcast.Core
{
    /// <summary>
    /// Provides extension methods for the <see cref="IReadOnlyDictionary{TKey,TValue}"/> interface.
    /// </summary>
    internal static class ReadOnlyDictionaryConfigurationExtensions
    {
        /// <summary>
        /// Gets a string value.
        /// </summary>
        /// <param name="keyValues">Key-values.</param>
        /// <param name="key">The key.</param>
        /// <param name="defaultValue">An optional value to return if the key was not found.</param>
        /// <returns>The value.</returns>
        public static string GetStringValue(this IReadOnlyDictionary<string, string> keyValues, string key, string defaultValue = null)
        {
            if (keyValues == null) throw new ArgumentNullException(nameof(keyValues));
            if (!keyValues.TryGetValue(key, out var arg))
            {
                arg = defaultValue ??
                      throw new InvalidOperationException($"Failed to get a string value for key '{key}'.");
            }

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

            if (keyValues == null) throw new ArgumentNullException(nameof(keyValues));
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
            if (keyValues == null) throw new ArgumentNullException(nameof(keyValues));
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

            if (keyValues == null) throw new ArgumentNullException(nameof(keyValues));
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
            if (keyValues == null) throw new ArgumentNullException(nameof(keyValues));
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

            if (keyValues == null) throw new ArgumentNullException(nameof(keyValues));
            return keyValues.TryGetValue(key, out var arg) && int.TryParse(arg, out value);
        }
    }
}
