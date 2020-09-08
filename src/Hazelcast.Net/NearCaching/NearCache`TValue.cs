// Copyright (c) 2008-2020, Hazelcast, Inc. All Rights Reserved.
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
using System.Threading.Tasks;
using Hazelcast.Core;
using Hazelcast.Serialization;

namespace Hazelcast.NearCaching
{
    /// <summary>
    /// Represents a typed Near Cache.
    /// </summary>
    /// <typeparam name="TValue">The type of the values stored in the cache.</typeparam>
    internal class NearCache<TValue> : IAsyncDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NearCache{TValue}"/> class.
        /// </summary>
        /// <param name="cache">A non-typed Near Cache.</param>
        public NearCache(NearCache cache)
        {
            InnerCache = cache;
        }

        /// <summary>
        /// Gets the inner non-typed Near Cache.
        /// </summary>
        public NearCache InnerCache { get; }

        /// <summary>
        /// Tries to add a value to the cache.
        /// </summary>
        /// <param name="keyData">The key data.</param>
        /// <param name="valueData">The value data.</param>
        /// <returns><c>true</c> if the value could be added; otherwise <c>false</c>.</returns>
        public async ValueTask<bool> TryAddAsync(IData keyData, IData valueData)
            => await InnerCache.TryAddAsync(keyData, valueData).CAF();

        /// <summary>
        /// Tries to get a value from, or add a value to, the cache.
        /// </summary>
        /// <param name="keyData">The key data.</param>
        /// <param name="valueFactory">A factory that accepts the key data and returns the value data.</param>
        /// <returns>An attempt at getting or adding a value to the cache.</returns>
        public async Task<Attempt<TValue>> TryGetOrAddAsync(IData keyData, Func<IData, Task<IData>> valueFactory)
        {
            try
            {
                var (success, valueObject) = await InnerCache.TryGetOrAddAsync(keyData, valueFactory).CAF();
                var value = ToTValue(valueObject);
                if (success && valueObject == null) Console.WriteLine("MEH! got null valueObject from the cache");
                if (success && value == null) Console.WriteLine("MEH! got null value from " + valueObject);
                if (!success) Console.WriteLine("MEH! failed to get value");
                if (success) return value;
                return Attempt.Fail(value);
            }
            catch
            {
                InnerCache.Remove(keyData);
                throw;
            }
        }

        /// <summary>
        /// Tries to get a value from the cache.
        /// </summary>
        /// <param name="keyData">The key data.</param>
        /// <returns>An attempt at getting the value from the cache.</returns>
        public async ValueTask<Attempt<TValue>> TryGetAsync(IData keyData)
        {
            var (success, value) = await InnerCache.TryGetAsync(keyData).CAF();
            if (!success) return Attempt.Failed;

            return ToTValue(value);
        }

        /// <summary>
        /// Determines whether the cache contains an entry.
        /// </summary>
        /// <param name="keyData">The key data.</param>
        /// <returns>Whether the cache contains an entry with the specified key.</returns>
        public async ValueTask<bool> ContainsKeyAsync(IData keyData)
            => await InnerCache.ContainsKeyAsync(keyData).CAF();

        /// <summary>
        /// Removes an entry from the cache.
        /// </summary>
        /// <param name="keyData">The key data.</param>
        /// <returns>Whether an entry was removed.</returns>
        public bool Remove(IData keyData)
            => InnerCache.Remove(keyData);

        /// <summary>
        /// Clears the cache.
        /// </summary>
        public void Clear()
            => InnerCache.Clear();

        /// <summary>
        /// Converts a cached value object to a <typeparamref name="TValue"/> value.
        /// </summary>
        /// <param name="valueObject">The value object.</param>
        /// <returns>The <typeparamref name="TValue"/> value.</returns>
        /// <remarks>
        /// <para>Depending on the <see cref="InMemoryFormat"/> configured for the cache,
        /// the internal cached value can be either <see cref="IData"/> (i.e. serialized),
        /// or the de-serialized <typeparamref name="TValue"/> value.</para>
        /// </remarks>
        private TValue ToTValue(object valueObject)
        {
            if (valueObject == null) return default; // FIXME?

            return InnerCache.InMemoryFormat.Equals(InMemoryFormat.Binary)
                ? InnerCache.SerializationService.ToObject<TValue>(valueObject)
                : (TValue) valueObject;
        }

        /// <inheritdoc />
        public async ValueTask DisposeAsync()
        {
            await InnerCache.DisposeAsync().CAF();
        }
    }
}