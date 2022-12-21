// Copyright (c) 2008-2022, Hazelcast, Inc. All Rights Reserved.
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

// ReSharper disable once CheckNamespace
namespace System.Collections.Generic;

#if NETSTANDARD2_0

internal static class CollectionExtensions
{
    /// <summary>Tries to add the specified <paramref name="key" /> and <paramref name="value" /> to the <paramref name="dictionary" />.</summary>
    /// <param name="dictionary">A dictionary with keys of type <typeparamref name="TKey" /> and values of type <typeparamref name="TValue" />.</param>
    /// <param name="key">The key of the value to add.</param>
    /// <param name="value">The value to add.</param>
    /// <typeparam name="TKey">The type of the keys in the dictionary.</typeparam>
    /// <typeparam name="TValue">The type of the values in the dictionary.</typeparam>
    /// <exception cref="T:System.ArgumentNullException">
    /// <paramref name="dictionary" /> is <see langword="null" />.</exception>
    /// <returns>
    /// <see langword="true" /> when the <paramref name="key" /> and <paramref name="value" /> are successfully added to the <paramref name="dictionary" />; <see langword="false" /> when the <paramref name="dictionary" /> already contains the specified <paramref name="key" />, in which case nothing gets added.</returns>
    public static bool TryAdd<TKey, TValue>(
        this IDictionary<TKey, TValue> dictionary,
        TKey key,
        TValue value)
    {
        if (dictionary == null)
            throw new ArgumentNullException(nameof(dictionary));
        if (dictionary.ContainsKey(key))
            return false;
        dictionary.Add(key, value);
        return true;
    }
}

#endif