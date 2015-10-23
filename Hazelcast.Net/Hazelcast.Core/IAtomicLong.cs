/*
* Copyright (c) 2008-2015, Hazelcast, Inc. All Rights Reserved.
*
* Licensed under the Apache License, Version 2.0 (the "License");
* you may not use this file except in compliance with the License.
* You may obtain a copy of the License at
*
* http://www.apache.org/licenses/LICENSE-2.0
*
* Unless required by applicable law or agreed to in writing, software
* distributed under the License is distributed on an "AS IS" BASIS,
* WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
* See the License for the specific language governing permissions and
* limitations under the License.
*/

namespace Hazelcast.Core
{
    /// <summary>
    ///     IAtomicLong is a redundant and highly available distributed Atomic Long
    /// </summary>
    public interface IAtomicLong : IDistributedObject
    {
        /// <summary>Returns the name of this IAtomicLong instance.</summary>
        /// <remarks>Returns the name of this IAtomicLong instance.</remarks>
        /// <returns>name of this instance</returns>
        /// <summary>Atomically adds the given value to the current value.</summary>
        /// <remarks>Atomically adds the given value to the current value.</remarks>
        /// <param name="delta">the value to add</param>
        /// <returns>the updated value</returns>
        long AddAndGet(long delta);

        /// <summary>
        ///     Atomically sets the value to the given updated value
        ///     only if the current value
        ///     <code>==</code>
        ///     the expected value.
        /// </summary>
        /// <param name="expect">the expected value</param>
        /// <param name="update">the new value</param>
        /// <returns>
        ///     true if successful; or false if the actual value
        ///     was not equal to the expected value.
        /// </returns>
        bool CompareAndSet(long expect, long update);

        /// <summary>Atomically decrements the current value by one.</summary>
        /// <remarks>Atomically decrements the current value by one.</remarks>
        /// <returns>the updated value</returns>
        long DecrementAndGet();

        /// <summary>Gets the current value.</summary>
        /// <remarks>Gets the current value.</remarks>
        /// <returns>the current value</returns>
        long Get();

        /// <summary>Atomically adds the given value to the current value.</summary>
        /// <remarks>Atomically adds the given value to the current value.</remarks>
        /// <param name="delta">the value to add</param>
        /// <returns>the old value before the add</returns>
        long GetAndAdd(long delta);

        /// <summary>Atomically sets the given value and returns the old value.</summary>
        /// <remarks>Atomically sets the given value and returns the old value.</remarks>
        /// <param name="newValue">the new value</param>
        /// <returns>the old value</returns>
        long GetAndSet(long newValue);

        /// <summary>Atomically increments the current value by one.</summary>
        /// <remarks>Atomically increments the current value by one.</remarks>
        /// <returns>the updated value</returns>
        long IncrementAndGet();

        /// <summary>Atomically increments the current value by one.</summary>
        /// <remarks>Atomically increments the current value by one.</remarks>
        /// <returns>the old value</returns>
        long GetAndIncrement();

        /// <summary>Atomically sets the given value.</summary>
        /// <remarks>Atomically sets the given value.</remarks>
        /// <param name="newValue">the new value</param>
        void Set(long newValue);

       // /// <summary>
       // /// Alters the currently stored value by applying a function on it.
       // /// </summary>
       // /// <param name="function">the function</param>
       // /// <exception cref="ArgumentNullException">if function is null</exception>
       // void Alter(Func<long, long> function);

       // /// <summary>
       // /// Alters the currently stored value by applying a function on it and gets the result.
       // /// </summary>
       // /// <param name="function">the function</param>
       // /// <returns>the new value</returns>
       // /// <exception cref="ArgumentNullException">if function is null</exception>
       // long AlterAndGet(Func<long, long> function);

       ///// <summary>
       // /// Alters the currently stored value by applying a function on it and gets the old value.
       // /// </summary>
       // /// <param name="function">the function</param>
       // /// <returns>the old value</returns>
       // /// <exception cref="ArgumentNullException">if function is null</exception>
       // long GetAndAlter(Func<long, long> function);

       // /// <summary>
       // /// Applies a function on the value, the actual stored value will not change.
       // /// </summary>
       // /// <param name="function">the function</param>
       // /// <returns>the result of the function application</returns>
       // /// <exception cref="ArgumentNullException">if function is null</exception>
       // R Apply<R>(Func<long, R> function);
   

    }
}