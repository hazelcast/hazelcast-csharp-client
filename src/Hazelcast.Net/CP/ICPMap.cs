// Copyright (c) 2008-2023, Hazelcast, Inc. All Rights Reserved.
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

using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using Hazelcast.DistributedObjects;

namespace Hazelcast.CP;

/// <summary>
/// CPMap is a key-value store within CP. It supports atomic operations on an entry.
/// <remarks><para>CPMap is only available in <b>enterprise</b> cluster.</para><para>This data structure is not partitioned
/// across members in the cluster. It lives in one of the members.</para></remarks> 
/// </summary>
/// <typeparam name="TKey">Type of key in the map.</typeparam>
/// <typeparam name="TValue">Type of value in the map.</typeparam>
public interface ICPMap<TKey, TValue> : ICPDistributedObject
{
    /// <summary>
    /// Sets <paramref name="key"/> to <paramref name="value"/>.
    /// <remarks>See <see cref="SetAsync"/> for more optimal usage if existing value is not required.</remarks>
    /// </summary>
    /// <param name="key">The key to be set.</param>
    /// <param name="value">The value to be map to <paramref name="key"/>.</param>
    /// <returns>Value of the existing entry if any, otherwise null.</returns>
    [return:MaybeNull]
    Task<TValue> PutAsync([NotNull]TKey key, [NotNull]TValue value);
    
    /// <summary>
    /// Sets <paramref name="key"/> to <paramref name="value"/>.
    /// <remarks>This method should be preferred over <see cref="PutAsync"/> to reduce network footprint
    /// if the existing value map to <paramref name="key"/> is not required.</remarks>
    /// </summary>
    /// <param name="key">The key to be set.</param>
    /// <param name="value">The value to be map to <paramref name="key"/>.</param>
    Task SetAsync([NotNull]TKey key, [NotNull]TValue value);
    
    /// <summary>
    /// Gets value of the entry map to <paramref name="key"/> if any, otherwise null.
    /// </summary>
    /// <param name="key">The key of the entry.</param>
    /// <returns>Value of the entry if any, otherwise null.</returns>
    [return:MaybeNull]
    Task<TValue> GetAsync([NotNull]TKey key);

    /// <summary>
    /// Removes the entry if <paramref name="key"/> exists. Then, returns the value of the entry.
    /// </summary>
    /// <remarks>See <see cref="DeleteAsync"/> for more optimal usage if value of the entry is not required.</remarks>
    /// <param name="key">Key of the entry to be removed.</param>
    /// <returns>Value of the removed entry if any, otherwise null.</returns>
    [return:MaybeNull]
    Task<TValue> RemoveAsync([NotNull]TKey key);
    
    /// <summary>
    /// Deletes the entry if <paramref name="key"/> exists without returning value of the entry.
    /// <remarks>This method should be preferred over <see cref="RemoveAsync"/> to reduce network footprint
    /// if the value map to <paramref name="key"/> is not required.</remarks>
    /// </summary>
    /// <param name="key">Key of the entry to be deleted.</param>
    Task DeleteAsync([NotNull]TKey key);

    /// <summary>
    /// Atomically compares serialized forms of existing value with <paramref name="expectedValue"/>, and sets
    /// the <paramref name="newValue"/> if existing and expected values are equal. 
    /// </summary>
    /// <param name="key">Key of the entry.</param>
    /// <param name="expectedValue">Expected value map to <paramref name="key"/>.</param>
    /// <param name="newValue"><paramref name="newValue"/> to be set if existing value and <paramref name="expectedValue"/> are equal.</param>
    /// <returns>true if comparision and set operations are successful; otherwise false.</returns>
    Task<bool> CompareAndSetAsync([NotNull]TKey key, [NotNull]TValue expectedValue, [NotNull]TValue newValue);
}
