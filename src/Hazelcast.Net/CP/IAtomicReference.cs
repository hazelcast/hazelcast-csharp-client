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

using System.Threading.Tasks;

namespace Hazelcast.CP
{
    /// <summary>
    /// Defines a redundant and highly-available distributed atomic reference.
    /// </summary>
    public interface IAtomicReference<T>: ICPDistributedObject
    {
        /// <summary>
        /// Compares the value for equality and, if equal, replaces the current value.
        /// </summary>
        /// <param name="comparand">The value that is compared to the current value.</param>
        /// <param name="value">The value that replaces the current value if the comparison results in equality.</param>
        /// <returns>The updated value.</returns>
        Task<bool> CompareAndSetAsync(T comparand, T value);

        /// <summary>
        /// Gets the current value.
        /// </summary>
        /// <returns>The current value.</returns>
        /// <remarks>
        /// If <see cref="T"/> is a struct, method will return <c>default(T)</c> when reference is not set.
        /// You can make it return <c>null</c> instead by using <see cref="System.Nullable{T}"/>.
        /// </remarks>
        Task<T> GetAsync();

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="value">The value to set.</param>
        Task SetAsync(T value);

        /// <summary>
        /// Sets the current value, and returns the original value.
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <returns>The original value.</returns>
        /// <remarks>
        /// If <see cref="T"/> is a struct, method will return <c>default(T)</c> when reference is not set.
        /// You can make it return <c>null</c> instead by using <see cref="System.Nullable{T}"/>.
        /// </remarks>
        Task<T> GetAndSetAsync(T value);

        /// <summary>
        /// Checks if the stored reference is <c>null</c>.
        /// </summary>
        /// <returns><c>true</c> if <c>null</c>, <c>false</c> otherwise</returns>
        Task<bool> IsNullAsync();

        /// <summary>
        /// Clears current stored reference, so it becomes <c>null</c>.
        /// </summary>
        Task ClearAsync();

        /// <summary>
        /// Checks if the reference contains the value.
        /// </summary>
        /// <param name="value">The value to check (can be <c>null</c>).</param>
        /// <returns>Whether the reference contains the value specified.</returns>
        Task<bool> ContainsAsync(T value);
    }
}
