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
using Hazelcast.Protocol.Codecs;

namespace Hazelcast.CP
{
    /// <summary>
    /// Defines the CP subsystem.
    /// </summary>
    public interface ICPSubsystem
    {
        /// <summary>
        /// Gets an <see cref="IAtomicLong"/> distributed object.
        /// </summary>
        /// <param name="name">The unique name of the atomic long.</param>
        /// <returns>The atomic long that was retrieved or created.</returns>
        /// <remarks>
        /// <para>If an object with the specified <paramref name="name"/> does not
        /// exist already in the cluster, a new object is created.</para>
        /// </remarks>
        Task<IAtomicLong> GetAtomicLongAsync(string name);

        /// <summary>
        /// Gets an <see cref="IAtomicReference{T}"/> distributed object.
        /// </summary>
        /// <param name="name">The unique name of the atomic reference.</param>
        /// <returns>The atomic reference that was retrieved or created.</returns>
        /// <remarks>
        /// <para>If an object with the specified <paramref name="name"/> does not
        /// exist already in the cluster, a new object is created.</para>
        /// </remarks>
        Task<IAtomicReference<T>> GetAtomicReferenceAsync<T>(string name);

        /// <summary>
        /// Gets an <see cref="IFencedLock"/> distributed object.
        /// </summary>
        /// <param name="name">The unique name of the fenced lock.</param>
        /// <para>If an object with the specified <paramref name="name"/> does not
        /// exist already in the cluster, a new object is created.</para>
        /// <returns></returns>
        Task<IFencedLock> GetLockAsync(string name);

        /// <summary>
        /// Gets an <see cref="ICPMap{TKey,TValue}"/> distributed object.
        /// <remarks><para>CPMap is only available in <b>enterprise</b> cluster.</para>
        /// <para>The map will be created in <b>DEFAULT</b> CP group if no group name provided within <paramref name="name"/>.
        /// If a group name provided, first, the group will be initialized,
        /// if does not exist. Then, <see cref="ICPMap{TKey,TValue}"/> instance will be created on this group.
        /// </para>
        /// </remarks>
        /// </summary>
        /// <param name="name">The unique name of the map. It can contain the group name like <code>"myMap@group1"</code></param>
        /// <typeparam name="TKey">Type of the key.</typeparam>
        /// <typeparam name="TValue">Type of the value.</typeparam>
        Task<ICPMap<TKey, TValue>> GetMapAsync<TKey, TValue>(string name);
    }   
}
