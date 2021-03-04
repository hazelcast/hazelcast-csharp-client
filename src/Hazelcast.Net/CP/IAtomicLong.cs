// Copyright (c) 2008-2021, Hazelcast, Inc. All Rights Reserved.
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
    /// Defines a redundant and highly-available distributed atomic <c>long</c>.
    /// </summary>
    public interface IAtomicLong : ICPDistributedObject
    {
        /// <summary>
        /// Adds the specified value to the current value, and returns the updated value.
        /// </summary>
        /// <param name="value">The value to add.</param>
        /// <returns>The updated value.</returns>
        Task<long> AddAndGetAsync(long value);

        /// <summary>
        /// Compares the value for equality and, if equal, replaces the current value.
        /// </summary>
        /// <param name="comparand">The value that is compared to the current value.</param>
        /// <param name="value">The value that replaces the current value if the comparison results in equality.</param>
        /// <returns>The updated value.</returns>
        Task<bool> CompareAndSetAsync(long comparand, long value);

        /// <summary>
        /// Decrements the current value by one, and returns the updated value.
        /// </summary>
        /// <returns>The updated value.</returns>
        Task<long> DecrementAndGetAsync();

        /// <summary>
        /// Decrements the current value by one, and returns the original value.
        /// </summary>
        /// <returns>The original value.</returns>
        Task<long> GetAndDecrementAsync();

        /// <summary>
        /// Gets the current value.
        /// </summary>
        /// <returns>The current value.</returns>
        Task<long> GetAsync();

        /// <summary>
        /// Adds the specified value to the current value, and returns the original value.
        /// </summary>
        /// <param name="value">The value to add.</param>
        /// <returns>The original value.</returns>
        Task<long> GetAndAddAsync(long value);

        /// <summary>
        /// Sets the current value, and returns the original value.
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <returns>The original value.</returns>
        Task<long> GetAndSetAsync(long value);

        /// <summary>
        /// Increments the current value by one, and returns the updated value.
        /// </summary>
        /// <returns>The updated value.</returns>
        Task<long> IncrementAndGetAsync();

        /// <summary>
        /// Increments the current value by one, and returns the original value.
        /// </summary>
        /// <returns>The original value.</returns>
        Task<long> GetAndIncrementAsync();

        /// <summary>
        /// Sets the value.
        /// </summary>
        /// <param name="value">The value to set.</param>
        /// <returns>The updated value.</returns>
        Task SetAsync(long value);
    }
}